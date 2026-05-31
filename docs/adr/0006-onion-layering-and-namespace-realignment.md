# ADR-0006 — Onion layering + namespace realignment

- **Status:** Proposed
- **Date:** 2026-05-31
- **Deciders:** Fallout maintainers
- **Relates to:** ADR-[0004](0004-calendar-versioning-and-dual-pace-channels.md) (calendar versioning + channels — sets the breaking-change home), ADR-[0005](0005-ci-host-integration-ports-and-adapters.md) (the runtime-host ports the Application layer exposes), [docs/rebrand-plan.md](../rebrand-plan.md) (**amends** its deferral — see below)
- **Spike:** [docs/spikes/0002-onion-domain-ring.md](../spikes/0002-onion-domain-ring.md)

## Context

The project + namespace structure is inherited verbatim from NUKE: the rebrand ([#32](https://github.com/ChrisonSimtian/Fallout/issues/32)) did a **strict 1:1 prefix swap** (`Nuke.X` → `Fallout.X`) that deliberately preserved the existing shape. Two pathologies result:

1. **Namespace ≠ project.** Most projects declare types under `Fallout.Common.*` rather than their own name. The core user-facing API (`FalloutBuild`, `Target`, `[Parameter]`, and the ADR-0005 ports) lives in namespace `Fallout.Common` inside project `Fallout.Build`. `Fallout.Core` declares both `Fallout.Core.Planning` and `Fallout.Common.Execution`. Every `Fallout.Utilities.*` sub-project (except `.Text.Yaml`) declares `Fallout.Common.*`. `Fallout.Tooling` → `Fallout.Common.Tooling`. `Fallout.Solution` (singular) → `Fallout.Solutions` (plural). `Fallout.Tooling.Generator` → `Fallout.CodeGeneration`.

2. **`Fallout.Common.*` is a horizontal catch-all** contributed by **five** projects (`Build`, `Common`, `Core`, `Utilities`, `SourceGenerators`). It masks a layering that already exists *physically* — the `ProjectReference` graph is clean and acyclic (`Core`/`Utilities` at the bottom → `Build` → `Common` → `Cli` at the root) — but the namespaces lie about it.

### Why now, and why this amends the rebrand plan

`docs/rebrand-plan.md` explicitly **defers** "realigning project ↔ namespace" to "a future major version after the shim packages have sunset," citing the type-forwarding bridge as the blocker. This ADR **amends that deferral** on two grounds:

- **The bridge is re-pointable.** It is not raw `[TypeForwardedTo]`; it is the `TransitionShimGenerator` (`ShimAllPublicTypesUnder(from, to)`), a *prefix-remappable* subclass generator. We can restructure the `Fallout.*` namespaces freely and re-point the shim mappings; the `Nuke.*` surface stays frozen and consumers on it are unaffected.
- **ADR-0004 gives a clean home.** Namespace realignment is breaking → it lands on `experimental` and is **batched to the `2027.0.0` yearly major**. The work happens in 2026; native `Fallout.*` consumers are carried by re-pointed shims + the `Fallout.Migrate` codefix. Old `Fallout.*` namespaces are deleted at the cut, not dragged.

(The rebrand-plan is a transient maintainer-owned doc; this is a deliberate, recorded reversal of one of its deferrals, not a silent contradiction.)

## Decision

**Realign the runtime codebase to explicit onion layers, with `namespace = project = layer`.**

| Layer | Holds | Today (selected) → target |
|---|---|---|
| **`Fallout.Domain.*`** (innermost; zero Fallout deps) | Target graph, planning algorithms, execution status, the read-only build model | `Fallout.Core` (`Fallout.Core.Planning` + `Fallout.Common.Execution`'s `ITargetModel`/`ExecutionStatus`) → `Fallout.Domain` |
| **`Fallout.Application.*`** (orchestration + ports; depends only on Domain) | `FalloutBuild`, `Target`, `[Parameter]` & attributes, execution engine (`BuildManager`/`Executor`/`Planner`), middleware pipeline, value injection, `Host` base, the ports (`IBuildHost`, `IBuildReporter`, `IConfigurationGenerator`, `IBuildServer`) | `Fallout.Build` + the `Fallout.Common`/`Fallout.Common.Execution`/`Fallout.Common.CI` types it declares → `Fallout.Application` |
| **`Fallout.Infrastructure.*`** (adapters; depends on Application/Domain) | CI host adapters, tool-execution framework + tool wrappers, IO/Net/compression/globbing/text, project/solution model, process execution | `Fallout.Common.CI.*` → `.Infrastructure.CI.*`; `Fallout.Common.Tools.*` + `Fallout.Tooling` → `.Infrastructure.Tools.*`; `Fallout.Utilities.*` → `.Infrastructure.IO`/`.Net`/`.Text.*`; `Fallout.ProjectModel`/`Fallout.Solution(s)` → `.Infrastructure.ProjectModel`/`.Solutions` |
| **`Fallout.Cli`** (composition root) | Entry points, host integration | `Fallout.Cli`, `Fallout.MSBuildTasks`, `Fallout.Migrate` — unchanged |

**Outside the runtime onion** (build-time tooling, kept as-is): `Fallout.SourceGenerators`, `Fallout.Tooling.Generator` (the `Fallout.CodeGeneration` codegen), `Fallout.Migrate.Analyzers`. The vendored `Fallout.Persistence.Solution` keeps its namespace (Microsoft code).

**Rules:**
1. **`namespace == project == layer`.** No project declares a namespace rooted outside its layer. `Fallout.Common` is **dissolved**.
2. **Onion dependency rule, fitness-enforced.** Domain references no other Fallout assembly; Application references only Domain; Infrastructure references Application/Domain; only the Cli composition root references Infrastructure. One architecture-test per ring, added as each ring lands (extends the ADR-0005 boundary-test pattern).
3. **Breaking → `experimental` → `2027.0.0`** (ADR-0004). Re-point the `TransitionShimGenerator` mappings so the `Nuke.*` surface is unchanged; update the `Fallout.Migrate` codefix to rewrite old `Fallout.*` → new `Fallout.*` for native consumers. Per ADR-0004 a breaking PR targets `experimental` only and carries `target/2027` + `breaking-change` + a `CHANGELOG.md` migration entry.
4. **Ring-by-ring migration**, inner to outer — each ring is its own PR on `experimental`. The spike (0002) proves the mechanics on the Domain ring before the larger rings.

## Judgment calls (flagged for review — defaults chosen, easily changed)

- **Public API under `.Infrastructure`.** Tool wrappers and CI attributes are consumer-facing yet land under `Fallout.Infrastructure.*` (the hexagonal reading: adapters are infrastructure even when public). Consequence: consumer `using` directives grow (`using Fallout.Infrastructure.Tools.DotNet;`). **Mitigation:** ship a curated set of global usings in the `dotnet fallout` project template so day-to-day build authoring isn't verbose. *Accepted by maintainer; recorded so the ergonomics cost is visible.*
- **`Fallout.Components`** (the `ICompile`/`IPack`/… mixins) sits on top of Application and is user-facing. Default: keep as `Fallout.Components` (an application-adjacent convenience layer that depends on Application). Alternative: fold into `Fallout.Application.Components`.
- **Utilities as Infrastructure vs shared kernel.** IO/Net/compression are genuinely infrastructure. Pure-algorithmic helpers (collections, reflection, string) are more a shared kernel; default is to keep a minimal shared-kernel that Domain may depend on, rather than forcing it through Infrastructure (which would violate the inner ring). To be pinned during the Application/Infrastructure rings.

## Consequences

### Positive
- The namespace finally tells the truth about the layer, and the onion dependency rule is enforced by construction (fitness tests), not convention.
- `Fallout.Common` — the catch-all — is gone; no more "five projects, one namespace."
- Sets the stage cleanly for the public plugin SDK (milestone #7): adapters live in a named Infrastructure layer behind the Application-owned ports.

### Negative
- **Large, breaking, multi-PR.** Touches nearly every file's `namespace`/`using`. Mitigated by ring-by-ring sequencing, re-pointed shims, the migrate codefix, and batching to one yearly major.
- **Consumer churn** for native `Fallout.*` users (the `Nuke.*` shim users are insulated). The codefix + a CHANGELOG migration guide carry it.
- **`Fallout.Migrate`** must learn the intra-Fallout rename map in addition to the Nuke→Fallout one.

## Alternatives considered

- **Concern-aligned names** (`Fallout.Core`/`Build`/`Tooling`/`Tools`/`CI`/`IO`), namespace=project but not layer-labelled. Lower consumer surprise (keeps short names), fits the repo's by-concern convention. **Not chosen** — maintainer wants explicit onion layer names so the architecture is legible from the namespace alone.
- **Keep `Fallout.Common` as a thin facade** re-exporting the layered types. Less churn. **Not chosen** — keeps a name we want gone ("don't drag dead weight").
- **Minimal — fix only the internal mismatches.** Smallest break. **Not chosen** — leaves the catch-all and doesn't achieve the onion goal.
- **Defer to "after shim sunset" (status quo per rebrand-plan).** **Not chosen** — see "Why now"; the work is wanted this year.

## References

- Current-state map: see the structural survey summarized in the spike.
- Shim machinery: `src/Fallout.SourceGenerators/TransitionShimGenerator.cs`, `src/Shims/Nuke.*`
- Migrate codefix: `src/Fallout.Migrate`, `src/Fallout.Migrate.Analyzers`
- Deferral being amended: `docs/rebrand-plan.md` §"What's deliberately deferred"
- Spike: [docs/spikes/0002-onion-domain-ring.md](../spikes/0002-onion-domain-ring.md)
