namespace Fallout.Common.CI;

/// <summary>
/// The build's-eye view of the <strong>context</strong> of the environment hosting this run — the
/// facts of "what is this run, under what". From the build's perspective <em>GitHub Actions is the
/// host</em>; a future Fallout-owned runner would simply be another host adapter. The active,
/// cross-build supervisor that <em>launches</em> builds is a separate concern and takes a separate
/// name (Runner/Agent) — never reuse this one.
///
/// This is the context half of the runtime-host seam of
/// <see href="../../docs/adr/0005-ci-host-integration-ports-and-adapters.md">ADR-0005</see>;
/// the reporting half is <see cref="IBuildReporter"/>. The split is real: only CI hosts have run
/// context, but <em>every</em> host (including the local terminal) is an <see cref="IBuildReporter"/>.
/// Carries only the cross-host subset — provider-specific facts (GitHub's <c>Workflow</c>/<c>Action</c>, …)
/// stay on the concrete adapter. Supersedes the anemic <see cref="IBuildServer"/>.
/// </summary>
public interface IBuildHost
{
    /// <summary>The short branch name of the ref that triggered this run.</summary>
    string Branch { get; }

    /// <summary>The commit SHA that triggered this run.</summary>
    string Commit { get; }

    /// <summary>Whether this run was triggered by a pull/merge request. Hosts without the concept return <c>false</c>.</summary>
    bool IsPullRequest => false;
}
