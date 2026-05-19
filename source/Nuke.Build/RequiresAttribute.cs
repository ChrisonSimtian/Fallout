// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Reflection;
using Nuke.Common.Tooling;

namespace Nuke.Common;

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
