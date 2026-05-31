# Spike 0002 — Onion realignment: prove the Domain ring

- **Status:** Planned
- **Date:** 2026-05-31
- **Decision record:** [ADR-0006](../adr/0006-onion-layering-and-namespace-realignment.md)
- **Channel:** `experimental` (spike branch `spike/onion-structure`)
- **Time-box:** one focused session

> The full realignment (ADR-0006) is a large, breaking, ring-by-ring program. This spike does **only the innermost ring** — extract `Fallout.Domain` — to prove the three mechanics every later ring depends on, before committing to the larger ones. Throwaway-by-default.

## Hypothesis

> The pure-domain types can be lifted into a `Fallout.Domain` layer (`namespace = project = layer`) such that: (1) the Domain assembly references **no other Fallout assembly**, enforced by a fitness test; (2) the `TransitionShimGenerator` can be **re-pointed** so the `Nuke.*` surface is byte-for-byte unchanged; (3) the solution still builds and all tests pass — confirming the rename + shim-repoint + fitness loop that the Application and Infrastructure rings will reuse at larger scale.

## Scope

### In scope
1. **Create `Fallout.Domain`** (project + namespace). Move into it the pure-domain types: `Fallout.Core.Planning.*` (graph/topo-sort/cycle detection) and the domain types currently under `Fallout.Common.Execution` in the `Fallout.Core` project (`ITargetModel`, `ExecutionStatus`). Result: `Fallout.Core` is renamed/absorbed; no domain type remains under `Fallout.Common.*`.
2. **Re-point the shim.** Update the `ShimAllPublicTypesUnder` mappings so the moved types still surface at their original `Nuke.*` names (e.g. `Fallout.Domain.ITargetModel` → `Nuke.Common.Execution.ITargetModel`). Confirm the generated `Nuke.*` surface is unchanged.
3. **Fitness test.** Assert `Fallout.Domain` references no other `Fallout.*` assembly (the innermost-ring invariant). Demonstrate it fails on an injected violation, then revert.
4. **Update internal references.** `Fallout.Application`-to-be (currently `Fallout.Build`) and others that consumed the moved types via `Fallout.Common.Execution` now reference `Fallout.Domain`.

### Out of scope
- ❌ The Application and Infrastructure rings (later PRs).
- ❌ Renaming `Fallout.Build` → `Fallout.Application`, dissolving `Fallout.Common`, moving tools/CI/utilities — none of it yet.
- ❌ Updating the `Fallout.Migrate` codefix for native consumers (batched work; the spike only proves the shim path).
- ❌ Consumer-template global usings (an Application/Infrastructure-ring concern).

## Ordered steps

1. Branch is `spike/onion-structure` off `experimental`. (Done.)
2. Read the current `Fallout.Core` contents and every reference to its types across the solution (esp. `Fallout.Common.Execution` consumers in `Fallout.Build`).
3. Create `Fallout.Domain` project; move the pure-domain files; set `namespace = Fallout.Domain[.Planning]`.
4. Fix references (project refs + `using`s) across the solution.
5. Re-point the `TransitionShimGenerator` mapping(s); rebuild the `Nuke.*` shim and confirm its public surface is unchanged (diff the generated types, or assert key `Nuke.Common.Execution.*` types still resolve).
6. Add the Domain-ring fitness test; confirm green, then confirm it goes red on a deliberate `Fallout.*` reference, then revert.
7. Validate: `./build.sh Compile` + `./build.sh Test` (or the per-project equivalents) green; dogfood workflows unchanged.

## Success criteria
- ✅ `Fallout.Domain` exists; no domain type remains under `Fallout.Common.*`.
- ✅ Fitness test passes and demonstrably fails on a violation.
- ✅ `Nuke.*` shim surface unchanged (consumers on the bridge unaffected).
- ✅ Solution builds; tests green.
- ✅ A written verdict: did the rename + shim-repoint + fitness loop hold? What surprised us? Is it safe to scale to the Application ring (the big one — the user-facing API rename)?

## Risks / watch
- **Shim re-point is the real unknown.** If `ShimAllPublicTypesUnder` can't express the many-old-layers → one-`Nuke.Common` mapping cleanly, that's the key finding — it gates every later ring. Record precisely what the generator supports.
- **`Fallout.Core` already mixes `Fallout.Core.Planning` and `Fallout.Common.Execution`** — make sure only genuinely-pure types move; anything touching execution *orchestration* (not the model) belongs in the Application ring, not Domain.
- **Hidden inward references.** A "domain" type that secretly reaches into utilities/IO would break the zero-deps invariant — the fitness test will catch it; treat any such case as a finding about what's really domain.

## Exit
Set **Status: Done**, append the verdict, and feed it back into ADR-0006 (confirm/adjust the layer mapping, then promote `Proposed` → `Accepted`) before starting the Application ring.
