// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Fallout.Common;
using static Fallout.Common.ChangeLog.ChangelogTasks;

namespace Fallout.Components;

public interface IHasChangelog : IFalloutBuild
{
    // TODO: assert file exists
    string ChangelogFile => RootDirectory / "CHANGELOG.md";
    string NuGetReleaseNotes => GetNuGetReleaseNotes(ChangelogFile, (this as IHasGitRepository)?.GitRepository);
}