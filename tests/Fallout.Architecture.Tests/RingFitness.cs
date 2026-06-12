using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Shared assertion for onion-ring fitness (ADR-0006). Scans <paramref name="assemblies"/> for types residing in
/// <paramref name="ringNamespace"/> and asserts none of them depend on any of the <paramref name="forbidden"/>
/// namespaces.
/// <para>
/// It also fails if the namespace filter matches <b>zero</b> types: a NetArchTest rule over an empty type set
/// reports success, so a stale filter string (e.g. after a namespace rename) would silently turn the guard into a
/// no-op. The non-empty precondition keeps the gate honest.
/// </para>
/// </summary>
internal static class RingFitness
{
    public static void AssertNoDependencyOn(
        Assembly[] assemblies, string ringNamespace, string rationale, params string[] forbidden)
    {
        Types.InAssemblies(assemblies).That().ResideInNamespaceStartingWith(ringNamespace)
            .GetTypes().Should().NotBeEmpty(
                because: $"the '{ringNamespace}' filter must match real types — otherwise this fitness guard " +
                         "passes vacuously (the likely failure mode after a namespace rename)");

        var result = Types.InAssemblies(assemblies).That().ResideInNamespaceStartingWith(ringNamespace)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: rationale + " Offending types: " +
                     (result.FailingTypeNames is null ? "(none reported)" : string.Join(", ", result.FailingTypeNames)));
    }
}
