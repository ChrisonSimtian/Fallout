// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;

namespace Fallout.Common.Tools.Xunit;

partial class Xunit2SettingsExtensions
{
    public static Xunit2Settings AddTargetAssemblies(this Xunit2Settings toolSettings, IEnumerable<string> assemblyFiles)
    {
        return assemblyFiles.Aggregate(
            toolSettings,
            (current, assembly) => current.AddTargetAssemblyWithConfigs(assembly, string.Empty));
    }

    public static Xunit2Settings AddTargetAssemblies(this Xunit2Settings toolSettings, params string[] assemblyFiles)
    {
        return toolSettings.AddTargetAssemblies(assemblyFiles.AsEnumerable());
    }
}
