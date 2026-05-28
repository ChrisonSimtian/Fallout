using System;
using System.Linq;
using NuGet.Versioning;
using Fallout.Common;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;

namespace Fallout.Cli;

public static class ProjectUpdater
{
    public static void Update(string projectFile)
    {
        var buildProject = ProjectModelTasks.ParseProject(projectFile).NotNull();

        UpdateTargetFramework(buildProject);
        UpdateNukeCommonPackage(buildProject, out var previousPackageVersion);

        if (previousPackageVersion.MinVersion >= NuGetVersion.Parse("0.23.5"))
            RemoveLegacyFileIncludes(buildProject);

        buildProject.Save();
    }

    private static void UpdateTargetFramework(Microsoft.Build.Evaluation.Project buildProject)
    {
        buildProject.SetProperty("TargetFramework", "net8.0");
    }

    private static void UpdateNukeCommonPackage(Microsoft.Build.Evaluation.Project buildProject, out FloatRange previousPackageVersion)
    {
        var packageItem = buildProject.Items.SingleOrDefault(x => x.EvaluatedInclude == Constants.FalloutCommonPackageId).NotNull();
        previousPackageVersion = FloatRange.Parse(packageItem.GetMetadataValue("Version"));

        var latestPackageVersion = NuGetVersionResolver.GetLatestVersion(Constants.FalloutCommonPackageId, includePrereleases: false).GetAwaiter().GetResult();
        if (previousPackageVersion.Satisfies(NuGetVersion.Parse(latestPackageVersion)))
            return;

        packageItem.SetMetadataValue("Version", latestPackageVersion);
    }

    private static void RemoveLegacyFileIncludes(Microsoft.Build.Evaluation.Project buildProject)
    {
        var legacyIncludes =
            new[]
            {
                "csproj.DotSettings",
                "build.ps1",
                "build.sh",
                ".nuke",
                "global.json",
                "nuget.config",
                "azure-pipelines.yml",
                "Jenkinsfile",
                "appveyor.yml",
                ".travis.yml",
                "GitVersion.yml"
            };

        buildProject.Xml.Items
            .Where(x => x.ItemType == "None").ToList()
            .Where(x => x.Include.ContainsAnyOrdinalIgnoreCase(legacyIncludes) ||
                        x.Remove.ContainsAnyOrdinalIgnoreCase(legacyIncludes)).ToList()
            .ForEach(x =>
            {
                var itemGroupElement = x.Parent;
                itemGroupElement.RemoveChild(x);
                if (itemGroupElement.Children.Count == 0)
                    itemGroupElement.Parent.RemoveChild(itemGroupElement);
            });
    }
}
