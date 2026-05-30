using Fallout.Common.CI.GitHubActions;
using Fallout.Components;

// macOS and Windows runs are reserved for post-merge validation on the
// long-lived branches (experimental, main, release/YYYY, support/*). PRs and
// feature-branch pushes get Linux-only for fast, cheap feedback. Cross-platform
// regressions on those branches surface as a red commit — same fail-fast model.
[GitHubActions(
    "macos-latest",
    GitHubActionsImage.MacOsLatest,
    FetchDepth = 0,
    Submodules = GitHubActionsSubmodules.Recursive,
    OnPushBranches = new[] { ExperimentalBranch, MainBranch, ReleaseBranchPattern, SupportBranchPattern },
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) },
    PublishArtifacts = false)]
[GitHubActions(
    "windows-latest",
    GitHubActionsImage.WindowsLatest,
    FetchDepth = 0,
    Submodules = GitHubActionsSubmodules.Recursive,
    OnPushBranches = new[] { ExperimentalBranch, MainBranch, ReleaseBranchPattern, SupportBranchPattern },
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) },
    PublishArtifacts = false)]
// pull_request only — same-repo branches would otherwise fire both push and
// pull_request events on every push, double-running the validation.
//
// CheckoutRef = github.head_ref pins checkout to the PR source branch instead of the merge SHA,
// keeping HEAD attached so GitHubTasksTest.GitHubRepositoryFromLocalDirectoryTest (which reads
// .git/HEAD via GitRepository.FromLocalDirectory) resolves a non-null branch.
[GitHubActions(
    "ubuntu-latest",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    Submodules = GitHubActionsSubmodules.Recursive,
    CheckoutRef = "${{ github.head_ref }}",
    // Trigger for PRs targeting experimental, main, or any release/YYYY / support/*
    // branch — all are long-lived and protected; all require the ubuntu-latest check.
    OnPullRequestBranches = new[] { ExperimentalBranch, MainBranch, ReleaseBranchPattern, SupportBranchPattern },
    OnPullRequestExcludePaths = new[] { "docs/**", ".assets/**", "**/*.md" },
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) },
    PublishArtifacts = false)]
partial class Build
{
    // The release workflow is intentionally hand-written at
    // .github/workflows/release.yml — that lets us name the GitHub secret
    // NUGET_API_KEY (conventional screaming-snake-case) while keeping the
    // Build.cs property name NuGetApiKey (idiomatic C#). The NUKE attribute
    // generator would force the two to match.
    const string ReleaseWorkflow = "release";
}
