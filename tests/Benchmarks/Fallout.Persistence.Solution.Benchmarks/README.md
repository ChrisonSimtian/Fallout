# Fallout.Persistence.Solution.Benchmarks

BenchmarkDotNet measurements for the `.slnx` parser inlined from Microsoft's `vs-solutionpersistence` (see #248). Establishes a **baseline** before any optimisation work — so we can later prove (with numbers, not vibes) whether replacing the inlined parser with a proper `XmlSerializer` implementation actually buys anything.

## Scenarios

| Dimension | Values |
|---|---|
| Project count | 1, 10, 100, 1000 |
| Layout | flat (`WithFolders=false`) and grouped into folders of 10 (`WithFolders=true`) |

Cartesian product = 8 measurements per benchmark method. Fixtures are generated programmatically in `[GlobalSetup]` (deterministic, no committed binary blobs).

## Running locally

```sh
dotnet run --project tests/Benchmarks/Fallout.Persistence.Solution.Benchmarks/ -c Release --filter '*'
```

Release config matters: Debug numbers are meaningless. BenchmarkDotNet enforces this by default and refuses to run a Debug build, but the explicit `-c Release` keeps invocation muscle-memory aligned with what's required.

Common filters:

```sh
# Just the small cases:
dotnet run --project tests/Benchmarks/Fallout.Persistence.Solution.Benchmarks/ -c Release --filter '*ProjectCount=1*' '*ProjectCount=10*'

# Just one benchmark:
dotnet run --project tests/Benchmarks/Fallout.Persistence.Solution.Benchmarks/ -c Release --filter '*SlnxParseBenchmarks.ParseSlnx*'

# Help on the full invocation surface:
dotnet run --project tests/Benchmarks/Fallout.Persistence.Solution.Benchmarks/ -c Release -- --help
```

## What's measured

- **`SlnxParseBenchmarks.ParseSlnx`** — `SolutionSerializers.GetSerializerByMoniker(...).OpenAsync(...)` end-to-end on a temp `.slnx` file matching the parameter combination. Includes XML reading, model construction, folder hierarchy resolution. Excludes the Fallout-facade `path.ReadSolution()` wrapping (trivial) so the numbers attribute purely to the parser.

`[MemoryDiagnoser]` is on, so allocations + Gen0/Gen1/Gen2 counts are reported alongside time. Allocation pressure is often the bigger win in parser rewrites than wall-time.

## CI integration

**Not run in CI.** Benchmarks in CI are noisy (shared-runner variance), slow (a sweep takes ~3-5 minutes), and the numbers from a GitHub-Actions VM rarely compare meaningfully across runs. The CI check on this project is just `dotnet build` (does the benchmark project compile against the current API surface).

If/when we want continuous benchmark tracking, a dedicated machine + a results-database pattern is the right shape — out of scope here.

## Related

- **#248** — the inlining PR that made us own this code in the first place.
- **#258** — broader BenchmarkDotNet coverage tracking issue (tool wrapper, source generators, globbing, etc.).
