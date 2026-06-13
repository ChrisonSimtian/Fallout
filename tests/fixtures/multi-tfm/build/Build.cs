using Fallout.Application;
using Fallout.Application.Tools.DotNet;
using Fallout.Core.IO;
using static Fallout.Application.Tools.DotNet.DotNetTasks;

/// <summary>
/// multi-tfm fixture: per-TFM build. Compiles a library that targets netstandard2.0 + net10.0, so a single
/// DotNetBuild produces one output per target framework. Driven by Fallout.Fixtures.Tests.
/// </summary>
class Build : FalloutBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    Target Compile => _ => _
        .Executes(() =>
        {
            DotNetBuild(_ => _.SetProjectFile(RootDirectory / "src" / "MultiLib" / "MultiLib.csproj"));
        });
}
