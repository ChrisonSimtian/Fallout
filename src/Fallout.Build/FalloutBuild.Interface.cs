using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.ValueInjection;

namespace Fallout.Common;

public abstract partial class FalloutBuild
{
    IReadOnlyCollection<ExecutableTarget> IFalloutBuild.ExecutableTargets => ExecutableTargets;

    // The public properties stay ExecutableTarget-typed for derived builds; the IFalloutBuild
    // contract exposes the Core-pure ITargetModel projection (IReadOnlyCollection<T> is covariant).
    IReadOnlyCollection<ITargetModel> IFalloutBuild.ExecutionPlan => ExecutionPlan;
    IReadOnlyCollection<ITargetModel> IFalloutBuild.InvokedTargets => InvokedTargets;
    IReadOnlyCollection<ITargetModel> IFalloutBuild.SkippedTargets => SkippedTargets;
    IReadOnlyCollection<ITargetModel> IFalloutBuild.ScheduledTargets => ScheduledTargets;
    IReadOnlyCollection<ITargetModel> IFalloutBuild.RunningTargets => RunningTargets;
    IReadOnlyCollection<ITargetModel> IFalloutBuild.AbortedTargets => AbortedTargets;
    IReadOnlyCollection<ITargetModel> IFalloutBuild.FailedTargets => FailedTargets;
    IReadOnlyCollection<ITargetModel> IFalloutBuild.SucceededTargets => SucceededTargets;
    IReadOnlyCollection<ITargetModel> IFalloutBuild.FinishedTargets => FinishedTargets;

    bool IFalloutBuild.IsInterceptorExecution => IsInterceptorExecution;
    string[] IFalloutBuild.LoadedLocalProfiles => LoadedLocalProfiles;
    bool IFalloutBuild.IsOutputEnabled(DefaultOutput output) => IsOutputEnabled(output);

    AbsolutePath IFalloutBuild.RootDirectory => RootDirectory;
    AbsolutePath IFalloutBuild.TemporaryDirectory => TemporaryDirectory;
    AbsolutePath IFalloutBuild.BuildAssemblyFile => BuildAssemblyFile;
    AbsolutePath IFalloutBuild.BuildAssemblyDirectory => BuildAssemblyDirectory;
    AbsolutePath IFalloutBuild.BuildProjectDirectory => BuildProjectDirectory;
    AbsolutePath IFalloutBuild.BuildProjectFile => BuildProjectFile;
    Verbosity IFalloutBuild.Verbosity => Verbosity;
    Host IFalloutBuild.Host => Host;
    bool IFalloutBuild.Plan => Plan;
    bool IFalloutBuild.Help => Help;
    bool IFalloutBuild.NoLogo => NoLogo;
    bool IFalloutBuild.IsLocalBuild => IsLocalBuild;
    bool IFalloutBuild.IsServerBuild => IsServerBuild;
    bool IFalloutBuild.Continue => Continue;

    T IFalloutBuild.TryGetValue<T>(Expression<Func<T>> parameterExpression)
    {
        return ValueInjectionUtility.TryGetValue(parameterExpression);
    }

    T IFalloutBuild.TryGetValue<T>(Expression<Func<object>> parameterExpression)
    {
        return ValueInjectionUtility.TryGetValue<T>(parameterExpression);
    }
}
