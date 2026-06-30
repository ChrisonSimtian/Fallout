using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fallout.Common.CI;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.Tooling;

namespace Fallout.Common;

public interface IFalloutBuild
{
    void ReportSummary(Configure<Dictionary<string, string>> configurator = null);

    internal IReadOnlyCollection<ExecutableTarget> ExecutableTargets { get; }
    internal IReadOnlyCollection<IBuildExtension> BuildExtensions { get; }
    internal bool IsInterceptorExecution { get; }
    internal string[] LoadedLocalProfiles { get; }
    internal bool IsOutputEnabled(DefaultOutput output);

    IReadOnlyCollection<ITargetModel> ExecutionPlan { get; }
    IReadOnlyCollection<ITargetModel> InvokedTargets { get; }
    IReadOnlyCollection<ITargetModel> SkippedTargets { get; }
    IReadOnlyCollection<ITargetModel> ScheduledTargets { get; }
    IReadOnlyCollection<ITargetModel> RunningTargets { get; }
    IReadOnlyCollection<ITargetModel> AbortedTargets { get; }
    IReadOnlyCollection<ITargetModel> FailedTargets { get; }
    IReadOnlyCollection<ITargetModel> SucceededTargets { get; }
    IReadOnlyCollection<ITargetModel> FinishedTargets { get; }

    bool IsSucceeding { get; }
    bool IsFailing { get; }
    bool IsFinished { get; }
    int? ExitCode { get; set; }

    AbsolutePath RootDirectory { get; }
    AbsolutePath TemporaryDirectory { get; }
    AbsolutePath BuildAssemblyFile { get; }
    AbsolutePath BuildAssemblyDirectory { get; }
    AbsolutePath BuildProjectDirectory { get; }
    AbsolutePath BuildProjectFile { get; }

    Verbosity Verbosity { get; }
    Host Host { get; }
    bool Plan { get; }
    bool Help { get; }
    bool NoLogo { get; }
    bool IsLocalBuild { get; }
    bool IsServerBuild { get; }
    bool Continue { get; }
    Partition Partition { get; }

    T TryGetValue<T>(Expression<Func<T>> parameterExpression) where T : class;

    T TryGetValue<T>(Expression<Func<object>> parameterExpression);
}
