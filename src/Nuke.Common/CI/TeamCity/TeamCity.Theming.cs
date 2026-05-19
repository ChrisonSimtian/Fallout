// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using Nuke.Common.Execution;
using Nuke.Common.Execution.Theming;
using Nuke.Common.Utilities;

namespace Nuke.Common.CI.TeamCity;

public partial class TeamCity
{
    internal override IHostTheme Theme => AnsiConsoleHostTheme.Default256AnsiColorTheme;
    internal override string OutputTemplate => Logging.StandardOutputTemplate;

    protected internal override IDisposable WriteBlock(string text)
    {
        return DelegateDisposable.CreateBracket(
            () => OpenBlock(text),
            () => CloseBlock(text));
    }

    protected internal override void ReportWarning(string text, string details = null)
    {
        WriteWarning(text);
    }

    protected internal override void ReportError(string text, string details = null)
    {
        WriteError(text, details);
    }

    protected internal override bool FilterMessage(string message)
    {
        if (!message.ContainsOrdinalIgnoreCase("##teamcity["))
            return false;

        Console.WriteLine(message);
        return true;
    }
}
