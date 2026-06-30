using System;
using System.Linq;
using System.Text.RegularExpressions;

using Fallout.Common;
namespace Fallout.Application.Tools.Gaming.Unity.Logging;

internal class LineMatcher
{
    public string RegexPattern { get; }
    public LogLevel LogLevel { get; }

    public LineMatcher(string regexPattern, LogLevel logLevel)
    {
        RegexPattern = regexPattern;
        LogLevel = logLevel;
    }

    public bool Matches(string message)
    {
        return Regex.IsMatch(message, RegexPattern);
    }
}
