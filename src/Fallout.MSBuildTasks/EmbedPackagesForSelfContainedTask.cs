// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;

namespace Fallout.MSBuildTasks;

[UsedImplicitly]
public class EmbedPackagesForSelfContainedTask : ContextAwareTask
{
    [Required]
    public string ProjectAssetsFile { get; set; }

    [Required]
    public string TargetFramework { get; set; }

    [Output]
    public ITaskItem[] TargetOutputs { get; set; }

    protected override bool ExecuteInner()
    {
        var packages = NuGetPackageResolver.GetLocalInstalledPackages(ProjectAssetsFile);
        TargetOutputs = packages
            .Where(x => !x.Id.StartsWithOrdinalIgnoreCase("microsoft.netcore.app.runtime"))
            .Where(x => Directory.GetDirectories(x.Directory, "tools").Any())
            .Select(x => new TaskItem(x.File)).ToArray<ITaskItem>();
        return true;
    }
}
