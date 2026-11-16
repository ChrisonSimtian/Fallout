using System;
using System.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Specs;

public class MSBuildLocatorSpecs
{
    // The test host registers MSBuild early, which hides #745. The fixture runs as its own
    // process, so Microsoft.Build is only found if the build engine registers MSBuildLocator
    // before the target is compiled.
    [Fact]
    public void Target_loads_an_msbuild_project_in_a_fresh_process()
    {
        // Arrange
        var root = Constants.TryGetRootDirectoryFrom(EnvironmentInfo.WorkingDirectory).NotNull();
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(root / "tests" / "Fixtures" / "MSBuildLocatorFixture");
        startInfo.ArgumentList.Add("--no-launch-profile");

        // Act
        using var process = Process.Start(startInfo).NotNull();
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        var exited = process.WaitForExit(TimeSpan.FromMinutes(5));

        // Assert
        exited.Should().BeTrue();
        process.ExitCode.Should().Be(0, "{0}{1}", output.Result, error.Result);
    }
}
