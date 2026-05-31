namespace Fallout.Common.CI;

/// <summary>
/// The build's-eye view of the environment hosting this run — the CI system (or the local terminal)
/// that Fallout is executing under. From the build's perspective <em>GitHub Actions is the host</em>;
/// a future Fallout-owned runner would simply be another host adapter. The active, cross-build
/// supervisor that <em>launches</em> builds is a separate concern and takes a separate name
/// (Runner/Agent) — never reuse this one.
///
/// This is the runtime-host port of <see href="../../docs/adr/0005-ci-host-integration-ports-and-adapters.md">ADR-0005</see>.
/// It deliberately carries only the <strong>cross-host</strong> contract: provider-specific facts
/// (GitHub's <c>Workflow</c>/<c>Action</c>, …) stay on the concrete adapter, and console rendering
/// stays on the <see cref="Host"/> base. Supersedes the anemic <see cref="IBuildServer"/>.
///
/// SPIKE NOTE (0001): modelled as a single port for now. The members split cleanly into a read
/// (context) half and a write (reporting) half — whether that becomes two ports is the open question
/// recorded for the spike verdict.
/// </summary>
public interface IBuildHost
{
    #region Context (read) — universal subset only; provider-specific facts live on the adapter

    /// <summary>The short branch name of the ref that triggered this run.</summary>
    string Branch { get; }

    /// <summary>The commit SHA that triggered this run.</summary>
    string Commit { get; }

    /// <summary>Whether this run was triggered by a pull/merge request. Hosts without the concept return <c>false</c>.</summary>
    bool IsPullRequest => false;

    #endregion

    #region Reporting (write) — routed to the host's native annotation channels

    /// <summary>Surface a warning through the host's native channel (e.g. a GitHub <c>::warning::</c> annotation).</summary>
    void ReportWarning(string text, string details = null);

    /// <summary>Surface an error through the host's native channel (e.g. a GitHub <c>::error::</c> annotation).</summary>
    void ReportError(string text, string details = null);

    #endregion
}
