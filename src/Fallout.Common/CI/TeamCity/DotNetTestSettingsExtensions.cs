using System;
using System.Linq;
using Fallout.Application.Tools.DotNet;
using Fallout.Infrastructure.Tooling;
using Fallout.Kernel.IO;
using Fallout.Kernel;

namespace Fallout.Infrastructure.CI.TeamCity;

public static class DotNetTestSettingsExtensions
{
    public static DotNetTestSettings AddTeamCityLogger(this DotNetTestSettings toolSettings)
    {
        Assert.True(TeamCity.Instance != null);
        var teamcityPackage = NuGetPackageResolver
            .GetLocalInstalledPackage("TeamCity.Dotnet.Integration", NuGetToolPathResolver.NuGetPackagesConfigFile)
            .NotNull("teamcityPackage != null");
        var loggerPath = teamcityPackage.Directory / "build" / "_common" / "vstest15";
        Assert.DirectoryExists(loggerPath);
        return toolSettings
            .SetLoggers("teamcity")
            .SetTestAdapterPath(loggerPath);
    }
}
