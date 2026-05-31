using System;
using Fallout.Common.Utilities;
using Fallout.Application.Execution.Theming;

namespace Fallout.Common.CI.GitLab;

public partial class GitLab
{
    internal override IHostTheme Theme => AnsiConsoleHostTheme.Default256AnsiColorTheme;

    protected internal override IDisposable WriteBlock(string text)
    {
        return DelegateDisposable.CreateBracket(
            () => BeginSection(text),
            () => EndSection(text));
    }
}
