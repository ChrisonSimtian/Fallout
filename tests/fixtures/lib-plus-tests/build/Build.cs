using Fallout.Application;
using Fallout.Application.Tools.DotNet;
using Fallout.Core.Collections;
using Fallout.Core.IO;
using Fallout.Core.IO.Globbing;
using static Fallout.Application.Tools.DotNet.DotNetTasks;

/// <summary>
/// lib-plus-tests fixture: Test + glob discovery. Globs the fixture for *.Tests.csproj and runs each via the
/// DotNet test wrapper — exercising AbsolutePath globbing and DotNetTest end-to-end. Driven by Fallout.Fixtures.Tests.
/// </summary>
class Build : FalloutBuild
{
    public static int Main() => Execute<Build>(x => x.Test);

    Target Test => _ => _
        .Executes(() =>
        {
            (RootDirectory / "tests").GlobFiles("**/*.Tests.csproj")
                .ForEach(project => DotNetTest(_ => _.SetProjectFile(project)));
        });
}
