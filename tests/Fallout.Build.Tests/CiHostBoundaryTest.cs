using System.Linq;
using Fallout.Common.CI;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Tests;

// ADR-0005: CI host integration is ports-and-adapters. The ports + kernel (Fallout.Build) must not
// depend on the adapters (Fallout.Common, where GitHubActions et al. live) — dependencies point
// inward only. This test holds that boundary; it is the construction-time guarantee that lets the
// seam be exposed publicly in the plugin SDK (milestone #7).
public class CiHostBoundaryTest
{
    [Fact]
    public void PortsAssembly_DoesNotReference_AdaptersAssembly()
    {
        var portsAssembly = typeof(IBuildHost).Assembly;

        // Guard: make sure we're actually inspecting the ports assembly and not something else,
        // so the assertion below can't pass vacuously.
        portsAssembly.GetName().Name.Should().Be("Fallout.Build");

        portsAssembly.GetReferencedAssemblies()
            .Select(x => x.Name)
            .Should().NotContain("Fallout.Common", "the ports layer must not depend on the adapters layer (ADR-0005)");
    }
}
