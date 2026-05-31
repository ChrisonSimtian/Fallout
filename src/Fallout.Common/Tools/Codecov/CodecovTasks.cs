using System;
using Fallout.Application.Tooling;
using Fallout.Infrastructure.Tooling;
using Fallout.Common;

namespace Fallout.Application.Tools.Codecov;

partial class CodecovTasks
{
    protected override string GetToolPath(ToolOptions options = null)
    {
        return NuGetToolPathResolver.GetPackageExecutable(
            packageId: PackageId,
            packageExecutable: EnvironmentInfo.Platform switch
            {
                PlatformFamily.Windows => "codecov.exe",
                PlatformFamily.OSX => "codecov-macos",
                PlatformFamily.Linux => "codecov-linux",
                _ => throw new ArgumentOutOfRangeException()
            });
    }
}
