// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Nuke.Common.CI.TeamCity;

[PublicAPI]
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
