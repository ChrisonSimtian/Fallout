using System;
using System.Linq;
using static Fallout.Application.Tools.DotNet.DotNetTasks;
using Fallout.Application;
using Fallout.Application.Tools.DotNet;
using Fallout.Application.Tooling;
using Fallout.Kernel.IO;
using Fallout.Kernel.Collections;
using Fallout.Kernel;

namespace Fallout.Application.Components;

public interface IPack : ICompile, IHasArtifacts
{
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";

    Target Pack => _ => _
        .DependsOn(Compile)
        .Produces(PackagesDirectory / "*.nupkg")
        .Executes(() =>
        {
            DotNetPack(_ => _
                .Apply(PackSettingsBase)
                .Apply(PackSettings));

            ReportSummary(_ => _
                .AddPair("Packages", PackagesDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });

    sealed Configure<DotNetPackSettings> PackSettingsBase => _ => _
        .SetProject(Solution)
        .SetConfiguration(Configuration)
        .SetNoBuild(SucceededTargets.Contains(Compile))
        .SetOutputDirectory(PackagesDirectory)
        .WhenNotNull(this as IHasGitRepository, (_, o) => _
            .SetRepositoryUrl(o.GitRepository.HttpsUrl))
        .WhenNotNull(this as IHasGitVersion, (_, o) => _
            .SetVersion(o.Versioning.NuGetVersionV2))
        .WhenNotNull(this as IHasNerdbankGitVersioning, (_, o) => _
            .SetVersion(o.Versioning.NuGetPackageVersion))
        .WhenNotNull(this as IHasChangelog, (_, o) => _
            .SetPackageReleaseNotes(o.NuGetReleaseNotes));

    Configure<DotNetPackSettings> PackSettings => _ => _;
}
