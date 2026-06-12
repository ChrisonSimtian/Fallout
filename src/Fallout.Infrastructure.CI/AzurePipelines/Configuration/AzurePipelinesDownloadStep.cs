using System;
using System.Linq;
using Fallout.Application.Utilities;
using Fallout.Core;

namespace Fallout.Infrastructure.CI.AzurePipelines.Configuration;

public class AzurePipelinesDownloadStep : AzurePipelinesStep
{
    public string ArtifactName { get; set; }
    public string DownloadPath { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        using (writer.WriteBlock("- task: DownloadBuildArtifacts@0"))
        {
            // writer.WriteLine("displayName: Download Artifacts");
            using (writer.WriteBlock("inputs:"))
            {
                writer.WriteLine($"artifactName: {ArtifactName}");
                writer.WriteLine($"downloadPath: {DownloadPath.SingleQuote()}");
            }
        }
    }
}
