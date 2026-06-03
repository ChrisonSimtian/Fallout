using System.Runtime.CompilerServices;
using Fallout.Infrastructure.Solutions;

namespace Fallout.Infrastructure.Solutions.Tests;

internal static class ModuleInit
{
    // The solution serializer adapter registers SolutionServices.Serializer from a [ModuleInitializer] in
    // Fallout.Infrastructure.Solutions, which only fires on first use of a type in that assembly. The build
    // runtime force-runs it (BuildManager), but a test host doesn't — so explicitly run the adapter
    // assembly's module constructor before any test calls AbsolutePath.ReadSolution().
    [ModuleInitializer]
    public static void EnsureSolutionSerializerRegistered()
        => RuntimeHelpers.RunModuleConstructor(typeof(SolutionReader).Module.ModuleHandle);
}
