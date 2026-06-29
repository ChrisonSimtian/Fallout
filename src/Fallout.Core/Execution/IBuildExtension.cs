using System.Collections.Generic;

namespace Fallout.Common.Execution;

/// <summary>
/// Root marker for a build extension — the stable seam the plugin SDK builds lifecycle
/// listeners on. Higher <see cref="Priority"/> runs earlier.
/// </summary>
public interface IBuildExtension
{
    float Priority { get; }
}

/// <summary>Build extension invoked once the executable targets have been created.</summary>
public interface IOnBuildCreated : IBuildExtension
{
    void OnBuildCreated(IReadOnlyCollection<ITargetModel> executableTargets);
}

/// <summary>Build extension invoked once the build has been initialized and the plan computed.</summary>
public interface IOnBuildInitialized : IBuildExtension
{
    void OnBuildInitialized(
        IReadOnlyCollection<ITargetModel> executableTargets,
        IReadOnlyCollection<ITargetModel> executionPlan);
}

/// <summary>Build extension invoked when a target is skipped.</summary>
public interface IOnTargetSkipped : IBuildExtension
{
    void OnTargetSkipped(ITargetModel target);
}

/// <summary>Build extension invoked when a target starts running.</summary>
public interface IOnTargetRunning : IBuildExtension
{
    void OnTargetRunning(ITargetModel target);
}

/// <summary>Build extension invoked when a target succeeds.</summary>
public interface IOnTargetSucceeded : IBuildExtension
{
    void OnTargetSucceeded(ITargetModel target);
}

/// <summary>Build extension invoked when a target fails.</summary>
public interface IOnTargetFailed : IBuildExtension
{
    void OnTargetFailed(ITargetModel target);
}

/// <summary>Build extension invoked once the build has finished (whatever the outcome).</summary>
public interface IOnBuildFinished : IBuildExtension
{
    void OnBuildFinished();
}
