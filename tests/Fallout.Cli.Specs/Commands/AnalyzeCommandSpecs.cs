using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fallout.Cli.Commands;
using Fallout.Common;
using Fallout.NuGet.Analysis;
using FluentAssertions;
using Xunit;

namespace Fallout.Cli.Specs.Commands;

public sealed class AnalyzeCommandSpecs : IDisposable
{
    private readonly string workspace = Path.Combine(Path.GetTempPath(), "fallout-analyze-" + Guid.NewGuid().ToString("N"));

    public AnalyzeCommandSpecs() => Directory.CreateDirectory(workspace);

    public void Dispose() => Directory.Delete(workspace, recursive: true);

    [Fact]
    public void Command_is_named_analyze()
        => new AnalyzeCommand().Name.Should().Be("analyze");

    [Fact]
    public void Options_default_to_a_table_that_warns_and_hides_conflicts()
    {
        // Act
        var parsed = AnalyzeOptions.TryParse([], out var options, out var error);

        // Assert
        parsed.Should().BeTrue();
        error.Should().BeNull();
        options.Path.Should().BeNull();
        options.TargetFramework.Should().BeNull();
        options.Severity.Should().Be(LogLevel.Warning);
        options.Format.Should().Be(AnalyzeOutputFormat.Table);
        options.ShowConflicts.Should().BeFalse();
        options.Verbose.Should().BeFalse();
        options.ExcludedPackageIds.Should().BeEmpty();
        options.ShowHelp.Should().BeFalse();
    }

    [Fact]
    public void Options_read_every_argument()
    {
        // Arrange
        string[] args = ["src/App", "--tfm", "net10.0", "--severity", "error", "--format", "flat", "--conflicts", "-v", "--exclude", "A, B", "--exclude", "C"];

        // Act
        AnalyzeOptions.TryParse(args, out var options, out _);

        // Assert
        options.Path.Should().Be("src/App");
        options.TargetFramework.Should().Be("net10.0");
        options.Severity.Should().Be(LogLevel.Error);
        options.Format.Should().Be(AnalyzeOutputFormat.Flat);
        options.ShowConflicts.Should().BeTrue();
        options.Verbose.Should().BeTrue();
        options.ExcludedPackageIds.Should().BeEquivalentTo("A", "B", "C");
    }

    [Theory]
    [InlineData("none", null)]
    [InlineData("trace", LogLevel.Trace)]
    [InlineData("normal", LogLevel.Normal)]
    [InlineData("info", LogLevel.Normal)]
    [InlineData("warning", LogLevel.Warning)]
    [InlineData("warn", LogLevel.Warning)]
    [InlineData("ERROR", LogLevel.Error)]
    public void Severity_names_map_to_log_levels(string name, LogLevel? expected)
    {
        // Act
        AnalyzeOptions.TryParse(["--severity", name], out var options, out _);

        // Assert
        options.Severity.Should().Be(expected);
    }

    [Theory]
    [InlineData("--nope", "Unknown option '--nope'.")]
    [InlineData("-x", "Unknown option '-x'.")]
    [InlineData("--severity", "--severity requires a value.")]
    [InlineData("--tfm", "--tfm requires a value.")]
    [InlineData("--exclude", "--exclude requires a value.")]
    public void Options_reject_unknown_and_incomplete_arguments(string arg, string expectedError)
    {
        // Act
        var parsed = AnalyzeOptions.TryParse([arg], out var options, out var error);

        // Assert
        parsed.Should().BeFalse();
        options.Should().BeNull();
        error.Should().Be(expectedError);
    }

    [Fact]
    public void Options_reject_an_unknown_severity_or_format()
    {
        AnalyzeOptions.TryParse(["--severity", "loud"], out _, out var severityError).Should().BeFalse();
        AnalyzeOptions.TryParse(["--format", "xml"], out _, out var formatError).Should().BeFalse();

        severityError.Should().Contain("Unknown severity 'loud'");
        formatError.Should().Contain("Unknown format 'xml'");
    }

    [Fact]
    public void Options_reject_a_second_path()
    {
        // Act
        var parsed = AnalyzeOptions.TryParse(["a.csproj", "b.csproj"], out _, out var error);

        // Assert
        parsed.Should().BeFalse();
        error.Should().Contain("Only one path");
    }

