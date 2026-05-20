// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Fallout.Common.Tools.MSBuild;

public static partial class MSBuildSettingsExtensions
{
    /// <summary><em>Sets <see cref="MSBuildSettings.TargetPath" />.</em></summary>
    public static MSBuildSettings SetSolutionFile(this MSBuildSettings toolSettings, string solutionFile)
    {
        return toolSettings.SetTargetPath(solutionFile);
    }

    /// <summary><em>Sets <see cref="MSBuildSettings.TargetPath" />.</em></summary>
    public static MSBuildSettings SetProjectFile(this MSBuildSettings toolSettings, string projectFile)
    {
        return toolSettings.SetTargetPath(projectFile);
    }
}
