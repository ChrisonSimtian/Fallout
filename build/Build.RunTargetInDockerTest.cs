// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using JetBrains.Annotations;
using Fallout.Common;
using Fallout.Common.Tools.Docker;
using Fallout.Common.Utilities;
using Serilog;

partial class Build
{
    [UsedImplicitly]
    Target RunTargetInDockerImageTest => _ => _
        .DockerRun(_ => _
            .EnableBuildCaching()
            .SetImage("mcr.microsoft.com/dotnet/sdk:6.0")
            .When(EnvironmentInfo.IsArm64, _ => _
                .SetPlatform("linux/arm64")
                .SetDotNetRuntime("linux-arm64"))
            .When(EnvironmentInfo.IsWin, _ => _
                .SetPlatform("windows/amd64")
                .SetDotNetRuntime("win-x64"))
            .When(EnvironmentInfo.IsLinux, _ => _
                .SetPlatform("linux/amd64")
                .SetDotNetRuntime("linux-x64")))
        .Executes(() =>
        {
            Log.Information("Hello, the computer name is {Name}", Environment.MachineName);
        });
}
