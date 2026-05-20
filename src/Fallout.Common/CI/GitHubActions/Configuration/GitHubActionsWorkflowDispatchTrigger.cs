// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.GitHubActions.Configuration;

[PublicAPI]
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
