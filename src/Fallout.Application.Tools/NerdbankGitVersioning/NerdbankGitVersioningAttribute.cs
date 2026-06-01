using System;
using System.Reflection;
using Fallout.Application.ValueInjection;
using Fallout.Application.Tooling;

using Fallout.Application.CI;
namespace Fallout.Application.Tools.NerdbankGitVersioning;

/// <summary>
/// Injects an instance of <see cref="NerdbankGitVersioning"/> based on the local repository.
/// </summary>
public class NerdbankGitVersioningAttribute : ValueInjectionAttributeBase
{
    public bool UpdateBuildNumber { get; set; }

    public override object GetValue(MemberInfo member, object instance)
    {
        var version = NerdbankGitVersioningTasks.NerdbankGitVersioningGetVersion(s => s
                .DisableProcessOutputLogging()
                .SetFormat(NerdbankGitVersioningFormat.json))
            .Result;

        if (UpdateBuildNumber)
        {
            CiHost.AzurePipelines?.UpdateBuildNumber(version.SemVer2);
            CiHost.TeamCity?.SetBuildNumber(version.SemVer2);
            CiHost.AppVeyor?.UpdateBuildVersion($"{version.SemVer2}.build.{CiHost.AppVeyor.BuildNumber}");
        }

        return version;
    }
}
