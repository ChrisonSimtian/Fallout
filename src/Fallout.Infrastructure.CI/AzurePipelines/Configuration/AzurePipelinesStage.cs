using System;
using System.Linq;
using Fallout.Application.CI;
using Fallout.Application.Utilities;
using Fallout.Application.Tooling;
using Fallout.Core;
using Fallout.Core.Collections;
using Fallout.Infrastructure.CI.AzurePipelines;

namespace Fallout.Infrastructure.CI.AzurePipelines.Configuration;

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
