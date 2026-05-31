using System;

namespace Fallout.Common.CI;

/// <summary>
/// The reporting half of the runtime-host seam of
/// <see href="../../docs/adr/0005-ci-host-integration-ports-and-adapters.md">ADR-0005</see> — the
/// channel a build uses to surface progress, warnings, and errors through the host's <em>native</em>
/// mechanisms (e.g. GitHub <c>::warning::</c> annotations, TeamCity service messages, Azure Pipelines
/// <c>##vso</c> issues, or — for the local terminal — plain console output).
///
/// Implemented by the <see cref="Host"/> base, so <strong>every</strong> host is a reporter, including
/// the local terminal. Concrete adapters customise behaviour by overriding the corresponding members
/// on <see cref="Host"/>. This is the companion to <see cref="IBuildHost"/> (the context half); the
/// two are separated because their implementor sets differ — every host reports, but only CI hosts
/// carry run context.
/// </summary>
public interface IBuildReporter
{
    /// <summary>Surface a warning through the host's native channel.</summary>
    void ReportWarning(string text, string details = null);

    /// <summary>Surface an error through the host's native channel.</summary>
    void ReportError(string text, string details = null);

    /// <summary>Open a collapsible/grouped output region; dispose to close it.</summary>
    IDisposable WriteBlock(string text);
}
