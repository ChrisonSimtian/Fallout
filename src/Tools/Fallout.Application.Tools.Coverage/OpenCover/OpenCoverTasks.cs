using Fallout.Common.Tooling;
using Fallout.Common.Utilities;

using Fallout.Common;
namespace Fallout.Application.Tools.Coverage.OpenCover;

public class OpenCoverVerbosityMappingAttribute : VerbosityMappingAttribute
{
    public OpenCoverVerbosityMappingAttribute()
        : base(typeof(OpenCoverVerbosity))
    {
        Quiet = nameof(OpenCoverVerbosity.Off);
        Minimal = nameof(OpenCoverVerbosity.Warn);
        Normal = nameof(OpenCoverVerbosity.Info);
        Verbose = nameof(OpenCoverVerbosity.Verbose);
    }
}

partial class OpenCoverSettingsExtensions
{
    public static OpenCoverSettings SetTargetSettings(this OpenCoverSettings toolSettings, ToolOptions targetSettings)
    {
        return toolSettings
            .SetTargetPath(targetSettings.ProcessToolPath)
            .SetTargetArguments(targetSettings.GetArguments().JoinSpace())
            .SetTargetDirectory(targetSettings.ProcessWorkingDirectory);
    }

    public static OpenCoverSettings ResetTargetSettings(this OpenCoverSettings toolSettings)
    {
        return toolSettings
            .ResetTargetPath()
            .ResetTargetArguments()
            .ResetTargetDirectory();
    }
}
