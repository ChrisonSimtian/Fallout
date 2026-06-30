using System;
using System.IO;
using Fallout.Cli.Commands;
using Fallout.Common.IO;
using FluentAssertions;
using Xunit;

namespace Fallout.Cli.Tests.Commands;

public class UpdateCommandTests
{
    [Fact]
    public void Name_IsUpdate()
        => new UpdateCommand(new FakeConsolePrompts()).Name.Should().Be("update");

    [Fact]
    public void Execute_NoBuildScript_DeclineAll_ReturnsZeroAndReportsCompletion()
    {
        var dir = (AbsolutePath)Path.Combine(Path.GetTempPath(), "fallout-update-" + Guid.NewGuid().ToString("N"));
        dir.CreateDirectory();
        var prompts = new FakeConsolePrompts { InvokeConfirmedActions = false };
        try
        {
            // No build script and every confirmation declined → no update steps run, but the command
            // still completes cleanly.
            new UpdateCommand(prompts).Execute([], dir, buildScript: null).Should().Be(0);

            prompts.Completions.Should().Contain("Updates");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
