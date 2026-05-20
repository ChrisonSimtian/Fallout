// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using FluentAssertions;
using Fallout.Common.IO;
using Fallout.Common.ProjectModel;
using Fallout.Common.Utilities;
using Xunit;

namespace Fallout.Common.Tests;

public class SolutionModelTest
{
    private static AbsolutePath RootDirectory => Constants.TryGetRootDirectoryFrom(EnvironmentInfo.WorkingDirectory).NotNull();

    private static AbsolutePath SolutionFile => RootDirectory / "fallout.slnx";

    [Fact]
    public void SolutionTest()
    {
        var solution = SolutionFile.ReadSolution();

        solution.SolutionFolders.Select(x => x.Name).Should().BeEquivalentTo("misc");

        var buildProject = solution.AllProjects.SingleOrDefault(x => x.Name == "_build");
        buildProject.Should().NotBeNull();

        // solution.SaveAs(solution.Path + ".bak");
    }

    [Fact]
    public void SolutionGetProjectsTest()
    {
        var solution = SolutionFile.ReadSolution();

        solution.GetAllProjects("*.Tests").Should().HaveCountGreaterOrEqualTo(2);
    }
}
