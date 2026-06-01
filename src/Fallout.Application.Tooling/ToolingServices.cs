using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fallout.Kernel.IO;

namespace Fallout.Application.Tooling;

// Ports for the impure tool-execution services (ADR-0006, onion realignment). The Application ring (tool
// wrappers, ToolTasks, ToolResolver, requirement/version attributes, build orchestration) depends only on
// these abstractions; the concrete process/resolver implementations live in Fallout.Infrastructure.Tooling
// and register adapters into ToolingServices via a module initializer. This is the inversion that lets the
// Application ring stay free of any Fallout.Infrastructure.* dependency (guarded by a fitness test).

/// <summary>Process-execution facade — runs tools/shells and turns a tool path into a <see cref="Tool"/>.</summary>
public interface IProcessExecutor
{
    IProcess StartProcess(
        string toolPath,
        string arguments = null,
        string workingDirectory = null,
        IReadOnlyDictionary<string, string> environmentVariables = null,
        int? timeout = null,
        bool? logOutput = null,
        bool? logInvocation = null,
        Action<OutputType, string> logger = null,
        Func<string, string> outputFilter = null);

    IProcess StartShell(
        string command,
        string workingDirectory = null,
        IReadOnlyDictionary<string, string> environmentVariables = null,
        int? timeout = null,
        bool? logOutput = null,
        bool? logInvocation = null,
        Action<OutputType, string> logger = null,
        Func<string, string> outputFilter = null);

    void CheckPathEnvironmentVariable();
    bool DefaultLogOutput { get; }

    /// <summary>Wraps a resolved tool path into an executable <see cref="Tool"/> delegate.</summary>
    Tool GetTool(string toolPath);
}

/// <summary>Tool/package path resolution + the resolver configuration the build pushes in.</summary>
public interface IToolPathResolver
{
    string TryGetEnvironmentExecutable(string environmentExecutable);
    string GetPathExecutable(string pathExecutable);
    string GetPackageExecutable(string packageId, string packageExecutable, string version = null, string framework = null);
    string GetNpmExecutable(string npmExecutable);
    AbsolutePath GetNuGetPackagesDirectory();
    string GetPackagesConfigFile(string projectDirectory);

    // Configuration the build manager / requirement service push into the resolvers at build start.
    string EmbeddedPackagesDirectory { set; }
    string NuGetPackagesConfigFile { get; set; }
    string NuGetAssetsConfigFile { set; }
    AbsolutePath NpmPackageJsonFile { set; }
}

/// <summary>Latest-version lookups (NuGet/npm registries).</summary>
public interface IToolVersionResolver
{
    Task<string> GetLatestNuGetVersion(string packageId, bool includePrereleases, bool includeUnlisted = false);
    Task<string> GetLatestNpmVersion(string packageId);
}

/// <summary>
/// Service locator holding the registered Infrastructure adapters for the tool-execution ports. Populated by
/// a module initializer in Fallout.Infrastructure.Tooling (same assembly), so it is set before any Application
/// code runs. Tests may swap implementations directly.
/// </summary>
public static class ToolingServices
{
    public static IProcessExecutor Process { get; set; }
    public static IToolPathResolver ToolPaths { get; set; }
    public static IToolVersionResolver Versions { get; set; }
}
