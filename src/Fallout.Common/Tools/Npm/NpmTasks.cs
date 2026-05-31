using System;
using Serilog.Events;
using Fallout.Application.Tooling;

namespace Fallout.Application.Tools.Npm;

[LogLevelPattern(LogEventLevel.Warning, "^(npmWARN|npm WARN)")]
[LogLevelPattern(LogEventLevel.Debug, "^(npm notice)")]
partial class NpmTasks;
