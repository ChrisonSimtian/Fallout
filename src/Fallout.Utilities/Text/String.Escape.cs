// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Fallout.Common.Utilities;

public static partial class StringExtensions
{
    public static string EscapeBraces(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return string.Empty;

        return str.NotNull().Replace("{", "{{").Replace("}", "}}");
    }
}
