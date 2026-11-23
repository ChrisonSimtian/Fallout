using System;
using System.Linq;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.GitHubActions.Configuration;

/// <summary>
/// The <c>workflow_run</c> trigger: starts a workflow after one or more other workflows run.
/// </summary>
public class GitHubActionsWorkflowRunTrigger : GitHubActionsDetailedTrigger
{
    /// <summary>
    /// Names of the upstream workflows, as given by their <c>name:</c> key.
    /// </summary>
    public string[] Workflows { get; set; }

    /// <summary>
    /// Activity types of the upstream run to react to, for example <c>completed</c>.
    /// </summary>
    public string[] Types { get; set; }

    /// <summary>
    /// Branches the upstream run must have happened on. Empty means any branch.
    /// </summary>
    public string[] Branches { get; set; }

    /// <inheritdoc/>
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("workflow_run:");

        void WriteValue(string value)
            => writer.WriteLine($"- {value.SingleQuoteIfNeeded(':', '#', ',', '.', '*', '!', '?', '+', '[', ']', '(', ')')}");

        using (writer.Indent())
        {
            writer.WriteLine("workflows:");
            using (writer.Indent())
            {
                Workflows.ForEach(WriteValue);
            }

            if (Types.Length > 0)
            {
                writer.WriteLine("types:");
                using (writer.Indent())
                {
                    Types.ForEach(WriteValue);
                }
            }

            if (Branches.Length > 0)
            {
                writer.WriteLine("branches:");
                using (writer.Indent())
                {
                    Branches.ForEach(WriteValue);
                }
            }
        }
    }
}
