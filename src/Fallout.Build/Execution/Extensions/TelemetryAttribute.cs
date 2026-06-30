using System;
using System.Collections.Generic;
using System.Linq;

namespace Fallout.Common.Execution;

internal class TelemetryAttribute : BuildExtensionAttributeBase, IOnBuildInitialized, IOnTargetSucceeded
{
    public void OnBuildInitialized(
        IReadOnlyCollection<ITargetModel> executableTargets,
        IReadOnlyCollection<ITargetModel> executionPlan)
    {
        if (Build.IsInterceptorExecution)
            return;

        Telemetry.BuildStarted(Build);
    }

    public void OnTargetSucceeded(ITargetModel target)
    {
        if (Build.IsInterceptorExecution)
            return;

        Telemetry.TargetSucceeded((ExecutableTarget)target, Build);
    }
}
