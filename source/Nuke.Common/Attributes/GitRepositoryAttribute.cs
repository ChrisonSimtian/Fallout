// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common.ValueInjection;

namespace Nuke.Common.Git;

/// <summary>
/// Injects an instance of <see cref="GitRepository"/> based on the local repository.
/// </summary>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
public class GitRepositoryAttribute : ValueInjectionAttributeBase
{
    public override object GetValue(MemberInfo member, object instance)
    {
        return GitRepository.FromLocalDirectory(Build.RootDirectory);
    }
}
