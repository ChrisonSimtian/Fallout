using System;
using System.Linq;

namespace Fallout.Common.CI.GitHubActions;

/// <summary>
/// Starts a generated workflow after other workflows run, using the <c>workflow_run</c> trigger.
/// Put it on the build class next to the <see cref="GitHubActionsAttribute"/> of the workflow that
/// should start, which is the downstream workflow. The upstream workflows need no change.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="ForWorkflows"/> to pick the generated workflow this trigger belongs to. It can only
/// be left empty when the build class declares a single workflow.
/// </para>
/// <para>
/// A <c>workflow_run</c> workflow runs on the latest commit of the default branch, not on the commit
/// that triggered the upstream run. To work on the upstream commit, check out
/// <c>github.event.workflow_run.head_sha</c>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GitHubActionsWorkflowRunAttribute : Attribute
{
    /// <param name="workflow">Name of the first upstream workflow (its <c>name:</c>, not the file name).</param>
    /// <param name="moreWorkflows">Names of further upstream workflows.</param>
    public GitHubActionsWorkflowRunAttribute(string workflow, params string[] moreWorkflows)
    {
        Workflows = new[] { workflow }.Concat(moreWorkflows ?? new string[0]).ToArray();
    }

    /// <summary>
    /// Names of the upstream workflows (their <c>name:</c>, not the file name) whose runs start this workflow.
    /// </summary>
    public string[] Workflows { get; }

    /// <summary>
    /// Activity types of the upstream run to react to. Defaults to <c>completed</c>.
    /// </summary>
    public string[] Types { get; set; } = { "completed" };

    /// <summary>
    /// Branches the upstream run must have happened on. Empty means any branch.
    /// </summary>
    public string[] Branches { get; set; } = new string[0];

    /// <summary>
    /// <c>workflow_run</c> also fires for failed or cancelled upstream runs. When set, jobs only run if the
    /// upstream run succeeded. Runs from other triggers are unaffected. The generated condition is
    /// <c>github.event_name != 'workflow_run' || github.event.workflow_run.conclusion == 'success'</c>.
    /// </summary>
    /// <remarks>
    /// The conclusion only exists once the upstream run has completed, so this needs <c>completed</c> in
    /// <see cref="Types"/>.
    /// </remarks>
    public bool RequireSuccess { get; set; }

    /// <summary>
    /// Names of the generated workflows (the name given to <see cref="GitHubActionsAttribute"/>) that
    /// start from this trigger. Can only be empty when the build class declares a single workflow.
    /// </summary>
    public string[] ForWorkflows { get; set; } = new string[0];
}
