// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common;
using Fallout.Common.Tools.NerdbankGitVersioning;

namespace Fallout.Components;

[PublicAPI]
public interface IHazNerdbankGitVersioning : INukeBuild
{
    [NerdbankGitVersioning] [Required] NerdbankGitVersioning Versioning => TryGetValue(() => Versioning);
}
