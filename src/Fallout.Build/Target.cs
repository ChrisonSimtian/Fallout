using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Execution;

namespace Fallout.Common;

/// <summary>
/// Defines a target.
/// </summary>
/// <example>
/// <code>
/// Target Restore => _ => _
///     .DependsOn(/* dependent target */)
///     .OnlyWhen(/* conditions for executing the target */)
///     .Executes(() => /* actions to be executed */);
/// </code>
/// </example>
public delegate ITargetDefinition Target(ITargetDefinition definition);

public delegate ITargetDefinition Setup(ITargetDefinition definition);

public delegate ITargetDefinition Cleanup(ITargetDefinition definition);

public static class ExecutableTargetExtensions
{
    // Accept the ITargetModel projection so these bind to the IFalloutBuild collection properties
    // too; the elements are always ExecutableTarget at runtime, which carries the Factory identity.
    public static bool Contains(this IEnumerable<ITargetModel> targets, Target target)
    {
        return targets.OfType<ExecutableTarget>().Any(x => x.Factory.Equals(target));
    }

    public static bool Contains(this IEnumerable<ITargetModel> targets, Setup target)
    {
        return targets.OfType<ExecutableTarget>().Any(x => x.Factory.Equals(target));
    }

    public static bool Contains(this IEnumerable<ITargetModel> targets, Cleanup target)
    {
        return targets.OfType<ExecutableTarget>().Any(x => x.Factory.Equals(target));
    }
}
