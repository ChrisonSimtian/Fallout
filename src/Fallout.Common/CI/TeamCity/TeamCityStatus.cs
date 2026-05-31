using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Fallout.Infrastructure.CI.TeamCity;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum TeamCityStatus
{
    NORMAL,
    WARNING,
    ERROR,
    FAILURE
}
