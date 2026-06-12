using System.Collections.Generic;
using System.Linq;

namespace Fallout.Migration.Shared;

/// <summary>
/// The single canonical correspondence between legacy <c>Nuke.*</c> namespaces and their post-onion
/// <c>Fallout.*</c> homes. Linked (as source) into the three consumers so there is ONE source of truth:
/// <list type="bullet">
///   <item>the transition-shim generator (Fallout.SourceGenerators) emits <c>Fallout.* → Nuke.*</c>
///         re-exports per shim package;</item>
///   <item>the migration rewriters (Fallout.Migrate, Fallout.Migrate.Analyzers) rewrite
///         <c>Nuke.* → Fallout.*</c>.</item>
/// </list>
/// A few NUKE namespaces SPLIT across rings post-onion (CI, ProjectModel, IO). The shim direction unions
/// every row for a Nuke prefix; the migration direction (a pure prefix replace) uses the single row flagged
/// <see cref="NukeNamespaceMapping.IsMigrationTarget"/> as that prefix's dominant home (the rarer secondary
/// home is shim-only — code using it keeps compiling via the shim, and the migration analyzer flags the
/// residue for manual touch-up; deliberate, given low migration demand).
/// </summary>
internal sealed class NukeNamespaceMapping
{
    public NukeNamespaceMapping(string nukePrefix, string falloutPrefix, string shimPackage, bool isMigrationTarget = true)
    {
        NukePrefix = nukePrefix;
        FalloutPrefix = falloutPrefix;
        ShimPackage = shimPackage;
        IsMigrationTarget = isMigrationTarget;
    }

    /// <summary>Legacy namespace prefix, e.g. <c>Nuke.Common.Tools</c>.</summary>
    public string NukePrefix { get; }

    /// <summary>Post-onion Fallout namespace prefix, e.g. <c>Fallout.Application.Tools</c>.</summary>
    public string FalloutPrefix { get; }

    /// <summary>Which shim package re-exports this row (<c>Nuke.Common</c> or <c>Nuke.Components</c>).</summary>
    public string ShimPackage { get; }

    /// <summary>
    /// True if this row is the dominant <c>Nuke→Fallout</c> migration target for its <see cref="NukePrefix"/>.
    /// Exactly one row per distinct <see cref="NukePrefix"/> is the migration target; split-namespace
    /// secondaries are shim-only (false).
    /// </summary>
    public bool IsMigrationTarget { get; }
}

internal static class NukeNamespaceMap
{
    public const string NukeCommonPackage = "Nuke.Common";
    public const string NukeComponentsPackage = "Nuke.Components";

    /// <summary>The canonical map. Order is not significant; consumers sort as needed.</summary>
    public static readonly IReadOnlyList<NukeNamespaceMapping> All = new[]
    {
        // ── Nuke.Common ───────────────────────────────────────────────────────────────────────────
        new NukeNamespaceMapping("Nuke.Common.Tooling",        "Fallout.Application.Tooling",        NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.Tools",          "Fallout.Application.Tools",          NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.Git",            "Fallout.Application.Git",            NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.Execution",      "Fallout.Application.Execution",      NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.ValueInjection", "Fallout.Application.ValueInjection", NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.ChangeLog",      "Fallout.Application.ChangeLog",      NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.Utilities",      "Fallout.Core",                     NukeCommonPackage),

        // Split namespaces: migration target = the dominant consumer-facing home; secondary = shim-only.
        // CI: providers (Infrastructure.CI) are what `using Nuke.Common.CI.<Provider>` consumers reference;
        // the port enums in Application.CI are the rarer secondary.
        new NukeNamespaceMapping("Nuke.Common.CI",             "Fallout.Infrastructure.CI",          NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.CI",             "Fallout.Application.CI",             NukeCommonPackage, isMigrationTarget: false),
        // ProjectModel: the Solution/Project model + [Solution] (Application.Solutions) is dominant; the
        // MSBuild evaluator (Infrastructure.ProjectModel) is the rarer secondary.
        new NukeNamespaceMapping("Nuke.Common.ProjectModel",   "Fallout.Application.Solutions",      NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.ProjectModel",   "Fallout.Infrastructure.ProjectModel", NukeCommonPackage, isMigrationTarget: false),
        // IO: AbsolutePath/glob fluent API (Kernel.IO) is dominant; the build-injection IO attrs
        // (Application.IO) are the secondary.
        new NukeNamespaceMapping("Nuke.Common.IO",             "Fallout.Core.IO",                  NukeCommonPackage),
        new NukeNamespaceMapping("Nuke.Common.IO",             "Fallout.Application.IO",             NukeCommonPackage, isMigrationTarget: false),

        // Root: NukeBuild/Target/[Parameter]/[Secret]/Host/… → Fallout.Application. Kept LAST so the more
        // specific Nuke.Common.* prefixes above win longest-prefix-first matching in the migration rewriter.
        new NukeNamespaceMapping("Nuke.Common",                "Fallout.Application",                NukeCommonPackage),

        // ── Nuke.Components ───────────────────────────────────────────────────────────────────────
        new NukeNamespaceMapping("Nuke.Components",            "Fallout.Application.Components",      NukeComponentsPackage),
    };

    /// <summary>
    /// NUKE consumer NuGet package ID → its post-onion Fallout package, for the migration's
    /// <c>&lt;PackageReference&gt;</c> rewrite. NUKE consumers reference <c>Nuke.Common</c> (→ the <c>Fallout</c>
    /// meta-package, which pulls every ring) and optionally <c>Nuke.Components</c> (→ <c>Fallout.Application.Components</c>);
    /// the bogus <c>Nuke.Build</c> package folds into the meta. Distinct from the namespace map — package IDs
    /// don't track namespaces post-onion (e.g. the <c>Fallout</c> meta has no namespace of its own).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> PackageIdMap = new Dictionary<string, string>
    {
        ["Nuke.Common"] = "Fallout",
        ["Nuke.Build"] = "Fallout",
        ["Nuke.Components"] = "Fallout.Application.Components",
    };

    /// <summary>Fallout→Nuke rows the shim generator emits for <paramref name="shimPackage"/> (unions splits).</summary>
    public static IEnumerable<NukeNamespaceMapping> ShimRowsFor(string shimPackage)
        => All.Where(x => x.ShimPackage == shimPackage);

    /// <summary>
    /// Nuke→Fallout migration prefix pairs, longest Nuke prefix first (so <c>Nuke.Common.Tools</c> is tried
    /// before <c>Nuke.Common</c>). Only dominant rows; one per distinct Nuke prefix.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> MigrationPairsLongestFirst { get; } =
        All.Where(x => x.IsMigrationTarget)
           .OrderByDescending(x => x.NukePrefix.Length)
           .Select(x => new KeyValuePair<string, string>(x.NukePrefix, x.FalloutPrefix))
           .ToArray();
}
