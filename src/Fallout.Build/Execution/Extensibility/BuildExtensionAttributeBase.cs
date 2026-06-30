using System;

namespace Fallout.Common.Execution;

[AttributeUsage(AttributeTargets.Class)]
public abstract class BuildExtensionAttributeBase : Attribute, IBuildExtension
{
    public IFalloutBuild Build { get; internal set; }
    public virtual float Priority { get; set; }
}

public interface IOnTargetSummaryUpdated : IBuildExtension
{
    void OnTargetSummaryUpdated(IFalloutBuild build, ExecutableTarget target);
}
