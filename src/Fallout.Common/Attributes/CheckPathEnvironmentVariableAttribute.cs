using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Application.Execution;
using Fallout.Infrastructure.Tooling;

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
