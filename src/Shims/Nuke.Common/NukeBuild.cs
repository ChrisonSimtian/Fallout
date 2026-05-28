// The TransitionShimGenerator handles namespace prefix swaps (Fallout.* -> Nuke.*)
// but cannot rename types. NukeBuild -> FalloutBuild and INukeBuild ->
// IFalloutBuild were renamed during the rebrand (#59), so we hand-write those
// two type-name shims here. Everything else in Fallout.Common.* keeps the same
// type name and is generated.

namespace Nuke.Common;

/// <summary>
/// Transition shim. Inherits from <see cref="Fallout.Common.FalloutBuild"/>.
/// </summary>
public abstract class NukeBuild : Fallout.Common.FalloutBuild
{
}

/// <summary>
/// Transition shim. Extends <see cref="Fallout.Common.IFalloutBuild"/>.
/// </summary>
public interface INukeBuild : Fallout.Common.IFalloutBuild
{
}
