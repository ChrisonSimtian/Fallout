// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using JetBrains.Annotations;
using System.Text.RegularExpressions;

namespace Nuke.Common.Utilities;

public static partial class StringExtensions
{
    /// <summary>
    /// Returns the first index of a given regular expression.
    /// </summary>
    [Pure]
    public static int IndexOfRegex(this string text, [RegexPattern] string expression)
    {
        var regex = new Regex(expression, RegexOptions.Compiled);
        return regex.Match(text).Index;
    }
}
