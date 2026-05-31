using System;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace Fallout.Infrastructure.CI.TravisCI;

public enum TravisCIEventType
{
    push,
    pull_request,
    api,
    cron
}
