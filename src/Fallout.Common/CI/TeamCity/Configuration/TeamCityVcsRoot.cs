// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.Utilities;

namespace Fallout.Common.CI.TeamCity.Configuration;

[PublicAPI]
public class TeamCityVcsRoot : ConfigurationEntity
{
    public string Id => "DslContext.settingsRoot";

    public override void Write(CustomFileWriter writer)
    {
    }
}
