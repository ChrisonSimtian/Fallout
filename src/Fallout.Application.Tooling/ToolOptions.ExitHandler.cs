using System;

namespace Fallout.Application.Tooling;

partial class ToolOptions
{
    internal Func<ToolOptions, IProcess, object> ProcessExitHandler { get; set; }
}
