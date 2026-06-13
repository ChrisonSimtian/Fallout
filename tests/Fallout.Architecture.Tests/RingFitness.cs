using System.Linq;
using FluentAssertions;
using NetArchTest.Rules;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Asserts a ring depends only within its declared layer (see <see cref="RingModel"/>): its types must not
/// depend on any other ring's namespace. Also fails if the namespace filter matches zero types — a NetArchTest
/// rule over an empty set reports success, so a stale filter (e.g. after a namespace rename) would silently turn
/// the guard into a no-op.
/// </summary>
internal static class RingFitness
{
    public static void AssertDependsOnlyWithinLayer(Ring ring)
    {
        var forbidden = RingModel.ForbiddenFor(ring);

        Types.InAssemblies(ring.Assemblies).That().ResideInNamespaceStartingWith(ring.Namespace)
            .GetTypes().Should().NotBeEmpty(
                because: $"the '{ring.Namespace}' filter must match real types — otherwise the {ring.Name}-ring " +
                         "layering guard passes vacuously (the likely failure mode after a namespace rename)");

        var result = Types.InAssemblies(ring.Assemblies).That().ResideInNamespaceStartingWith(ring.Namespace)
            .Should().NotHaveDependencyOnAny(forbidden)
            .GetResult();

        var allowed = ring.MayDependOn.Length == 0 ? "(no other Fallout ring)" : string.Join(", ", ring.MayDependOn);
        result.IsSuccessful.Should().BeTrue(
            because: $"the {ring.Name} ring may depend only on [{allowed}] plus non-Fallout libraries; it must not " +
                     $"reach [{string.Join(", ", forbidden)}]. Offending types: " +
                     (result.FailingTypeNames is null ? "(none reported)" : string.Join(", ", result.FailingTypeNames)));
    }
}
