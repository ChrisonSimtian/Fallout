using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Core.IO.Globbing;
using static Fallout.CodeGeneration.CodeGenerator;
using static Fallout.CodeGeneration.ReferenceUpdater;
using static Fallout.Application.Tools.Git.GitTasks;
using Fallout.Application;
using Fallout.Application.Tools.Git;
using Fallout.Application.Tools.GitHub;
using Fallout.Core.IO;
using Fallout.Core.Collections;

partial class Build
{
    AbsolutePath SpecificationsDirectory => RootDirectory / "src" / "Fallout.Application.Tools";
    AbsolutePath ReferencesDirectory => RootDirectory / "docs" / "cli-tools";

    // Tool specs live one level down as <Tool>/<Tool>.json. SpecificationsDirectory is the
    // Fallout.Application.Tools project root, so exclude build output (obj/bin) whose *.json
    // artifacts would otherwise be picked up as specs.
    IEnumerable<AbsolutePath> SpecificationFiles =>
        SpecificationsDirectory.GlobFiles("*/*.json")
            .Where(x => x.Parent.Name is not ("obj" or "bin"));

    Target References => _ => _
        .Requires(() => GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            ReferencesDirectory.CreateOrCleanDirectory();

            UpdateReferences(SpecificationFiles.Select(x => (string)x), ReferencesDirectory);
        });

    Target GenerateTools => _ => _
        .Executes(() =>
        {
            SpecificationFiles.ForEach(x =>
                GenerateCode(
                    x,
                    namespaceProvider: x => $"Fallout.Application.Tools.{x.Name}",
                    sourceFileProvider: x => GitRepository.SetBranch(MainBranch).GetGitHubBrowseUrl(x.SpecificationFile)));
        });
}
