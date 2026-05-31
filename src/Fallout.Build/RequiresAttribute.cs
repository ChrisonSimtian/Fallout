using System;
using System.Linq;
using System.Reflection;
using Fallout.Common;
using Fallout.Application.Tooling;

namespace Fallout.Application;

public abstract class RequiresAttribute : Attribute
{
    public abstract ToolRequirement GetRequirement();
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class RequiresAttribute<T> : RequiresAttribute
    where T : IRequireTool
{
    public string Version { get; set; }

    public override ToolRequirement GetRequirement()
    {
        return typeof(T).GetCustomAttribute<ToolAttribute>().NotNull().GetRequirement(Version);
    }
}
