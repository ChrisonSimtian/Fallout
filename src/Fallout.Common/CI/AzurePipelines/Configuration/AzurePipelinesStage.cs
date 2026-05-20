// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.AzurePipelines.Configuration;

[PublicAPI]
public class AzurePipelinesStage : ConfigurationEntity
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public AzurePipelinesImage? Image { get; set; }
    public AzurePipelinesStage[] Dependencies { get; set; }
    public AzurePipelinesJob[] Jobs { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        using (writer.WriteBlock($"- stage: {Name}"))
        {
            writer.WriteLine($"displayName: {DisplayName.SingleQuote()}");
            writer.WriteLine($"dependsOn: [ {Dependencies.Select(x => x.Name).JoinCommaSpace()} ]");

            if (Image != null)
            {
                using (writer.WriteBlock("pool:"))
                {
                    writer.WriteLine($"vmImage: {Image.Value.GetValue().SingleQuote()}");
                }
            }

            using (writer.WriteBlock("jobs:"))
            {
                Jobs.ForEach(x => x.Write(writer));
            }
        }
    }
}
