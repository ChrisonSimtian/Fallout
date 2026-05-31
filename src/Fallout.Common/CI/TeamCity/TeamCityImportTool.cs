using System;
using System.Linq;

namespace Fallout.Infrastructure.CI.TeamCity;

public enum TeamCityImportTool
{
    /// <summary>dotCover reports</summary>
    dotcover,

    /// <summary>PartCover reports</summary>
    partcover,

    /// <summary>NCover reports</summary>
    ncover,

    /// <summary>NCover3 reports</summary>
    ncover3
}
