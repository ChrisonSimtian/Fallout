using Fallout.Common.CI;
using Fallout.Common.CI.GitHubActions;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Tests;

// ADR-0005: a CI host adapter satisfies BOTH runtime-host ports — context (IBuildHost) and
// reporting (IBuildReporter, inherited from the Host base). GitHubActions is the canonical adapter.
public class CiHostPortsTest
{
    [Fact]
    public void GitHubActions_ImplementsBothRuntimeHostPorts()
    {
        typeof(IBuildHost).IsAssignableFrom(typeof(GitHubActions)).Should().BeTrue();
        typeof(IBuildReporter).IsAssignableFrom(typeof(GitHubActions)).Should().BeTrue();
    }
}
