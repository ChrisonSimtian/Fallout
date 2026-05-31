
namespace Fallout.Common.CI;

/// <summary>
/// Superseded by <see cref="IBuildHost"/> (ADR-0005). Kept for backwards compatibility; its eventual
/// removal is a breaking change batched to the next yearly major, not done here.
/// </summary>
public interface IBuildServer
{
    string Branch { get; }

    string Commit { get; }
}
