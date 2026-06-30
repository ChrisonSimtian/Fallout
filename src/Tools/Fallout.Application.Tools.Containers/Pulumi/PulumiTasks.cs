using System;
using Fallout.Common.Tooling;
using Serilog.Events;

using Fallout.Common;
namespace Fallout.Application.Tools.Containers.Pulumi;

[LogLevelPattern(LogEventLevel.Warning, "^warning:")]
partial class PulumiTasks;
