using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Fallout.Domain.Planning;

namespace Fallout.Domain.Benchmarks;

/// <summary>
/// Characterises <see cref="TopoSort.Order{T}"/> — the pure scheduling algorithm that orders the target graph on
/// every build (Fallout.Domain, the innermost ring; no I/O). Sweeps node count over a realistic branching DAG so
/// the scaling shape is visible. Numbers here are the regression baseline for the build-planning hot path; the
/// algorithm itself is unchanged by the onion rename, so they double as the "no slower than before" reference.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("domain", "toposort")]
public class TopoSortBenchmarks
{
    [Params(10, 100, 1000)]
    public int NodeCount;

    private int[] _nodes = Array.Empty<int>();
    private Dictionary<int, int[]> _dependencies = new();

    [GlobalSetup]
    public void Setup()
    {
        _nodes = Enumerable.Range(0, NodeCount).ToArray();
        _dependencies = new Dictionary<int, int[]>(NodeCount);

        // Deterministic branching DAG: every edge points to an earlier node, so it's acyclic by construction.
        var rng = new Random(Seed: 42);
        foreach (var node in _nodes)
        {
            var deps = new List<int>(capacity: 3);
            if (node > 0) deps.Add(node - 1);              // a spine
            if (node > 3) deps.Add(rng.Next(0, node));     // + a couple of back-edges -> branching
            if (node > 7) deps.Add(rng.Next(0, node));
            _dependencies[node] = deps.Distinct().ToArray();
        }
    }

    [Benchmark(Baseline = true)]
    public PlanResult<int> Order() => TopoSort.Order(_nodes, node => _dependencies[node]);

    [Benchmark]
    public PlanResult<int> OrderStrict() => TopoSort.Order(_nodes, node => _dependencies[node], strict: true);
}
