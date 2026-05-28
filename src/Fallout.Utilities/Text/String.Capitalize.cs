using System.Diagnostics;
using System.Globalization;

namespace Fallout.Common.Utilities;

[DebuggerNonUserCode]
[DebuggerStepThrough]
public static partial class StringExtensions
{
    /// <summary>
    /// Converts the first character of a given string to upper-case.
    /// </summary>
    public static string Capitalize(this string text)
    {
        return !text.IsNullOrEmpty()
            ? text.Substring(startIndex: 0, length: 1).ToUpper(CultureInfo.InvariantCulture) +
              text.Substring(startIndex: 1)
            : text;
    }
}
