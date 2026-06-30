using System;
using Fallout.Common.Tooling;

using Fallout.Common;
namespace Fallout.Application.Tools.Shell.PowerShell;

partial class PowerShellTasks
{
    protected override string GetToolPath(ToolOptions options = null)
    {
        return ToolPathResolver.GetPathExecutable(EnvironmentInfo.IsWin ? "powershell" : "pwsh");
    }
}
