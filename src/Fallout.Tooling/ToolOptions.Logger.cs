using System;
using System.Reflection;
using Fallout.Common.Utilities;

namespace Fallout.Common.Tooling;

partial class ToolOptions
{
    internal Action<OutputType, string> ProcessLogger { get; set; }

    internal partial Action<OutputType, string> GetLogger()
    {
        var commandAttribute = GetType().GetCustomAttribute<CommandAttribute>().NotNull();
        var toolInstance = commandAttribute.Type.CreateInstance<ToolTasks>();
        return toolInstance.GetLogger(this);
    }
}
