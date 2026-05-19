// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Tooling;

namespace Nuke.Common.CI.GitHubActions;

[PublicAPI]
public enum GitHubActionsTrigger
{
    [EnumValue("push")] Push,
    [EnumValue("pull_request")] PullRequest,
    [EnumValue("workflow_dispatch")] WorkflowDispatch
}
