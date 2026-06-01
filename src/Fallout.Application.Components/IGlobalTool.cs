using System;
using System.IO;
using System.Linq;
using static Fallout.Application.Tools.DotNet.DotNetTasks;
using Fallout.Application;
using Fallout.Application.Tools.DotNet;

namespace Fallout.Application.Components;

public interface IGlobalTool : IFalloutBuild
{
    string GlobalToolPackageName => Path.GetFileNameWithoutExtension(BuildProjectFile);
    string GlobalToolVersion => "1.0.0";

    Target PackGlobalTool => _ => _
        .Unlisted()
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(BuildProjectFile)
                .SetOutputDirectory(TemporaryDirectory));
        });

    Target InstallGlobalTool => _ => _
        .Unlisted()
        .DependsOn(UninstallGlobalTool)
        .DependsOn(PackGlobalTool)
        .Executes(() =>
        {
            DotNetToolInstall(_ => _
                .SetPackageName(GlobalToolPackageName)
                .EnableGlobal()
                .AddSources(TemporaryDirectory)
                .SetVersion(GlobalToolVersion));
        });

    Target UninstallGlobalTool => _ => _
        .Unlisted()
        .ProceedAfterFailure()
        .Executes(() =>
        {
            DotNetToolUninstall(_ => _
                .SetPackageName(GlobalToolPackageName)
                .EnableGlobal());
        });
}
