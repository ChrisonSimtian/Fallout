// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Microsoft.Build.Framework;

namespace Nuke.MSBuildTasks;

public static class TaskItemExtensions
{
    public static string GetMetadataOrNull(this ITaskItem taskItem, string metdataName)
    {
        return taskItem.MetadataNames.Cast<string>().Contains(metdataName)
            ? taskItem.GetMetadata(metdataName)
            : null;
    }
}
