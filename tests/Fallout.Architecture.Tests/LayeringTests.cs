using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Onion layering fitness (ADR-0006), driven by <see cref="RingModel"/>. Each ring may depend only within its
/// declared layer — every other known ring is forbidden — so an illegal edge fails here: Application →
/// Infrastructure, any ring → the <c>Fallout.Cli</c> composition root, a ring → the Migrate/MSBuild tooling, or a
/// ring → a project it shouldn't know about. This replaces the earlier per-ring negative guards with a single
/// positive whitelist over the whole ring DAG.
/// </summary>
public class LayeringTests
{
    public static IEnumerable<object[]> Rings => RingModel.RingNames;

    [Theory]
    [MemberData(nameof(Rings))]
    public void Ring_depends_only_within_its_declared_layer(string ringName) =>
        RingFitness.AssertDependsOnlyWithinLayer(RingModel.ByName(ringName));

    [Fact]
    public void Meta_package_is_an_aggregator_with_no_types_of_its_own() =>
        // The `Fallout` meta-package is a thin aggregator: it references every ring and ships the MSBuild
        // props/targets + analyzer, but defines no types itself — so it can't quietly become a back-door
        // dependency hub. Any Fallout.* type shipping from that assembly is a regression.
        Types.InAssembly(Assembly.Load("Fallout")).That().ResideInNamespaceStartingWith("Fallout")
            .GetTypes().Should().BeEmpty(
                because: "the Fallout meta-package must stay an aggregator — references only, no types of its own");
}
