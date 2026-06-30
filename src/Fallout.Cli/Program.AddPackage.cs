using System;
using System.Linq;
using Fallout.Common;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;

namespace Fallout.Cli;

partial class Program
{
    public const string PACKAGE_TYPE_DOWNLOAD = "PackageDownload";
    public const string PACKAGE_TYPE_REFERENCE = "PackageReference";

    // Residual after the :add-package command moved to AddPackageCommand: this helper is shared with
    // cake and moves into a package service in the #392 collapse PR.
    internal static void AddOrReplacePackage(string packageId, string packageVersion, string packageType, string buildProjectFile)
    {
        var buildProject = ProjectModelTasks.ParseProject(buildProjectFile).NotNull();

        var previousPackage = buildProject.Items.SingleOrDefault(x => x.EvaluatedInclude == packageId);
        if (previousPackage != null)
            buildProject.RemoveItem(previousPackage);

        var packageDownloadItem = buildProject.AddItem(packageType, packageId).Single();
        packageDownloadItem.Xml.AddMetadata(
            "Version",
            packageType == PACKAGE_TYPE_REFERENCE ? packageVersion : $"[{packageVersion}]",
            expressAsAttribute: true);
        buildProject.Save();
    }
}
