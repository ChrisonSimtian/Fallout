using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fallout.Common;
using Fallout.Common.ChangeLog;
using Octokit;
using Fallout.Application;
using Fallout.Application.Tools.GitHub;
using Fallout.Kernel.IO;
using Fallout.Kernel;

using Fallout.Application.CI;
namespace Fallout.Application.Components;

[ParameterPrefix(GitHubRelease)]
public interface ICreateGitHubRelease : IHasGitRepository, IHasChangelog
{
    public const string GitHubRelease = nameof(GitHubRelease);

    [Parameter] [Secret] string GitHubToken => TryGetValue(() => GitHubToken) ?? CiHost.GitHubActions?.Token;

    string Name { get; }
    bool Prerelease => false;
    bool Draft => false;

    IEnumerable<AbsolutePath> AssetFiles { get; }

    Target CreateGitHubRelease => _ => _
        .Requires(() => GitHubToken)
        .Executes(async () =>
        {
            async Task<Release> GetOrCreateRelease()
            {
                try
                {
                    return await GitHubTasks.GitHubClient.Repository.Release.Create(
                        GitRepository.GetGitHubOwner(),
                        GitRepository.GetGitHubName(),
                        new NewRelease(Name)
                        {
                            Name = Name,
                            Prerelease = Prerelease,
                            Draft = Draft,
                            Body = ChangelogTasks.ExtractChangelogSectionNotes(ChangelogFile).JoinNewLine()
                        });

                }
                catch
                {
                    return await GitHubTasks.GitHubClient.Repository.Release.Get(
                        GitRepository.GetGitHubOwner(),
                        GitRepository.GetGitHubName(),
                        Name);
                }
            }

            GitHubTasks.GitHubClient.Credentials = new Credentials(GitHubToken.NotNull());

            var release = await GetOrCreateRelease();

            var uploadTasks = AssetFiles.Select(async x =>
            {
                await using var assetFile = File.OpenRead(x);
                var asset = new ReleaseAssetUpload
                            {
                                FileName = x.Name,
                                ContentType = "application/octet-stream",
                                RawData = assetFile
                            };
                await GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, asset);
            }).ToArray();

            Task.WaitAll(uploadTasks);
        });
}
