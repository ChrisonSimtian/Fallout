using Fallout.Application;
using Fallout.Application.Tools.DotNet;
using Fallout.Core.IO;
using static Fallout.Application.Tools.DotNet.DotNetTasks;

/// <summary>
/// packable fixture: Pack + target orchestration. Pack depends on Compile (a DependsOn chain), then packs the
/// library to ./artifacts — exercising DotNetPack and producing a real .nupkg. Driven by Fallout.Fixtures.Tests.
/// </summary>
class Build : FalloutBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    AbsolutePath PackLibProject => RootDirectory / "src" / "PackLib" / "PackLib.csproj";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Compile => _ => _
        .Executes(() =>
        {
            DotNetBuild(_ => _.SetProjectFile(PackLibProject));
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(PackLibProject)
                .SetOutputDirectory(ArtifactsDirectory)
                // Reuse Compile's output (Debug) instead of letting pack default to Release and rebuild.
                .SetConfiguration("Debug")
                .SetNoBuild(true));
        });
}
