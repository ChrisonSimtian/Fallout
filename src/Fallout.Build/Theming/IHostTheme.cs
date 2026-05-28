using System;
using System.Linq;

namespace Fallout.Common.Execution.Theming;

public interface IHostTheme
{
    void WriteSuccess(string text = null);
    void WriteVerbose(string text = null);
    void WriteDebug(string text = null);
    void WriteInformation(string text = null);
    void WriteWarning(string text = null);
    void WriteError(string text = null);

    internal string FormatSuccess(string text);
    internal string FormatVerbose(string text);
    internal string FormatDebug(string text);
    internal string FormatInformation(string text);
    internal string FormatWarning(string text);
    internal string FormatError(string text);
}
