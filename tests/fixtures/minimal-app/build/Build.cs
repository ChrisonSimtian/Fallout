using Fallout.Application;
using Fallout.Application.Tools.DotNet;
using Fallout.Core.IO;
using static Fallout.Application.Tools.DotNet.DotNetTasks;

/// <summary>
/// minimal-app fixture: the Compile happy path. A Fallout build that compiles a single console project
/// via the DotNet tool wrapper. Driven out-of-process by Fallout.Fixtures.Tests.
/// </summary>
class Build : FalloutBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    Target Compile => _ => _
        .Executes(() =>
        {
            DotNetBuild(_ => _.SetProjectFile(RootDirectory / "src" / "App" / "App.csproj"));
        });
}
