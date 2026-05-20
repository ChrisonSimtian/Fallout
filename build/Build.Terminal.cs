// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Fallout.Common;
using Fallout.Common.Execution;

[DisableDefaultOutput<Terminal>(
    DefaultOutput.Logo,
    DefaultOutput.Timestamps,
    DefaultOutput.TargetHeader,
    DefaultOutput.ErrorsAndWarnings,
    DefaultOutput.TargetOutcome,
    DefaultOutput.BuildOutcome)]
partial class Build
{
}

public class DisableDefaultOutputAttribute<T> : DisableDefaultOutputAttribute
    where T : Host
{
    public DisableDefaultOutputAttribute(params DefaultOutput[] disabledOutputs)
        : base(disabledOutputs)
    {
    }

    public override bool IsApplicable(INukeBuild build) => build.Host is T;
}
