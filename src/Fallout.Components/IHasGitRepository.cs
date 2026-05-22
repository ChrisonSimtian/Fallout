// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Fallout.Common;
using Fallout.Common.Git;

namespace Fallout.Components;

public interface IHasGitRepository : IFalloutBuild
{
    [GitRepository] [Required] GitRepository GitRepository => TryGetValue(() => GitRepository);
}
