using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fallout.Architecture.Tests;

/// <summary>One onion ring: its namespace, the assemblies that make it up, and the rings it may depend on.</summary>
internal sealed record Ring(string Name, string Namespace, Assembly[] Assemblies, string[] MayDependOn);

/// <summary>
/// The onion architecture (ADR-0006) expressed as data: every Fallout ring and the rings it is allowed to
/// depend on. The layering tests derive each ring's <i>forbidden</i> set as "every other known ring, minus the
/// allowed ones, minus itself", and assert the ring depends on nothing in that set. That makes the rule a
/// positive whitelist — a ring may depend only within its declared layer — which catches an illegal edge to
/// <b>any</b> other current ring (Application → Infrastructure, anything → Fallout.Cli, a ring → Migrate/MSBuild
/// tooling, …), not just a hand-picked few. Non-Fallout libraries are unconstrained.
/// </summary>
internal static class RingModel
{
    // Every Fallout ring/area namespace in the repo. A ring depending on anything here that isn't in its
    // allowed list (or its own namespace) fails the layering test. The bare `Fallout` meta-package is
    // intentionally absent — it's an aggregator package, not a namespace (and every ring starts with "Fallout.").
    public static readonly string[] Universe =
    [
        "Fallout.Domain",
        "Fallout.Core",
        "Fallout.Build.Shared",
        "Fallout.Application",
        "Fallout.Infrastructure",
        "Fallout.Cli",
        "Fallout.Persistence",
        "Fallout.Migrate",
        "Fallout.MSBuildTasks",
        "Fallout.SourceGenerators",
        "Fallout.CodeGeneration", // Fallout.Tooling.Generator's RootNamespace
    ];

    public static readonly IReadOnlyList<Ring> Rings =
    [
        // Innermost rings — zero Fallout dependencies.
        new Ring("Domain", "Fallout.Domain",
            [typeof(global::Fallout.Domain.Planning.TopoSort).Assembly],
            MayDependOn: []),

        new Ring("Core", "Fallout.Core",
            [
                typeof(global::Fallout.Core.IO.AbsolutePath).Assembly,
                typeof(global::Fallout.Core.IO.Globbing.Globbing).Assembly,
                typeof(global::Fallout.Core.IO.Compression.CompressionExtensions).Assembly,
                typeof(global::Fallout.Core.Net.HttpRequestBuilder).Assembly,
                typeof(global::Fallout.Core.Text.Json.JsonExtensions).Assembly,
                typeof(global::Fallout.Core.Text.Yaml.YamlExtensions).Assembly,
            ],
            MayDependOn: []),

        // Shared build helper — kernel-adjacent, may use Core.
        new Ring("Build.Shared", "Fallout.Build.Shared",
            [Assembly.Load("Fallout.Build.Shared")],
            MayDependOn: ["Fallout.Core"]),

        // Application ring — the engine + tool/CI/solution vocabulary and ports. Inward only.
        new Ring("Application", "Fallout.Application",
            [
                typeof(global::Fallout.Application.FalloutBuild).Assembly,
                typeof(global::Fallout.Application.Components.ICompile).Assembly,
                typeof(global::Fallout.Application.Tooling.ToolTasks).Assembly,
                typeof(global::Fallout.Application.Tools.DotNet.DotNetTasks).Assembly,
                typeof(global::Fallout.Application.Solutions.Solution).Assembly,
            ],
            MayDependOn: ["Fallout.Domain", "Fallout.Core", "Fallout.Build.Shared"]),

        // Infrastructure ring — the adapters. May depend inward + on the vendored Persistence model.
        new Ring("Infrastructure", "Fallout.Infrastructure",
            [
                typeof(global::Fallout.Infrastructure.Tooling.NpmToolPathResolver).Assembly,
                typeof(global::Fallout.Infrastructure.CI.GitLab.GitLabProjectVisibility).Assembly,
                typeof(global::Fallout.Infrastructure.Solutions.SolutionReader).Assembly,
                typeof(global::Fallout.Infrastructure.ProjectModel.ProjectModelTasks).Assembly,
            ],
            MayDependOn: ["Fallout.Application", "Fallout.Domain", "Fallout.Core", "Fallout.Build.Shared", "Fallout.Persistence"]),
    ];

    public static IEnumerable<object[]> RingNames => Rings.Select(r => new object[] { r.Name });

    public static Ring ByName(string name) => Rings.Single(r => r.Name == name);

    /// <summary>The universe minus this ring's own namespace minus the rings it may depend on.</summary>
    public static string[] ForbiddenFor(Ring ring) =>
        Universe.Where(ns => ns != ring.Namespace && !ring.MayDependOn.Contains(ns)).ToArray();
}
