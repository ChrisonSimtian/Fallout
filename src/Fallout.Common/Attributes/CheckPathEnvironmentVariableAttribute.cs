using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Application.Execution;
using Fallout.Application.Tooling;

namespace Fallout.Application.Execution;

public class CheckPathEnvironmentVariableAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        ToolingServices.Process.CheckPathEnvironmentVariable();
    }
}
