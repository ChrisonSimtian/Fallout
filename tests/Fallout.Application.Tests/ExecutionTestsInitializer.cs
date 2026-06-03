using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Fallout.Application.Execution;

namespace Fallout.Application.Tests;

public static class ExecutionTestsInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Environment.SetEnvironmentVariable(Telemetry.OptOutEnvironmentKey, "true");
    }
}
