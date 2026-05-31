namespace Fallout.Domain.Execution;

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
