// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;

namespace Nuke.Components;

[PublicAPI]
public interface IHazConfiguration : INukeBuild
{
    [Parameter] Configuration Configuration => TryGetValue(() => Configuration) ??
                                               (IsLocalBuild ? Configuration.Debug : Configuration.Release);
}
