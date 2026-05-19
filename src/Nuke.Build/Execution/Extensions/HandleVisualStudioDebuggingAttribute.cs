// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Nuke.Common.IO;

namespace Nuke.Common.Execution;

internal class HandleVisualStudioDebuggingAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    private const int TimeoutInMilliseconds = 10_000;

    private AbsolutePath VisualStudioDebugFile => Constants.GetVisualStudioDebugFile(Build.RootDirectory);

    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        if (!ParameterService.GetParameter<bool>(Constants.VisualStudioDebugParameterName))
            return;

        VisualStudioDebugFile.WriteAllText(Process.GetCurrentProcess().Id.ToString());
        Assert.True(SpinWait.SpinUntil(() => Debugger.IsAttached, millisecondsTimeout: TimeoutInMilliseconds),
            $"VisualStudio debugger was not attached within {TimeoutInMilliseconds} milliseconds");
    }
}
