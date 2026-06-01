using Fallout.Application;
using Fallout.Application.Solutions;

class Build : FalloutBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    [Solution] readonly Solution Solution;

    Target Default => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("hello from fallout consumer (local source)");
            Serilog.Log.Information("solution name: {Name}", Solution?.Name ?? "<unbound>");
        });
}
