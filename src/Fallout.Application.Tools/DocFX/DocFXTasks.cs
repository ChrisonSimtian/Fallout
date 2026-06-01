using Serilog.Events;
using Fallout.Application.Tooling;

namespace Fallout.Application.Tools.DocFX;

[LogLevelPattern(LogEventLevel.Warning, $@"{TimestampPattern}Info\:\[ExtractMetadata\]No\ files\ are\ found")]
[LogLevelPattern(LogEventLevel.Warning, $@"{TimestampPattern}Warning\:")]
[LogLevelPattern(LogEventLevel.Error, $@"{TimestampPattern}Error\:")]
partial class DocFXTasks
{
    private const string TimestampPattern = @"^\[\d\d\-\d\d\-\d\d\s\d\d\:\d\d\:\d\d\.\d\d\d\]";
}
