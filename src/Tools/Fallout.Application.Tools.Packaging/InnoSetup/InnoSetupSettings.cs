using System;
using System.Linq;

using Fallout.Common;
namespace Fallout.Application.Tools.Packaging.InnoSetup;

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
