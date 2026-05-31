using System;
using Fallout.Common.Execution.Theming;
using Fallout.Common.Utilities;

namespace Fallout.Common.CI.Forgejo;

// Reporting half of the runtime-host seam (IBuildReporter, via the Host base). Forgejo Actions speaks
// the GitHub workflow-command dialect, so reporting COMPOSES the shared WorkflowCommands helper rather
// than coupling to the GitHubActions adapter (ADR-0005).
public partial class Forgejo
{
    internal override IHostTheme Theme => AnsiConsoleHostTheme.Default256AnsiColorTheme;

    protected internal override IDisposable WriteBlock(string text)
    {
        return DelegateDisposable.CreateBracket(
            () => WorkflowCommands.Group(text),
            () => WorkflowCommands.EndGroup(text));
    }

    protected internal override void ReportWarning(string text, string details = null)
    {
        WorkflowCommands.WriteWarning(text);
    }

    protected internal override void ReportError(string text, string details = null)
    {
        WorkflowCommands.WriteError(text);
    }

    protected internal override bool FilterMessage(string message)
    {
        if (!message.StartsWith("::") && !message.StartsWith("##["))
            return false;

        Console.WriteLine(message);
        return true;
    }
}
