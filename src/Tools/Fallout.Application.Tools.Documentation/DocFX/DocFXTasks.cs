using Fallout.Common.Tooling;
using Serilog.Events;

using Fallout.Common;
namespace Fallout.Application.Tools.Documentation.DocFX;

[LogLevelPattern(LogEventLevel.Warning, $@"{TimestampPattern}Info\:\[ExtractMetadata\]No\ files\ are\ found")]
[LogLevelPattern(LogEventLevel.Warning, $@"{TimestampPattern}Warning\:")]
[LogLevelPattern(LogEventLevel.Error, $@"{TimestampPattern}Error\:")]
partial class DocFXTasks
{
    private const string TimestampPattern = @"^\[\d\d\-\d\d\-\d\d\s\d\d\:\d\d\:\d\d\.\d\d\d\]";
}
