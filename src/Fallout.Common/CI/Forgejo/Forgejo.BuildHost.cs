namespace Fallout.Common.CI.Forgejo;

// ADR-0005: Forgejo satisfies the runtime-host context port. Reporting (IBuildReporter) comes from
// the Host base plus this adapter's overrides in Forgejo.Theming.cs.
public partial class Forgejo : IBuildHost
{
    string IBuildHost.Branch => RefName;
    string IBuildHost.Commit => Sha;
    // IBuildHost.IsPullRequest is satisfied implicitly by the public IsPullRequest property.
}
