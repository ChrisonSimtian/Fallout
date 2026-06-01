using System;
using System.Diagnostics;

namespace Fallout.Application.Tooling;

/// <summary>
/// The process-execution port (ADR-0006 step 3). Abstracts the single impure step of running a tool —
/// spawning an OS process — so the tooling <em>vocabulary</em> (<see cref="ToolOptions"/>, the generated
/// wrappers) stays pure and side-effect-free, and builds become unit-testable by swapping in a fake
/// runner via <c>ProcessTasks.Runner</c>. The default adapter is <c>SystemProcessRunner</c> (both in the
/// outer Fallout.Infrastructure.Tooling ring).
/// </summary>
public interface IProcessRunner
{
    IProcess Start(ProcessStartInfo startInfo, int? timeout, Action<OutputType, string> logger, Func<string, string> outputFilter);
}
