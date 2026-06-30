using System;
using System.Linq;
using Fallout.Common.Tooling;
using Serilog.Events;

using Fallout.Common;
namespace Fallout.Application.Tools.Containers.Docker;

[LogErrorAsStandard]
[LogLevelPattern(LogEventLevel.Warning, "^WARNING!")]
partial class DockerTasks;
