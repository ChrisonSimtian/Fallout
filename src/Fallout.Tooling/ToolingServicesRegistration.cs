using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fallout.Application.Tooling;
using Fallout.Common.IO;

namespace Fallout.Infrastructure.Tooling;

// Infrastructure adapters for the Application-ring tool-execution ports (ADR-0006). Each is a thin forward to
// the existing process/resolver statics; the module initializer registers them into ToolingServices when the
// assembly loads — before any Application code can touch a Fallout.Tooling type. Adapters reference both rings
// (Infrastructure → Application port), which is the correct inward onion direction.

internal sealed class ProcessExecutorAdapter : IProcessExecutor
{
    public IProcess StartProcess(
        string toolPath, string arguments, string workingDirectory,
        IReadOnlyDictionary<string, string> environmentVariables, int? timeout,
        bool? logOutput, bool? logInvocation, Action<OutputType, string> logger, Func<string, string> outputFilter)
        => ProcessTasks.StartProcess(toolPath, arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, logger, outputFilter);

    public IProcess StartShell(
        string command, string workingDirectory,
        IReadOnlyDictionary<string, string> environmentVariables, int? timeout,
        bool? logOutput, bool? logInvocation, Action<OutputType, string> logger, Func<string, string> outputFilter)
        => ProcessTasks.StartShell(command, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, logger, outputFilter);

    public void CheckPathEnvironmentVariable() => ProcessTasks.CheckPathEnvironmentVariable();
    public bool DefaultLogOutput => ProcessTasks.DefaultLogOutput;
    public Tool GetTool(string toolPath) => new ToolExecutor(toolPath).Execute;
}

internal sealed class ToolPathResolverAdapter : IToolPathResolver
{
    public string TryGetEnvironmentExecutable(string environmentExecutable) => ToolPathResolver.TryGetEnvironmentExecutable(environmentExecutable);
    public string GetPathExecutable(string pathExecutable) => ToolPathResolver.GetPathExecutable(pathExecutable);
    public string GetPackageExecutable(string packageId, string packageExecutable, string version, string framework) => NuGetToolPathResolver.GetPackageExecutable(packageId, packageExecutable, version, framework);
    public string GetNpmExecutable(string npmExecutable) => NpmToolPathResolver.GetNpmExecutable(npmExecutable);
    public AbsolutePath GetNuGetPackagesDirectory() => NuGetPackageResolver.GetPackagesDirectory(NuGetToolPathResolver.NuGetPackagesConfigFile);
    public string GetPackagesConfigFile(string projectDirectory) => NuGetPackageResolver.GetPackagesConfigFile(projectDirectory);

    public string EmbeddedPackagesDirectory { set => NuGetToolPathResolver.EmbeddedPackagesDirectory = value; }
    public string NuGetPackagesConfigFile { get => NuGetToolPathResolver.NuGetPackagesConfigFile; set => NuGetToolPathResolver.NuGetPackagesConfigFile = value; }
    public string NuGetAssetsConfigFile { set => NuGetToolPathResolver.NuGetAssetsConfigFile = value; }
    public AbsolutePath NpmPackageJsonFile { set => NpmToolPathResolver.NpmPackageJsonFile = value; }
}

internal sealed class ToolVersionResolverAdapter : IToolVersionResolver
{
    public Task<string> GetLatestNuGetVersion(string packageId, bool includePrereleases, bool includeUnlisted) => NuGetVersionResolver.GetLatestVersion(packageId, includePrereleases, includeUnlisted);
    public Task<string> GetLatestNpmVersion(string packageId) => NpmVersionResolver.GetLatestVersion(packageId);
}

internal static class ToolingServicesRegistration
{
    // CA2255: the module initializer is the intended wiring point — it registers the Infrastructure adapters
    // into the Application-ring ToolingServices when this assembly loads, before any Application code runs.
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Register()
    {
        ToolingServices.Process ??= new ProcessExecutorAdapter();
        ToolingServices.ToolPaths ??= new ToolPathResolverAdapter();
        ToolingServices.Versions ??= new ToolVersionResolverAdapter();
    }
}
