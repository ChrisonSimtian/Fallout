// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System.Text.RegularExpressions;

namespace Fallout.Migrate;

internal static class CsprojRewriter
{
    // PackageReference / ProjectReference `Include="Nuke.X"` → `Include="Fallout.X"`.
    private static readonly Regex PackageReferencePattern =
        new(@"(?<=\b(?:Include|Update|Remove)="")Nuke\.(?=[A-Z])", RegexOptions.Compiled);

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

    public static RewriteResult Rewrite(string original)
    {
        var edits = 0;

        var content = PackageReferencePattern.Replace(original, _ => { edits++; return "Fallout."; });
        content = MSBuildPropertyPattern.Replace(content, _ => { edits++; return "Fallout"; });

        return new RewriteResult(content, edits);
    }
}
