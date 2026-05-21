// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE


namespace Fallout.Common.Utilities;

public static partial class StringExtensions
{
    /// <summary>
    /// Indicates whether a specified string is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// Indicates whether a specified string is null, empty, or only white-space.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// Returns <value>null</value> if the specified string is empty.
    /// </summary>
    public static string ToNullIfEmpty(this string str)
    {
        return str.IsNullOrEmpty() ? null : str;
    }

    /// <summary>
    /// Returns <value>null</value> if the specified string is empty or only white-space.
    /// </summary>
    public static string ToNullIfWhiteSpace(this string str)
    {
        return str.IsNullOrWhiteSpace() ? null : str;
    }
}
