using System;
using System.Linq;
using Fallout.Application.Tooling;
using Fallout.Kernel;

namespace Fallout.Application.Tools.MSpec;

partial class MSpecTasks
{
    protected override string GetToolPath(ToolOptions options = null)
    {
        return ToolingServices.ToolPaths.GetPackageExecutable(
            PackageId,
            EnvironmentInfo.Is64Bit ? "mspec-clr4.exe" : "mspec-x86-clr4.exe");
    }
}
