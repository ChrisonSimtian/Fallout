using System;
using System.IO;
using Fallout.Cli.Commands;
using Fallout.Common.IO;
using FluentAssertions;
using Xunit;

namespace Fallout.Cli.Tests.Commands;

public class CompleteCommandTests
{
    [Fact]
    public void Name_IsComplete()
        => new CompleteCommand().Name.Should().Be("complete");

    [Fact]
    public void Execute_WithoutRootDirectory_ReturnsZero()
        => new CompleteCommand().Execute(["fallout "], rootDirectory: null, buildScript: null).Should().Be(0);

    [Fact]
    public void Execute_WordNotStartingWithCommandName_ReturnsZero()
    {
        var dir = (AbsolutePath)Path.Combine(Path.GetTempPath(), "fallout-complete-" + Guid.NewGuid().ToString("N"));
        dir.CreateDirectory();
        try
        {
            // Completion only fires for the `fallout` command line; anything else short-circuits to 0.
            new CompleteCommand().Execute(["notfallout foo"], dir, buildScript: null).Should().Be(0);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
