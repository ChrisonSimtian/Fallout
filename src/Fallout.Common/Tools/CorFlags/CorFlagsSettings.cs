// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Reflection;

namespace Fallout.Common.Tools.CorFlags;

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
