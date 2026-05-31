using System;
using System.Linq;

namespace Fallout.Infrastructure.CI.AzurePipelines;

public enum AzurePipelinesJobStatus
{
    Canceled,
    Failed,
    Succeeded,
    SucceededWithIssues
}
