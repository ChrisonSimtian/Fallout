# Spike 0003 — Onion realignment: the Infrastructure ring (steps 3–5)

- **Status:** In progress — step 3 done; steps 4–5 (the bulk) remain.
- **Date:** 2026-05-31
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

### Remaining (this ring)
4. **Tool vocabulary + `Components` → Application** — `Fallout.Common.Tools.*` (60+ wrappers) and `Fallout.Components` move to `Fallout.Application.Tools.*` / `Fallout.Application.Components`. (Now unblocked by step 3 — execution is behind the port, so the wrappers are pure command-builders.)
5. **I/O adapters → Infrastructure** — CI host adapters (`Fallout.Common.CI.<provider>`) → `Fallout.Infrastructure.CI.*`; `Fallout.Common.IO` + the `Fallout.Utilities.IO/.Net/.Compression/.Globbing` I/O bits → `Fallout.Infrastructure.*`; the `ProcessTasks`/`ToolExecutor` executor side → `Fallout.Infrastructure`; `Fallout.ProjectModel`/`Fallout.Solution` → `Fallout.Infrastructure.*`.

### Design decisions already made (don't relitigate)
- **Layer names**: explicit onion (`Fallout.Domain`/`.Application`/`.Infrastructure`, `Fallout.Cli` root). Public tool/CI API lives under `.Infrastructure` (accepted; ease consumer ergonomics later with template global usings).
- **Tool wrappers = Application vocabulary** (not Infrastructure): they're pure command builders; execution is the port. `Components` folds into Application (it composes wrappers).
- **Utilities split**: pure helpers (collections, reflection, string) → a **shared kernel below Application** (a small `Fallout.Kernel`-style project that Application + Infrastructure depend on; Domain stays zero-dep). Genuine I/O → `Fallout.Infrastructure.*`.
- **Shipping**: breaking → `experimental` → 2026 major. Shim/migration redesigned at the END (deferred).

## The reusable tool: `tools/OnionRewriter`

Semantic namespace mover (MSBuildWorkspace + symbol resolution). Run: `dotnet run --project tools/OnionRewriter` (dry-run) / `-- --apply`. It rewrites namespace declarations in a source project, fixes references repo-wide by the symbol each binds to, and reconciles `using`s.

**Needs generalizing for steps 4–5**: it currently hard-codes a **single** prefix swap (`Fallout.Common`→`Fallout.Application`) for **one** source assembly (`Fallout.Build`). The Infra ring needs a **multi-rule map** (per-namespace targets: `*.Tools`→Application, `*.CI`→Infrastructure, `*.IO`→Infrastructure, …) over the `Fallout.Common` project + the utilities/model projects.

### Hard-won tool lessons (bake these into the generalized version)
These edge cases each cost a build cycle on the Application ring — the generalized tool must keep them:
1. **Classify moved types by assembly NAME + a moved-set membership** (not symbol identity — a referenced project's assembly symbol is a *different instance* per compilation, so identity comparison fails cross-project). Combine with skipping package-consumer projects to avoid the `Fallout.Build`-named NuGet package collision.
2. **Skip package consumers** (`Consumer.NuGet`, `Nuke.Consumer`) — they compile against the published package, not local source.
3. **Detect non-type references too**: attribute names bind to the **constructor** (`IMethodSymbol`), extension calls (`x.NotNull()`) bind to the **reduced method** — take the declaring type in both, else their `using`s are missed.
4. **Nested-type qualified refs** (`Ns.Outer.Nested`): only remap a qualified name's `Left` when it equals the type's namespace exactly; let the inner qualified-name visit handle nested cases (else you drop the outer-type qualifier).
5. **`using` drop rule**: drop a movable `using` when its namespace is **fully evacuated** (no residual declarations anywhere — would dangle) OR when a moved type was imported from it and no residual type still is. Don't touch pre-existing unused usings of surviving namespaces.
6. **Source files that change namespace** lose same-namespace access to **residual** siblings — add explicit `using`s for the residual movable namespaces they use (e.g. a file leaving `Fallout.Common` still using a `Fallout.Common`-residual helper).
7. **Snapshots churn**: the solution generator + cake-migration Verify snapshots embed namespaces/project names — accept the `.received` after applying. Excluding shim consumer/test projects from `fallout.slnx` also churns the solution-generator snapshot.

## Next-session checklist
1. (Optional) Merge the stack first: `#343` then `#349` onto `experimental`, then rebase `spike/onion-infrastructure`.
2. Generalize `OnionRewriter` to a multi-rule namespace map; add a `Fallout.Kernel` (or chosen name) for pure helpers.
3. Split `Fallout.Utilities` (pure → kernel, I/O → Infrastructure); dry-run to scope steps 4–5.
4. Apply (scope-then-apply); update Verify snapshots; keep `Nuke.*` shim consumers/tests excluded.
5. **Last**: project-FILE renames — `Fallout.Build`→`Fallout.Application` assembly, `Fallout.Tooling` split; then the shim/migration redesign.

## Exit
Update **Status** as steps land; once steps 4–5 are applied and green, push `spike/onion-infrastructure` and open its PR (`target/2026` + `breaking-change`, base = `spike/onion-application` until the stack merges).
