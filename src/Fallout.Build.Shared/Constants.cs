// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.IO;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common;

/// <summary>
/// Set of constants shared between libraries and IDE extensions.
/// </summary>
[UsedImplicitly]
internal static class Constants
{
    internal const string NukeFileName = NukeDirectoryName;
    internal const string NukeDirectoryName = ".fallout";
    // Legacy directory name from the pre-Fallout era. Read-only: lets existing
    // consumer projects keep building until they migrate (manually or via the
    // forthcoming Fallout.Migrate CLI). New setups always use .fallout/.
    internal const string LegacyNukeDirectoryName = ".nuke";
    internal const string NukeCommonPackageId = "Fallout.Common";
    internal const string BuildSchemaFileName = "build.schema.json";
    internal const string VisualStudioDebugFileName = $"{VisualStudioDebugParameterName}.log";

    internal const string TargetsSeparator = "+";
    internal const string RootDirectoryParameterName = "Root";
    internal const string InvokedTargetsParameterName = "Target";
    internal const string SkippedTargetsParameterName = "Skip";
    internal const string LoadedLocalProfilesParameterName = "Profile";

    public const string VisualStudioDebugParameterName = "visual-studio-debug";
    internal const string CompletionParameterName = "shell-completion";
    internal const string ParametersFilePrefix = "parameters";
    internal const string DefaultProfileName = "$default";

    internal const string GlobalToolVersionEnvironmentKey = "NUKE_GLOBAL_TOOL_VERSION";
    internal const string GlobalToolStartTimeEnvironmentKey = "NUKE_GLOBAL_TOOL_START_TIME";
    internal const string InterceptorEnvironmentKey = "NUKE_INTERNAL_INTERCEPTOR";

    // Canonical project URLs. Until P7 (domain registration) lands, these all point at the GitHub fork.
    // To migrate to fallout.<tld>, edit FalloutWebsite / FalloutRepository here — call sites already use the constants.
    internal const string FalloutOwner = "ChrisonSimtian";
    internal const string FalloutRepoName = "Fallout";
    internal const string FalloutWebsite = "https://github.com/" + FalloutOwner + "/" + FalloutRepoName;
    internal const string FalloutRepository = FalloutWebsite;
    internal const string FalloutRepositoryGit = FalloutWebsite + ".git";
    internal const string FalloutRawRepository = "https://raw.githubusercontent.com/" + FalloutOwner + "/" + FalloutRepoName + "/main";
    internal const string FalloutDocsUrl = FalloutWebsite;  // Replaced by docs.fallout.<tld> after #41.
    internal const string FalloutTelemetryDocsUrl = FalloutWebsite + "#telemetry";
    internal const string FalloutNotificationsUrl = FalloutRawRepository + "/notifications.json";

    // Upstream NUKE references — only for attribution / fallback recognition of legacy project URLs.
    internal const string UpstreamNukeRepository = "https://github.com/nuke-build/nuke";
    internal const string UpstreamNukeRepositoryGit = UpstreamNukeRepository + ".git";

    internal static AbsolutePath GlobalTemporaryDirectory => Path.GetTempPath();
    internal static AbsolutePath GlobalNukeDirectory =>  EnvironmentInfo.SpecialFolder(SpecialFolders.UserProfile) / ".fallout";

    [CanBeNull]
    internal static AbsolutePath TryGetRootDirectoryFrom(AbsolutePath startDirectory, bool includeLegacy = true)
    {
        var rootDirectory = new DirectoryInfo(startDirectory)
            .DescendantsAndSelf(x => x.Parent)
            .FirstOrDefault(x => x.GetDirectories(NukeDirectoryName).Any() ||
                                 x.GetDirectories(LegacyNukeDirectoryName).Any() ||
                                 includeLegacy && x.GetFiles(NukeFileName).Any())
            ?.FullName;
        return rootDirectory != GlobalNukeDirectory.Parent ? (AbsolutePath) rootDirectory : null;
    }

    internal static bool IsLegacy(AbsolutePath rootDirectory)
    {
        return File.Exists(rootDirectory / NukeFileName);
    }

    internal static AbsolutePath GetNukeDirectory(AbsolutePath rootDirectory)
    {
        var newDir = rootDirectory / NukeDirectoryName;
        if (Directory.Exists(newDir))
            return newDir;
        var legacyDir = rootDirectory / LegacyNukeDirectoryName;
        return Directory.Exists(legacyDir) ? legacyDir : newDir;
    }

    internal static AbsolutePath GetTemporaryDirectory(AbsolutePath rootDirectory)
    {
        return !IsLegacy(rootDirectory)
            ? GetNukeDirectory(rootDirectory) / "temp"
            : rootDirectory / ".tmp";
    }

    internal static AbsolutePath GetCompletionFile(AbsolutePath rootDirectory)
    {
        var completionFileName = CompletionParameterName + ".yml";
        return File.Exists(rootDirectory / completionFileName)
            ? rootDirectory / completionFileName
            : GetTemporaryDirectory(rootDirectory) / completionFileName;
    }

    internal static AbsolutePath GetBuildAttemptFile(AbsolutePath rootDirectory)
    {
        return GetTemporaryDirectory(rootDirectory) / "build-attempt.log";
    }

    public static AbsolutePath GetVisualStudioDebugFile(AbsolutePath rootDirectory)
    {
        return GetTemporaryDirectory(rootDirectory) / $"{VisualStudioDebugParameterName}.log";
    }

    public static AbsolutePath GetReSharperSurrogateFile(AbsolutePath rootDirectory)
    {
        return GetTemporaryDirectory(rootDirectory) / "resharper-surrogate.log";
    }

    internal static AbsolutePath GetBuildSchemaFile(AbsolutePath rootDirectory)
    {
        return GetNukeDirectory(rootDirectory) / BuildSchemaFileName;
    }

    internal static AbsolutePath GetDefaultParametersFile(AbsolutePath rootDirectory)
    {
        return GetNukeDirectory(rootDirectory) / GetParametersFileName(DefaultProfileName);
    }

    internal static IEnumerable<AbsolutePath> GetParametersProfileFiles(AbsolutePath rootDirectory)
    {
        return new DirectoryInfo(GetNukeDirectory(rootDirectory)).GetFiles($"{ParametersFilePrefix}.*.json", SearchOption.TopDirectoryOnly)
            .Select(x => (AbsolutePath)x.FullName);
    }

    internal static AbsolutePath GetParametersProfileFile(AbsolutePath rootDirectory, string profile)
    {
        return GetNukeDirectory(rootDirectory) / GetParametersFileName(profile);
    }

    internal static string GetParametersFileName(string profile)
    {
        return profile == DefaultProfileName ? $"{ParametersFilePrefix}.json" : $"{ParametersFilePrefix}.{profile}.json";
    }

    public static IEnumerable<string> GetProfileNames(AbsolutePath rootDirectory)
    {
        return GetParametersProfileFiles(rootDirectory)
            .Select(x => x.ToString())
            .Select(Path.GetFileNameWithoutExtension)
            .Select(x => x.TrimStart(ParametersFilePrefix).TrimStart("."));
    }

    internal static string GetCredentialStoreName(AbsolutePath rootDirectory, [CanBeNull] string profile)
    {
        return $"NUKE: {rootDirectory} ({profile ?? DefaultProfileName})";
    }

    internal static string GetProfilePasswordParameterName(string profile)
    {
        return $"PARAMS_{profile.TrimStart(DefaultProfileName).ToUpperInvariant().Replace(".", "_")}_KEY".Replace("_", string.Empty);
    }
}
