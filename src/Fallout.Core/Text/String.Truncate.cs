using System;
using System.Linq;

namespace Fallout.Core;

partial class StringExtensions
{
    public static string Truncate(this string str, int maxChars)
    {
        return str.Length <= maxChars ? str : str.Substring(0, maxChars) + "…";
    }
}
