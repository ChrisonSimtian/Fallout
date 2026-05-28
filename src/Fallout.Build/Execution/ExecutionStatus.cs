using System;
using System.Linq;

namespace Fallout.Common.Execution;

public enum ExecutionStatus
{
    None,
    Scheduled,
    NotRun,
    Skipped,
    Succeeded,
    Failed,
    Running,
    Aborted,
    Collective
}
