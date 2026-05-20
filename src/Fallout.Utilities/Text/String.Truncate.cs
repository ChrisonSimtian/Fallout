// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Fallout.Common.Utilities;

partial class StringExtensions
{
    public static string Truncate(this string str, int maxChars)
    {
        return str.Length <= maxChars ? str : str.Substring(0, maxChars) + "…";
    }
}
