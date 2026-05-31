# Spike 0003 — Onion realignment: the Infrastructure ring (steps 3–5)

- **Status:** In progress — step 3 done; **steps 4a (Components) + 4b (Tools/Tooling) done**; step 5 (CI/IO/ProjectModel/Solution → Infrastructure + Utilities split / kernel) remains. Plus a follow-up: invert the tracked `ToolTasks`(App)→`ProcessTasks`(Infra) + resolver I/O dependency behind ports.
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

4b. **Tool vocabulary → Application; executor → Infrastructure** — the entangled one. `Fallout.Common.Tools.*` → `Fallout.Application.Tools.*`; the `Fallout.Common.Tooling` namespace (shared across `Fallout.Tooling` + `Fallout.Common`, mixing vocabulary and executor in one namespace) splits **per type**: vocabulary + ports + attributes + `ProcessException` → `Fallout.Application.Tooling`; the 13 impure executor types (`ProcessTasks`, `SystemProcessRunner`, `Process2`, `ProcessExtensions`, `ToolExecutor`, `ToolPathResolver`, `NuGet*`/`Npm*`/`Paket*` resolvers, `ToolingExtensions`) → `Fallout.Infrastructure.Tooling`. **Maintainer chose "true homes, defer port fix"**: types land where they belong, accepting `ToolTasks`(App)→`ProcessTasks`(Infra) + resolver I/O as a tracked Application→Infrastructure dependency to be inverted behind ports in a follow-up (no fitness test guards it yet — projects aren't split). 271 files, full suite green (7 cake-migration snapshots re-accepted — namespace-only churn). The prefix-only rewriter grew **per-type overrides** + **multi-assembly rules** + **lost-ancestor usings** + **cref rewriting** (lessons #8–#11). `AbsolutePath`/`Fallout.Common.IO` deferred to step 5 (still `Fallout.Common.IO` here, so no ring crossing yet — they'll need a shared kernel when IO moves).

### Remaining (this ring)
**Follow-up (port inversion)**: extract `IToolPathResolver` + package/version-resolver ports and route `ToolTasks.Run` through `IProcessRunner` so the Application ring stops calling the Infrastructure executor — clears the tracked violation.
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
8. **Classify moved types by their OUTERMOST enclosing type, not the symbol's own name** (found on 4a): a *nested* type's `ContainingNamespace` is the enclosing namespace, so a `{ns}.{Name}` lookup in the moved-set (top-level names only) misses it → it's misclassified as *residual*, which for a source file adds a `using` of the very namespace the move evacuates (dangling → CS0246). Fix: walk `ContainingType` to the top, key the moved-set check on that. Belt-and-suspenders: never add a residual `using` for a namespace not in `surviving`.
9. **Per-type overrides split one namespace across rings** (4b): `Fallout.Common.Tooling` holds both vocabulary (→Application) and executor (→Infrastructure). Prefix rules can't split a namespace, so the mapping became per-TYPE: `targetNs(type)` (override map beats prefix rule) replaced the per-namespace `mapNs`. Namespace *declarations* map by the moved types they declare (`MapDecl` — first moved type wins; blocks are single-ring since each type is its own file).
10. **Lost-ancestor usings** (4b — generalises #6 to non-movable parents): a file in `A.B.C` implicitly sees `A.B`/`A` with no `using`. Moving it to `X.Y.Z` severs that — e.g. `Fallout.Common.Tooling.*` files used `EnvironmentInfo`/`NotNull`/`Assert` from the **`Fallout.Common` root** with no import; after the move they need an explicit `using Fallout.Common;`. Fix: for a source file whose namespace changes, add a `using` for each strict ancestor of the old namespace that is (a) not an ancestor of the new namespace and (b) actually used. **This is the one that costs a whole build cycle if missed** (~160 CS0103/CS1061).
11. **Rewrite crefs too** (4b): doc-comment `<see cref="…"/>` names live in structured trivia — neither the prescan nor the rewriter descends there by default, so cref imports dangle and FQN crefs in the *generated* tool wrappers go stale (~7,600 CS1574/CS1580, non-fatal but review-blocking). Fix: prescan with `descendIntoTrivia: true` (so cref-only type refs still get their `using`), and give the `SyntaxFixer` `visitIntoStructuredTrivia: true` (so qualified cref names remap like any other qualified name).

## Next-session checklist
1. (Optional) Merge the stack first: `#343` then `#349` onto `experimental`, then rebase `spike/onion-infrastructure`.
2. ✅ `OnionRewriter` generalized — `Rule[]` table + per-type overrides + multi-assembly + lost-ancestor usings + cref rewriting. ✅ 4a Components and ✅ 4b Tools/Tooling (vocab→Application, executor→Infrastructure) committed, green.
3. **Step 5** (CI/IO/ProjectModel/Solution → Infrastructure): add rules; decide where `AbsolutePath`/`Fallout.Common.IO` land (likely a shared `Fallout.Kernel` — Infrastructure would break the Application ring). `Fallout.Common.IO` spans 4 assemblies → one rule per declaring assembly. **Also** schedule the port-inversion follow-up for the 4b tracked violation.
4. Add a `Fallout.Kernel` (or chosen name) for pure helpers; split `Fallout.Utilities` (pure → kernel, I/O → Infrastructure). Note `Fallout.Common.IO` spans 4 assemblies — one rule each.
5. Apply (scope-then-apply); update each `ShimMarker.cs` `fromNamespacePrefix`; accept any Verify `.received`.
6. **Last**: project-FILE renames — `Fallout.Build`→`Fallout.Application` assembly, `Fallout.Tooling` split; then the shim/migration redesign.

## Exit
Update **Status** as steps land; once steps 4–5 are applied and green, push `spike/onion-infrastructure` and open its PR (`target/2026` + `breaking-change`, base = `spike/onion-application` until the stack merges).
