// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Fallout.Common.Tooling;

// TODO: Add similar methods to NuGetPackageResolver
[PublicAPI]
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
