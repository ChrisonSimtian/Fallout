using System.Runtime.CompilerServices;
using Fallout.Infrastructure.ProjectModel;
using Fallout.Infrastructure.Solutions;

namespace Fallout.Common.Tests;

internal static class ModuleInit
{
    // These tests also call AbsolutePath.ReadSolution(), which needs SolutionServices.Serializer registered
    // from Fallout.Infrastructure.Solutions' module initializer — force it (see Solution.Tests/ModuleInit).
    [ModuleInitializer]
    public static void EnsureSolutionSerializerRegistered()
        => RuntimeHelpers.RunModuleConstructor(typeof(SolutionReader).Module.ModuleHandle);

    // Microsoft.Build is excluded from runtime output (ExcludeAssets="runtime") and resolved
    // at runtime via the AssemblyResolve handler that ProjectModelTasks installs from its own
    // [ModuleInitializer]. If a test method declares a local of a Microsoft.Build type, the
    // JIT resolves Microsoft.Build.dll before any Fallout.ProjectModel code runs — and the
    // resolver is not yet installed. Touching ProjectModelTasks here forces Fallout.ProjectModel
    // to load (and its module initializer to fire) before xUnit JITs the first test.
    [ModuleInitializer]
    public static void EnsureMSBuildResolverRegistered()
    {
        ProjectModelTasks.Initialize();
    }
}
