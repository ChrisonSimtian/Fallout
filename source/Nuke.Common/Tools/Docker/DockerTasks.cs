// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Nuke.Common.Tooling;
using Serilog.Events;

namespace Nuke.Common.Tools.Docker;

[LogErrorAsStandard]
[LogLevelPattern(LogEventLevel.Warning, "^WARNING!")]
partial class DockerTasks;
