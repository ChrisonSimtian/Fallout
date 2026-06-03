using System;
using System.IO;
using System.Linq;
using Fallout.Infrastructure.CI.TeamCity;

namespace Fallout.Infrastructure.CI.Tests;

public class TestTeamCityAttribute : TeamCityAttribute, ITestConfigurationGenerator
{
    public StreamWriter Stream { get; set; }

    protected override StreamWriter CreateStream()
    {
        return Stream;
    }
}
