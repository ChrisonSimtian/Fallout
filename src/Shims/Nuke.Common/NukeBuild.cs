// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

namespace Nuke.Common;

/// <summary>
/// Transition shim. Inherits from <see cref="Fallout.Common.FalloutBuild"/>. Replace your
/// <c>using Nuke.Common;</c> with <c>using Fallout.Common;</c> and <c>NukeBuild</c> with
/// <c>FalloutBuild</c> when you're ready — see <c>fallout-migrate</c>.
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
