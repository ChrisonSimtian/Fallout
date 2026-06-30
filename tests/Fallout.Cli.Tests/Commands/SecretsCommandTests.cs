using System;
using Fallout.Cli.Commands;
using FluentAssertions;
using Xunit;

namespace Fallout.Cli.Tests.Commands;

public class SecretsCommandTests
{
    [Fact]
    public void Name_IsSecrets()
        => new SecretsCommand(new FakeConsolePrompts()).Name.Should().Be("secrets");

    [Fact]
    public void Execute_WithoutRootDirectory_Throws()
    {
        var action = () => new SecretsCommand(new FakeConsolePrompts())
            .Execute([], rootDirectory: null, buildScript: null);

        action.Should().Throw<Exception>().WithMessage("*No root directory*");
    }
}
