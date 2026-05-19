// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.AzurePipelines.Configuration;

[PublicAPI]
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
