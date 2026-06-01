using System;
using System.Linq;
using Fallout.Application.CI;
using Fallout.Application.Utilities;

namespace Fallout.Infrastructure.CI.TeamCity.Configuration;

public class TeamCityBuildTypeVcsRoot : ConfigurationEntity
{
    public TeamCityVcsRoot Root { get; set; }
    public bool ShowDependenciesChanges { get; set; }
    public bool CleanCheckoutDirectory { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        using (writer.WriteBlock("vcs"))
        {
            writer.WriteLine($"root({Root.Id})");
            if (CleanCheckoutDirectory)
                writer.WriteLine("cleanCheckout = true");
            if (ShowDependenciesChanges)
                writer.WriteLine("showDependenciesChanges = true");
        }
    }
}
