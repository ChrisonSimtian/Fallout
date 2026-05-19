// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.TeamCity.Configuration;

[PublicAPI]
public class TeamCityVcsTrigger : TeamCityTrigger
{
    public string[] BranchFilters { get; set; }
    public string[] TriggerRules { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        using (writer.WriteBlock("vcs"))
        {
            writer.WriteArray("branchFilter", BranchFilters);
            writer.WriteArray("triggerRules", TriggerRules);
        }
    }
}
