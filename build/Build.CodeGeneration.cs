using System;
using static Fallout.CodeGeneration.CodeGenerator;
using static Fallout.CodeGeneration.ReferenceUpdater;
using static Fallout.Application.Tools.Git.GitTasks;
using Fallout.Application;
using Fallout.Application.Tools.Git;
using Fallout.Application.Tools.GitHub;
using Fallout.Kernel.IO;
using Fallout.Kernel.Collections;

partial class Build
{
    AbsolutePath SpecificationsDirectory => RootDirectory / "src" / "Fallout.Common" / "Tools";
    AbsolutePath ReferencesDirectory => RootDirectory / "docs" / "cli-tools";

    Target References => _ => _
        .Requires(() => GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            ReferencesDirectory.CreateOrCleanDirectory();

            UpdateReferences(SpecificationsDirectory, ReferencesDirectory);
        });

    Target GenerateTools => _ => _
        .Executes(() =>
        {
            SpecificationsDirectory.GlobFiles("*/*.json").ForEach(x =>
                GenerateCode(
                    x,
                    namespaceProvider: x => $"Fallout.Common.Tools.{x.Name}",
                    sourceFileProvider: x => GitRepository.SetBranch(MainBranch).GetGitHubBrowseUrl(x.SpecificationFile)));
        });
}
