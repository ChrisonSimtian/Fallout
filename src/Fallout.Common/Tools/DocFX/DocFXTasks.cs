// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using Fallout.Common.Tooling;
using Serilog.Events;

namespace Fallout.Common.Tools.DocFX;

[LogLevelPattern(LogEventLevel.Warning, $@"{TimestampPattern}Info\:\[ExtractMetadata\]No\ files\ are\ found")]
[LogLevelPattern(LogEventLevel.Warning, $@"{TimestampPattern}Warning\:")]
[LogLevelPattern(LogEventLevel.Error, $@"{TimestampPattern}Error\:")]
partial class DocFXTasks
{
    private const string TimestampPattern = @"^\[\d\d\-\d\d\-\d\d\s\d\d\:\d\d\:\d\d\.\d\d\d\]";
}
