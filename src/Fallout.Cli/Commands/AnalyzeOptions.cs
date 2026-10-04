using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common;
using Fallout.NuGet.Analysis;

namespace Fallout.Cli.Commands;

internal enum AnalyzeOutputFormat
{
    Table,
    Flat,
}

/// <summary>The parsed arguments of <c>fallout :analyze packages</c>.</summary>
/// <param name="Path">A <c>.csproj</c>, a directory or a <c>.sln</c>/<c>.slnx</c> file. <c>null</c> means the working directory.</param>
/// <param name="TargetFramework">Only analyze this target framework. <c>null</c> means all.</param>
/// <param name="Severity">The level findings are logged at in the flat format. <c>null</c> means log no findings.</param>
/// <param name="Format">How findings are printed.</param>
/// <param name="ShowConflicts">Whether version conflicts are listed. They are counted in the summary either way.</param>
/// <param name="Verbose">Whether the conflicts table names the projects.</param>
/// <param name="ExcludedPackageIds">Packages that are never reported.</param>
/// <param name="ShowHelp">Whether only the usage text was requested.</param>
internal sealed record AnalyzeOptions(
    string Path,
    string TargetFramework,
    LogLevel? Severity,
    AnalyzeOutputFormat Format,
    bool ShowConflicts,
    bool Verbose,
    IReadOnlyCollection<string> ExcludedPackageIds,
    bool ShowHelp)
{
    internal const string Usage =
        "Usage: fallout :analyze packages [<path>] [--tfm <moniker>] [--severity none|trace|normal|warning|error] " +
        "[--format table|flat] [--conflicts] [--verbose] [--exclude <id>[,<id>...]] [--help]";

    /// <summary>Parses the arguments after <c>packages</c>. On failure, <paramref name="error"/> says what is wrong.</summary>
    internal static bool TryParse(string[] args, out AnalyzeOptions options, out string error)
    {
        string path = null;
        string tfm = null;
        LogLevel? severity = LogLevel.Warning;
        var format = AnalyzeOutputFormat.Table;
        var showConflicts = false;
        var verbose = false;
        var showHelp = false;
        var excludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        options = null;
        error = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg.ToLowerInvariant())
            {
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--tfm":
                    if (!TryGetValue(args, ref i, arg, out tfm, out error))
                    {
                        return false;
                    }

                    break;
                case "--severity":
                    if (!TryGetValue(args, ref i, arg, out var severityText, out error))
                    {
                        return false;
                    }

                    if (!TryParseSeverity(severityText, out severity))
                    {
                        error = $"Unknown severity '{severityText}' (use none|trace|normal|warning|error).";
                        return false;
                    }

                    break;
                case "--format":
                    if (!TryGetValue(args, ref i, arg, out var formatText, out error))
                    {
                        return false;
                    }

                    if (!TryParseFormat(formatText, out format))
                    {
                        error = $"Unknown format '{formatText}' (use table|flat).";
                        return false;
                    }

                    break;
                case "--conflicts":
                    showConflicts = true;
                    break;
                case "--verbose":
                case "-v":
                    verbose = true;
                    break;
                case "--exclude":
                    if (!TryGetValue(args, ref i, arg, out var excludeText, out error))
                    {
                        return false;
                    }

                    foreach (var id in excludeText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        excludes.Add(id);
                    }

                    break;
                default:
                    if (arg.StartsWith('-'))
                    {
                        error = $"Unknown option '{arg}'.";
                        return false;
                    }

                    if (path != null)
                    {
                        error = $"Only one path can be given, but found '{path}' and '{arg}'.";
                        return false;
                    }

                    path = arg;
                    break;
            }
        }

        options = new AnalyzeOptions(path, tfm, severity, format, showConflicts, verbose, excludes, showHelp);
        return true;
    }

    /// <summary>
    /// <c>1</c> when <c>--severity error</c> is set and a finding that is reported exists, otherwise <c>0</c>.
    /// Conflicts only count when <c>--conflicts</c> lists them, so the exit code never depends on
    /// findings that the output does not show.
    /// </summary>
    internal int GetExitCode(IEnumerable<Finding> findings)
    {
        var reported = findings.Any(x => x.Kind != FindingKind.VersionConflict || ShowConflicts);
        return Severity == LogLevel.Error && reported ? 1 : 0;
    }

    private static bool TryGetValue(string[] args, ref int index, string option, out string value, out string error)
    {
        error = null;
        value = null;

        if (index + 1 >= args.Length)
        {
            error = $"{option} requires a value.";
            return false;
        }

        value = args[++index];
        return true;
    }

    private static bool TryParseSeverity(string value, out LogLevel? severity)
    {
        severity = LogLevel.Warning;
        switch (value.ToLowerInvariant())
        {
            case "none":
                severity = null;
                return true;
            case "trace":
                severity = LogLevel.Trace;
                return true;
            case "normal":
            case "info":
            case "information":
                severity = LogLevel.Normal;
                return true;
            case "warning":
            case "warn":
                severity = LogLevel.Warning;
                return true;
            case "error":
                severity = LogLevel.Error;
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseFormat(string value, out AnalyzeOutputFormat format)
    {
        format = AnalyzeOutputFormat.Table;
        switch (value.ToLowerInvariant())
        {
            case "table":
                return true;
            case "flat":
            case "lines":
                format = AnalyzeOutputFormat.Flat;
                return true;
            default:
                return false;
        }
    }
}
