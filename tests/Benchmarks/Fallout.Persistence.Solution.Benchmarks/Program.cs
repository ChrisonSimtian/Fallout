using BenchmarkDotNet.Running;

namespace Fallout.Persistence.Solution.Benchmarks;

public class Program
{
    public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
