using System;
using System.Linq;
using System.Reflection;
using NuGet.Versioning;
using Fallout.Common.Utilities;
using Fallout.Application.ValueInjection;
using Fallout.Infrastructure.Tooling;

namespace Fallout.Application.Tooling;

public class LatestNuGetVersionAttribute : ValueInjectionAttributeBase
{
    private readonly string _packageId;

    public LatestNuGetVersionAttribute(string packageId)
    {
        _packageId = packageId;
    }

    public bool IncludePrerelease { get; set; }
    public bool IncludeUnlisted { get; set; }

    public override object GetValue(MemberInfo member, object instance)
    {
        var version = NuGetVersionResolver.GetLatestVersion(_packageId, IncludePrerelease, IncludeUnlisted).GetAwaiter().GetResult();
        return member.GetMemberType() == typeof(string)
            ? version
            : NuGetVersion.Parse(version);
    }
}
