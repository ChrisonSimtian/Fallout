using System;
using System.Reflection;

namespace Fallout.Application.Tools.CorFlags;

partial class CorFlagsSettings
{
    string FormatBoolean(bool? value, PropertyInfo property)
        => value switch
        {
            true => "+",
            false => "-",
            null => null
        };
}
