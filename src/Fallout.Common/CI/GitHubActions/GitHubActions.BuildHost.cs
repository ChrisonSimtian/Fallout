using System;

namespace Fallout.Common.CI.GitHubActions;

// ADR-0005 / spike 0001: GitHubActions as the first named adapter for the runtime-host port.
// Implemented explicitly so the port contract sits alongside — not on top of — the existing public
// surface (Ref/Sha/IsPullRequest, WriteWarning/WriteError) and the Host base's protected rendering
// members. These impls are additive and dormant: current call sites are unchanged, so build output
// stays byte-identical. They exist to prove the seam compiles and is satisfiable end-to-end.
public partial class GitHubActions
{
    // Context — reuse the same mapping the IBuildServer impl already uses.
    string IBuildHost.Branch => Ref;
    string IBuildHost.Commit => Sha;
    // IBuildHost.IsPullRequest is satisfied implicitly by the public IsPullRequest property.

    // Reporting — route to GitHub's native annotation commands (::warning:: / ::error::).
    void IBuildHost.ReportWarning(string text, string details) =>
        WriteWarning(string.IsNullOrEmpty(details) ? text : $"{text} — {details}");

    void IBuildHost.ReportError(string text, string details) =>
        WriteError(string.IsNullOrEmpty(details) ? text : $"{text} — {details}");
}
