// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Nuke.Common.Tools.Unity.Logging;

internal enum MatchType
{
    None = 0,
    Inclusive = 1,
    Exclusive = 2
}
