using System;
using System.Linq;
using Fallout.Application.Utilities;
using Fallout.Kernel;

namespace Fallout.Infrastructure.CI.TeamCity.Configuration;

public class TeamCityFinishBuildTrigger : TeamCityTrigger
{
    public TeamCityBuildType BuildType { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        using (writer.WriteBlock("finishBuildTrigger"))
        {
            writer.WriteLine($"buildType = {$"${{{BuildType.Id}.id}}".DoubleQuote()}");
        }
    }
}
