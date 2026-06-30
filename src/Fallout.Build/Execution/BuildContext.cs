using System;
using System.Collections.Generic;
using System.Threading;
using Fallout.Common.Execution;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities.Collections;
using Fallout.Common.ValueInjection;

namespace Fallout.Build.Execution;

/// <summary>
/// Holds the state of one build run. <see cref="BuildManager.Execute{T}"/> creates it with
/// <see cref="Activate"/> and disposes it when the run ends.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Current"/> is stored in an <see cref="AsyncLocal{T}"/>, so concurrent runs each see their own context.
/// The context owns the cancellation handlers, the event subscriptions, the <see cref="ParameterService"/>
/// and the in-memory log sink.
/// </para>
/// <para>
/// On dispose, the context also resets some process-wide state: the value-injection cache
/// and the NuGet and npm tool path resolvers. That state is shared, so concurrent runs in one process can still
/// affect each other through it.
/// </para>
/// <para>
/// Part of the engine de-statification plan, which moves the logging scope and the tool-path config onto this context too.
/// See <c>docs/engine-de-statification.md</c>.
/// </para>
/// <para>
/// Internal until the SDK has a public contract (see <see href="https://github.com/Fallout-build/Fallout/issues/307">#307</see>).
/// </para>
/// </remarks>
internal sealed class BuildContext : IDisposable
{
    private static readonly AsyncLocal<BuildContext> currentInstance = new();

    /// <summary>The context for the current build run, or <c>null</c> outside a run.</summary>
    public static BuildContext Current => currentInstance.Value;

    private readonly LinkedList<Action> cancellationHandlers = new();
    private readonly ConsoleCancelEventHandler onCancelKeyPress;
    private readonly EventHandler onToolOptionsCreated;

    /// <summary>
    /// The parameter service for this run. <see cref="ParameterService.Instance"/> returns it while this context
    /// is <see cref="Current"/>. It is discarded with the context, so the next run starts with no arguments
    /// from files or from the commit message.
    /// </summary>
    public ParameterService Parameters { get; } = ParameterService.CreateDefault();

    /// <summary>
    /// The in-memory log sink for this run. FT-6 / <see href="https://github.com/Fallout-build/Fallout/issues/311">#311</see>:
    /// <c>Logging.ConfigureInMemory</c> wires Serilog to this instance at the top of the run and
    /// <c>WriteErrorsAndWarnings</c> reads it back at the end, both inside the scope — so a run's
    /// warnings and errors are discarded with it rather than resurfacing in the next build.
    /// </summary>
    public Logging.InMemorySink LogSink { get; } = new();

    private BuildContext()
    {
        onCancelKeyPress = (_, _) => cancellationHandlers.ForEach(x => x());
        onToolOptionsCreated = (options, _) => VerbosityMapping.Apply((ToolOptions)options);
        Console.CancelKeyPress += onCancelKeyPress;
        ToolOptions.Created += onToolOptionsCreated;
    }

    /// <summary>Creates the ambient context for a build run and installs it as <see cref="Current"/>.</summary>
    public static BuildContext Activate() => currentInstance.Value = new BuildContext();

    public void RegisterCancellationHandler(Action handler) => cancellationHandlers.AddFirst(handler);

    public void UnregisterCancellationHandler(Action handler) => cancellationHandlers.Remove(handler);

    public void Dispose()
    {
        // These subscriptions belong to this instance, so removing them never affects another context.
        // The per-run services (Parameters, LogSink) are dropped with the context, so they need no reset.
        Console.CancelKeyPress -= onCancelKeyPress;
        ToolOptions.Created -= onToolOptionsCreated;

        // The rest is process-wide state. Only the current context may reset it, so a context that was
        // replaced and is disposed late does not clear the state of the newer run.
        if (!ReferenceEquals(currentInstance.Value, this))
        {
            return;
        }

        ValueInjectionUtility.ClearCache();
        NuGetToolPathResolver.Reset();
        NpmToolPathResolver.Reset();

        currentInstance.Value = null;
    }
}
