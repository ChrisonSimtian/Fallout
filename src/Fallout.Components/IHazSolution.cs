// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Fallout.Common;
using Fallout.Common.ProjectModel;

namespace Fallout.Components;

public interface IHazSolution : IFalloutBuild
{
    [Solution] [Required] Solution Solution => TryGetValue(() => Solution);
}
