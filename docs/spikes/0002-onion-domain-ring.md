# Spike 0002 — Onion realignment: prove the Domain ring

- **Status:** Planned
- **Date:** 2026-05-31
- **Decision record:** [ADR-0006](../adr/0006-onion-layering-and-namespace-realignment.md)
- **Channel:** `experimental` (spike branch `spike/onion-structure`)
- **Time-box:** one focused session

> The full realignment (ADR-0006) is a large, breaking, ring-by-ring program. This spike does **only the innermost ring** — extract `Fallout.Domain` — to prove the three mechanics every later ring depends on, before committing to the larger ones. Throwaway-by-default.

## Hypothesis

> The pure-domain types can be lifted into a `Fallout.Domain` layer (`namespace = project = layer`) such that: (1) the Domain assembly references **no other Fallout assembly**, enforced by a fitness test; (2) the solution still builds and all tests pass — confirming the move + reference-fixup + fitness loop that the Application and Infrastructure rings will reuse at larger scale.

## Scope

### In scope
1. **Create `Fallout.Domain`** (project + namespace). Move into it the pure-domain types: `Fallout.Core.Planning.*` (graph/topo-sort/cycle detection) and the domain types currently under `Fallout.Common.Execution` in the `Fallout.Core` project (`ITargetModel`, `ExecutionStatus`). Result: `Fallout.Core` is renamed/absorbed; no domain type remains under `Fallout.Common.*`.
2. **Fitness test.** Assert `Fallout.Domain` references no other `Fallout.*` assembly (the innermost-ring invariant). Demonstrate it fails on an injected violation, then revert.
3. **Update internal references.** `Fallout.Application`-to-be (currently `Fallout.Build`) and others that consumed the moved types via `Fallout.Common.Execution` now reference `Fallout.Domain`.

### Out of scope
- ❌ The Application and Infrastructure rings (later PRs).
- ❌ Renaming `Fallout.Build` → `Fallout.Application`, dissolving `Fallout.Common`, moving tools/CI/utilities — none of it yet.
- ❌ **Shim / migration parity — deferred wholesale** (ADR-0006). We do *not* re-point the `TransitionShimGenerator` here; the `Nuke.*` surface is allowed to lapse on `experimental`. The migration strategy is redesigned as its own phase once the target shape settles.
- ❌ Consumer-template global usings (an Application/Infrastructure-ring concern).

## Ordered steps

1. Branch is `spike/onion-structure` off `experimental`. (Done.)
2. Read the current `Fallout.Core` contents and every reference to its types across the solution (esp. `Fallout.Common.Execution` consumers in `Fallout.Build`).
3. Create `Fallout.Domain` project; move the pure-domain files; set `namespace = Fallout.Domain[.Planning]`.
4. Fix references (project refs + `using`s) across the solution. The `Nuke.*` shim build may break here — that's expected and allowed (shim parity deferred); if it blocks the solution build, temporarily exclude the affected shim project rather than re-pointing it.
5. Add the Domain-ring fitness test; confirm green, then confirm it goes red on a deliberate `Fallout.*` reference, then revert.
6. Validate: `./build.sh Compile` + `./build.sh Test` (or the per-project equivalents) green; dogfood workflows unchanged.

## Success criteria
- ✅ `Fallout.Domain` exists; no domain type remains under `Fallout.Common.*`.
- ✅ Fitness test passes and demonstrably fails on a violation.
- ✅ Solution (excluding deferred shim parity) builds; tests green.
- ✅ A written verdict: did the move + reference-fixup + fitness loop hold? What surprised us? Is it safe to scale to the Application ring (the big one — the user-facing API rename)?

## Risks / watch
- **What's *really* domain.** `Fallout.Core` already mixes `Fallout.Core.Planning` and `Fallout.Common.Execution` — move only genuinely-pure types; anything touching execution *orchestration* (not the model) belongs in the Application ring, not Domain.
- **Hidden inward references.** A "domain" type that secretly reaches into utilities/IO would break the zero-deps invariant — the fitness test will catch it; treat any such case as a finding about what's really domain. This is now the spike's primary unknown (the shim question is deferred out).
- **Shim fallout is expected, not a failure.** If moving types breaks the `Nuke.*` shim build, that's the deferred concern surfacing — note it and move on; do not rabbit-hole on re-pointing.

## Exit
Set **Status: Done**, append the verdict, and feed it back into ADR-0006 (confirm/adjust the layer mapping, then promote `Proposed` → `Accepted`) before starting the Application ring.
