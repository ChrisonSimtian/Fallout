using System;
using Serilog.Events;
using Fallout.Application.Tooling;

namespace Fallout.Application.Tools.Pulumi;

[LogLevelPattern(LogEventLevel.Warning, "^warning:")]
partial class PulumiTasks;
