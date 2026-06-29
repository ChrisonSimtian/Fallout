using System;
using System.Linq;

namespace Fallout.Common.Tooling;

public class ToolRequirement;

public class PathToolRequirement(string pathExecutable) : ToolRequirement
{
    public string PathExecutable { get; init; } = pathExecutable;
}

public class NuGetPackageRequirement(string packageId, string version = null) : ToolRequirement
{
    public string PackageId { get; init; } = packageId;
    public string Version { get; init; } = version ?? NuGetVersionResolver.GetLatestVersion(packageId, includePrereleases: false).GetAwaiter().GetResult();
}

public class NpmPackageRequirement(string packageId, string version = null) : ToolRequirement
{
    public string PackageId { get; init; } = packageId;
    public string Version { get; init; } = version ?? NpmVersionResolver.GetLatestVersion(packageId).GetAwaiter().GetResult();
}

public class AptGetPackageRequirement(string packageId) : ToolRequirement
{
    public string PackageId { get; init; } = packageId;
}
