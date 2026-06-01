using Fallout.Application.Solutions;
using Fallout.Kernel.IO;

namespace Fallout.Infrastructure.Solutions;

/// <summary>
/// Direct entry into the solution serializer for build-time tooling (the strongly-typed-solution source
/// generator). It bypasses the runtime <see cref="SolutionServices"/> locator: a Roslyn generator host
/// can't run this assembly's module initializer to register the adapter, so it calls the adapter directly.
/// Runtime build code keeps using <c>AbsolutePath.ReadSolution()</c> (port-based).
/// </summary>
public static class SolutionReader
{
    public static Solution Read(AbsolutePath path) => new SolutionSerializerAdapter().Open(path);
}
