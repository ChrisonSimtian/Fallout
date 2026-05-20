// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fallout.Common.Tools.Unity.Logging;

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
