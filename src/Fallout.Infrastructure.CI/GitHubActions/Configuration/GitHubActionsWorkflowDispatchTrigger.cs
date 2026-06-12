using System;
using System.Linq;
using Fallout.Application.Utilities;
using Fallout.Core;
using Fallout.Core.Collections;

namespace Fallout.Infrastructure.CI.GitHubActions.Configuration;

public class GitHubActionsWorkflowDispatchTrigger : GitHubActionsDetailedTrigger
{
    public string[] OptionalInputs { get; set; }
    public string[] RequiredInputs { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("workflow_dispatch:");
        using (writer.Indent())
        {
            writer.WriteLine("inputs:");
            using (writer.Indent())
            {
                void WriteInput(string input, bool required)
                {
                    writer.WriteLine($"{input}:");
                    using (writer.Indent())
                    {
                        writer.WriteLine($"description: {input.SplitCamelHumpsWithKnownWords().JoinSpace().DoubleQuote()}");
                        writer.WriteLine($"required: {required.ToString().ToLowerInvariant()}");
                    }
                }

                OptionalInputs.ForEach(x => WriteInput(x, required: false));
                RequiredInputs.ForEach(x => WriteInput(x, required: true));
            }
        }
    }
}
