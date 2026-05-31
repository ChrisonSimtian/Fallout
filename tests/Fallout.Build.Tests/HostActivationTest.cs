using FluentAssertions;
using Xunit;

namespace Fallout.Common.Tests;

// ADR-0005 (#3): host discovery probes the overridable Host.IsActive member rather than the
// magic-string IsRunning{TypeName} convention. These hosts are private-nested, so they report
// IsPublic == false and are excluded from the global Host discovery scan — no pollution.
public class HostActivationTest
{
    // New path: an adapter overrides IsActive directly — no IsRunning{Name} static required.
    private sealed class OverrideHost : Host
    {
        protected internal override bool IsActive => true;
        public bool Probe() => IsActive;
    }

    // Legacy path: no override; the default IsActive must fall back to the IsRunning{TypeName} static.
    private sealed class LegacyHost : Host
    {
        public static bool IsRunningLegacyHost => true;
        public bool Probe() => IsActive;
    }

    [Fact]
    public void OverriddenIsActive_NeedsNoLegacyStatic()
    {
        new OverrideHost().Probe().Should().BeTrue();
    }

    [Fact]
    public void DefaultIsActive_FallsBackToLegacyConvention()
    {
        new LegacyHost().Probe().Should().BeTrue();
    }
}
