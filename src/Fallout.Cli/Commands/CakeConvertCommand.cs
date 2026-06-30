using System.Collections.Generic;
using System.Linq;
using Fallout.Cli.Prompts;
using Fallout.Common;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Utilities;
using static Fallout.Common.EnvironmentInfo;

namespace Fallout.Cli.Commands;

/// <summary>
/// <c>fallout :cake-convert</c>: best-effort conversion of <c>*.cake</c> scripts to Fallout C#.
/// </summary>
public sealed class CakeConvertCommand : IFalloutCommand
{
    private readonly IConsolePrompts _prompts;

    public CakeConvertCommand(IConsolePrompts prompts) => _prompts = prompts;

    public string Name => "cake-convert";

    // The .cake syntax-rewriting helpers (GetCakeFiles/GetCakeConvertedContent/GetCakePackages) and
    // the shared GetConfiguration/AddOrReplacePackage helpers remain on Program until the #392
    // collapse PR; GetCakeConvertedContent/GetCakePackages are also exercised directly by tests.
    public int Execute(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
    {
        Program.PrintInfo();
        Logging.Configure();
        Telemetry.ConvertCake();
        ProjectModelTasks.Initialize();

        Host.Warning(
            new[]
            {
                "Converting .cake files is a best effort approach using syntax rewriting.",
                "Compile errors are to be expected, however, the following elements are currently covered:",
                "  - Target definitions",
                "  - Default targets",
                "  - Parameter declarations",
                "  - Absolute paths",
                "  - Globbing patterns",
                "  - Tool invocations (dotnet CLI, SignTool)",
                "  - Addin and tool references",
            }.JoinNewLine());

        Host.Debug();
        if (!_prompts.PromptForConfirmation("Continue?"))
            return 0;
        Host.Debug();

        if (buildScript == null &&
            _prompts.PromptForConfirmation("Should a Fallout project be created for better results?"))
        {
            Program.Setup(args, rootDirectory: null, buildScript: null);
        }

        var buildScriptFile = WorkingDirectory / Program.CurrentBuildScriptName;
        var buildProjectFile = buildScriptFile.Exists()
            ? Program.GetConfiguration(buildScriptFile, evaluate: true)
                .GetValueOrDefault(Program.BUILD_PROJECT_FILE, defaultValue: null)
            : null;

        foreach (var cakeFile in Program.GetCakeFiles())
        {
            var outputFile = cakeFile.Parent / cakeFile.NameWithoutExtension.Capitalize() + ".cs";
            var content = Program.GetCakeConvertedContent(cakeFile.ReadAllText());
            outputFile.WriteAllText(content);
        }

        if (buildProjectFile != null)
        {
            var packages = Program.GetCakeFiles().SelectMany(x => Program.GetCakePackages(x.ReadAllText()));
            foreach (var package in packages)
                Program.AddOrReplacePackage(package.Id, package.Version, package.Type, buildProjectFile);
        }

        return 0;
    }
}
