using BenchmarkDotNet.Running;

namespace Fallout.Domain.Benchmarks;

public class Program
{
    // Run all:           dotnet run -c Release --project tests/Benchmarks/Fallout.Domain.Benchmarks
    // Run a subset:      ... -- --filter *TopoSort*
    // Quick smoke check: ... -- --job dry
    public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
