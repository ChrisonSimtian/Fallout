using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Fallout.Build.Execution;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.Utilities;
using Serilog;
using Xunit;

namespace Fallout.Common.Specs.Execution;

/// <summary>
/// Runs a real build through <see cref="BuildManager.Execute{T}"/> twice in one process (FT-9,
/// <see href="https://github.com/Fallout-build/Fallout/issues/314">#314</see>). This checks that each run
/// gets its own state and that nothing carries over.
/// </summary>
/// <remarks>
/// Two process-wide inputs must be fixed for a build to run under a test host. Both are candidates to
/// make per-run later:
/// <list type="bullet">
/// <item><description><see cref="EnvironmentInfo.ArgumentParser"/>: <c>Execute</c> reads the invoked
/// targets from the process arguments, which here belong to the test runner. The specs replace it and
/// restore it afterwards.</description></item>
/// <item><description><see cref="FalloutBuild.RootDirectory"/>: it is set once in a static constructor,
/// so it always points at this repo's root. That root has the <c>.fallout</c> marker the first spec
/// checks.</description></item>
/// </list>
/// </remarks>
[Collection(ProcessGlobalStateCollection.Name)]
public class BuildManagerReentrancySpecs : IDisposable
{
    private static readonly Expression<Func<ReentrancyBuild, Target>>[] defaultTargets = { x => x.Ping };

    private readonly ArgumentParser originalArguments = EnvironmentInfo.ArgumentParser;
    private readonly ILogger originalLogger = Log.Logger;

    public BuildManagerReentrancySpecs()
    {
        // A local build without a `.fallout` marker waits for a key press (UpdateNotificationAttribute).
        // Check the marker for every spec, so a moved spec fails here and does not hang.
        Constants.GetFalloutDirectory(FalloutBuild.RootDirectory).DirectoryExists().Should().BeTrue(
            "the harness runs a local build, which prompts for input when the root has no .fallout marker");

        EnvironmentInfo.ArgumentParser = new ArgumentParser(new[] { nameof(ReentrancyBuild.Ping) });
        ReentrancyBuild.Runs.Clear();
    }

    public void Dispose()
    {
        EnvironmentInfo.ArgumentParser = originalArguments;
        Log.Logger = originalLogger;
        ReentrancyBuild.Runs.Clear();
    }

    [Fact]
    public void A_build_runs_end_to_end_under_the_harness()
    {
        var exitCode = Run();

        exitCode.Should().Be(0);
        ReentrancyBuild.Runs.Should().ContainSingle();
        BuildContext.Current.Should().BeNull("the scope is disposed when Execute returns");
    }

    [Fact]
    public void A_second_run_in_the_same_process_also_succeeds()
    {
        // The first run logs a warning. Host.WriteErrorsAndWarnings then replaces Log.Logger.
        // Before the fix, the replaced logger kept `build.log` open. The second run then failed to
        // reset that file and returned the error exit code without any message.
        Run().Should().Be(0);

        Run().Should().Be(0, "a second in-process run must not inherit the first run's open log file");
    }

    [Fact]
    public void A_second_run_gets_its_own_context_and_services()
    {
        RunSucceeding();
        RunSucceeding();

        var (first, second) = FirstTwoRuns();

        second.Context.Should().NotBeSameAs(first.Context, "FT-2 (#307)");
        second.Parameters.Should().NotBeSameAs(first.Parameters, "FT-4 (#309)");
        second.LogSink.Should().NotBeSameAs(first.LogSink, "FT-6 (#311)");
    }

    [Fact]
    public void A_second_run_does_not_see_the_first_runs_log_events()
    {
        RunSucceeding();
        RunSucceeding();

        var (first, second) = FirstTwoRuns();

        // Each run starts with an empty sink and ends with only its own warning. Carry-over from the
        // first run would show up in the second run's entry snapshot.
        first.EventsOnEntry.Should().BeEmpty();
        second.EventsOnEntry.Should().BeEmpty();
        second.EventsAfterOwnWarning.Should().ContainSingle().Which.Should().Be(second.Sentinel);
    }

    [Fact]
    public void The_static_facades_inside_a_run_point_at_that_run()
    {
        RunSucceeding();

        var run = ReentrancyBuild.Runs.Single();

        run.Parameters.Should().BeSameAs(run.Context.Parameters);
        run.LogSink.Should().BeSameAs(run.Context.LogSink);
    }

    private static int Run() => BuildManager.Execute(defaultTargets);

    private static void RunSucceeding() => Run().Should().Be(0, "the run must reach its target");

    private static (RunObservation First, RunObservation Second) FirstTwoRuns()
    {
        ReentrancyBuild.Runs.Should().HaveCount(2, "both runs must reach their target");
        return (ReentrancyBuild.Runs[0], ReentrancyBuild.Runs[1]);
    }

    /// <summary>What one run of the fixture build observed about its own per-run state.</summary>
    private sealed record RunObservation(
        BuildContext Context,
        ParameterService Parameters,
        Logging.InMemorySink LogSink,
        string Sentinel,
        IReadOnlyCollection<string> EventsOnEntry,
        IReadOnlyCollection<string> EventsAfterOwnWarning);

    /// <summary>
    /// The build the specs run. Its one target records the per-run state it sees and logs one warning,
    /// so a later run can be checked for carry-over.
    /// </summary>
    private class ReentrancyBuild : FalloutBuild
    {
        public static readonly List<RunObservation> Runs = new();

        public Target Ping => _ => _
            .Executes(() =>
            {
                var sink = Logging.InMemorySink.Instance;
                var eventsOnEntry = Texts(sink);
                var sentinel = $"reentrancy-run-{Runs.Count}";

                Log.Warning(sentinel);

                Runs.Add(new RunObservation(
                    Context: BuildContext.Current,
                    Parameters: ParameterService.Instance,
                    LogSink: sink,
                    Sentinel: sentinel,
                    EventsOnEntry: eventsOnEntry,
                    EventsAfterOwnWarning: Texts(sink)));
            });

        private static IReadOnlyCollection<string> Texts(Logging.InMemorySink sink) =>
            sink.LogEvents.Select(x => x.MessageTemplate.Text).ToList();
    }
}
