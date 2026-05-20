// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Fallout.Common.IO;
using Fallout.Common.ProjectModel;
using Fallout.Common.Utilities;
using static Fallout.Common.ControlFlow;

namespace Fallout.Common.Execution;

internal static partial class Telemetry
{
    // https://docs.microsoft.com/en-us/dotnet/core/tools/telemetry
    // https://dotnet.microsoft.com/platform/telemetry
    // https://docs.microsoft.com/en-us/azure/azure-monitor/app/console
    // https://docs.microsoft.com/en-us/azure/azure-monitor/app/ip-collection

    public const string OptOutEnvironmentKey = "NUKE_TELEMETRY_OPTOUT";
    public const int CurrentVersion = 1;

    // Telemetry is currently a no-op for Fallout: the previous InstrumentationKey routed data
    // to the original NUKE maintainer's Azure Application Insights, which we don't own and shouldn't
    // populate. When/if we stand up a Fallout-controlled endpoint, fill in the key here.
    // Original NUKE key (do NOT reuse): "4b987be9-f807-4846-b777-4291f3a5ad8b"
    private const string InstrumentationKey = "";
    private const string VersionPropertyName = "NukeTelemetryVersion";

    private static readonly TelemetryClient s_client;
    private static readonly int? s_confirmedVersion;

    static Telemetry()
    {
        var optoutParameter = ParameterService.GetParameter<string>(OptOutEnvironmentKey) ?? string.Empty;
        if (optoutParameter == "1" || optoutParameter.EqualsOrdinalIgnoreCase(bool.TrueString))
            return;

        // No telemetry endpoint configured for Fallout yet — short-circuit before doing any work.
        // The awareness/disclosure plumbing stays in place for when we wire up a real endpoint.
        if (string.IsNullOrEmpty(InstrumentationKey))
            return;

        ProjectModelTasks.Initialize();
        s_confirmedVersion = SuppressErrors(CheckAwareness, includeStackTrace: true);
        if (s_confirmedVersion == null)
            return;

        var configuration = TelemetryConfiguration.CreateDefault();
        configuration.ConnectionString = $"InstrumentationKey={InstrumentationKey}";
        s_client = new TelemetryClient(configuration);
        s_client.Context.Session.Id = Guid.NewGuid().ToString();
        s_client.Context.Location.Ip = "N/A";
        s_client.Context.Cloud.RoleInstance = "N/A";
    }

    private static int? CheckAwareness()
    {
        AbsolutePath GetCookieFile(string name, int version)
            => Constants.GlobalNukeDirectory / "telemetry-awareness" / $"v{version}" / name;

        // Check for calls from Fallout.GlobalTool and custom global tools
        if (SuppressErrors(() => NukeBuild.BuildProjectFile, logWarning: false) == null)
        {
            var cookieName = Assembly.GetEntryAssembly().NotNull().GetName().Name;
            var cookieFile = GetCookieFile(cookieName, CurrentVersion);
            if (!cookieFile.Exists())
            {
                PrintDisclosure($"create awareness cookie for {cookieName.SingleQuote()}");
                cookieFile.TouchFile();
            }

            return CurrentVersion;
        }

        var project = ProjectModelTasks.ParseProject(NukeBuild.BuildProjectFile);
        var property = project.Properties.SingleOrDefault(x => x.Name.EqualsOrdinalIgnoreCase(VersionPropertyName));
        if (property?.EvaluatedValue != CurrentVersion.ToString())
        {
            if (NukeBuild.IsServerBuild)
            {
                PrintDisclosure(action: null);
                return null;
            }

            PrintDisclosure($"set the {VersionPropertyName.SingleQuote()} property");
            project.SetProperty(VersionPropertyName, CurrentVersion.ToString());
            project.Save();
        }

        for (var version = CurrentVersion; version > 0; version--)
        {
            var cookieFile = GetCookieFile("Fallout.GlobalTool", version);
            if (cookieFile.FileExists())
                return version;
        }

        return NukeBuild.IsServerBuild ? CurrentVersion : null;
    }

    private static void PrintDisclosure(string action)
    {
        var disclosure =
            new[]
            {
                $"Telemetry v{CurrentVersion}",
                "------------",
                "Fallout collects anonymous usage data in order to help us improve your experience.",
                $"Read more about scope, data points, and opt-out: {Constants.FalloutTelemetryDocsUrl}",
                string.Empty
            }.JoinNewLine();

        if (action != null &&
            Environment.UserInteractive &&
            !Console.IsInputRedirected)
        {
            Host.Information(disclosure);
            Thread.Sleep(2000);
            Host.Information($"Press <Enter> to {action} ...");
            WaitForEnter();
        }
        else
        {
            Host.Warning(disclosure);
            Host.Warning("Run in interactive console to fix this warning");
        }
    }

    private static void WaitForEnter()
    {
        ConsoleKey response;
        do
        {
            response = Console.ReadKey(intercept: true).Key;
        } while (response != ConsoleKey.Enter);
    }
}