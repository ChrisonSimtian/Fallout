// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.TeamCity.Configuration;

[PublicAPI]
public class TeamCityArtifactDependency : TeamCityDependency
{
    public TeamCityBuildType BuildType { get; set; }
    public string[] ArtifactRules { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        using (writer.WriteBlock($"artifacts({BuildType.Id})"))
        {
            writer.WriteArray("artifactRules", ArtifactRules);
        }
    }
}
