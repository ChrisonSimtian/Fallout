using System;
using System.Linq;
using System.Reflection;

namespace Fallout.Common.Tooling;

public class NpmPackageAttribute : ToolInjectionAttributeBase
{
    private readonly string _packageId;
    private readonly string _packageExecutable;

    public NpmPackageAttribute(string packageId, string packageExecutable = null)
    {
        _packageId = packageId;
        _packageExecutable = packageExecutable;
    }

    public string Version { get; set; }

    public override ToolRequirement GetRequirement(MemberInfo member)
    {
        return new NpmPackageRequirement(_packageId, Version);
    }

    public override object GetValue(MemberInfo member, object instance)
    {
        var name = _packageExecutable ?? member.Name.ToLowerInvariant();
        return ToolResolver.TryGetEnvironmentTool(name) ??
               ToolResolver.GetNpmTool(name);
    }
}
