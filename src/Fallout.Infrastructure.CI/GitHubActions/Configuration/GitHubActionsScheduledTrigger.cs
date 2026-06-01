using System;
using System.Linq;
using Fallout.Application.Utilities;

namespace Fallout.Infrastructure.CI.GitHubActions.Configuration;

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
