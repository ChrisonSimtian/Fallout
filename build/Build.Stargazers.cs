// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tools.GitHub;
using Fallout.Common.Utilities;

partial class Build
{
    AbsolutePath StargazersFile => TemporaryDirectory / "stargazers.csv";

    [UsedImplicitly]
    Target UpdateStargazers => _ => _
        .Executes(async () =>
        {
            var stargazerUsers = await GitHubTasks.GitHubClient.Activity.Starring.GetAllStargazers(
                GitRepository.GetGitHubOwner(),
                GitRepository.GetGitHubName());
            var stargazerEntries = stargazerUsers.Select(async x =>
            {
                var user = await GitHubTasks.GitHubClient.User.Get(x.Login);
                return new[]
                       {
                           user.Login.DoubleQuote(),
                           user.Name.DoubleQuote(),
                           user.Company.DoubleQuote(),
                           user.Location.DoubleQuote(),
                           user.Email.DoubleQuote(),
                           user.Blog.DoubleQuote()
                       };
            }).ToList();

            await Task.WhenAll(stargazerEntries);

            StargazersFile.WriteAllLines(
                new[] { new[] { "Login", "Name", "Company", "Location", "Email", "Blog" } }
                    .Concat(stargazerEntries.Select(x => x.Result).OrderBy(x => x.First()))
                    .Select(x => x.JoinComma()));
        });
}
