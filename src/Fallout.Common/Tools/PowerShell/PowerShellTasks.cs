using System;
using Fallout.Application.Tooling;
using Fallout.Infrastructure.Tooling;
using Fallout.Common;

namespace Fallout.Application.Tools.PowerShell;

partial class PowerShellTasks
{
    protected override string GetToolPath(ToolOptions options = null)
    {
        return ToolPathResolver.GetPathExecutable(EnvironmentInfo.IsWin ? "powershell" : "pwsh");
    }
}
