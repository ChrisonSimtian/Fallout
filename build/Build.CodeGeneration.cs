using System;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Git;
using Fallout.Application.Tools.VersionControl.GitHub;
using Fallout.Common.Utilities.Collections;
using static Fallout.CodeGeneration.CodeGenerator;
using static Fallout.CodeGeneration.ReferenceUpdater;
using static Fallout.Application.Tools.VersionControl.Git.GitTasks;

partial class Build
{
    AbsolutePath SpecificationsDirectory => RootDirectory / "src" / "Tools";
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
            SpecificationsDirectory.GlobFiles("Fallout.Application.Tools.*/*/*.json").ForEach(x =>
                GenerateCode(
                    x,
                    namespaceProvider: x =>
                    {
                        // .../src/Tools/Fallout.Application.Tools.<Family>/<Tool>/<Tool>.json
                        AbsolutePath spec = x.SpecificationFile;
                        var familyAssembly = spec.Parent.Parent.Name;
                        var leaf = familyAssembly[(familyAssembly.LastIndexOf('.') + 1)..];
                        return x.Name == leaf ? familyAssembly : $"{familyAssembly}.{x.Name}";
                    },
                    sourceFileProvider: x => GitRepository.SetBranch(MainBranch).GetGitHubBrowseUrl(x.SpecificationFile)));
        });
}
