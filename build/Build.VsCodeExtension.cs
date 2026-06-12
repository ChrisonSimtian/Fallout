using System.Text.Json;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.Npm;
using static Fallout.Common.Tools.Npm.NpmTasks;

partial class Build
{
    AbsolutePath VsCodeExtensionDirectory => RootDirectory / "poc" / "vscode-fallout";

    Target PackVsCodeExtension => _ => _
        .Description("Builds and packages the VS Code extension PoC into a .vsix under output/.")
        .Produces(OutputDirectory / "*.vsix")
        .Executes(() =>
        {
            NpmCi(_ => _
                .SetProcessWorkingDirectory(VsCodeExtensionDirectory));

            // vsce requires a LICENSE next to the manifest; the repo-root LICENSE is the single
            // source of truth, so it is copied (and gitignored) rather than duplicated.
            (RootDirectory / "LICENSE").Copy(VsCodeExtensionDirectory / "LICENSE", ExistsPolicy.FileOverwrite);

            OutputDirectory.CreateDirectory();
            using var manifest = JsonDocument.Parse((VsCodeExtensionDirectory / "package.json").ReadAllText());
            var version = manifest.RootElement.GetProperty("version").GetString();
            NpmRun(_ => _
                .SetProcessWorkingDirectory(VsCodeExtensionDirectory)
                .SetCommand("package")
                .SetArguments("--out", OutputDirectory / $"fallout-buildview-{version}.vsix"));
        });
}
