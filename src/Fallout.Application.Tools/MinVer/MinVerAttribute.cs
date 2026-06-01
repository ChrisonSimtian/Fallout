using System;
using System.Linq;
using System.Reflection;
using Fallout.Application.ValueInjection;
using Fallout.Application.Tooling;

using Fallout.Application.CI;
namespace Fallout.Application.Tools.MinVer;

/// <summary>
/// Injects an instance of <see cref="MinVer"/> based on the local repository.
/// </summary>
public class MinVerAttribute : ValueInjectionAttributeBase
{
    public string Framework { get; set; }
    public bool UpdateBuildNumber { get; set; }

    public override object GetValue(MemberInfo member, object instance)
    {
        var version = MinVerTasks.MinVer(s => s
                .SetFramework(Framework)
                .DisableProcessOutputLogging())
            .Result;

        if (UpdateBuildNumber)
        {
            CiHost.AzurePipelines?.UpdateBuildNumber(version.Version);
            CiHost.TeamCity?.SetBuildNumber(version.Version);
            CiHost.AppVeyor?.UpdateBuildVersion(version.Version);
        }

        return version;
    }
}
