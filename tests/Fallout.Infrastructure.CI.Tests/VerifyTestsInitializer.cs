using System;
using System.Linq;
using System.Runtime.CompilerServices;
using VerifyTests;

namespace Fallout.Infrastructure.CI.Tests;

public static class VerifyTestsInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Environment.SetEnvironmentVariable("DiffEngine_Disabled", "true");
        Environment.SetEnvironmentVariable("Verify_DisableClipboard", "true");
        VerifyDiffPlex.Initialize();
    }
}
