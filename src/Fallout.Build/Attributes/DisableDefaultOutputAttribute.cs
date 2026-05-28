using System;
using System.Linq;

namespace Fallout.Common;

[AttributeUsage(AttributeTargets.Class)]
public class DisableDefaultOutputAttribute : Attribute
{
    public DisableDefaultOutputAttribute(params DefaultOutput[] disabledOutputs)
    {
        DisabledOutputs = disabledOutputs.Length > 0
            ? disabledOutputs
            : Enum.GetValues(typeof(DefaultOutput)).Cast<DefaultOutput>().ToArray();
    }

    public DefaultOutput[] DisabledOutputs { get; }

    public virtual bool IsApplicable(IFalloutBuild build)
    {
        return true;
    }
}

public enum DefaultOutput
{
    Logo,
    TargetHeader,
    TargetCollapse,
    ErrorsAndWarnings,
    TargetOutcome,
    BuildOutcome,
    Timestamps
}
