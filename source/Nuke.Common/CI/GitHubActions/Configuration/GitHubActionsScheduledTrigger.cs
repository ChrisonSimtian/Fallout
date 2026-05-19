// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.GitHubActions.Configuration;

[PublicAPI]
public class GitHubActionsScheduledTrigger : GitHubActionsDetailedTrigger
{
    public string Cron { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("schedule:");
        using (writer.Indent())
        {
            writer.WriteLine($"- cron: '{Cron}'");
        }
    }
}
