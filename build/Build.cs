// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging;
using Fallout.Common;
using Fallout.Common.CI;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.Execution;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Common.ProjectModel;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.GitHub;
using Fallout.Common.Tools.GitVersion;
using Fallout.Common.Utilities;
using Fallout.Components;
using static Fallout.Common.ControlFlow;
using static Fallout.Common.Tools.DotNet.DotNetTasks;
using static Fallout.Common.Tools.ReSharper.ReSharperTasks;

[DotNetVerbosityMapping]
[ShutdownDotNetAfterServerBuild]
partial class Build
    : NukeBuild,
        IHazChangelog,
        IHazGitRepository,
        IHazGitVersion,
        IHazSolution,
        IRestore,
        ICompile,
        IPack,
        ITest,
        IReportCoverage,
        IReportIssues,
        IReportDuplicates,
        IPublish,
        ICreateGitHubRelease
{
    public static int Main() => Execute<Build>(x => ((IPack)x).Pack);

    [CI] readonly GitHubActions GitHubActions;

    GitVersion GitVersion => From<IHazGitVersion>().Versioning;
    GitRepository GitRepository => From<IHazGitRepository>().GitRepository;

    [Solution(GenerateProjects = true)] readonly Solution Solution;
    Fallout.Common.ProjectModel.Solution IHazSolution.Solution => Solution;

    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath SourceDirectory => RootDirectory / "source";

    const string MainBranch = "main";

    // Versioning constants moved from former Build.GitFlow.cs.
    // Trunk-based; Nerdbank.GitVersioning will replace GitVersion in a follow-up PR.
    [Parameter] readonly bool Major;
    string MajorMinorPatchVersion => Major ? $"{GitVersion.Major + 1}.0.0" : GitVersion.MajorMinorPatch;
    string MilestoneTitle => $"v{MajorMinorPatchVersion}";

    AbsolutePath IHazArtifacts.ArtifactsDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before<IRestore>()
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("*/bin", "*/obj").DeleteDirectories();
            OutputDirectory.CreateOrCleanDirectory();
        });

    Configure<DotNetBuildSettings> ICompile.CompileSettings => _ => _
        .When(!ScheduledTargets.Contains(((IPublish)this).Publish) && !ScheduledTargets.Contains(Install), _ => _
            .ClearProperties());

    Configure<DotNetPublishSettings> ICompile.PublishSettings => _ => _
        .When(!ScheduledTargets.Contains(((IPublish)this).Publish) && !ScheduledTargets.Contains(Install), _ => _
            .ClearProperties());

    IEnumerable<(Fallout.Common.ProjectModel.Project Project, string Framework)> ICompile.PublishConfigurations =>
        from project in new[] { Solution.Fallout_GlobalTool, Solution.Fallout_MSBuildTasks }
        from framework in project.GetTargetFrameworks()
        select (project, framework);

    IEnumerable<Fallout.Common.ProjectModel.Project> ITest.TestProjects => Partition.GetCurrent(Solution.GetAllProjects("*.Tests"));

    [Parameter]
    public int TestDegreeOfParallelism { get; } = 1;

    Configure<DotNetTestSettings> ITest.TestSettings => _ => _
        .SetProcessEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", bool.TrueString);

    Target ITest.Test => _ => _
        .Inherit<ITest>()
        .Partition(2);

    bool IReportCoverage.CreateCoverageHtmlReport => true;
    bool IReportCoverage.ReportToCodecov => false;

    IEnumerable<(string PackageId, string Version)> IReportIssues.InspectCodePlugins
        => new (string PackageId, string Version)[]
           {
               new("ReSharperPlugin.CognitiveComplexity", ReSharperPluginLatest)
           };

    bool IReportIssues.InspectCodeFailOnWarning => false;
    bool IReportIssues.InspectCodeReportWarnings => true;
    IEnumerable<string> IReportIssues.InspectCodeFailOnIssues => new string[0];
    IEnumerable<string> IReportIssues.InspectCodeFailOnCategories => new string[0];

    // Local Terminal runs use a placeholder version so packed nupkgs don't
    // collide with real releases. CI runs let Nerdbank.GitVersioning inject the
    // real version via MSBuild ($PackageVersion).
    Configure<DotNetPackSettings> IPack.PackSettings => _ => _
        .When(Host is Terminal, _ => _
            .SetVersion(DefaultDeploymentVersion));

    string DefaultDeploymentVersion => "9999.0.0";

    [Parameter] [Secret] readonly string NuGetApiKey;

    // Publishing to GitHub Packages on this fork until the post-hard-fork
    // project rename lands. nuget.org would require the new name and can't
    // be done under "Fallout.*" — see project_nuke_strategy memory note.
    // Repository owner comes from GITHUB_REPOSITORY_OWNER, automatically set
    // by GitHub Actions runners. ChrisonSimtian is the local-dev fallback.
    string IPublish.NuGetSource =>
        $"https://nuget.pkg.github.com/{EnvironmentInfo.GetVariable("GITHUB_REPOSITORY_OWNER") ?? "ChrisonSimtian"}/index.json";
    string IPublish.NuGetApiKey => NuGetApiKey;

    Target IPublish.Publish => _ => _
        .Inherit<IPublish>()
        .Consumes(From<IPack>().Pack)
        .Requires(() => GitRepository.IsOnMainBranch() && Host is GitHubActions && GitHubActions.Workflow == ReleaseWorkflow)
        .WhenSkipped(DependencyBehavior.Execute);

    IEnumerable<AbsolutePath> NuGetPackageFiles
        => From<IPack>().PackagesDirectory.GlobFiles("*.nupkg");

    Target DeletePackages => _ => _
        .DependentFor<IPublish>()
        .After<IPack>()
        .OnlyWhenStatic(() => Host is Terminal)
        .Executes(() =>
        {
            var packagesDirectory = NuGetPackageResolver.GetPackagesDirectory(packagesConfigFile: BuildProjectFile);
            var packageDirectories = packagesDirectory.GlobDirectories($"nuke.*/{DefaultDeploymentVersion}");
            packageDirectories.DeleteDirectories();
        });

    string ICreateGitHubRelease.Name => MajorMinorPatchVersion;
    IEnumerable<AbsolutePath> ICreateGitHubRelease.AssetFiles => NuGetPackageFiles;

    // Rewritten override (not inheriting the base) so we can build a rich
    // release body that lists the milestone contents. The base implementation
    // reads CHANGELOG.md's [vNext] section, which we don't maintain on this
    // fork. We rely on milestones + GitHub's auto-generated PR list instead.
    Target ICreateGitHubRelease.CreateGitHubRelease => _ => _
        .TriggeredBy<IPublish>()
        .ProceedAfterFailure()
        .Requires(() => From<ICreateGitHubRelease>().GitHubToken)
        .OnlyWhenStatic(() => GitRepository.IsOnMainBranch())
        .Executes(async () =>
        {
            var client = GitHubTasks.GitHubClient;
            client.Credentials = new global::Octokit.Credentials(From<ICreateGitHubRelease>().GitHubToken.NotNull());
            var owner = GitRepository.GetGitHubOwner();
            var name = GitRepository.GetGitHubName();
            var version = MajorMinorPatchVersion;

            // Find a milestone whose title starts with this release's version
            // (matches both our "10.2.0 - <desc>" convention and the legacy
            // "v10.2.0" form). First match wins.
            var milestones = await client.Issue.Milestone.GetAllForRepository(
                owner, name,
                new global::Octokit.MilestoneRequest { State = global::Octokit.ItemStateFilter.All });
            var milestone = milestones.FirstOrDefault(m =>
                m.Title.StartsWith(version, StringComparison.Ordinal) ||
                m.Title.StartsWith($"v{version}", StringComparison.Ordinal));

            var milestoneItems = milestone == null
                ? Array.Empty<global::Octokit.Issue>()
                : (await client.Issue.GetAllForRepository(owner, name, new global::Octokit.RepositoryIssueRequest
                {
                    Milestone = milestone.Number.ToString(),
                    State = global::Octokit.ItemStateFilter.Closed
                })).ToArray();

            // Build the release body. GitHub will append its auto-generated
            // PR-since-last-tag list because GenerateReleaseNotes=true.
            var body = new System.Text.StringBuilder();
            if (milestone != null)
            {
                body.AppendLine($"**Milestone:** [{milestone.Title}]({milestone.HtmlUrl})");
                body.AppendLine();

                var pulls = milestoneItems.Where(i => i.PullRequest != null).OrderBy(i => i.Number).ToArray();
                var issues = milestoneItems.Where(i => i.PullRequest == null).OrderBy(i => i.Number).ToArray();

                if (pulls.Length > 0)
                {
                    body.AppendLine("### Pull requests in this milestone");
                    foreach (var pr in pulls)
                        body.AppendLine($"- {pr.Title} (#{pr.Number}) — @{pr.User.Login}");
                    body.AppendLine();
                }

                if (issues.Length > 0)
                {
                    body.AppendLine("### Issues closed in this milestone");
                    foreach (var issue in issues)
                        body.AppendLine($"- {issue.Title} (#{issue.Number})");
                    body.AppendLine();
                }
            }

            global::Octokit.Release release;
            try
            {
                release = await client.Repository.Release.Create(owner, name,
                    new global::Octokit.NewRelease(version)
                    {
                        Name = version,
                        Body = body.ToString(),
                        Prerelease = false,
                        Draft = false,
                        GenerateReleaseNotes = true
                    });
            }
            catch
            {
                release = await client.Repository.Release.Get(owner, name, version);
            }

            foreach (var assetPath in NuGetPackageFiles)
            {
                await using var assetStream = System.IO.File.OpenRead(assetPath);
                await client.Repository.Release.UploadAsset(release, new global::Octokit.ReleaseAssetUpload
                {
                    FileName = assetPath.Name,
                    ContentType = "application/octet-stream",
                    RawData = assetStream
                });
            }

            // Notify each milestone item that it's shipped.
            foreach (var item in milestoneItems)
                await client.Issue.Comment.Create(owner, name, item.Number, $"Released in {version}! 🎉");
        });

    Target Install => _ => _
        .DependsOn<IPack>()
        .Executes(() =>
        {
            SuppressErrors(() => DotNet($"tool uninstall -g {Solution.Fallout_GlobalTool.Name}"), logWarning: false);
            DotNet($"tool install -g {Solution.Fallout_GlobalTool.Name} --add-source {OutputDirectory} --version {DefaultDeploymentVersion}");
        });

    T From<T>()
        where T : INukeBuild
        => (T)(object)this;
}
