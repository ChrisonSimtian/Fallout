using System;
using System.Collections.Generic;
using System.Threading;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities.Collections;
using Fallout.Common.ValueInjection;

namespace Fallout.Common.Execution;

/// <summary>
/// Per-run, process-ambient build state. Activated once at the top of <see cref="BuildManager.Execute{T}"/>
/// and disposed at the end of the run, so the process-global statics a build touches are owned by a
/// scope rather than leaking across invocations (the cleanup FT-1 centralised now lives here).
/// The static surface (e.g. <see cref="BuildManager.CancellationHandler"/>) reads through
/// <see cref="Current"/>; <c>AsyncLocal</c> keeps concurrent/nested runs isolated.
/// </summary>
/// <remarks>
/// FT-2 / <see href="https://github.com/Fallout-build/Fallout/issues/307">#307</see>. Intentionally
/// <c>internal</c> — not a public contract until the SDK lands (milestone #7). Subsequent steps move
/// the per-run services (parameters, logging scope, tool-path config) onto this context.
/// </remarks>
internal sealed class BuildContext : IDisposable
{
    private static readonly AsyncLocal<BuildContext> s_current = new();

    /// <summary>The context for the current build run, or <c>null</c> outside a run.</summary>
    public static BuildContext Current => s_current.Value;

    private readonly LinkedList<Action> _cancellationHandlers = new();
    private readonly ConsoleCancelEventHandler _onCancelKeyPress;
    private readonly EventHandler _onToolOptionsCreated;

    /// <summary>The per-run parameter service (FT-4 / #309) — replaces the process-global
    /// <c>ParameterService.Instance</c>, so prod and tests exercise the same instance form.</summary>
    public ParameterService Parameters { get; } =
        new(() => EnvironmentInfo.ArgumentParser, () => EnvironmentInfo.Variables);

    /// <summary>The per-run in-memory log sink (FT-6 / #311) — Serilog writes to it during the run and
    /// <c>WriteErrorsAndWarnings</c> reads it; per-run scope keeps log events from carrying across runs.</summary>
    public Logging.InMemorySink LogSink { get; } = new();

    private BuildContext()
    {
        _onCancelKeyPress = (_, _) => _cancellationHandlers.ForEach(x => x());
        _onToolOptionsCreated = (options, _) => VerbosityMapping.Apply((ToolOptions)options);
        Console.CancelKeyPress += _onCancelKeyPress;
        ToolOptions.Created += _onToolOptionsCreated;
    }

    /// <summary>Creates the ambient context for a build run and installs it as <see cref="Current"/>.</summary>
    public static BuildContext Activate() => s_current.Value = new BuildContext();

    public void RegisterCancellationHandler(Action handler) => _cancellationHandlers.AddFirst(handler);

    public void UnregisterCancellationHandler(Action handler) => _cancellationHandlers.Remove(handler);

    public void Dispose()
    {
        // Undo this run's process-global state (the FT-1 cleanup, now owned by the scope).
        Console.CancelKeyPress -= _onCancelKeyPress;
        ToolOptions.Created -= _onToolOptionsCreated;
        // The per-run LogSink (FT-6) and Parameters (FT-4) are owned by this context and discarded
        // with it — no explicit reset needed. These remaining statics aren't yet context-owned:
        ValueInjectionUtility.ClearCache();
        NuGetToolPathResolver.Reset();
        NpmToolPathResolver.Reset();

        if (ReferenceEquals(s_current.Value, this))
            s_current.Value = null;
    }
}
