using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Fallout.Migrate;

public sealed class Migration
{
    private readonly string _rootDirectory;
    private readonly bool _dryRun;
    private readonly TextWriter _log;

    public Migration(string rootDirectory, bool dryRun, TextWriter log)
    {
        _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        _dryRun = dryRun;
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public Summary Run()
    {
        var summary = new Summary();

        RewriteCsprojs(summary);
        RewriteCsFiles(summary);
        RewriteBootstrapScripts(summary);
        RenameNukeDirectory(summary);

        return summary;
    }

    private void RewriteCsprojs(Summary summary)
    {
        var falloutVersion = ResolveFalloutVersion();
        foreach (var path in EnumerateFiles("*.csproj"))
            ApplyRewrite(path, content => CsprojRewriter.Rewrite(content, falloutVersion), summary);
    }

    // Pinned into migrated `<PackageReference Include="Fallout.X" Version="..." />` lines.
    // Uses the running migrate tool's own SemVer (Nerdbank.GitVersioning, set on
    // AssemblyInformationalVersion) so the migration output aligns with the tool the user
    // just installed. For dev/local builds without a `+` in InformationalVersion (i.e. no
    // build-metadata suffix), falls back to a known-published floor so we never emit a
    // bogus pin like Version="LOCAL". Inlined to keep Fallout.Migrate dependency-free.
    private static string ResolveFalloutVersion()
    {
        const string Fallback = "11.0.0";

        var informational = typeof(Migration).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (string.IsNullOrEmpty(informational))
            return Fallback;

        var plusIndex = informational.IndexOf('+');
        if (plusIndex == -1)
            return Fallback;

        return informational[..plusIndex];
    }

    private void RewriteCsFiles(Summary summary)
    {
        foreach (var path in EnumerateFiles("*.cs"))
            ApplyRewrite(path, CodeRewriter.Rewrite, summary);
    }

    private void RewriteBootstrapScripts(Summary summary)
    {
        foreach (var name in new[] { "build.cmd", "build.ps1", "build.sh" })
        {
            var path = Path.Combine(_rootDirectory, name);
            if (File.Exists(path))
                ApplyRewrite(path, ScriptRewriter.Rewrite, summary);
        }
    }

    private void RenameNukeDirectory(Summary summary)
    {
        var legacy = Path.Combine(_rootDirectory, ".nuke");
        var canonical = Path.Combine(_rootDirectory, ".fallout");

        if (!Directory.Exists(legacy))
            return;

        if (Directory.Exists(canonical))
        {
            summary.Warnings.Add(
                "Both .nuke/ and .fallout/ exist. Skipped rename; merge their contents manually.");
            return;
        }

        Log($"rename  {RelativePath(legacy)} -> {RelativePath(canonical)}");
        if (!_dryRun)
            Directory.Move(legacy, canonical);
        summary.DirectoriesRenamed++;
    }

    private IEnumerable<string> EnumerateFiles(string pattern)
    {
        foreach (var file in Directory.EnumerateFiles(_rootDirectory, pattern, SearchOption.AllDirectories))
        {
            if (IsIgnored(file))
                continue;
            yield return file;
        }
    }

    private static bool IsIgnored(string path)
    {
        return path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || path.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private void ApplyRewrite(string path, Func<string, RewriteResult> rewriter, Summary summary)
    {
        string original;
        try
        {
            original = File.ReadAllText(path);
        }
        catch (IOException ex)
        {
            summary.Warnings.Add($"could not read {RelativePath(path)}: {ex.Message}");
            return;
        }

        var result = rewriter(original);
        if (result.EditCount == 0)
            return;

        Log($"edit    {RelativePath(path)}  ({result.EditCount} change{(result.EditCount == 1 ? "" : "s")})");
        summary.FilesChanged++;
        summary.EditCount += result.EditCount;

        if (!_dryRun)
            File.WriteAllText(path, result.Content);
    }

    private string RelativePath(string absolute) =>
        Path.GetRelativePath(_rootDirectory, absolute).Replace('\\', '/');

    private void Log(string line) => _log.WriteLine(line);

    public sealed class Summary
    {
        public int FilesChanged { get; set; }
        public int EditCount { get; set; }
        public int DirectoriesRenamed { get; set; }
        public List<string> Warnings { get; } = new();
    }
}

public readonly record struct RewriteResult(string Content, int EditCount);
