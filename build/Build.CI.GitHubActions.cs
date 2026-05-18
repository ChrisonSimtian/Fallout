// Copyright 2023 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using Nuke.Common.CI.GitHubActions;
using Nuke.Components;

[GitHubActions(
    "windows-latest",
    GitHubActionsImage.WindowsLatest,
    FetchDepth = 0,
    OnPushBranchesIgnore = new[] { MainBranch },
    OnPullRequestBranches = new[] { MainBranch },
    OnPullRequestExcludePaths = new[] { "docs/**", "images/**", "**/*.md" },
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) },
    PublishArtifacts = false)]
[GitHubActions(
    "macos-latest",
    GitHubActionsImage.MacOsLatest,
    FetchDepth = 0,
    OnPushBranchesIgnore = new[] { MainBranch },
    OnPullRequestBranches = new[] { MainBranch },
    OnPullRequestExcludePaths = new[] { "docs/**", "images/**", "**/*.md" },
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) },
    PublishArtifacts = false)]
[GitHubActions(
    "ubuntu-latest",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPushBranchesIgnore = new[] { MainBranch },
    OnPullRequestBranches = new[] { MainBranch },
    OnPullRequestExcludePaths = new[] { "docs/**", "images/**", "**/*.md" },
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) },
    PublishArtifacts = false)]
partial class Build
{
    // AlphaDeployment workflow removed in trunk migration. The release pipeline
    // is reintroduced in the follow-up Nerdbank.GitVersioning PR with proper
    // main-branch publish semantics. Code paths still referencing this constant
    // (Test.OnlyWhenStatic, Pack.PackSettings, Publish.Requires, DeletePackages)
    // are intentionally preserved — they evaluate to false until a workflow
    // with this name exists again.
    const string AlphaDeployment = "alpha-deployment";
}
