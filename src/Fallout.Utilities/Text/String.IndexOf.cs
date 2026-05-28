using System.Text.RegularExpressions;

namespace Fallout.Common.Utilities;

public static partial class StringExtensions
{
    /// <summary>
    /// Returns the first index of a given regular expression.
    /// </summary>
    public static int IndexOfRegex(this string text, string expression)
    {
        var regex = new Regex(expression, RegexOptions.Compiled);
        return regex.Match(text).Index;
    }
}
