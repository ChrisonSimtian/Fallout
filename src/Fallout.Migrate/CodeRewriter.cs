using System.Text.RegularExpressions;

namespace Fallout.Migrate;

internal static class CodeRewriter
{
    // Anchored prefix swap: `\bNuke\.` → `Fallout.`. Covers using directives,
    // attribute references, qualified type names, namespace declarations.
    // The trailing `(?=[A-Z])` lookahead avoids matching `Nuke.json` filenames
    // or other lowercase tails the prefix audit deliberately preserved.
    private static readonly Regex NamespacePrefix =
        new(@"\bNuke\.(?=[A-Z])", RegexOptions.Compiled);

    // Bare type renames done in the Fallout rebrand (#59).
    private static readonly Regex NukeBuildType = new(@"\bNukeBuild\b", RegexOptions.Compiled);
    private static readonly Regex INukeBuildType = new(@"\bINukeBuild\b", RegexOptions.Compiled);

    public static RewriteResult Rewrite(string original)
    {
        var edits = 0;

        var content = NamespacePrefix.Replace(original, _ => { edits++; return "Fallout."; });
        content = INukeBuildType.Replace(content, _ => { edits++; return "IFalloutBuild"; });
        content = NukeBuildType.Replace(content, _ => { edits++; return "FalloutBuild"; });

        return new RewriteResult(content, edits);
    }
}
