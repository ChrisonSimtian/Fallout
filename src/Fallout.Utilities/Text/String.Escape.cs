using System;
using System.Linq;
using Fallout.Common;

namespace Fallout.Kernel;

public static partial class StringExtensions
{
    public static string EscapeBraces(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return string.Empty;

        return str.NotNull().Replace("{", "{{").Replace("}", "}}");
    }
}
