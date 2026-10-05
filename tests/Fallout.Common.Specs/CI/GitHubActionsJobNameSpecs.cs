using System;
using System.Linq;
using Fallout.Common.CI;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.CI.GitHubActions.Configuration;
using Fallout.Common.Execution;
using Fallout.Common.Utilities;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Specs.CI;

public class GitHubActionsJobNameSpecs
{
    [Fact]
    public void Unset_job_name_uses_image_value()
    {
        var job = GetJobs(new[] { GitHubActionsImage.WindowsLatest }, jobName: null).Single();

        job.Name.Should().Be("windows-latest");
    }

    [Fact]
    public void Unset_job_name_keeps_dots_replaced_in_matrix()
    {
        var jobs = GetJobs(new[] { GitHubActionsImage.UbuntuLatest, GitHubActionsImage.Ubuntu2204 }, jobName: null);

        jobs.Select(x => x.Name).Should().Equal("ubuntu-latest", "ubuntu-22_04");
    }

    [Fact]
    public void Job_name_is_used_as_job_id_and_name()
    {
        var job = GetJobs(new[] { GitHubActionsImage.WindowsLatest }, "build-windows").Single();

        job.Name.Should().Be("build-windows");
        using var stream = new System.IO.MemoryStream();
        using (var streamWriter = new System.IO.StreamWriter(stream, leaveOpen: true))
        {
            job.Write(new CustomFileWriter(streamWriter, 2, "#"));
        }

        var output = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        output.Should().StartWith("build-windows:").And.Contain("name: build-windows");
    }

    [Fact]
    public void Job_name_with_multiple_images_throws()
    {
        var act = () => GetJobs(new[] { GitHubActionsImage.UbuntuLatest, GitHubActionsImage.WindowsLatest }, "build");

        act.Should().Throw<Exception>().WithMessage("*JobName*");
    }

    [Theory]
    [InlineData("build")]
    [InlineData("_build")]
    [InlineData("Build-1_x")]
    public void Valid_job_name_does_not_throw(string jobName)
    {
        var act = () => GetJobs(new[] { GitHubActionsImage.UbuntuLatest }, jobName);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1build")]
    [InlineData("-build")]
    [InlineData("build job")]
    [InlineData("build.job")]
    public void Invalid_job_name_throws(string jobName)
    {
        var act = () => GetJobs(new[] { GitHubActionsImage.UbuntuLatest }, jobName);

        act.Should().Throw<Exception>().WithMessage("*JobName*");
    }

    private static GitHubActionsJob[] GetJobs(GitHubActionsImage[] images, string jobName)
    {
        var build = new ConfigurationGenerationSpecs.TestBuild();
        var relevantTargets = ExecutableTargetFactory.CreateAll(build, x => x.Compile);
        var attribute = new TestGitHubActionsAttribute(images[0], images[1..])
                        {
                            On = new[] { GitHubActionsTrigger.Push },
                            InvokedTargets = new[] { nameof(ConfigurationGenerationSpecs.TestBuild.Test) },
                            JobName = jobName
                        };
        ((ConfigurationAttributeBase)attribute).Build = build;
        return ((GitHubActionsConfiguration)attribute.GetConfiguration(relevantTargets)).Jobs;
    }
}
