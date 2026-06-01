using System;
using System.Linq;
using Fallout.Application.Tooling;

namespace Fallout.Infrastructure.CI.GitHubActions;

public enum GitHubActionsTrigger
{
    [EnumValue("push")] Push,
    [EnumValue("pull_request")] PullRequest,
    [EnumValue("workflow_dispatch")] WorkflowDispatch
}
