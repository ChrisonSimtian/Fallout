using System;
using System.Collections.Generic;
using System.Linq;

namespace Fallout.Common.Execution;

internal class EventInvoker : BuildExtensionAttributeBase,
    IOnBuildCreated,
    IOnBuildInitialized,
    IOnTargetRunning,
    IOnTargetSkipped,
    IOnTargetSucceeded,
    IOnTargetFailed,
    IOnBuildFinished
{
    public void OnBuildCreated(IReadOnlyCollection<ITargetModel> executableTargets)
    {
        ((FalloutBuild)Build).OnBuildCreated();
    }

    public void OnBuildInitialized(
        IReadOnlyCollection<ITargetModel> executableTargets,
        IReadOnlyCollection<ITargetModel> executionPlan)
    {
        ((FalloutBuild)Build).OnBuildInitialized();
    }

    public void OnTargetRunning(ITargetModel target)
    {
        ((FalloutBuild)Build).OnTargetRunning(target.Name);
    }

    public void OnTargetSkipped(ITargetModel target)
    {
        ((FalloutBuild)Build).OnTargetSkipped(target.Name);
    }

    public void OnTargetSucceeded(ITargetModel target)
    {
        ((FalloutBuild)Build).OnTargetSucceeded(target.Name);
    }

    public void OnTargetFailed(ITargetModel target)
    {
        ((FalloutBuild)Build).OnTargetFailed(target.Name);
    }

    public void OnBuildFinished()
    {
        ((FalloutBuild)Build).OnBuildFinished();
    }
}
