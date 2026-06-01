using System.Linq;
using System.Text.RegularExpressions;
using Fallout.Migration.Shared;

namespace Fallout.Migrate;

internal static class CodeRewriter
{
    // Per-namespace prefix rewrites driven by the canonical Nuke<->Fallout map (longest Nuke prefix first,
    // so `Nuke.Common.Tools` resolves to `Fallout.Application.Tools` before the catch-all `Nuke.Common` →
    // `Fallout.Application` rule). This replaces the old blind `Nuke.` → `Fallout.` swap, which the onion
    // realignment broke: it produced dead `Fallout.Common.*` namespaces (now `Fallout.Application.*` etc.).
    // Each prefix is matched on a word boundary + a `.`/non-identifier tail so we hit namespace segments,
    // not filenames (`Nuke.json`) or unrelated identifiers.
    private static readonly (Regex Pattern, string Replacement)[] s_namespaceRewrites =
        NukeNamespaceMap.MigrationPairsLongestFirst
            .Select(p => (new Regex($@"\b{Regex.Escape(p.Key)}(?=\.|\b)", RegexOptions.Compiled), p.Value))
            .ToArray();

    // Bare type renames done in the Fallout rebrand (#59).
    private static readonly Regex NukeBuildType = new(@"\bNukeBuild\b", RegexOptions.Compiled);
    private static readonly Regex INukeBuildType = new(@"\bINukeBuild\b", RegexOptions.Compiled);

    public static RewriteResult Rewrite(string original)
    {
        var edits = 0;

        var content = original;
        foreach (var (pattern, replacement) in s_namespaceRewrites)
            content = pattern.Replace(content, _ => { edits++; return replacement; });

        content = INukeBuildType.Replace(content, _ => { edits++; return "IFalloutBuild"; });
        content = NukeBuildType.Replace(content, _ => { edits++; return "FalloutBuild"; });

        return new RewriteResult(content, edits);
    }
}
