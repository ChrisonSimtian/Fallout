// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

namespace Fallout.Common.CI.TeamCity;

[PublicAPI]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum TeamCityStatus
{
    NORMAL,
    WARNING,
    ERROR,
    FAILURE
}
