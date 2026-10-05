using System;
using System.Linq;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.GitHubActions.Configuration;

public class GitHubActionsWorkflowRunTrigger : GitHubActionsDetailedTrigger
{
    public string[] Workflows { get; set; }
    public string[] Types { get; set; }
    public string[] Branches { get; set; }

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
