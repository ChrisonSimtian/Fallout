using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fallout.Application.Execution;
using Fallout.Application.CI;
using Fallout.Application.Tooling;
using Fallout.Core.IO;

namespace Fallout.Application;

public interface IFalloutBuild
{
    void ReportSummary(Configure<Dictionary<string, string>> configurator = null);

    internal IReadOnlyCollection<ExecutableTarget> ExecutableTargets { get; }
    internal IReadOnlyCollection<IBuildExtension> BuildExtensions { get; }
    internal bool IsInterceptorExecution { get; }
    internal string[] LoadedLocalProfiles { get; }
    internal bool IsOutputEnabled(DefaultOutput output);

    IReadOnlyCollection<ExecutableTarget> ExecutionPlan { get; }
    IReadOnlyCollection<ExecutableTarget> InvokedTargets { get; }
    IReadOnlyCollection<ExecutableTarget> SkippedTargets { get; }
    IReadOnlyCollection<ExecutableTarget> ScheduledTargets { get; }
    IReadOnlyCollection<ExecutableTarget> RunningTargets { get; }
    IReadOnlyCollection<ExecutableTarget> AbortedTargets { get; }
    IReadOnlyCollection<ExecutableTarget> FailedTargets { get; }
    IReadOnlyCollection<ExecutableTarget> SucceededTargets { get; }
    IReadOnlyCollection<ExecutableTarget> FinishedTargets { get; }

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

    public T TryGetValue<T>(Expression<Func<T>> parameterExpression) where T : class;

    public T TryGetValue<T>(Expression<Func<object>> parameterExpression);
}
