using Fallout.Common.Tooling;
using Fallout.Common.Utilities;

namespace Fallout.Common.Tools.DotCover;

partial class DotCoverTasks
{
    protected override string GetToolPath(ToolOptions options = null)
    {
        return NuGetToolPathResolver.GetPackageExecutable(
            PackageId,
            EnvironmentInfo.IsWin ? "dotCover.exe" : "dotCover.sh|dotCover.dll");
    }
}

partial class DotCoverAnalyseSettingsExtensions
{
    public static DotCoverAnalyseSettings SetTargetSettings(this DotCoverAnalyseSettings toolSettings, ToolOptions targetSettings)
    {
        return toolSettings
            .SetTargetExecutable(targetSettings.ProcessToolPath)
            .SetTargetArguments(targetSettings.GetArguments().JoinSpace())
            .SetTargetWorkingDirectory(targetSettings.ProcessWorkingDirectory);
    }

    public static DotCoverAnalyseSettings ResetTargetSettings(this DotCoverAnalyseSettings toolSettings)
    {
        return toolSettings
            .ResetTargetExecutable()
            .ResetTargetArguments()
            .ResetTargetWorkingDirectory();
    }
}

partial class DotCoverCoverSettingsExtensions
{
    public static DotCoverCoverSettings SetTargetSettings(this DotCoverCoverSettings toolSettings, ToolOptions targetSettings)
    {
        return toolSettings
            .SetTargetExecutable(targetSettings.ProcessToolPath)
            .SetTargetArguments(targetSettings.GetArguments().JoinSpace())
            .SetTargetWorkingDirectory(targetSettings.ProcessWorkingDirectory);
    }

    public static DotCoverCoverSettings ResetTargetSettings(this DotCoverCoverSettings toolSettings)
    {
        return toolSettings
            .ResetTargetExecutable()
            .ResetTargetArguments()
            .ResetTargetWorkingDirectory();
    }
}
