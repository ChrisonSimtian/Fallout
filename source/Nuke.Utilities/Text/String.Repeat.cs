// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using JetBrains.Annotations;

namespace Nuke.Common.Utilities;

public static partial class StringExtensions
{
    [Pure]
    public static string Repeat(this char ch, int count)
    {
        return new string(ch, count);
    }
}
