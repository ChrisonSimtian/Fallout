using System.Text.Json;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.Npm;
using Fallout.Common.Utilities;
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

            // Versioning contract: major.minor follow the framework (NBGV, calendar versioning),
            // the patch belongs to the extension and moves independently (package.json is its source).
            using var manifest = JsonDocument.Parse((VsCodeExtensionDirectory / "package.json").ReadAllText());
            var extensionPatch = manifest.RootElement.GetProperty("version").GetString().NotNull().Split('.')[2];
            var frameworkCore = ThisAssembly.AssemblyInformationalVersion.Split('+')[0].Split('-')[0].Split('.');
            var version = $"{frameworkCore[0]}.{frameworkCore[1]}.{extensionPatch}";

            NpmRun(_ => _
                .SetProcessWorkingDirectory(VsCodeExtensionDirectory)
                .SetCommand("package")
                .SetArguments(version, "--no-git-tag-version", "--no-update-package-json",
                    "--out", OutputDirectory / $"fallout-buildview-{version}.vsix"));
        });
}
