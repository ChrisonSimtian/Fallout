using System.Collections.Generic;

namespace Fallout.Common.Execution;

/// <summary>
/// Read-only projection of a build target: its identity, status, and dependency shape.
/// This is the stable, side-effect-free view that higher layers — and the future plugin SDK —
/// read against. Implementations (e.g. the live <c>ExecutableTarget</c> in <c>Fallout.Build</c>)
/// may carry far more state; this interface exposes only what is safe to observe.
/// </summary>
public interface ITargetModel
{
    /// <summary>The target's name.</summary>
    string Name { get; }

    /// <summary>Human-readable description, if any.</summary>
    string Description { get; }

    /// <summary>Whether this target runs when no targets are explicitly invoked.</summary>
    bool IsDefault { get; }

    /// <summary>Whether this target is listed in help/plan output.</summary>
    bool Listed { get; }

    /// <summary>The target's current lifecycle status.</summary>
    ExecutionStatus Status { get; }

    /// <summary>Names of targets this target depends on for execution (must run before it).</summary>
    IReadOnlyCollection<string> ExecutionDependencyNames { get; }

    /// <summary>Names of targets that only constrain ordering relative to this target.</summary>
    IReadOnlyCollection<string> OrderDependencyNames { get; }

    /// <summary>Names of targets this target triggers once it has run.</summary>
    IReadOnlyCollection<string> TriggerNames { get; }
}
