// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;

namespace Nuke.Common.Git;

/// <summary>Transition shim for <see cref="Fallout.Common.Git.GitRepositoryAttribute"/>.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class GitRepositoryAttribute : Fallout.Common.Git.GitRepositoryAttribute
{
}
