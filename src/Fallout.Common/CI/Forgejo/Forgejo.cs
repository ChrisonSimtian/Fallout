using System.Diagnostics.CodeAnalysis;

namespace Fallout.Common.CI.Forgejo;

/// <summary>
/// Runtime-host adapter for <a href="https://forgejo.org/docs/latest/user/actions/">Forgejo Actions</a>.
/// Forgejo Actions is GitHub-Actions-compatible, so it exposes the same <c>GITHUB_*</c> environment
/// variables and speaks the same workflow-command dialect — see <see cref="WorkflowCommands"/> and
/// <c>Forgejo.Theming.cs</c>. First adapter to use the ADR-0005 #3 detection style: it overrides
/// <see cref="Host.IsActive"/> directly instead of declaring an <c>IsRunning{Name}</c> static.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Forgejo : Host, IBuildServer
{
    public new static Forgejo Instance => Host.Instance as Forgejo;

    // NAIVE detection — TODO(forgejo): verify against a live instance. Forgejo Actions sets
    // GITHUB_ACTIONS=true for compatibility, so we must key off a Forgejo-distinctive variable to
    // avoid colliding with the GitHubActions adapter. FORGEJO_ACTIONS is a guess pending the
    // instance being stood up; the runner's actual distinctive variable is unconfirmed.
    protected internal override bool IsActive => EnvironmentInfo.HasVariable("FORGEJO_ACTIONS");

    internal Forgejo()
    {
    }

    string IBuildServer.Branch => RefName;
    string IBuildServer.Commit => Sha;

    // GitHub-Actions-compatible run context. Provider-specific facts (instance URL, etc.) can be
    // added once the live instance confirms what Forgejo exposes beyond the GITHUB_* surface.
    public string Repository => EnvironmentInfo.GetVariable("GITHUB_REPOSITORY");
    public string RepositoryOwner => EnvironmentInfo.GetVariable("GITHUB_REPOSITORY_OWNER");
    public string Actor => EnvironmentInfo.GetVariable("GITHUB_ACTOR");
    public string Workflow => EnvironmentInfo.GetVariable("GITHUB_WORKFLOW");
    public string Sha => EnvironmentInfo.GetVariable("GITHUB_SHA");
    public string Ref => EnvironmentInfo.GetVariable("GITHUB_REF");
    public string RefName => EnvironmentInfo.GetVariable("GITHUB_REF_NAME");
    public string EventName => EnvironmentInfo.GetVariable("GITHUB_EVENT_NAME");
    public string ServerUrl => EnvironmentInfo.GetVariable("GITHUB_SERVER_URL");
    public string Token => EnvironmentInfo.GetVariable("GITHUB_TOKEN");
    public long RunId => EnvironmentInfo.GetVariable<long>("GITHUB_RUN_ID");
    public long RunNumber => EnvironmentInfo.GetVariable<long>("GITHUB_RUN_NUMBER");
    public bool IsPullRequest => EventName == "pull_request";
}
