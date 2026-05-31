using System.Linq;
using System.Reflection;
using FluentAssertions;
using Fallout.Domain.Planning;
using NetArchTest.Rules;
using Xunit;

namespace Fallout.Domain.Tests;

/// <summary>
/// Fallout.Domain is the innermost onion ring (ADR-0006; originally issue #88's "reactor core"):
/// pure domain types and graph algorithms that touch no I/O and depend on nothing else in the repo.
/// These tests guard that invariant — the dependency rule every outer ring builds on.
/// </summary>
public class ArchitectureFitnessTests
{
    private static readonly Assembly DomainAssembly = typeof(TopoSort).Assembly;

    [Fact]
    public void Domain_has_no_io_process_console_or_logging_dependency()
    {
        // Scope to our own Fallout.* types only. This excludes build-tool noise injected into the
        // assembly that we don't author and can't keep pure: the generated `ThisAssembly`
        // (Nerdbank.GitVersioning, no namespace) and `Coverlet.Core.Instrumentation.Tracker.*`
        // (coverage instrumentation under `./build.ps1 Test`, which legitimately touches System.IO).
        // Precise tokens (e.g. "System.Diagnostics.Process") rather than the broad "System.Diagnostics"
        // namespace also avoid NetArchTest false-positives on generic types.
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespaceStartingWith("Fallout")
            .Should()
            .NotHaveDependencyOnAny(
                "System.IO",
                "System.Diagnostics.Process",
                "System.Console",
                "Serilog")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Fallout.Domain must stay pure; offending types: " + FailingTypes(result));
    }

    [Fact]
    public void Domain_does_not_depend_on_any_outer_ring()
    {
        // ADR-0006 onion rule: the innermost ring references no outer ring — and crucially none of the
        // dissolving `Fallout.Common.*` catch-all (the whole point of the realignment) nor the
        // Application/Infrastructure rings to come.
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespaceStartingWith("Fallout")
            .Should()
            .NotHaveDependencyOnAny(
                "Fallout.Common",
                "Fallout.Application",
                "Fallout.Infrastructure",
                "Fallout.Build",
                "Fallout.Components",
                "Fallout.Tooling",
                "Fallout.Utilities",
                "Fallout.ProjectModel")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Fallout.Domain is the innermost ring and must reference no other Fallout project; " +
                     "offending types: " + FailingTypes(result));
    }

    private static string FailingTypes(TestResult result) =>
        result.FailingTypeNames is null ? "(none reported)" : string.Join(", ", result.FailingTypeNames);
}
