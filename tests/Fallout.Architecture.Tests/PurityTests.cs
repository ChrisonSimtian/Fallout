using Xunit;
using Arch = Fallout.Architecture.Tests.FalloutArchitecture;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Purity invariants — what an assembly is allowed to reach for in the BCL / third-party world.
/// </summary>
public class PurityTests
{
    // Anything under System.IO, the System.Diagnostics.Process API, the console, or Serilog. Matched against the
    // dependency type's full name (anchored alternation). Mirrors the original NetArchTest guard from issue #88.
    private const string ImpureSurface =
        @"^(?:System\.IO\.|System\.Diagnostics\.Process|System\.Console|Serilog(?:\.|$))";

    [Fact]
    public void Core_stays_pure_no_io_process_console_or_logging() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.Core)
                .Should().NotDependOnAnyTypesThat().HaveFullNameMatching(ImpureSurface),
            "Fallout.Core is held to a strict purity bar — no System.IO, Process, Console, or Serilog (issue #88)",
            KnownViolations.None);
}
