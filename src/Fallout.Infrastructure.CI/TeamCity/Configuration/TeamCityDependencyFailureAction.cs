using System;
using System.Linq;

namespace Fallout.Infrastructure.CI.TeamCity.Configuration;

public enum TeamCityDependencyFailureAction
{
    // TODO: add description from web UI
    AddProblem,
    FailToStart,
    Ignore,
    Cancel
}
