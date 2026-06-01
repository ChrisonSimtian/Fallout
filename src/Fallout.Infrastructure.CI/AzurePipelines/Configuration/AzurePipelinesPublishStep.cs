using System;
using System.Linq;
using Fallout.Application.Utilities;
using Fallout.Kernel;

namespace Fallout.Infrastructure.CI.AzurePipelines.Configuration;

public class AzurePipelinesPublishStep : AzurePipelinesStep
{
    public string ArtifactName { get; set; }
    public string PathToPublish { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        using (writer.WriteBlock("- task: PublishBuildArtifacts@1"))
        {
            writer.WriteLine("displayName: " + $"Publish: {ArtifactName}".SingleQuote());
            using (writer.WriteBlock("inputs:"))
            {
                writer.WriteLine($"artifactName: {ArtifactName}");
                writer.WriteLine($"pathToPublish: {PathToPublish.SingleQuote()}");
            }
        }
    }
}
