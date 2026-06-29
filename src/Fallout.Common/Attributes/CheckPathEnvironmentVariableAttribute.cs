using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Tooling;

namespace Fallout.Common.Execution;

public class CheckPathEnvironmentVariableAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    public void OnBuildInitialized(
        IReadOnlyCollection<ITargetModel> executableTargets,
        IReadOnlyCollection<ITargetModel> executionPlan)
    {
        ProcessTasks.CheckPathEnvironmentVariable();
    }
}
