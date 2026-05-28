using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Fallout.Common.CI;
using Fallout.Common.Git;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;
using Fallout.Common.ValueInjection;

namespace Fallout.Common.Execution;

internal partial class Telemetry
{
    private static readonly string[] s_knownTargets = { "Restore", "Compile", "Test" };

    private static Dictionary<string, string> GetCommonProperties(IFalloutBuild build = null)
    {
        var version = ControlFlow.SuppressErrors(
            () =>
            {
                var dotnet = ToolResolver.GetEnvironmentOrPathTool("dotnet");
                return dotnet.Invoke($"--version", logInvocation: false, logOutput: false).StdToText();
            },
            logWarning: false);

        return new Dictionary<string, string>
               {
                   ["os_platform"] = EnvironmentInfo.Platform.ToString(),
                   ["os_architecture"] = RuntimeInformation.OSArchitecture.ToString(),
                   ["version_dotnet_sdk"] = version,
                   ["version_fallout_common"] = build != null ? typeof(FalloutBuild).Assembly.GetVersionText() : null,
                   ["version_fallout_global_tool"] = build != null
                       ? LegacyEnvironment.ReadFromVariables(
                           EnvironmentInfo.Variables,
                           Constants.GlobalToolVersionEnvironmentKey,
                           Constants.LegacyGlobalToolVersionEnvironmentKey)
                       : Assembly.GetEntryAssembly().GetVersionText()
               };
    }

    private static Dictionary<string, string> GetRepositoryProperties(string directory)
    {
        var repository = ControlFlow.SuppressErrors(() => GitRepository.FromLocalDirectory(directory), logWarning: false);
        if (repository == null)
            return new Dictionary<string, string>();

        var providers =
            new List<(Func<bool>, string)>
            {
                (() => repository.Endpoint?.ContainsOrdinalIgnoreCase("github.com") ?? false, "GitHub"),
                (() => repository.Endpoint?.ContainsOrdinalIgnoreCase("gitlab.com") ?? false, "GitLab"),
                (() => repository.Endpoint?.ContainsOrdinalIgnoreCase("bitbucket.org") ?? false, "Bitbucket"),
                (() => repository.Endpoint?.ContainsOrdinalIgnoreCase("jetbrains.space") ?? false, "JetBrains"),
                (() => repository.Endpoint?.ContainsOrdinalIgnoreCase("visualstudio.com") ?? false, "Azure")
            };

        var branches =
            new List<(Func<bool>, string)>
            {
                (() => repository.IsOnMainOrMasterBranch(), "main"),
                (() => repository.IsOnDevelopBranch(), "develop"),
                (() => repository.IsOnReleaseBranch(), "release"),
                (() => repository.IsOnHotfixBranch(), "hotfix")
            };

        return new Dictionary<string, string>
               {
                   ["repo_provider"] = providers.FirstOrDefault(x => x.Item1.Invoke()).Item2,
                   ["repo_branch"] = branches.FirstOrDefault(x => x.Item1.Invoke()).Item2,
                   ["repo_url"] = repository.SshUrl?.GetSHA256Hash()[..6],
                   ["repo_commit"] = repository.Commit?.GetSHA256Hash()[..6]
               };
    }

    private static ReadOnlyDictionary<string, string> GetBuildProperties(IFalloutBuild build)
    {
        var startTimeString = LegacyEnvironment.ReadFromVariables(
            EnvironmentInfo.Variables,
            Constants.GlobalToolStartTimeEnvironmentKey,
            Constants.LegacyGlobalToolStartTimeEnvironmentKey);
        var compileTime = startTimeString != null
            ? DateTime.Now.Subtract(DateTime.Parse(startTimeString))
            : default(TimeSpan?);

        return new Dictionary<string, string>
               {
                   ["compile_time"] = compileTime?.TotalSeconds.ToString("F0"),
                   ["target_framework"] = EnvironmentInfo.Framework.ToString(),
                   ["host"] = GetTypeName(build.Host),
                   ["build_type"] = build.BuildProjectFile != null ? "Project" : "Global Tool",
                   ["num_targets"] = build.ExecutableTargets.Count.ToString(),
                   ["num_custom_extensions"] = build.BuildExtensions.Select(x => x.GetType()).Count(IsCustomType).ToString(),
                   ["num_custom_components"] = build.GetType().GetInterfaces().Count(IsCustomType).ToString(),
                   ["num_partitioned_targets"] = build.ExecutableTargets.Count(x => x.PartitionSize.HasValue).ToString(),
                   ["num_secrets"] = ValueInjectionUtility.GetParameterMembers(build.GetType(), includeUnlisted: true)
                       .Count(x => x.HasCustomAttribute<SecretAttribute>()).ToString(),
                   ["config_generators"] = build.GetType().GetCustomAttributes<ConfigurationAttributeBase>()
                       .Select(GetTypeName).Distinct().OrderBy(x => x).JoinCommaSpace(),
                   ["build_components"] = build.GetType().GetInterfaces().Where(x => IsCommonType(x) && x != typeof(IFalloutBuild))
                       .Select(GetTypeName).Distinct().OrderBy(x => x).JoinCommaSpace()
               }.AsReadOnly();
    }

    private static Dictionary<string, string> GetTargetProperties(IFalloutBuild build, ExecutableTarget target)
    {
        return new Dictionary<string, string>
               {
                   ["target_name"] = target.Name,
                   ["target_duration"] = target.Duration.TotalSeconds.ToString("F0"),
                   ["target_current_partition"] = build.Partition.Part.ToString(),
                   ["target_total_partitions"] = build.Partition.Total.ToString()
               };
    }

    private static Dictionary<string, string> GetGeneratorProperties(Type hostType, string generatorId)
    {
        return new Dictionary<string, string>
               {
                   ["generator_host"] = GetTypeName(hostType),
                   ["generator_id"] = generatorId.GetSHA256Hash()[..6]
               };
    }

    private static bool IsCommonType(Type type)
    {
        // Recognize both the new Fallout repository and the legacy NUKE repository — types compiled
        // from older NUKE versions still embed the upstream RepositoryUrl in their assembly metadata.
        return type.Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Any(x => x is { Key: "RepositoryUrl" } &&
                      (x.Value == Constants.FalloutRepositoryGit ||
                       x.Value == Constants.UpstreamNukeRepositoryGit));
    }

    private static bool IsCustomType(Type type)
    {
        return !IsCommonType(type);
    }

    private static string GetTypeName(Type type)
    {
        return IsCommonType(type)
            ? type.Name.TrimEnd(nameof(Attribute))
            : type.Descendants(x => x.BaseType).FirstOrDefault(IsCommonType) is { } commonType
                ? $"{GetTypeName(commonType)} (Custom)"
                : "<Custom>";
    }

    private static string GetTypeName(object obj)
    {
        return GetTypeName(obj.GetType());
    }
}
