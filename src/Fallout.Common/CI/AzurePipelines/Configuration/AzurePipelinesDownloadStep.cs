// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.Utilities;

namespace Fallout.Common.CI.AzurePipelines.Configuration;

[PublicAPI]
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
