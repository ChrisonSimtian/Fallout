// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;

namespace Fallout.Common.Utilities;

[PublicAPI]
[DebuggerNonUserCode]
[DebuggerStepThrough]
public static partial class StringExtensions
{
    /// <summary>
    /// Converts the first character of a given string to upper-case.
    /// </summary>
    [Pure]
    public static string Capitalize(this string text)
    {
        return !text.IsNullOrEmpty()
            ? text.Substring(startIndex: 0, length: 1).ToUpper(CultureInfo.InvariantCulture) +
              text.Substring(startIndex: 1)
            : text;
    }
}
