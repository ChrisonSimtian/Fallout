using System;
using System.Linq;
using Fallout.Kernel;

namespace Fallout.Application.Tooling;

public static class ToolResolver
{
    public static Tool GetTool(string toolPath)
    {
        Assert.FileExists(toolPath);
        return ToolingServices.Process.GetTool(toolPath);
    }

    public static Tool GetNuGetTool(string packageId, string packageExecutable, string version = null, string framework = null)
    {
        var toolPath = ToolingServices.ToolPaths.GetPackageExecutable(packageId, packageExecutable, version, framework);
        return GetTool(toolPath);
    }

    public static Tool GetNpmTool(string npmExecutable)
    {
        var toolPath = ToolingServices.ToolPaths.GetNpmExecutable(npmExecutable);
        return GetTool(toolPath);
    }

    public static Tool TryGetEnvironmentTool(string name)
    {
        var toolPath = ToolingServices.ToolPaths.TryGetEnvironmentExecutable($"{name.ToUpperInvariant()}_EXE");
        if (toolPath == null)
            return null;

        return GetTool(toolPath);
    }

    public static Tool GetPathTool(string name)
    {
        var toolPath = ToolingServices.ToolPaths.GetPathExecutable(name);
        return GetTool(toolPath);
    }

    public static Tool GetEnvironmentOrPathTool(string name)
    {
        return TryGetEnvironmentTool(name) ?? GetPathTool(name);
    }
}
