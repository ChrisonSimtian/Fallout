using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fallout.Migration.Shared;

namespace Fallout.Migrate;

internal static class CsprojRewriter
{
    // The known NUKE→Fallout package-ID rewrites (canonical map). Post-onion these are NOT a `Nuke.X →
    // Fallout.X` prefix swap: NUKE's Nuke.Common consumer package maps to the `Fallout` meta-package, and
    // Nuke.Components to Fallout.Application.Components. A blind swap would emit dead `Fallout.Common`/
    // `Fallout.Components` package IDs.
    private static readonly IReadOnlyDictionary<string, string> s_packageIdMap = NukeNamespaceMap.PackageIdMap;

    // Combined rewrite: a known Nuke.X PackageReference WITH an inline Version attribute → its Fallout
    // package at the current Fallout version. NUKE-era pins (e.g. `Version="10.1.0"`) don't exist as the
    // Fallout packages and produce NU1603 which `WarningsAsErrors` escalates; bumping in the same pass
    // avoids a broken post-migrate build (#217). Tolerates extra attributes between Include and Version.
    private static readonly Regex NukePackageWithInlineVersionPattern = new(
        @"(?<prefix><PackageReference\s+Include="")(?<id>Nuke\.[A-Z][A-Za-z0-9.]+)(?<between>""[^>]*?\s+Version="")[^""]+",
        RegexOptions.Compiled);

    // PackageReference / ProjectReference `Include="Nuke.X"` → the mapped Fallout package — catches refs
    // that DON'T have an inline Version (central package management). Runs after the inline-version pass.
    private static readonly Regex PackageReferencePattern =
        new(@"(?<=\b(?:Include|Update|Remove)="")(?<id>Nuke\.[A-Za-z0-9.]+)", RegexOptions.Compiled);

    // MSBuild element/property names that begin with `Nuke` followed by an uppercase
    // letter (e.g. <NukeRootDirectory>...). Limited to known consumer-facing names from
    // P3.5b so we don't rewrite unrelated user-defined identifiers that happen to start
    // with the literal "Nuke".
    private static readonly Regex MSBuildPropertyPattern = new(
        @"\bNuke(?=" +
        "(?:RootDirectory|ScriptDirectory|TelemetryVersion|BaseDirectory|BaseNamespace|" +
        "UseNestedNamespaces|RepositoryUrl|UpdateReferences|ContinueOnError|TaskTimeout|" +
        "Timeout|TasksEnabled|DefaultExcludes|ExcludeBoot|ExcludeConfig|ExcludeLogs|" +
        "ExcludeDirectoryBuild|ExcludeCi|SpecificationFiles|ExternalFiles|TasksAssembly|" +
        "TasksDirectory)\\b)",
        RegexOptions.Compiled);

    // Strip explicit `System.Security.Cryptography.Xml` PackageReferences. NUKE-era projects
    // often pinned this directly at an older major (e.g. 9.x). Fallout.Common 10.2.12+ transitively
    // requires a newer version (10.0.6+) and the conflict trips NU1605 ("Detected package
    // downgrade"). Removing the explicit pin lets the transitive version win, which is what the
    // migrated project wants (#217). Matches a self-closing element with optional surrounding
    // indentation + trailing newline.
    private static readonly Regex CryptographyXmlPackageRefPattern = new(
        @"^[ \t]*<PackageReference\s+Include=""System\.Security\.Cryptography\.Xml""[^/]*/>\s*\r?\n?",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public static RewriteResult Rewrite(string original, string falloutVersion)
    {
        var edits = 0;
        var content = original;

        // Pass 1 — combined Include + Version rewrite for known Nuke.X PackageReferences with inline Version.
        content = NukePackageWithInlineVersionPattern.Replace(content, m =>
        {
            if (!s_packageIdMap.TryGetValue(m.Groups["id"].Value, out var falloutId))
                return m.Value; // unknown Nuke.* package — leave it (don't invent a dead Fallout id)
            edits++;
            return m.Groups["prefix"].Value + falloutId + m.Groups["between"].Value + falloutVersion;
        });

        // Pass 2 — Include/Update/Remove rewrites for anything Pass 1 didn't consume (CPM-managed
        // PackageReferences without inline Version, ProjectReferences). Known package IDs only.
        content = PackageReferencePattern.Replace(content, m =>
        {
            if (!s_packageIdMap.TryGetValue(m.Groups["id"].Value, out var falloutId))
                return m.Value;
            edits++;
            return falloutId;
        });
        content = MSBuildPropertyPattern.Replace(content, _ => { edits++; return "Fallout"; });

        // Pass 3 — strip the stale System.Security.Cryptography.Xml direct pin.
        content = CryptographyXmlPackageRefPattern.Replace(content, _ => { edits++; return string.Empty; });

        return new RewriteResult(content, edits);
    }
}
