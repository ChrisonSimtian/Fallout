// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE


namespace Fallout.Common.Utilities;

public static partial class StringExtensions
{
    public static string Indent(this string text, int count)
    {
        return ' '.Repeat(count) + text;
    }
}
