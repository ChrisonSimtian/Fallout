# Spike 0003 — Onion realignment: the Infrastructure ring (steps 3–5)

- **Status:** In progress — step 3 done; **step 4a (Components → Application) done**; steps 4b (Tools+Tooling) and 5 (I/O → Infrastructure) remain.
- **Date:** 2026-05-31 (updated 2026-06-01)
- **Decision record:** [ADR-0006](../adr/0006-onion-layering-and-namespace-realignment.md)
- **Channel:** `experimental` (branch `spike/onion-infrastructure`)

> Handoff doc for picking up the onion realignment next session. Captures the whole workstream's
> state, the decisions already made, the reusable tooling, and the precise next steps.

## Where the whole realignment stands

Onion layers, `namespace = project = layer`, **breaking, whole-onion in the 2026 major** (the 2026
major hasn't cut; breaking work rides it — ADR-0006). Branch stack on `experimental`:

```
experimental
  └─ spike/onion-structure      Domain ring (step 1)        → PR #343  (target/2026, breaking)
       └─ spike/onion-application  Application ring (step 2) → PR #349  (stacks on #343, breaking)
            └─ spike/onion-infrastructure  Infra ring (3–5)  → (unpushed; step 3 committed)
```

(Also open, unrelated-ish: PR #341 = CI ports/adapters, additive; PR #342 = other maintainer's promote.)

### Done
1. **Domain ring** — `Fallout.Core` → `Fallout.Domain`; `ITargetModel`/`ExecutionStatus`/`Planning` out of `Fallout.Common.*`. Fitness test: Domain depends on no outer ring. (PR #343)
2. **Application ring** — everything `Fallout.Build` declared under `Fallout.Common.*` (`FalloutBuild`, `Target`, `[Parameter]`, `Host`, engine, value injection, CI ports) → `Fallout.Application.*`. ~297 files, full suite green (~450 tests). (PR #349)
3. **Infra step 3 — execution port** — `IProcessRunner` + `SystemProcessRunner` + `ProcessTasks.Runner` seam; the impure `Process.Start` is now behind the port. Tooling vocabulary can now be pure. (committed on this branch; 64/64 Tooling tests)

### Done (cont.)
4a. **Components → Application** — `Fallout.Components` (20 files, single namespace) → `Fallout.Application.Components`. Clean isolated move; full suite green. Shim repointed (see below). Confirmed the generalized rule-table rewriter on an easy target before the hard Tooling move.

### Remaining (this ring)
4b. **Tool vocabulary → Application — entangled, NOT a clean prefix move.** `Fallout.Common.Tools.*` (126 files) inherit/return vocabulary (`ToolTasks`, `ToolOptions`, `Output`, `OutputType`, `Configure<T>`, `IProcess`, `Tool`, `ToolResolver`) that lives in the **`Fallout.Common.Tooling` namespace interleaved with the executor** (`ProcessTasks`, `SystemProcessRunner`, `ToolPathResolver`, package/version resolvers) in the *same namespace + projects* (`Fallout.Tooling`, `Fallout.Common`). Step 3's port only abstracts process *spawning* — `ToolTasks.Run` still calls `ProcessTasks` directly, and tool/package *path resolution* still does I/O from vocabulary. So Tools.* → Application drags the whole Tooling namespace; a true vocab/executor split needs more port extraction (`IToolPathResolver`, package-resolver ports) than exists today. **Decision pending** (see options recorded for the maintainer): move Tools+Tooling together to Application now and carve the executor to Infrastructure later, vs. extract ports first. Also lurking: where pervasive value-types (`AbsolutePath`, `Fallout.Common.IO`) land — Infrastructure would break the Application ring, so they likely belong in the shared kernel.
5. **I/O adapters → Infrastructure** — CI host adapters (`Fallout.Common.CI.<provider>`) → `Fallout.Infrastructure.CI.*`; `Fallout.Common.IO` + the `Fallout.Utilities.IO/.Net/.Compression/.Globbing` I/O bits → `Fallout.Infrastructure.*`; the `ProcessTasks`/`ToolExecutor` executor side → `Fallout.Infrastructure`; `Fallout.ProjectModel`/`Fallout.Solution` → `Fallout.Infrastructure.*`.

### Design decisions already made (don't relitigate)
- **Layer names**: explicit onion (`Fallout.Domain`/`.Application`/`.Infrastructure`, `Fallout.Cli` root). Public tool/CI API lives under `.Infrastructure` (accepted; ease consumer ergonomics later with template global usings).
- **Tool wrappers = Application vocabulary** (not Infrastructure): they're pure command builders; execution is the port. `Components` folds into Application (it composes wrappers).
- **Utilities split**: pure helpers (collections, reflection, string) → a **shared kernel below Application** (a small `Fallout.Kernel`-style project that Application + Infrastructure depend on; Domain stays zero-dep). Genuine I/O → `Fallout.Infrastructure.*`.
- **Shipping**: breaking → `experimental` → 2026 major. Shim/migration redesigned at the END (deferred).

## The reusable tool: `tools/OnionRewriter`

Semantic namespace mover (MSBuildWorkspace + symbol resolution). Run: `dotnet run --project tools/OnionRewriter` (dry-run) / `-- --apply`. It rewrites namespace declarations in a source project, fixes references repo-wide by the symbol each binds to, and reconciles `using`s.

**Generalized (done for step 4a)**: now driven by a `Rule[]` table — each rule is `(OldPrefix, NewPrefix, SourceAssembly)`, collected across *multiple* source assemblies. Edit the table per step (step 4a = the single Components rule). `IsMovable`/`MapNs`/`IsSourceFor` derive from the table; moved-type collection and the surviving-namespace scan iterate all source projects (source dirs derived from the loaded projects, not hard-coded). Steps 4b/5 just add rules — except `Fallout.Common.IO` is declared across **four** assemblies (Common + Utilities + Utilities.IO.Compression/Globbing), so that move needs one rule per declaring assembly (or generalize `SourceAssembly` to a set).

**Companion (non-semantic) edits the rewriter does NOT do** — it operates on symbols, never string literals, so per step also update by hand: transition-shim **`ShimMarker.cs`** `fromNamespacePrefix` strings (the `ShimAllPublicTypesUnder` source prefix — load-bearing: the shim generator mirrors that prefix, so a stale value silently stops emitting the shims and the hand-written `IHaz*` aliases fail to compile); any Migrate rename-map strings; Verify snapshots that embed names.

### Hard-won tool lessons (bake these into the generalized version)
These edge cases each cost a build cycle on the Application ring — the generalized tool must keep them:
1. **Classify moved types by assembly NAME + a moved-set membership** (not symbol identity — a referenced project's assembly symbol is a *different instance* per compilation, so identity comparison fails cross-project). Combine with skipping package-consumer projects to avoid the `Fallout.Build`-named NuGet package collision.
2. **Skip package consumers** (`Consumer.NuGet`, `Nuke.Consumer`) — they compile against the published package, not local source.
3. **Detect non-type references too**: attribute names bind to the **constructor** (`IMethodSymbol`), extension calls (`x.NotNull()`) bind to the **reduced method** — take the declaring type in both, else their `using`s are missed.
4. **Nested-type qualified refs** (`Ns.Outer.Nested`): only remap a qualified name's `Left` when it equals the type's namespace exactly; let the inner qualified-name visit handle nested cases (else you drop the outer-type qualifier).
5. **`using` drop rule**: drop a movable `using` when its namespace is **fully evacuated** (no residual declarations anywhere — would dangle) OR when a moved type was imported from it and no residual type still is. Don't touch pre-existing unused usings of surviving namespaces.
6. **Source files that change namespace** lose same-namespace access to **residual** siblings — add explicit `using`s for the residual movable namespaces they use (e.g. a file leaving `Fallout.Common` still using a `Fallout.Common`-residual helper).
7. **Snapshots churn**: the solution generator + cake-migration Verify snapshots embed namespaces/project names — accept the `.received` after applying. Excluding shim consumer/test projects from `fallout.slnx` also churns the solution-generator snapshot. (Step 4a churned none — project *names* were unchanged, only the namespace.)
8. **Classify moved types by their OUTERMOST enclosing type, not the symbol's own name** (found on 4a): a *nested* type's `ContainingNamespace` is the enclosing namespace, so a `{ns}.{Name}` lookup in the moved-set (top-level names only) misses it → it's misclassified as *residual*, which for a source file adds a `using` of the very namespace the move evacuates (dangling → CS0246). Fix in `IsMovedType`: walk `ContainingType` to the top, key the moved-set check on that. Belt-and-suspenders: never add a residual `using` for a namespace not in `surviving`.

## Next-session checklist
1. (Optional) Merge the stack first: `#343` then `#349` onto `experimental`, then rebase `spike/onion-infrastructure`.
2. ✅ `OnionRewriter` generalized to a `Rule[]` table (done on 4a). ✅ Components → Application (4a, green, committed).
3. **Decide step 4b approach** (Tools+Tooling): move-together-then-carve vs. extract-ports-first; and where `AbsolutePath`/`Fallout.Common.IO` land (likely a shared `Fallout.Kernel`). Then add the rules and dry-run.
4. Add a `Fallout.Kernel` (or chosen name) for pure helpers; split `Fallout.Utilities` (pure → kernel, I/O → Infrastructure). Note `Fallout.Common.IO` spans 4 assemblies — one rule each.
5. Apply (scope-then-apply); update each `ShimMarker.cs` `fromNamespacePrefix`; accept any Verify `.received`.
6. **Last**: project-FILE renames — `Fallout.Build`→`Fallout.Application` assembly, `Fallout.Tooling` split; then the shim/migration redesign.

## Exit
Update **Status** as steps land; once steps 4–5 are applied and green, push `spike/onion-infrastructure` and open its PR (`target/2026` + `breaking-change`, base = `spike/onion-application` until the stack merges).
