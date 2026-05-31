namespace Fallout.Common.CI.GitHubActions;

// ADR-0005 / spike 0001: GitHubActions as the first named adapter for the runtime-host context port.
// Implemented explicitly so the port contract sits alongside — not on top of — the existing public
// surface (Ref/Sha/IsPullRequest). Reporting is NOT here: it comes from the Host base (IBuildReporter)
// plus this adapter's existing ReportWarning/ReportError/WriteBlock overrides in GitHubActions.Theming.cs.
public partial class GitHubActions : IBuildHost
{
    // Reuse the same mapping the IBuildServer impl already uses.
    string IBuildHost.Branch => Ref;
    string IBuildHost.Commit => Sha;
    // IBuildHost.IsPullRequest is satisfied implicitly by the public IsPullRequest property.
}
