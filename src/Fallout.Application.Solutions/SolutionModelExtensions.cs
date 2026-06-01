using Fallout.Kernel.IO;

namespace Fallout.Application.Solutions;

public static class SolutionModelExtensions
{
    public static Solution ReadSolution(this AbsolutePath path)
    {
        return SolutionServices.Serializer.Open(path);
    }

    public static Solution ReadSolution<T>(this AbsolutePath path)
        where T : Solution
    {
        return SolutionServices.Serializer.Open<T>(path);
    }
}
