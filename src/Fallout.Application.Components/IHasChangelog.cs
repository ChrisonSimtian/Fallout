using System;
using System.Linq;
using static Fallout.Application.ChangeLog.ChangelogTasks;
using Fallout.Application;
using Fallout.Application.ChangeLog;

namespace Fallout.Application.Components;

public interface IHasChangelog : IFalloutBuild
{
    // TODO: assert file exists
    string ChangelogFile => RootDirectory / "CHANGELOG.md";
    string NuGetReleaseNotes => GetNuGetReleaseNotes(ChangelogFile, (this as IHasGitRepository)?.GitRepository);
}