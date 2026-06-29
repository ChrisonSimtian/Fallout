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
