// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Fallout.Common.Tools.InnoSetup;

public partial class InnoSetupSettings
{
    private static string GetInnoSetupBool(bool? value)
    {
        return value switch
        {
            null => null,
            true => "+",
            false => "-"
        };
    }

    private string GetOutput()
    {
        return GetInnoSetupBool(Output);
    }
}
