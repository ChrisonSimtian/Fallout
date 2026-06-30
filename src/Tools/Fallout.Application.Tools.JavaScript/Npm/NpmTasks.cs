using System;
using Fallout.Common.Tooling;
using Serilog.Events;

using Fallout.Common;
namespace Fallout.Application.Tools.JavaScript.Npm;

[LogLevelPattern(LogEventLevel.Warning, "^(npmWARN|npm WARN)")]
[LogLevelPattern(LogEventLevel.Debug, "^(npm notice)")]
partial class NpmTasks;
