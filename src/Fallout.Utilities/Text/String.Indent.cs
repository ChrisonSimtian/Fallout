// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using JetBrains.Annotations;

namespace Fallout.Common.Utilities;

public static partial class StringExtensions
{
    [Pure]
    public static string Indent(this string text, int count)
    {
        return ' '.Repeat(count) + text;
    }
}
