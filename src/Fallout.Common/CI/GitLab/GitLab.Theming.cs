using System;
using Fallout.Common.Execution.Theming;
using Fallout.Common.Utilities;

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
