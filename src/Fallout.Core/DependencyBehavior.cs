namespace Fallout.Common;

/// <summary>
/// The behavior of dependent targets if the target is skipped.
/// </summary>
public enum DependencyBehavior
{
    /// <summary>
    /// Skip all dependencies which are not required by another target.
    /// </summary>
    Skip,

    /// <summary>
    /// Execute all dependencies.
    /// </summary>
    Execute
}
