using System;

using Fallout.Common;
namespace Fallout.Application.Tools.DotNet.CorFlags;

partial class CorFlagsSettings
{
    private static string FormatBoolean(bool? value)
        => value switch
        {
            true => "+",
            false => "-",
            null => null
        };
}
