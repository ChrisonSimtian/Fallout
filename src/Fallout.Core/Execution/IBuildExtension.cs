namespace Fallout.Common.Execution;

/// <summary>
/// Root marker for a build extension — the stable seam the plugin SDK builds lifecycle
/// listeners on. Higher <see cref="Priority"/> runs earlier.
/// </summary>
public interface IBuildExtension
{
    float Priority { get; }
}

/// <summary>
/// Build extension invoked once the build has finished (whatever the outcome).
/// </summary>
public interface IOnBuildFinished : IBuildExtension
{
    void OnBuildFinished();
}
