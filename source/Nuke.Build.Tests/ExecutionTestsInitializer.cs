// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Nuke.Common.Execution;

namespace Nuke.Common.Tests;

public static class ExecutionTestsInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Environment.SetEnvironmentVariable(Telemetry.OptOutEnvironmentKey, "true");
    }
}
