using System;
using System.Linq;
using Serilog.Events;
using Fallout.Application.Tooling;

namespace Fallout.Application.Tools.Docker;

[LogErrorAsStandard]
[LogLevelPattern(LogEventLevel.Warning, "^WARNING!")]
partial class DockerTasks;
