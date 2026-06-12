using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Onion fitness for the Core (kernel) ring (ADR-0006). <c>Fallout.Core*</c> is the innermost shared ring —
/// pure helpers plus the fluent IO/HTTP/JSON/YAML vocabulary that the Domain, Application, and Infrastructure
/// rings build on. It must reach no outer ring: not <c>Fallout.Application.*</c>, not
/// <c>Fallout.Infrastructure.*</c>, not the <c>Fallout.Cli</c> composition root (nor any legacy
/// pre-realignment name). This is the kernel-side counterpart to the Domain purity test, and the guard the
/// original review found missing.
/// </summary>
public class CoreRingFitnessTests
{
    // The Core ring spans six assemblies: the base (Fallout.Core: helpers, AbsolutePath, fluent FS) plus the
    // IO/Net/Text sub-packages. Each is anchored by a public type so the reference is compile-checked.
    private static readonly Assembly[] CoreAssemblies =
    [
        typeof(global::Fallout.Core.IO.AbsolutePath).Assembly,
        typeof(global::Fallout.Core.IO.Globbing.Globbing).Assembly,
        typeof(global::Fallout.Core.IO.Compression.CompressionExtensions).Assembly,
        typeof(global::Fallout.Core.Net.HttpRequestBuilder).Assembly,
        typeof(global::Fallout.Core.Text.Json.JsonExtensions).Assembly,
        typeof(global::Fallout.Core.Text.Yaml.YamlExtensions).Assembly,
    ];

    [Fact]
    public void Core_ring_does_not_depend_on_any_outer_ring()
    {
        var result = Types.InAssemblies(CoreAssemblies)
            .That().ResideInNamespaceStartingWith("Fallout.Core")
            .Should()
            .NotHaveDependencyOnAny(
                "Fallout.Application",
                "Fallout.Infrastructure",
                "Fallout.Cli",
                // legacy pre-realignment names — guarded so a regression can't reintroduce a dependency on them
                "Fallout.Common",
                "Fallout.Build",
                "Fallout.Components",
                "Fallout.ProjectModel")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "the Core kernel ring is innermost and must reach no outer ring; offending types: " +
                     FailingTypes(result));
    }

    private static string FailingTypes(TestResult result) =>
        result.FailingTypeNames is null ? "(none reported)" : string.Join(", ", result.FailingTypeNames);
}
