# ADR-0005 — CI host integration as ports & adapters (hexagonal seam)

- **Status:** Proposed
- **Date:** 2026-05-31
- **Deciders:** Fallout maintainers
- **Relates to:** ADR-[0001](0001-cd-primitives-attributes-vs-tasks.md) (CD primitives — attributes vs tasks), ADR-[0004](0004-calendar-versioning-and-dual-pace-channels.md) (calendar versioning + channels), milestone [#6](https://github.com/ChrisonSimtian/Fallout/milestone/6) (plugin foundation — internal), milestone [#7](https://github.com/ChrisonSimtian/Fallout/milestone/7) (public plugin SDK), RFCs [#97](https://github.com/ChrisonSimtian/Fallout/issues/97)–[#101](https://github.com/ChrisonSimtian/Fallout/issues/101)
- **Spike:** [docs/spikes/0001-ci-ports-and-adapters.md](../spikes/0001-ci-ports-and-adapters.md)

## Context

CI host integration is the framework's most-replicated extension point — eleven providers today (`AppVeyor`, `AzurePipelines`, `Bamboo`, `Bitbucket`, `Bitrise`, `GitHubActions`, `GitLab`, `Jenkins`, `SpaceAutomation`, `TeamCity`, `TravisCI`), each a folder under `src/Fallout.Common/CI/<Provider>/`. The public plugin SDK (milestone #7) explicitly names **"CI host adapters"** as an extension point it will expose. Before that surface goes public — additive-only, forever — the seam behind it needs to be **named, de-anemic-ed, and enforced**. This ADR records the shape we commit to.

### What's actually there today

A single provider folder conflates **two distinct concerns** with two distinct lifecycles:

1. **Config generation (design-time / output).** `[GitHubActions(...)]` on the build class is reflected into a POCO and serialized to `.github/workflows/*.yml`. The port already exists and is healthy: `IConfigurationGenerator` (`src/Fallout.Build/CICD/IConfigurationGenerator.cs`), implemented by `ConfigurationAttributeBase`. This is the pattern ADR-0001 builds on.

2. **Runtime host integration (execution-time).** Detecting we're running *inside* a host (`IsRunningGitHubActions => HasVariable("GITHUB_ACTIONS")`), emitting host commands (`::group::`, `::error::`), exposing `Branch`/`Commit`, and routing warnings/errors to host-native annotations. The "port" here is the `Host` base class (`src/Fallout.Build/Host.cs` + `Host.Activation.cs`) plus the **anemic** `IBuildServer` interface — currently just `Branch` and `Commit` (`src/Fallout.Build/CICD/IBuildServer.cs`).

### The latent structure is already correct

The dependency direction already obeys the hexagon's hardest rule: **ports live in `Fallout.Build`; the eleven adapters in `Fallout.Common.CI.*` depend inward on them.** Nothing needs re-plumbing. Three things are nonetheless wrong-shaped for a public extension point:

- **The runtime port is anemic and entangled.** `IBuildServer` says almost nothing, and the real host contract is fused into `Host`, a base class that *also* owns logging, theming, and console-output formatting (`WriteLogo`, `WriteBlock`, `WriteTargetOutcome`, …). "Am I a CI host" and "how do I render a build summary" are different jobs welded together.
- **The boundary is unenforced.** Nothing prevents core code from reaching for a concrete `GitHubActions` type. Today it's clean by discipline; a public SDK needs it clean by construction.
- **Adapters aren't named as adapters.** Discovery is reflection-by-convention (`Host.Default` scans public `Host` subclasses for a static `IsRunning{Name}` property). It works, but it's implicit — there's no declared "this type is the GitHub adapter for these ports" contract for an external plugin author to implement against.

### Onion or hexagonal?

For a subsystem whose entire value is *N interchangeable providers behind one contract*, **ports-and-adapters (hexagonal) is the right model** — it names the symmetry between the driven side (write a workflow file, push an annotation) and the driving side (the host environment that drives our run). Onion's concentric inward-dependency rule is the *constraint we keep*; hexagonal is the *vocabulary we adopt*. They compose: a hexagon obeys the onion dependency rule at its boundary. We are **not** adopting a `Fallout.Application.*` / `Fallout.Infrastructure.*` project renaming — that is gratuitous public churn (see Alternatives) that buys nothing the seam doesn't already give us.

## Decision

**1. Model CI host integration as two named ports, not one.**

| Port | Concern | Status | Lives in |
|---|---|---|---|
| **Config-generation port** — `IConfigurationGenerator` | Design-time: emit `.yml`/`.xml`/`.toml` committed to git | **Exists, keep as-is** | `Fallout.Build/CICD/` |
| **Runtime-host port** — *new, working name `IBuildHost`* | Execution-time: detect host, expose VCS/run facts, emit host annotations & summaries | **Formalize** | `Fallout.Build/CICD/` |

The runtime-host port subsumes and fattens the anemic `IBuildServer`. It carries the *host integration* contract (environment facts, annotation/log reporting, output/summary writing) and is deliberately **separated from the `Host` logging/theming base** — `Host` may *implement* the port, but the port does not inherit `Host`'s console-rendering responsibilities.

**2. The eleven `Fallout.Common.CI.<Provider>` types are adapters.** Document and treat them as such. **Do not relocate or rename them in this work** — naming the role is the deliverable, not moving the files.

**3. The public consumer surface stays, as a facade over the ports.** `[GitHubActions(...)]`, `GitHubActions.Instance`, `[CI]` injection (`CIAttribute`), and the `Host` API all keep working unchanged. Statics delegate *inward* to the ports / resolved instances — the same pattern a static `Log` facade uses over DI-resolved logging. **This delegation discipline is the rule that prevents a half-and-half mess** (ports bolted on while a static singleton still does the real work).

**4. Enforce the boundary with an architecture fitness test.** Assert that `Fallout.Build` (ports + kernel) never references a concrete provider type under `Fallout.Common.CI.*`, and that adapters depend inward only. This is what *holds* the hexagon once it exists, and it is a prerequisite for exposing the seam publicly.

**5. Compatibility strategy: additive now, deletions batched to the year cut.**

This is the load-bearing decision and the answer to "clean break or backwards-compatible?": **they are separable, and we take both — in sequence.**

- **All seam work is additive and ships continuously** through `main` → `-preview`: new port interface(s), adapters implementing them, the fitness test, and the delegating facades are all non-breaking. They land mid-year on the `2026` line.
- **Legacy paths are marked, not deleted:** `[Obsolete]` for plain deprecation, `[Experimental("FALLOUT0xx")]` (see [docs/experimental-apis.md](../experimental-apis.md) for the registry — allocate a fresh ID, do not reuse) for not-yet-stable replacements.
- **The genuinely breaking steps are deferred and batched.** Removing deprecated surface, dropping the `Nuke.*` shims, or reshaping a *consumer-implemented* interface land on `experimental` and ship at the next yearly major (`2027.0.0`), per ADR-0004. Mid-year `main`/production stays strictly non-breaking.
- **Type relocation, if ever needed, is non-breaking too:** `[TypeForwardedTo]` plus the existing `Fallout.SourceGenerators.TransitionShimGenerator` (`src/Shims/`) forward old namespaces, so even moving a public type to its own adapter assembly does not break callers.

Net: the better architecture arrives **immediately**, the dead weight is shed **on schedule** at the cut, and the trunk stays **green throughout**. Crucially, per ADR-0004 a deliberate break could only *ship* at the yearly major anyway — so "additive now" costs zero timeline versus "clean break now," and avoids months of an un-shippable trunk.

## Consequences

### Positive

- **Stable seam before it goes public.** Milestone #7 can expose "implement `IBuildHost` + `IConfigurationGenerator` to add a CI host" against a contract that's been dogfooded by the eleven in-tree adapters first.
- **Separation untangles the `Host` god-base.** Splitting "am I a CI host" from "how do I render output" makes both independently testable and lets a host adapter exist without inheriting console-formatting machinery.
- **No flag day, no broken trunk.** Every interim build compiles, passes, and is dogfoodable via `./build.sh`.
- **Generalizes cleanly.** Proving the port shape on GitHub Actions first (the spike) de-risks the other ten before any of them are touched.

### Negative

- **A transition period with two ways to express the runtime host** (the old `Host`/`IBuildServer` path and the new port). Mitigated by the facade discipline (§3) and a dated removal entry batched to `2027.0.0`.
- **Fitness test maintenance.** The boundary assertion must be kept honest as new providers land; a too-strict rule could false-positive on shared helpers. Scope it to "core → concrete provider type," not "core → `Fallout.Common.CI` namespace at all."
- **Naming risk.** `IBuildHost` vs `IBuildServer` vs `ICiHost` is a public name we'll live with. The name is provisional until the spike validates the shape; do not treat it as locked by this ADR.

## Alternatives considered

### A. Clean break — rebuild the CI subsystem fresh on the 2027 line

Rejected. The break buys only *deletion* of compatibility surface, not any *capability* the additive path lacks — the new ports sit cleanly over the existing facade. A clean break would mean a broken, un-shippable trunk for months in exchange for **zero** earlier delivery (ADR-0004 gates the break to the year cut regardless). "Drag no dead weight" is satisfied by scheduling the deletions at the cut, not by starting over.

### B. Full Domain/Application/Infrastructure project renaming

Rejected. Renaming `Fallout.Common.CI.*` → `Fallout.Infrastructure.*` etc. is a large public namespace/package break (mitigable with shims, but still churn) that delivers nothing the named-ports-in-place approach doesn't. It also fights the repo's established by-provider convention (AGENTS.md: prefer existing patterns). Onion layer *names* are not the goal; the enforced seam is.

### C. Status quo — keep `Host` + `IBuildServer` as the de facto contract

Rejected. The runtime port is too anemic and too entangled with console rendering to expose as a public plugin extension point. Shipping milestone #7 on top of it would lock in the entanglement as public API.

## References

- Config-generation port: `src/Fallout.Build/CICD/IConfigurationGenerator.cs`, `ConfigurationAttributeBase.cs`
- Runtime host (today): `src/Fallout.Build/Host.cs`, `Host.Activation.cs`, `src/Fallout.Build/CICD/IBuildServer.cs`
- Consumer injection: `src/Fallout.Build/CICD/CIAttribute.cs`
- Canonical adapter: `src/Fallout.Common/CI/GitHubActions/` (`GitHubActions.cs`, `GitHubActionsAttribute.cs`, `GitHubActions.Client.cs`)
- Live dogfood usage: `build/Build.CI.GitHubActions.cs`
- Compatibility machinery: `src/Shims/`, `Fallout.SourceGenerators.TransitionShimGenerator`, [docs/experimental-apis.md](../experimental-apis.md)
- Spike plan: [docs/spikes/0001-ci-ports-and-adapters.md](../spikes/0001-ci-ports-and-adapters.md)