    [Theory]
    [InlineData(null, false, false, 0)]
    [InlineData(LogLevel.Warning, true, true, 0)]
    [InlineData(LogLevel.Error, false, false, 0)]
    [InlineData(LogLevel.Error, true, false, 1)]
    public void Exit_code_is_one_only_for_severity_error_with_a_reported_redundancy(
        LogLevel? severity,
        bool hasRedundancy,
        bool showConflicts,
        int expectedExitCode)
    {
        // Arrange
        var options = Options(severity, showConflicts);
        var findings = hasRedundancy ? new[] { Finding(FindingKind.RedundantViaPackage) } : Array.Empty<Finding>();

        // Act
        var exitCode = options.GetExitCode(findings);

        // Assert
        exitCode.Should().Be(expectedExitCode);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void Exit_code_counts_conflicts_only_when_they_are_listed(bool showConflicts, int expectedExitCode)
    {
        // Arrange
        var options = Options(LogLevel.Error, showConflicts);

        // Act
        var exitCode = options.GetExitCode([Finding(FindingKind.VersionConflict)]);

        // Assert
        exitCode.Should().Be(expectedExitCode);
    }

    [Fact]
    public async Task Missing_subcommand_returns_one()
        => (await Run()).Should().Be(1);

    [Fact]
    public async Task Unknown_subcommand_returns_one()
        => (await Run("nuget")).Should().Be(1);

    [Fact]
    public async Task Unknown_option_returns_one()
        => (await Run("packages", "--nope")).Should().Be(1);

    [Fact]
    public async Task Help_returns_zero()
        => (await Run("packages", "--help")).Should().Be(0);

    [Fact]
    public async Task A_path_that_does_not_exist_returns_one()
        => (await Run("packages", Path.Combine(workspace, "missing"))).Should().Be(1);

    [Fact]
    public async Task A_directory_without_restored_projects_returns_zero()
    {
        // Arrange
        File.WriteAllText(Path.Combine(workspace, "App.csproj"), "<Project />");

        // Act
        var exitCode = await Run("packages", workspace, "--severity", "error");

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task A_redundant_reference_fails_the_run_only_with_severity_error()
    {
        // Arrange
        WriteRestoredProject(
            "App",
            directPackages: ["A", "B"],
            graph: """
                   "A/1.0.0": { "type": "package", "dependencies": { "B": "1.0.0" } },
                   "B/1.0.0": { "type": "package" }
                   """,
            versions: new Dictionary<string, string> { ["A"] = "1.0.0", ["B"] = "1.0.0" });

        // Act
        var byDefault = await Run("packages", workspace);
        var asError = await Run("packages", workspace, "--severity", "error");
        var asFlat = await Run("packages", workspace, "--format", "flat", "--severity", "error");

        // Assert
        byDefault.Should().Be(0);
        asError.Should().Be(1);
        asFlat.Should().Be(1);
    }

    [Fact]
    public async Task A_hidden_version_conflict_does_not_fail_the_run()
    {
        // Arrange
        WriteRestoredProject("One", ["C"], """ "C/1.0.0": { "type": "package" } """, new Dictionary<string, string> { ["C"] = "1.0.0" });
        WriteRestoredProject("Two", ["C"], """ "C/2.0.0": { "type": "package" } """, new Dictionary<string, string> { ["C"] = "2.0.0" });

        // Act
        var hidden = await Run("packages", workspace, "--severity", "error");
        var listed = await Run("packages", workspace, "--severity", "error", "--conflicts");

        // Assert
        hidden.Should().Be(0);
        listed.Should().Be(1);
    }

    [Fact]
    public async Task An_excluded_package_is_not_reported()
    {
        // Arrange
        WriteRestoredProject(
            "App",
            directPackages: ["A", "B"],
            graph: """
                   "A/1.0.0": { "type": "package", "dependencies": { "B": "1.0.0" } },
                   "B/1.0.0": { "type": "package" }
                   """,
            versions: new Dictionary<string, string> { ["A"] = "1.0.0", ["B"] = "1.0.0" });

        // Act
        var exitCode = await Run("packages", workspace, "--severity", "error", "--exclude", "B");

        // Assert
        exitCode.Should().Be(0);
    }

    private static AnalyzeOptions Options(LogLevel? severity, bool showConflicts)
        => new(null, null, severity, AnalyzeOutputFormat.Table, showConflicts, Verbose: false, [], ShowHelp: false);

    private static Finding Finding(FindingKind kind)
        => new() { Kind = kind, PackageId = "X" };

    private static Task<int> Run(params string[] args)
        => new AnalyzeCommand().ExecuteAsync(args, rootDirectory: null, buildScript: null);

    /// <summary>Writes <c>&lt;name&gt;/&lt;name&gt;.csproj</c> and the <c>obj/project.assets.json</c> a restore would produce for it.</summary>
    private void WriteRestoredProject(
        string name,
        string[] directPackages,
        string graph,
        IReadOnlyDictionary<string, string> versions)
    {
        var directory = Path.Combine(workspace, name);
        Directory.CreateDirectory(Path.Combine(directory, "obj"));
        File.WriteAllText(Path.Combine(directory, name + ".csproj"), "<Project />");

        var direct = string.Join(",\n", Array.ConvertAll(
            directPackages,
            x => $$"""
                   "{{x}}": { "target": "Package", "version": "[{{versions[x]}}, )" }
                   """));

        File.WriteAllText(
            Path.Combine(directory, "obj", "project.assets.json"),
            $$"""
              {
                "version": 3,
                "targets": { "net10.0": { {{graph}} } },
                "project": {
                  "version": "1.0.0",
                  "restore": {
                    "projectName": "{{name}}",
                    "projectPath": "{{Path.Combine(directory, name + ".csproj").Replace("\\", "\\\\")}}",
                    "frameworks": { "net10.0": { "targetAlias": "net10.0" } }
                  },
                  "frameworks": { "net10.0": { "targetAlias": "net10.0", "dependencies": { {{direct}} } } }
                }
              }
              """);
    }
}
