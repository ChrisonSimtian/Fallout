using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Application.Execution;

using Fallout.Application.CI;
namespace Fallout.Application.Tools.DotCover;

public class TeamCitySetDotCoverHomePathAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        CiHost.TeamCity?.SetConfigurationParameter("teamcity.dotCover.home", DotCoverTasks.DotCoverPath);
    }
}
