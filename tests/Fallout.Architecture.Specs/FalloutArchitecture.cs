using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Fallout.Architecture.Specs;

/// <summary>
/// The loaded picture of the repo's runtime assemblies, plus the small vocabulary the fitness rules are
/// written against. Layering in this codebase is an <b>assembly</b> concern, not a namespace one — the legacy
/// NUKE namespaces (<c>Fallout.Common.*</c>, …) deliberately span several assemblies — so every rule scopes its
/// subject and target by assembly, never by namespace.
/// </summary>
internal static class FalloutArchitecture
{
    // Governed runtime libraries (assembly simple names). Mirrors the csproj reference set.
    public const string Core = "Fallout.Core";
    public const string Utilities = "Fallout.Utilities";
    public const string UtilitiesIoCompression = "Fallout.Utilities.IO.Compression";
    public const string UtilitiesIoGlobbing = "Fallout.Utilities.IO.Globbing";
    public const string UtilitiesNet = "Fallout.Utilities.Net";
    public const string UtilitiesTextJson = "Fallout.Utilities.Text.Json";
    public const string UtilitiesTextYaml = "Fallout.Utilities.Text.Yaml";
    public const string Solution = "Fallout.Solution";
    public const string ToolingExecution = "Fallout.Application.Tooling.Execution";
    public const string ToolingRequirements = "Fallout.Application.Tooling.Requirements";
    public const string Tooling = "Fallout.Tooling";
    public const string ProjectModel = "Fallout.ProjectModel";
    public const string BuildShared = "Fallout.Build.Shared";
    public const string Build = "Fallout.Build";
    public const string Common = "Fallout.Common";
    public const string Components = "Fallout.Components";
    public const string Cli = "Fallout.Cli";

    // Transition shims.
    public const string NukeCommon = "Nuke.Common";
    public const string NukeBuild = "Nuke.Build";
    public const string NukeComponents = "Nuke.Components";

    /// <summary>The utility satellites — each may depend only on <see cref="Utilities"/>.</summary>
    public static readonly string[] UtilitySatellites =
    [
        UtilitiesIoCompression, UtilitiesIoGlobbing, UtilitiesNet, UtilitiesTextJson, UtilitiesTextYaml,
    ];

    /// <summary>
    /// Assemblies whose own namespaces should be rooted at the assembly name (the naming-alignment subjects).
    /// Excludes the vendored <c>Fallout.Persistence.Solution</c> parser, which is not ours to rename.
    /// </summary>
    public static readonly string[] RuntimeLibraries =
    [
        Core, Utilities, UtilitiesIoCompression, UtilitiesIoGlobbing, UtilitiesNet, UtilitiesTextJson,
        UtilitiesTextYaml, Solution, ToolingExecution, ToolingRequirements, Tooling, ProjectModel, BuildShared,
        Build, Common, Components, Cli, NukeCommon, NukeBuild, NukeComponents,
    ];

    private static readonly IReadOnlyDictionary<string, System.Reflection.Assembly> Loaded = LoadProductionAssemblies();

    /// <summary>The architecture under test — built once from every loaded production assembly.</summary>
    public static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(Loaded.Values.ToArray()).Build();

    private static IReadOnlyDictionary<string, System.Reflection.Assembly> LoadProductionAssemblies()
    {
        var directory = AppContext.BaseDirectory;

        // Out of governance scope: this test assembly, the Roslyn build-time tooling, and the Migrate tooling.
        // (They aren't referenced by the csproj, so normally they aren't even here — belt and braces.)
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Fallout.Architecture.Specs.dll",
            "Fallout.SourceGenerators.dll",
            "Fallout.Tooling.Generator.dll",
            "Fallout.Migrate.dll",
            "Fallout.Migrate.Analyzers.dll",
        };

        var files = Directory.EnumerateFiles(directory, "Fallout.*.dll")
            .Concat(Directory.EnumerateFiles(directory, "Nuke.*.dll"))
            .Where(file => !excluded.Contains(Path.GetFileName(file)));

        var map = new Dictionary<string, System.Reflection.Assembly>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            var assembly = System.Reflection.Assembly.LoadFrom(file);
            map[assembly.GetName().Name!] = assembly;
        }

        return map;
    }

    /// <summary>The loaded reflection assembly for <paramref name="name"/>, or a helpful failure if it is missing.</summary>
    public static System.Reflection.Assembly Asm(string name) =>
        Loaded.TryGetValue(name, out var assembly)
            ? assembly
            : throw new InvalidOperationException(
                $"Assembly '{name}' was not loaded. Add a ProjectReference for it in " +
                $"Fallout.Architecture.Specs.csproj. Loaded: {string.Join(", ", Loaded.Keys.OrderBy(k => k))}.");

    /// <summary>Every loaded in-repo assembly except the named ones.</summary>
    public static System.Reflection.Assembly[] AllAssembliesExcept(params string[] names)
    {
        var excluded = new HashSet<string>(names, StringComparer.Ordinal);
        return Loaded.Values.Where(a => !excluded.Contains(a.GetName().Name!)).ToArray();
    }

    /// <summary>Every loaded <c>Fallout.*</c> assembly (i.e. excluding the <c>Nuke.*</c> shims).</summary>
    public static System.Reflection.Assembly[] FalloutAssemblies() =>
        Loaded.Values.Where(a => a.GetName().Name!.StartsWith("Fallout.", StringComparison.Ordinal)).ToArray();

    // Our own code lives under Fallout.* or Nuke.*. Constraining every subject/target to these namespaces
    // strips the no-namespace, compiler/tooling-generated types that get injected into every assembly —
    // `ThisAssembly` (Nerdbank.GitVersioning), `RefSafetyRulesAttribute`, coverlet instrumentation — which
    // would otherwise read as spurious cross-assembly dependencies and naming violations. (The original #88
    // NetArchTest guard scoped itself the same way, via ResideInNamespaceStartingWith("Fallout").)
    public const string OwnNamespacePattern = @"^(?:Fallout|Nuke)\.";

    /// <summary>
    /// "Every <i>first-party</i> type residing in any of these assemblies" — usable both as a rule subject (it
    /// has <c>.Should()</c>) and as a dependency target (it is an <c>IObjectProvider&lt;IType&gt;</c>). Handles
    /// the <c>ResideInAssembly(first, rest)</c> spread and the first-party namespace filter in one place.
    /// </summary>
    public static GivenTypesConjunction TypesIn(params System.Reflection.Assembly[] assemblies) =>
        Types().That().ResideInAssembly(assemblies[0], assemblies.Skip(1).ToArray())
            .And().ResideInNamespaceMatching(OwnNamespacePattern);

    /// <summary>"Every type residing in any of these named assemblies".</summary>
    public static GivenTypesConjunction TypesIn(params string[] assemblyNames) =>
        TypesIn(assemblyNames.Select(Asm).ToArray());
}
