using FluentAssertions;
using Fallout.Common.Execution;
using Xunit;

namespace Fallout.Common.Specs;

/// <summary>
/// Isolation harness for the per-run <see cref="BuildContext"/> (FT-9 / #314). Asserts the
/// guarantees FT-1/2/4/6 introduced: a build run is scoped, and re-invoking in the same process
/// starts from fresh per-run state with no carry-over. (A full reentrant <c>BuildManager.Execute</c>
/// run needs a Console-driven build harness and is a separate addition.)
/// </summary>
public class BuildContextSpecs
{
    [Fact]
    public void Activate_installs_the_context_as_Current()
    {
        using var context = BuildContext.Activate();
        BuildContext.Current.Should().BeSameAs(context);
    }

    [Fact]
    public void Dispose_clears_Current()
    {
        var context = BuildContext.Activate();
        BuildContext.Current.Should().NotBeNull();

        context.Dispose();

        BuildContext.Current.Should().BeNull();
    }

    [Fact]
    public void Each_run_gets_fresh_per_run_state()
    {
        ParameterService firstParameters;
        Logging.InMemorySink firstLogSink;
        using (var first = BuildContext.Activate())
        {
            firstParameters = first.Parameters;
            firstLogSink = first.LogSink;
        }

        using var second = BuildContext.Activate();
        second.Parameters.Should().NotBeSameAs(firstParameters);
        second.LogSink.Should().NotBeSameAs(firstLogSink);
    }

    [Fact]
    public void Static_facades_route_to_the_active_context()
    {
        using var context = BuildContext.Activate();
        ParameterService.Instance.Should().BeSameAs(context.Parameters);
        Logging.InMemorySink.Instance.Should().BeSameAs(context.LogSink);
    }

    [Fact]
    public void Static_facades_fall_back_outside_a_run()
    {
        // After any run has been disposed, the facades must still resolve (the lazy fallback) rather
        // than throw — and they no longer point at a disposed run's state.
        using (BuildContext.Activate())
        {
        }

        BuildContext.Current.Should().BeNull();
        ParameterService.Instance.Should().NotBeNull();
        Logging.InMemorySink.Instance.Should().NotBeNull();
    }
}
