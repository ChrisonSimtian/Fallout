using System;
using System.Linq;
using Fallout.Common.Tooling;

using Fallout.Common;
namespace Fallout.Application.Tools.Coverage.Coverlet;

public static partial class CoverletSettingsExtensions
{
    /// <summary>
    /// <p><em>Sets <see cref="CoverletSettings.Target"/> and <see cref="CoverletSettings.TargetArgs"/> to the values defined by <paramref name="targetSettings"/>.</em></p>
    /// </summary>
    /// <returns></returns>
    public static CoverletSettings SetTargetSettings(this CoverletSettings toolSettings, ToolOptions targetSettings)
    {
        return toolSettings
            .SetTarget(targetSettings.ProcessToolPath)
            .SetTargetArgs(targetSettings.GetArguments());
    }

    /// <summary>
    /// <p><em>Resets <see cref="CoverletSettings.Target"/> and <see cref="CoverletSettings.TargetArgs"/>.</em></p>
    /// </summary>
    public static CoverletSettings ResetTargetSettings(this CoverletSettings toolSettings)
    {
        return toolSettings
            .ResetTarget()
            .ClearTargetArgs();
    }
}
