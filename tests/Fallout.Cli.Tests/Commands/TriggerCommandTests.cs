using System;
using System.IO;
using Fallout.Cli.Commands;
using Fallout.Common.IO;
using FluentAssertions;
using Xunit;

namespace Fallout.Cli.Tests.Commands;

public class TriggerCommandTests
{
    [Fact]
    public void Name_IsTrigger()
        => new TriggerCommand().Name.Should().Be("trigger");

    [Fact]
    public void Execute_OutsideGitRepository_Throws()
    {
        var dir = (AbsolutePath)Path.Combine(Path.GetTempPath(), "fallout-trigger-" + Guid.NewGuid().ToString("N"));
        dir.CreateDirectory();
        try
        {
            var action = () => new TriggerCommand().Execute(["a message"], dir, buildScript: null);

            // No resolvable Git repository at a throwaway temp dir → the command fails rather than
            // pushing anything.
            action.Should().Throw<Exception>();
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
