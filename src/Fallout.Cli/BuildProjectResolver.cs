using System.IO;
using static Fallout.Build.Shared.Constants;
using Fallout.Kernel.IO;
using Fallout.Kernel;
using Fallout.Build.Shared;

namespace Fallout.Cli;

/// <summary>
/// Resolves the build project file (.csproj) for a Fallout-managed repository.
/// Reads the optional <c>BuildProjectFile</c> entry from <c>.fallout/parameters.json</c> when set;
/// otherwise falls back to the convention <c>build/_build.csproj</c>.
/// </summary>
internal static class BuildProjectResolver
{
    private const string BuildProjectFileKey = "BuildProjectFile";

    public static AbsolutePath Resolve(AbsolutePath rootDirectory)
    {
        Assert.NotNull(rootDirectory);

        var parametersFile = GetDefaultParametersFile(rootDirectory);
        if (File.Exists(parametersFile))
        {
            var configured = parametersFile.ReadJsonObject()?[BuildProjectFileKey]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(configured))
            {
                var configuredPath = Path.IsPathRooted(configured)
                    ? (AbsolutePath)configured
                    : rootDirectory / configured;
                Assert.True(File.Exists(configuredPath),
                    $"BuildProjectFile '{configured}' from '{parametersFile}' does not exist (resolved as '{configuredPath}').");
                return configuredPath;
            }
        }

        var defaultPath = rootDirectory / "build" / "_build.csproj";
        Assert.True(File.Exists(defaultPath),
            $"Could not locate build project. Looked for '{defaultPath}' (convention) and 'BuildProjectFile' is not set in '{parametersFile}'. " +
            $"Run 'fallout :setup' to scaffold a build, or set 'BuildProjectFile' in '{parametersFile}'.");
        return defaultPath;
    }
}
