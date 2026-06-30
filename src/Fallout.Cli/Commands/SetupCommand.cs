using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Fallout.Cli.Prompts;
using Fallout.Common;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;
using Spectre.Console;
using static Fallout.Common.Constants;
using static Fallout.Common.EnvironmentInfo;
using static Fallout.Common.Utilities.TemplateUtility;

namespace Fallout.Cli.Commands;

/// <summary>
/// <c>fallout :setup</c>: scaffolds a new build (build scripts, build project, configuration) interactively.
/// </summary>
public sealed class SetupCommand : IFalloutCommand
{
    private const string TARGET_FRAMEWORK = "net8.0";

    private readonly IConsolePrompts _prompts;
    private readonly IBuildScaffolder _scaffolder;

    public SetupCommand(IConsolePrompts prompts, IBuildScaffolder scaffolder)
    {
        _prompts = prompts;
        _scaffolder = scaffolder;
    }

    public string Name => "setup";

    public int Execute(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
    {
        ToolBanner.Print();
        Logging.Configure();
        Telemetry.SetupBuild();

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Let's setup a new build![/]");
        AnsiConsole.WriteLine();

        #region Basic

        var nukeLatestReleaseVersion = NuGetVersionResolver.GetLatestVersion(FalloutCommonPackageId, includePrereleases: false);
        var nukeLatestPrereleaseVersion = NuGetVersionResolver.GetLatestVersion(FalloutCommonPackageId, includePrereleases: true);
        var nukeLatestLocalVersion = NuGetPackageResolver.GetGlobalInstalledPackage(FalloutCommonPackageId, version: null, packagesConfigFile: null)
            ?.Version.ToString();

        if (rootDirectory == null)
            rootDirectory = WorkingDirectory.FindParentOrSelf(x => x.ContainsDirectory(".git") || x.ContainsDirectory(".svn"));

        if (rootDirectory == null)
        {
            Host.Warning("Could not find root directory. Falling back to working directory ...");
            rootDirectory = WorkingDirectory;
        }
        _prompts.ShowInput("deciduous_tree", "Root directory", rootDirectory);

        var buildProjectName = _prompts.PromptForInput("How should the project be named?", "_build");
        _prompts.ClearPreviousLine();
        _prompts.ShowInput("bookmark", "Build project name", buildProjectName);

        var buildProjectRelativeDirectory = _prompts.PromptForInput("Where should the project be located?", "./build");
        _prompts.ClearPreviousLine();
        _prompts.ShowInput("round_pushpin", "Build project location", buildProjectRelativeDirectory);

        var nukeVersion = _prompts.PromptForChoice("Which Fallout.Common version should be used?",
            new[]
                {
                    ("latest release", nukeLatestReleaseVersion.GetAwaiter().GetResult()),
                    ("latest prerelease", nukeLatestPrereleaseVersion.GetAwaiter().GetResult()),
                    ("latest local", nukeLatestLocalVersion),
                    ("same as global tool", typeof(SetupCommand).GetTypeInfo().Assembly.GetVersionText())
                }
                .Where(x => x.Item2 != null)
                .Distinct(x => x.Item2)
                .Select(x => (x.Item2, $"{x.Item2} ({x.Item1})")).ToArray());
        _prompts.ShowInput("gem_stone", "Fallout.Common version", nukeVersion);

        var solutionFile = (AbsolutePath) _prompts.PromptForChoice(
            "Which solution should be the default?",
            choices: new DirectoryInfo(rootDirectory)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(x => x.FullName.EndsWithOrdinalIgnoreCase(".sln"))
                .OrderByDescending(x => x.FullName)
                .Select(x => (x, rootDirectory.GetRelativePathTo(x.FullName).ToString()))
                .Concat((null, "None")).ToArray())?.FullName;
        _prompts.ShowInput("toolbox", "Default solution", solutionFile != null ? rootDirectory.GetRelativePathTo(solutionFile) : "<none>");

        #endregion

        #region Generation

        var buildDirectory = rootDirectory / buildProjectRelativeDirectory;
        var buildProjectFile = rootDirectory / buildProjectRelativeDirectory / buildProjectName + ".csproj";
        var buildProjectGuid = Guid.NewGuid().ToString().ToUpper();

        (rootDirectory / FalloutDirectoryName).CreateDirectory();

        _scaffolder.WriteBuildScripts(
            scriptDirectory: WorkingDirectory,
            rootDirectory,
            buildDirectory,
            buildProjectName);

        _scaffolder.WriteConfigurationFile(rootDirectory, solutionFile);

        if (solutionFile != null)
        {
            var solutionFileContent = solutionFile.ReadAllLines().ToList();
            var buildProjectFileRelative = solutionFile.Parent.GetWinRelativePathTo(buildProjectFile);
            _scaffolder.UpdateSolutionFileContent(solutionFileContent, buildProjectFileRelative, buildProjectGuid, buildProjectName);
            solutionFile.WriteAllLines(solutionFileContent, Encoding.UTF8);
        }

        buildProjectFile.WriteAllLines(
            FillTemplate(
                _scaffolder.GetTemplate("_build.csproj"),
                GetDictionary(
                    new
                    {
                        RootDirectory = buildDirectory.GetWinRelativePathTo(rootDirectory),
                        ScriptDirectory = buildDirectory.GetWinRelativePathTo(WorkingDirectory),
                        TargetFramework = TARGET_FRAMEWORK,
                        TelemetryVersion = Telemetry.CurrentVersion,
                        NukeVersion = nukeVersion,
                    })));

        (buildDirectory / "Directory.Build.props").WriteAllLines(_scaffolder.GetTemplate("Directory.Build.props"));
        (buildDirectory / "Directory.Build.targets").WriteAllLines(_scaffolder.GetTemplate("Directory.Build.targets"));
        (buildDirectory / "Build.cs").WriteAllLines(FillTemplate(_scaffolder.GetTemplate("Build.cs")));
        (buildDirectory / "Configuration.cs").WriteAllLines(_scaffolder.GetTemplate("Configuration.cs"));

        #endregion

        _prompts.ShowCompletion("Setup");

        return 0;
    }
}
