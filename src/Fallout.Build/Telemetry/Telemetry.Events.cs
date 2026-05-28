using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;
using Serilog;

namespace Fallout.Common.Execution;

internal partial class Telemetry
{
    public static void BuildStarted(IFalloutBuild build)
    {
        TrackEvent(
            eventName: nameof(BuildStarted),
            propertiesProvider: () =>
                GetCommonProperties(build)
                    .AddDictionary(GetBuildProperties(build))
                    .AddDictionary(GetRepositoryProperties(build.RootDirectory)));
    }

    public static void TargetSucceeded(ExecutableTarget target, IFalloutBuild build)
    {
        if (!target.Name.EqualsAnyOrdinalIgnoreCase(s_knownTargets) ||
            target.Status != ExecutionStatus.Succeeded)
            return;

        TrackEvent(
            eventName: nameof(TargetSucceeded),
            propertiesProvider: () =>
                GetCommonProperties(build)
                    .AddDictionary(GetTargetProperties(build, target))
                    .AddDictionary(GetBuildProperties(build))
                    .AddDictionary(GetRepositoryProperties(build.RootDirectory)));
    }

    public static void ConfigurationGenerated(Type hostType, string generatorId, IFalloutBuild build)
    {
        TrackEvent(
            eventName: nameof(ConfigurationGenerated),
            propertiesProvider: () =>
                GetCommonProperties(build)
                    .AddDictionary(GetGeneratorProperties(hostType, generatorId))
                    .AddDictionary(GetBuildProperties(build))
                    .AddDictionary(GetRepositoryProperties(EnvironmentInfo.WorkingDirectory)));
    }

    public static void SetupBuild()
    {
        TrackEvent(
            eventName: nameof(SetupBuild),
            propertiesProvider: () =>
                GetCommonProperties()
                    .AddDictionary(GetRepositoryProperties(EnvironmentInfo.WorkingDirectory)));
    }

    public static void ConvertCake()
    {
        TrackEvent(
            eventName: nameof(ConvertCake),
            propertiesProvider: () =>
                GetCommonProperties()
                    .AddDictionary(GetRepositoryProperties(EnvironmentInfo.WorkingDirectory)));
    }

    public static void AddPackage()
    {
        TrackEvent(
            eventName: nameof(AddPackage),
            propertiesProvider: () =>
                GetCommonProperties()
                    .AddDictionary(GetRepositoryProperties(EnvironmentInfo.WorkingDirectory)));
    }

    private static void TrackEvent(string eventName, Func<Dictionary<string, string>> propertiesProvider)
    {
        // No-op until a Fallout-controlled telemetry backend lands (#79). Public callers
        // (BuildStarted, TargetSucceeded, etc.) stay so we don't have to thread the change
        // back through their call sites when telemetry is reintroduced.
        _ = eventName;
        _ = propertiesProvider;
    }
}
