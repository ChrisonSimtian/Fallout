using System;
using System.Diagnostics;
using Fallout.Application.Tooling;

namespace Fallout.Infrastructure.Tooling;

/// <summary>
/// Default <see cref="IProcessRunner"/> — spawns a real OS process. This is the infrastructure adapter
/// behind the execution port; the rest of the tooling layer never touches <see cref="Process"/> directly.
/// </summary>
public sealed class SystemProcessRunner : IProcessRunner
{
    public IProcess Start(ProcessStartInfo startInfo, int? timeout, Action<OutputType, string> logger, Func<string, string> outputFilter)
    {
        var process = Process.Start(startInfo);
        if (process == null)
            return null;

        var output = ProcessTasks.GetOutputCollection(process, logger, outputFilter);
        return new Process2(process, outputFilter, timeout, output);
    }
}
