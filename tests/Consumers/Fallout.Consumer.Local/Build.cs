// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE
//
// Fallout consumer against this repo's local source. Catches breakage of the
// public Fallout surface in the current PR.

using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.ProjectModel;

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
