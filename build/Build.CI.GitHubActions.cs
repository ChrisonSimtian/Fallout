using Fallout.Common.CI.GitHubActions;
using Fallout.Components;

// macOS and Windows runs are reserved for main-branch validation (post-merge
// and release pipelines). PRs and feature-branch pushes get Linux-only for
// fast, cheap feedback.
[GitHubActions(
    "macos-latest",
    GitHubActionsImage.MacOsLatest,
    FetchDepth = 0,
    Submodules = GitHubActionsSubmodules.Recursive,
    OnPushBranches = new[] { MainBranch },
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) },
    PublishArtifacts = false)]
[GitHubActions(
    "windows-latest",
    GitHubActionsImage.WindowsLatest,
    FetchDepth = 0,
    Submodules = GitHubActionsSubmodules.Recursive,
    OnPushBranches = new[] { MainBranch },
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
    OnPullRequestBranches = new[] { MainBranch },
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
