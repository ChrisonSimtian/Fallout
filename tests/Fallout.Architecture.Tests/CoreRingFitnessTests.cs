using System.Reflection;
using Xunit;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Onion fitness for the Core (kernel) ring (ADR-0006). <c>Fallout.Core*</c> is the innermost shared ring —
/// pure helpers plus the fluent IO/HTTP/JSON/YAML vocabulary that the Domain, Application, and Infrastructure
/// rings build on. It must reach no outer ring: not <c>Fallout.Application.*</c>, not
/// <c>Fallout.Infrastructure.*</c>, not the <c>Fallout.Cli</c> composition root. The kernel-side counterpart to
/// the Domain purity test, and the guard the original review found missing.
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
    public void Core_ring_does_not_depend_on_any_outer_ring() =>
        RingFitness.AssertNoDependencyOn(
            CoreAssemblies,
            ringNamespace: "Fallout.Core",
            rationale: "the Core kernel ring is innermost and must reach no outer ring.",
            "Fallout.Application", "Fallout.Infrastructure", "Fallout.Cli");
}
