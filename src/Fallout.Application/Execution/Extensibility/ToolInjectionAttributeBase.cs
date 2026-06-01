using System;
using System.Linq;
using System.Reflection;
using Fallout.Application.ValueInjection;

namespace Fallout.Application.Tooling;

public abstract class ToolInjectionAttributeBase : ValueInjectionAttributeBase
{
    public override bool SuppressWarnings => true;

    public abstract ToolRequirement GetRequirement(MemberInfo member);
}
