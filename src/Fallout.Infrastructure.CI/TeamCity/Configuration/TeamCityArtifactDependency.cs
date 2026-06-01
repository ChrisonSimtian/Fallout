using System;
using System.Linq;
using Fallout.Application.Utilities;

namespace Fallout.Infrastructure.CI.TeamCity.Configuration;

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
