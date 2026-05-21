// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Fallout.Common.Tooling;

namespace Fallout.Common.CI.GitHubActions;

public enum GitHubActionsTrigger
{
    [EnumValue("push")] Push,
    [EnumValue("pull_request")] PullRequest,
    [EnumValue("workflow_dispatch")] WorkflowDispatch
}
