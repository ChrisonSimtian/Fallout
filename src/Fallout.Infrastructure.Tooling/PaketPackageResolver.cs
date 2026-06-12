using System;
using System.IO;
using System.Linq;
using Fallout.Core;

namespace Fallout.Infrastructure.Tooling;

// TODO: Add similar methods to NuGetPackageResolver
public static class PaketPackageResolver
{
    public static string GetLocalInstalledPackageDirectory(string packageId, string packagesConfigFile)
    {
        var packagesDirectory = GetPackagesDirectory(packagesConfigFile);
        return Path.Combine(packagesDirectory, packageId);
    }

    private static string GetPackagesDirectory(string packagesConfigFile)
    {
        return Path.Combine(Path.GetDirectoryName(packagesConfigFile).NotNull(), "packages");
    }
}
