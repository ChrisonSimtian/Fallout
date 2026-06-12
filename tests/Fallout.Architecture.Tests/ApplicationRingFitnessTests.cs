using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Onion fitness for the Application ring (ADR-0006). The tool/CI/build vocabulary in the
/// <c>Fallout.Application.*</c> namespaces must not depend on the outer <c>Fallout.Infrastructure.*</c> ring:
/// the impure process/resolver implementations are reached only through the ports in
/// <c>Fallout.Application.Tooling</c> (see <c>ToolingServices</c>), with the concrete adapters registered from
/// Infrastructure via a module initializer. This guards the inversion that the tool-layer split (step 4b)
/// deferred. (Houses ring fitness in its own project so loading every ring's assembly cannot perturb
/// load-order-sensitive tests elsewhere, e.g. the schema/AppDomain-scan tests in Fallout.Application.Tests.)
/// </summary>
public class ApplicationRingFitnessTests
{
    // Application-ring types are spread across five assemblies: Fallout.Application (root/Execution/CI/IO),
    // Fallout.Application.Components (the ICompile/ITest/IPack composition interfaces), Fallout.Application.Tooling
    // (tool vocabulary + ports), Fallout.Application.Tools (the 60+ tool wrappers), and Fallout.Application.Solutions
    // (solution model + ports — step 5c; the co-hosted Infrastructure.Solutions serializer adapter is a separate
    // assembly, and the namespace clause below scopes the scan to Fallout.Application.* regardless).
    private static readonly Assembly[] ApplicationAssemblies =
    [
        typeof(global::Fallout.Application.FalloutBuild).Assembly,
        typeof(global::Fallout.Application.Components.ICompile).Assembly,
        typeof(global::Fallout.Application.Tooling.ToolTasks).Assembly,
        typeof(global::Fallout.Application.Tools.DotNet.DotNetTasks).Assembly,
        typeof(global::Fallout.Application.Solutions.Solution).Assembly,
    ];

    [Fact]
    public void Application_ring_does_not_depend_on_Infrastructure()
    {
        var result = Types.InAssemblies(ApplicationAssemblies)
            .That().ResideInNamespaceStartingWith("Fallout.Application")
            .Should()
            .NotHaveDependencyOn("Fallout.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "the Application ring must reach Infrastructure only through Fallout.Application.Tooling " +
                     "ports (ToolingServices); offending types: " + FailingTypes(result));
    }

    private static string FailingTypes(TestResult result) =>
        result.FailingTypeNames is null ? "(none reported)" : string.Join(", ", result.FailingTypeNames);
}
