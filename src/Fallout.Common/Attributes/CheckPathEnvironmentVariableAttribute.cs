using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Tooling;
using Fallout.Application.Execution;

namespace Fallout.Common.Execution;

public class CheckPathEnvironmentVariableAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        ProcessTasks.CheckPathEnvironmentVariable();
    }
}
