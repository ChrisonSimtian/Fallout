using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Fallout.Build.Execution;
using Fallout.Common.Execution;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Fallout.Common.Specs;

[Collection(ProcessGlobalStateCollection.Name)]
public class LogSinkScopeQaSpecs
{
    private static LogEvent Event(string text, LogEventLevel level = LogEventLevel.Warning)
        => new(DateTimeOffset.UtcNow, level, null, new MessageTemplateParser().Parse(text), []);

    private static string[] Texts(Logging.InMemorySink sink)
        => sink.LogEvents.Select(x => x.RenderMessage()).ToArray();

    [Fact]
    public void Each_run_gets_its_own_sink()
    {
        // Arrange
        using var runA = BuildContext.Activate();
        var sinkA = Logging.InMemorySink.Instance;

        // Act
        using var runB = BuildContext.Activate();
        var sinkB = Logging.InMemorySink.Instance;

        // Assert
        sinkB.Should().NotBeSameAs(sinkA);
    }

    [Fact]
    public void Facade_returns_the_same_sink_throughout_one_run()
    {
        // Arrange
        using var run = BuildContext.Activate();

        // Act
        var first = Logging.InMemorySink.Instance;
        var second = Logging.InMemorySink.Instance;

        // Assert
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void Events_from_run_A_are_not_visible_in_run_B()
    {
        // Arrange
        using (BuildContext.Activate())
        {
            Logging.InMemorySink.Instance.Emit(Event("from A"));
            Texts(Logging.InMemorySink.Instance).Should().Contain("from A");
        }

        // Act
        using var runB = BuildContext.Activate();

        // Assert
        Texts(Logging.InMemorySink.Instance).Should().BeEmpty();
    }

    [Fact]
    public void Events_from_run_B_do_not_appear_in_run_A_sink()
    {
        // Arrange
        using var runA = BuildContext.Activate();
        var sinkA = Logging.InMemorySink.Instance;

        // Act
        using var runB = BuildContext.Activate();
        Logging.InMemorySink.Instance.Emit(Event("from B"));

        // Assert
        Texts(sinkA).Should().BeEmpty();
    }

    [Fact]
    public void Fallback_outside_a_run_is_stable()
    {
        // Arrange
        BuildContext.Current.Should().BeNull();

        // Act
        var first = Logging.InMemorySink.Instance;
        var second = Logging.InMemorySink.Instance;

        // Assert
        first.Should().NotBeNull();
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void Fallback_is_not_the_sink_of_a_run()
    {
        // Arrange
        var fallback = Logging.InMemorySink.Instance;

        // Act
        using var run = BuildContext.Activate();

        // Assert
        Logging.InMemorySink.Instance.Should().NotBeSameAs(fallback);
    }

    [Fact]
    public void Run_events_do_not_leak_into_the_fallback()
    {
        // Arrange
        var fallback = Logging.InMemorySink.Instance;
        fallback.Clear();

        // Act
        using (BuildContext.Activate())
            Logging.InMemorySink.Instance.Emit(Event("in run"));

        // Assert
        Texts(fallback).Should().BeEmpty();
    }

    [Fact]
    public async Task Concurrent_runs_do_not_see_each_others_events()
    {
        // Arrange
        using var gate = new System.Threading.Barrier(2);

        async Task<(string[] Texts, Logging.InMemorySink Sink)> Run(string tag)
        {
            await Task.Yield();
            using var ctx = BuildContext.Activate();
            var sink = Logging.InMemorySink.Instance;
            gate.SignalAndWait();
            for (var i = 0; i < 200; i++)
            {
                sink.Emit(Event($"{tag}-{i}"));
                if (i % 20 == 0) await Task.Yield();
            }

            gate.SignalAndWait();
            return (Texts(Logging.InMemorySink.Instance), sink);
        }

        // Act
        var results = await Task.WhenAll(Task.Run(() => Run("A")), Task.Run(() => Run("B")));

        // Assert
        results[0].Sink.Should().NotBeSameAs(results[1].Sink);
        results[0].Texts.Should().HaveCount(200).And.OnlyContain(x => x.StartsWith("A-"));
        results[1].Texts.Should().HaveCount(200).And.OnlyContain(x => x.StartsWith("B-"));
    }

    [Fact]
    public async Task Facade_in_a_child_task_resolves_the_parent_runs_sink()
    {
        // Arrange
        using var run = BuildContext.Activate();
        var expected = Logging.InMemorySink.Instance;

        // Act
        var seen = await Task.Run(() => Logging.InMemorySink.Instance);

        // Assert
        seen.Should().BeSameAs(expected);
    }

    [Fact]
    public void End_of_run_report_contains_only_that_runs_warnings_and_errors()
    {
        // Arrange
        using (BuildContext.Activate())
            Logging.InMemorySink.Instance.Emit(Event("old warning"));

        // Act
        using var run = BuildContext.Activate();
        Logging.InMemorySink.Instance.Emit(Event("new warning"));
        Logging.InMemorySink.Instance.Emit(Event("new error", LogEventLevel.Error));

        // Assert
        Texts(Logging.InMemorySink.Instance).Should().Equal("new warning", "new error");
    }

    [Fact]
    public void Clear_empties_the_current_runs_sink()
    {
        // Arrange
        using var run = BuildContext.Activate();
        var sink = Logging.InMemorySink.Instance;
        sink.Emit(Event("x"));

        // Act
        sink.Clear();

        // Assert
        sink.LogEvents.Should().BeEmpty();
    }

    [Fact]
    public void Clear_in_one_run_leaves_another_runs_sink_intact()
    {
        // Arrange
        using var runA = BuildContext.Activate();
        var sinkA = Logging.InMemorySink.Instance;
        sinkA.Emit(Event("keep"));
        using var runB = BuildContext.Activate();

        // Act
        Logging.InMemorySink.Instance.Clear();

        // Assert
        Texts(sinkA).Should().Equal("keep");
    }

    [Fact]
    public void Disposing_a_run_does_not_clear_the_fallback()
    {
        // Arrange
        var fallback = Logging.InMemorySink.Instance;
        fallback.Clear();
        fallback.Emit(Event("fallback event"));

        // Act
        using (BuildContext.Activate()) { }

        // Assert
        Texts(fallback).Should().Equal("fallback event");
        fallback.Clear();
    }

    [Fact]
    public void Cancellation_handlers_see_the_run_sink_on_a_thread_without_the_run_context()
    {
        // Arrange
        using var context = BuildContext.Activate();
        context.LogSink.Emit(Event("from the run"));
        Logging.InMemorySink seen = null;
        context.RegisterCancellationHandler(() => seen = Logging.InMemorySink.Instance);

        // Act (Console.CancelKeyPress fires on a thread that does not carry the run's AsyncLocal)
        using (ExecutionContext.SuppressFlow())
        {
            var thread = new Thread(context.RunCancellationHandlers);
            thread.Start();
            thread.Join();
        }

        // Assert
        seen.Should().BeSameAs(context.LogSink);
        Texts(seen).Should().Equal("from the run");
    }

    [Fact]
    public void Configured_sink_collects_run_warnings_for_the_summary()
    {
        // Arrange
        using var context = BuildContext.Activate();
        var logger = new Serilog.LoggerConfiguration()
            .WriteTo.Sink(Logging.InMemorySink.Instance, LogEventLevel.Warning)
            .CreateLogger();

        // Act
        logger.Information("ignored");
        logger.Warning("kept");

        // Assert
        Texts(context.LogSink).Should().Equal("kept");
    }
}
