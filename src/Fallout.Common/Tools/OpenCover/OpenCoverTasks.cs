// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using JetBrains.Annotations;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;

namespace Fallout.Common.Tools.OpenCover;

[PublicAPI]
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
