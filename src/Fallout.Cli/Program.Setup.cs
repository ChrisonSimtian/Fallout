using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Fallout.Common;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;
using Spectre.Console;
using static Fallout.Common.Constants;
using static Fallout.Common.EnvironmentInfo;
using static Fallout.Common.Tooling.ProcessTasks;
using static Fallout.Common.Utilities.TemplateUtility;

namespace Fallout.Cli;

partial class Program
{
    // ReSharper disable InconsistentNaming

    private const string PROJECT_KIND = "9A19103F-16F7-4668-BE54-9A1E7A4F7556";

    // Transitional shim: cake (still a legacy handler) invokes setup directly. Removed once cake is
    // converted; the dispatcher itself resolves SetupCommand from the registry, not this.
    internal static int Setup(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
        => new Commands.SetupCommand(s_prompts).Execute(args, rootDirectory, buildScript);

    // Residual after the :setup command moved to SetupCommand: these scaffolding helpers are shared
    // with update (and UpdateSolutionFileContent is exercised directly by tests). They move into a
    // scaffolding service in the #392 collapse PR.
    internal static void UpdateSolutionFileContent(
        List<string> content,
        string buildProjectFileRelative,
        string buildProjectGuid,
        string buildProjectName)
    {
        if (content.Any(x => x.Contains(buildProjectFileRelative)))
            return;

        var globalIndex = content.IndexOf("Global");
        Assert.True(globalIndex != -1, "Could not find a 'Global' section in solution file");

        var projectConfigurationIndex = content.FindIndex(x => x.Contains("GlobalSection(ProjectConfigurationPlatforms)"));
        if (projectConfigurationIndex == -1)
        {
            var solutionConfigurationIndex = content.FindIndex(x => x.Contains("GlobalSection(SolutionConfigurationPlatforms)"));
            if (solutionConfigurationIndex == -1)
            {
                content.Insert(globalIndex + 1, "\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
                content.Insert(globalIndex + 2, "\t\tDebug|Any CPU = Debug|Any CPU");
                content.Insert(globalIndex + 3, "\t\tRelease|Any CPU = Release|Any CPU");
                content.Insert(globalIndex + 4, "\tEndGlobalSection");

                solutionConfigurationIndex = globalIndex + 1;
            }

            var endGlobalSectionIndex = content.FindIndex(solutionConfigurationIndex, x => x.Contains("EndGlobalSection"));

            content.Insert(endGlobalSectionIndex + 1, "\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            content.Insert(endGlobalSectionIndex + 2, "\tEndGlobalSection");

            projectConfigurationIndex = endGlobalSectionIndex + 1;
        }

        content.Insert(projectConfigurationIndex + 1, $"\t\t{{{buildProjectGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
        content.Insert(projectConfigurationIndex + 2, $"\t\t{{{buildProjectGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU");

        content.Insert(globalIndex,
            $"Project(\"{{{PROJECT_KIND}}}\") = \"{buildProjectName}\", \"{buildProjectFileRelative}\", \"{{{buildProjectGuid}}}\"");
        content.Insert(globalIndex + 1,
            "EndProject");
    }

    internal static string[] GetTemplate(string templateName)
    {
        return ResourceUtility.GetResourceAllLines<Program>($"templates.{templateName}");
    }


    internal static void WriteBuildScripts(
        AbsolutePath scriptDirectory,
        AbsolutePath rootDirectory,
        AbsolutePath buildDirectory,
        string buildProjectName)
    {
        (scriptDirectory / "build.sh").WriteAllLines(
            FillTemplate(
                GetTemplate("build.sh"),
                tokens: GetDictionary(
                    new
                    {
                        RootDirectory = scriptDirectory.GetUnixRelativePathTo(rootDirectory),
                    })),
            platformFamily: PlatformFamily.Linux);

        (scriptDirectory / "build.ps1").WriteAllLines(
            FillTemplate(
                GetTemplate("build.ps1"),
                tokens: GetDictionary(
                    new
                    {
                        RootDirectory = scriptDirectory.GetWinRelativePathTo(rootDirectory),
                    })),
            platformFamily: PlatformFamily.Windows);

        // .config/dotnet-tools.json pins Fallout.GlobalTools as a local tool so the thin shims
        // (build.sh / build.ps1) can `dotnet tool restore` and `dotnet fallout` deterministically.
        // Skip if the consumer already has a manifest — they may have other tools pinned and we
        // don't want to clobber. They can add the `fallout.globaltools` entry manually.
        var toolManifest = rootDirectory / ".config" / "dotnet-tools.json";
        if (!toolManifest.FileExists())
        {
            (rootDirectory / ".config").CreateDirectory();
            toolManifest.WriteAllLines(
                FillTemplate(
                    GetTemplate("dotnet-tools.json"),
                    tokens: GetDictionary(
                        new
                        {
                            FalloutCliVersion = typeof(Program).GetTypeInfo().Assembly.GetVersionText(),
                        })));
        }

        MakeExecutable(scriptDirectory / "build.sh");

        void MakeExecutable(AbsolutePath scriptFile)
        {
            if (rootDirectory.ContainsDirectory(".git"))
                StartProcess("git", $"update-index --add --chmod=+x {scriptFile}", logInvocation: false, logOutput: false);

            if (rootDirectory.ContainsDirectory(".svn"))
                StartProcess("svn", $"propset svn:executable on {scriptFile}", logInvocation: false, logOutput: false);

            if (IsUnix)
                StartProcess("chmod", $"+x {scriptFile}", logInvocation: false, logOutput: false);
        }
    }

    internal static void WriteConfigurationFile(AbsolutePath rootDirectory, AbsolutePath solutionFile)
    {
        var parametersFile = GetDefaultParametersFile(rootDirectory);
        var dictionary = new Dictionary<string, string> { ["$schema"] = BuildSchemaFileName };
        if (solutionFile != null)
            dictionary["Solution"] = rootDirectory.GetUnixRelativePathTo(solutionFile).ToString();
        parametersFile.WriteJson(dictionary, JsonExtensions.DefaultSerializerOptions);
    }
}
