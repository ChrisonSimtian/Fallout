// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Reflection;
using Nuke.Common.Utilities;

namespace Nuke.Common.Tooling;

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
