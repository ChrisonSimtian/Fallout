using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.AzurePipelines.Configuration;

public class AzurePipelinesConfiguration : ConfigurationEntity
{
    public string[] VariableGroups { get; set; }

    public AzurePipelinesVcsPushTrigger VcsPushTrigger { get; set; }

    public AzurePipelinesVcsPushTrigger VcsPullRequestTrigger { get; set; }

    public AzurePipelinesStage[] Stages { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        if (VariableGroups.Length > 0)
        {
            using (writer.WriteBlock("variables:"))
            {
                VariableGroups.ForEach(x => writer.WriteLine($"- group: {x}"));
                writer.WriteLine();
            }
        }

        if (VcsPushTrigger != null)
        {
            using (writer.WriteBlock("trigger:"))
            {
                VcsPushTrigger.Write(writer);
                writer.WriteLine();
            }
        }

        if (VcsPullRequestTrigger != null)
        {
            using (writer.WriteBlock("pr:"))
            {
                VcsPullRequestTrigger.Write(writer);
                writer.WriteLine();
            }
        }

        using (writer.WriteBlock("stages:"))
        {
            Stages.ForEach(x => x.Write(writer));
        }
    }
}
