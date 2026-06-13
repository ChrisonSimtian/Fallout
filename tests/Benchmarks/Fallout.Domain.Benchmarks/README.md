# Fallout.Domain benchmarks

BenchmarkDotNet coverage of the **build-graph scheduling hot path** — `TopoSort.Order` in `Fallout.Domain`
(the innermost ring; pure, no I/O). This is the algorithm that orders the target graph on every build.

## Run

```sh
# Full run (real numbers):
dotnet run -c Release --project tests/Benchmarks/Fallout.Domain.Benchmarks

# A subset / a quick smoke check:
dotnet run -c Release --project tests/Benchmarks/Fallout.Domain.Benchmarks -- --filter '*TopoSort*'
dotnet run -c Release --project tests/Benchmarks/Fallout.Domain.Benchmarks -- --job dry   # 1 op, just proves it runs
```

The project is in `fallout.slnx` so it's **compile-gated** by the normal build, but it is **not run** by the fast
`Test` gate (benchmarks are slow). It runs in the dedicated `benchmarks` workflow (weekly + on demand).

## Regression gate

The `benchmarks` GitHub workflow (`.github/workflows/benchmarks.yml`) runs the benchmarks and compares them to
the committed baseline (`tests/Benchmarks/baselines/TopoSort.baseline.json`) via
[`compare-to-baseline.sh`](../compare-to-baseline.sh); it fails if any benchmark's mean time **or** allocations
regress beyond the threshold (default **+25%**, generous to absorb runner noise).

**Baselines are machine-specific.** The committed baseline must be captured on the CI runner image, not a dev
laptop — run the workflow with the `refresh-baseline` input to regenerate and commit it. (The seed baseline was
captured locally on macOS and should be refreshed from a runner before the gate is trusted.)

Because the onion re-layering was structural, `TopoSort` and the other pure algorithms are unchanged from the
pre-rename version — so these numbers also serve as the "no slower than the previous version" reference for this
path. (A literal cross-version A/B is impractical: the rename changed every namespace/package, so old and new
Fallout can't be referenced side-by-side in one benchmark.)

## ⚠️ Known finding: `TopoSort.Order` scales super-linearly

The initial baseline shows roughly **O(n³)** growth, not the O(V+E) a topological sort should have:

| NodeCount | Mean | Allocated |
|---|---|---|
| 10 | ~1.8 µs | ~8 KB |
| 100 | ~0.5 ms | ~177 KB |
| 1000 | **~466 ms** | **~12 MB** |

This is **pre-existing** (the algorithm predates the rename). It's harmless for normal target graphs (tens of
targets → microseconds), but would hurt if `TopoSort` is ever applied to large *project* graphs. Optimizing it to
O(V+E) is a tracked follow-up — separate from this gate, whose job is only to prevent things getting *worse*.
