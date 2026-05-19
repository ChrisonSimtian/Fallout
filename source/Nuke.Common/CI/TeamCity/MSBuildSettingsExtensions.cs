// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Tools.MSBuild;

namespace Nuke.Common.CI.TeamCity;

[PublicAPI]
public static class MSBuildSettingsExtensions
{
    public static MSBuildSettings AddTeamCityLogger(this MSBuildSettings toolSettings)
    {
        var teamCity = TeamCity.Instance.NotNull("TeamCity.Instance != null");
        var teamCityLogger = teamCity.ConfigurationProperties["teamcity.dotnet.msbuild.extensions4.0"];
        return toolSettings
            .AddLoggers($"JetBrains.BuildServer.MSBuildLoggers.MSBuildLogger,{teamCityLogger}")
            .EnableNoConsoleLogger();
    }
}
