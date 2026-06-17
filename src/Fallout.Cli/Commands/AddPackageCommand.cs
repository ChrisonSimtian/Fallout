using System.Linq;
using Fallout.Common;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;

namespace Fallout.Cli.Commands;

/// <summary>
/// <c>fallout :add-package</c>: adds (or upgrades) a NuGet package reference in the build project.
/// </summary>
public sealed class AddPackageCommand : IFalloutCommand
{
    public string Name => "add-package";

    public int Execute(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
    {
        Program.PrintInfo();
        Logging.Configure();
        Telemetry.AddPackage();
        ProjectModelTasks.Initialize();

        var packageId = args.ElementAt(0);
        var packageVersion =
            (EnvironmentInfo.GetNamedArgument<string>("version") ??
             args.ElementAtOrDefault(1) ??
             NuGetVersionResolver.GetLatestVersion(packageId, includePrereleases: false).GetAwaiter().GetResult() ??
             NuGetPackageResolver.GetGlobalInstalledPackage(packageId, version: null, packagesConfigFile: null)?.Version.ToString())
            .NotNull("packageVersion != null");

        // GetConfiguration / AddOrReplacePackage / BUILD_PROJECT_FILE / PACKAGE_TYPE_* are shared
        // helpers still on Program; they move into services in the final #392 collapse PR.
        var configuration = Program.GetConfiguration(buildScript, evaluate: true);
        var buildProjectFile = configuration[Program.BUILD_PROJECT_FILE];
        Host.Information($"Installing {packageId}/{packageVersion} to {buildProjectFile} ...");
        Program.AddOrReplacePackage(packageId, packageVersion, Program.PACKAGE_TYPE_DOWNLOAD, buildProjectFile);
        DotNetTasks.DotNet($"restore {buildProjectFile}");

        var installedPackage = NuGetPackageResolver.GetGlobalInstalledPackage(packageId, packageVersion, packagesConfigFile: null)
            .NotNull("installedPackage != null");
        var hasToolsDirectory = installedPackage.Directory.GlobDirectories("tools").Any();
        if (!hasToolsDirectory)
            Program.AddOrReplacePackage(packageId, packageVersion, Program.PACKAGE_TYPE_REFERENCE, buildProjectFile);

        Host.Information($"Done installing {packageId}/{packageVersion} to {buildProjectFile}");
        return 0;
    }
}
