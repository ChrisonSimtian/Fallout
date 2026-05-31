using System;
using System.Linq;
using System.Reflection;
using NuGet.Versioning;
using Fallout.Common.Utilities;
using Fallout.Application.ValueInjection;

namespace Fallout.Application.Tooling;

public class LatestNpmVersionAttribute : ValueInjectionAttributeBase
{
    private readonly string _packageId;

    public LatestNpmVersionAttribute(string packageId)
    {
        _packageId = packageId;
    }

    public override object GetValue(MemberInfo member, object instance)
    {
        var version = ToolingServices.Versions.GetLatestNpmVersion(_packageId).GetAwaiter().GetResult();
        return member.GetMemberType() == typeof(string)
            ? version
            : SemanticVersion.Parse(version);
    }
}
