using Fallout.Cli.Commands;
using FluentAssertions;
using Xunit;

namespace Fallout.Cli.Tests.Commands;

public class CakeCleanCommandTests
{
    [Fact]
    public void Name_IsCakeClean()
        => new CakeCleanCommand(new FakeConsolePrompts()).Name.Should().Be("cake-clean");

    [Fact]
    public void Execute_WhenDeletionDeclined_ReturnsZeroAndDeletesNothing()
    {
        // ConfirmationResult = false → the "Delete?" prompt is declined, so no .cake files are removed.
        var prompts = new FakeConsolePrompts { ConfirmationResult = false };

        new CakeCleanCommand(prompts).Execute([], rootDirectory: null, buildScript: null).Should().Be(0);
    }
}
