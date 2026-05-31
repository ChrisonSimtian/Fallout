using System;
using System.Linq;
using static Fallout.Common.ChangeLog.ChangelogTasks;
using Fallout.Application;

namespace Fallout.Application.Components;

public interface IHasChangelog : IFalloutBuild
{
    // TODO: assert file exists
    string ChangelogFile => RootDirectory / "CHANGELOG.md";
    string NuGetReleaseNotes => GetNuGetReleaseNotes(ChangelogFile, (this as IHasGitRepository)?.GitRepository);
}