using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Fallout.Fixtures.Tests;

/// <summary>
/// End-to-end fixture suite. Each case drives a synthetic mini-repo under <c>tests/fixtures/</c> by running its
/// Fallout build out-of-process (<c>dotnet run --project &lt;fixture&gt;/build -- &lt;target&gt; --root &lt;fixture&gt;</c>),
/// then asserts the target succeeded (exit 0) and produced the expected artifact. This exercises the framework
/// the way a real consumer does — engine startup, the DotNet tool wrappers, globbing, packaging — against real
/// project shapes, rather than in-process unit tests.
/// </summary>
[Trait("Category", "Fixtures")]
public class FixtureBuildTests
{
    private readonly ITestOutputHelper _output;

    public FixtureBuildTests(ITestOutputHelper output) => _output = output;

    public static IEnumerable<object[]> Cases =>
    [
        // fixture          target     expected artifacts (relative to the fixture root; empty = exit-code only)
        new object[] { "minimal-app",    "Compile", new[] { "src/App/bin/Debug/net10.0/App.dll" } },
        new object[] { "lib-plus-tests", "Test",    Array.Empty<string>() },
        // multi-TFM: a single build must produce BOTH framework outputs — assert each.
        new object[] { "multi-tfm",      "Compile", new[]
        {
            "src/MultiLib/bin/Debug/netstandard2.0/MultiLib.dll",
            "src/MultiLib/bin/Debug/net10.0/MultiLib.dll",
        } },
        new object[] { "packable",       "Pack",    new[] { "artifacts/Fallout.Fixtures.PackLib.1.2.3.nupkg" } },
    ];

    [Theory]
    [MemberData(nameof(Cases))]
    public void Fallout_build_drives_the_fixture(string fixture, string target, string[] expectedArtifacts)
    {
        var fixtureDir = Path.Combine(FixturesRoot, fixture);
        Directory.Exists(fixtureDir).Should().BeTrue($"fixture '{fixture}' should exist at {fixtureDir}");

        var (exitCode, log) = RunFalloutTarget(fixtureDir, target);
        _output.WriteLine(log);

        exitCode.Should().Be(0, $"the '{target}' target should succeed for the '{fixture}' fixture");

        foreach (var expected in expectedArtifacts)
        {
            var artifact = Path.Combine(fixtureDir, expected.Replace('/', Path.DirectorySeparatorChar));
            File.Exists(artifact).Should().BeTrue($"the '{target}' target should produce '{expected}'");
        }
    }

    private static (int ExitCode, string Log) RunFalloutTarget(string fixtureDir, string target)
    {
        var psi = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = fixtureDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        // dotnet run --project <fixture>/build/_build.csproj -c Debug -- <target> --root <fixture>
        foreach (var arg in new[]
                 {
                     "run", "--project", Path.Combine(fixtureDir, "build", "_build.csproj"),
                     "-c", "Debug", "--", target, "--root", fixtureDir,
                 })
        {
            psi.ArgumentList.Add(arg);
        }

        var sb = new StringBuilder();
        using var process = new Process { StartInfo = psi };
        process.OutputDataReceived += (_, e) => { if (e.Data != null) { lock (sb) sb.AppendLine(e.Data); } };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) { lock (sb) sb.AppendLine(e.Data); } };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (!process.WaitForExit(milliseconds: 5 * 60 * 1000))
        {
            try { process.Kill(entireProcessTree: true); } catch { /* best effort */ }
            return (-1, sb + Environment.NewLine + "[timed out after 5 minutes]");
        }

        process.WaitForExit(); // ensure the async output readers are flushed
        return (process.ExitCode, sb.ToString());
    }

    private static string FixturesRoot { get; } = LocateFixturesRoot();

    private static string LocateFixturesRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "fallout.slnx")))
            dir = dir.Parent;
        if (dir is null)
            throw new InvalidOperationException($"Could not locate the repo root (fallout.slnx) from {AppContext.BaseDirectory}");
        return Path.Combine(dir.FullName, "tests", "fixtures");
    }
}
