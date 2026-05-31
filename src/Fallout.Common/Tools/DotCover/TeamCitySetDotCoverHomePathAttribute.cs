using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.CI.TeamCity;
using Fallout.Application.Execution;

namespace Fallout.Application.Tools.DotCover;

public class TeamCitySetDotCoverHomePathAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        TeamCity.Instance?.SetConfigurationParameter("teamcity.dotCover.home", DotCoverTasks.DotCoverPath);
    }
}
