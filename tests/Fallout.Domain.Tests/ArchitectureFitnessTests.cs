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
        // ADR-0006 onion rule: the innermost ring references no outer ring — not the Core kernel ring, the
        // Application/Infrastructure rings, nor the Cli composition root. We deliberately do NOT list the
        // pre-realignment names (Fallout.Common/Build/Components/Utilities/ProjectModel): those namespaces no
        // longer exist, so guarding them is dead weight — and "Fallout.Build" would false-positive against the
        // real, allowed `Fallout.Build.Shared` project (NetArchTest matches by namespace-segment prefix).
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespaceStartingWith("Fallout")
            .Should()
            .NotHaveDependencyOnAny(
                "Fallout.Core",
                "Fallout.Application",
                "Fallout.Infrastructure",
                "Fallout.Cli")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Fallout.Domain is the innermost ring and must reference no other Fallout project; " +
                     "offending types: " + FailingTypes(result));
    }

    [Fact]
    public void Fitness_scan_is_not_vacuous() =>
        // Both rules above scan `Fallout`-namespaced types in the Domain assembly. A NetArchTest rule over an
        // empty type set reports success, so if that filter ever matched nothing (e.g. after a namespace rename)
        // the purity/ring guards would silently become no-ops. Assert the scan is non-empty so it can't.
        Types.InAssembly(DomainAssembly).That().ResideInNamespaceStartingWith("Fallout")
            .GetTypes().Should().NotBeEmpty(
                because: "the Domain fitness filter must match real types, else the purity/ring guards pass vacuously");

    private static string FailingTypes(TestResult result) =>
        result.FailingTypeNames is null ? "(none reported)" : string.Join(", ", result.FailingTypeNames);
}
