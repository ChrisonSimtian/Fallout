// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using FluentAssertions;
using Fallout.Common.IO;
using Fallout.Common.ProjectModel;
using Xunit;

namespace Fallout.Common.Tests;

public class ProjectModelTest
{
    private static AbsolutePath RootDirectory => Constants.TryGetRootDirectoryFrom(EnvironmentInfo.WorkingDirectory).NotNull();

    private static AbsolutePath SolutionFile => RootDirectory / "fallout.slnx";

    [Fact]
    public void ProjectTest()
    {
        var solution = SolutionFile.ReadSolution();
        var project = solution.Projects.Single(x => x.Name == "Fallout.ProjectModel");

        var action = new Action(() => project.GetMSBuildProject());
        action.Should().NotThrow();

        project.GetTargetFrameworks().Should().Equal("net8.0", "net9.0", "net10.0");
        project.HasPackageReference("Microsoft.Build.Locator").Should().BeTrue();
        project.GetPackageReferenceVersion("Microsoft.Build.Locator").Should().Be("1.7.8");
        project.GetPackageReferenceVersion("Microsoft.Build").Should().Be("18.0.2");
    }

    [Fact]
    public void MSBuildProjectTest()
    {
        var solution = SolutionFile.ReadSolution();
        var project = solution.Projects.Single(x => x.Name == "Fallout.ProjectModel");

        var msbuildProject = project.GetMSBuildProject(targetFramework: "net8.0");

        var package = msbuildProject.GetItems("PackageVersion").FirstOrDefault(x => x.EvaluatedInclude == "Microsoft.Build");
        package.Should().NotBeNull();
        package.GetMetadataValue("Version").Should().Be("17.11.48");
    }
}
