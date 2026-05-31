using System;
using Fallout.Application.Execution.Theming;
using Fallout.Kernel;

namespace Fallout.Infrastructure.CI.GitLab;

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
