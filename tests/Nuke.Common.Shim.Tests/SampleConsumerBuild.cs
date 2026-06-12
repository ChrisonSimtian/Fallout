//
// This file is the **compile-only test** for the Nuke.Common shim. It mirrors what a
// typical NUKE-era consumer Build.cs imports. If this file compiles, the MVP shim covers
// the surface it claims to cover. Nothing here is executed.

#pragma warning disable CS0649  // unassigned readonly — these are injected at runtime
#pragma warning disable CS0169  // unused fields — same reason

using Nuke.Common;
using Fallout.Core.IO.Globbing;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities;

namespace Nuke.Common.Shim.Tests;

// Consumer's typical Build.cs entry-point — a pure NUKE-era surface: everything resolves through the
// Nuke.* shim (Solution/GitRepository/[Parameter]/CI hosts/DelegateDisposable), with no Fallout.* imports.
// Enums (GlobbingCaseSensitivity) aren't shimmed (SHIM002 — can't subclass cross-assembly), so that one
// stays a fully-qualified canonical reference — exactly what `fallout-migrate` would otherwise flip.
[GlobbingOptions(Fallout.Core.IO.Globbing.GlobbingCaseSensitivity.CaseInsensitive)]
public abstract class SampleConsumerBuild : NukeBuild, INukeBuild
{
    [Parameter("Configuration to build")] readonly string Configuration;
    [Parameter] readonly bool RunTests;
    [Secret] readonly string NuGetApiKey;
    [Solution] readonly Solution Solution;
    [Solution("path/to/explicit.slnx")] readonly Solution ExplicitSolution;
    [GitRepository] readonly GitRepository GitRepository;

    // CI-host shims expose only the static `Instance` accessor. Consumers can
    // still chain into instance members because the returned type is canonical.
    // Field-injection patterns like `[CI] readonly GitHubActions GitHubActions`
    // are intentionally NOT supported — those need `fallout-migrate` to flip
    // the type reference to canonical.
    void TouchCiHostShims()
    {
        _ = GitHubActions.Instance?.Workflow;
        _ = AzurePipelines.Instance?.AgentName;
        _ = TeamCity.Instance?.BuildNumber;
        _ = AppVeyor.Instance?.AccountName;
        _ = AppVeyor.MessageLimit;
    }

    // DelegateDisposable shim re-exposes the static factories. Consumer
    // usage stays canonical-typed at runtime (the factories return
    // canonical IDisposable instances).
    void TouchDelegateDisposableShim()
    {
        using var bracket = DelegateDisposable.CreateBracket(
            setup: () => { },
            cleanup: () => { });
        using var typed = DelegateDisposable.CreateBracket(
            setup: () => 42,
            cleanup: _ => { });
    }
}
