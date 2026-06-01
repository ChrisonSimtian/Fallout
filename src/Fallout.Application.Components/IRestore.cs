using System;
using System.Linq;
using static Fallout.Application.Tools.DotNet.DotNetTasks;
using Fallout.Application;
using Fallout.Application.Tools.DotNet;
using Fallout.Application.Tooling;

namespace Fallout.Application.Components;

public interface IRestore : IHasSolution, IFalloutBuild
{
    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .Apply(RestoreSettingsBase)
                .Apply(RestoreSettings));
        });

    sealed Configure<DotNetRestoreSettings> RestoreSettingsBase => _ => _
        .SetProjectFile(Solution)
        .SetIgnoreFailedSources(IgnoreFailedSources);
    // RestorePackagesWithLockFile
    // .SetProperty("RestoreLockedMode", true));

    Configure<DotNetRestoreSettings> RestoreSettings => _ => _;

    [Parameter("Ignore unreachable sources during " + nameof(Restore))]
    bool IgnoreFailedSources => TryGetValue<bool?>(() => IgnoreFailedSources) ?? false;
}
