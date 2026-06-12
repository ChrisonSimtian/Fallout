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

- **The bridge is not a blocker.** The rebrand-plan deferred this work *because* it assumed a rigid `[TypeForwardedTo]` bridge. The actual machinery is the `TransitionShimGenerator` (`ShimAllPublicTypesUnder(from, to)`), a *prefix-remappable* subclass generator — so restructuring `Fallout.*` doesn't orphan the `Nuke.*` surface in principle. We nonetheless **defer the migration strategy wholesale** (see Decision §"Migration & shim strategy") rather than re-point ring-by-ring; the point here is only that the stated blocker doesn't bind.
- **ADR-0004 gives a clean home.** Namespace realignment is breaking → it lands on `experimental` and is **batched to the next yearly major cut**. The `2026` major has **not cut yet** (no calendar-version GA tag exists; CHANGELOG `[Unreleased] — 2026.0` is still accumulating breaking changes), so this work can ride the **2026** cut and ship this year. Rings that aren't done before `2026.0.0` cuts roll to `2027`. Either way native `Fallout.*` consumers are carried by the deferred migration phase; old `Fallout.*` namespaces are deleted at the cut, not dragged.

(The rebrand-plan is a transient maintainer-owned doc; this is a deliberate, recorded reversal of one of its deferrals, not a silent contradiction.)

## Decision

**Realign the runtime codebase to explicit onion layers, with `namespace = project = layer`.**

| Layer | Holds | Today (selected) → target |
|---|---|---|
| **`Fallout.Domain.*`** (innermost; zero Fallout deps) | Target graph, planning algorithms, execution status, the read-only build model | `Fallout.Core` (`Fallout.Core.Planning` + `Fallout.Common.Execution`'s `ITargetModel`/`ExecutionStatus`) → `Fallout.Domain` |
| **`Fallout.Application.*`** (orchestration + ports + vocabulary; depends only on Domain) | `FalloutBuild`, `Target`, `[Parameter]` & attributes, execution engine (`BuildManager`/`Executor`/`Planner`), middleware pipeline, value injection, `Host` base; the ports (`IBuildHost`, `IBuildReporter`, `IConfigurationGenerator`, `IBuildServer`, **and a process/tool-execution port**); the **typed tool vocabulary** (`Options`/`ToolOptions` model + the generated wrappers like `DotNetTasks` & settings — pure command builders); and the **`Fallout.Components` recipes** | `Fallout.Build` + the `Fallout.Common*` it declares; `Fallout.Common.Tools.*` (wrappers) + the pure part of `Fallout.Tooling`; `Fallout.Components` → `Fallout.Application.*` |
| **`Fallout.Infrastructure.*`** (I/O adapters behind the ports; depends on Application/Domain) | The adapters that actually touch the outside world: the **process/tool runner** (OS process spawn), filesystem, HTTP, tool-path/package resolvers, **CI host adapters**, config-file writers, project/solution readers | `Fallout.Common.CI.*` → `.Infrastructure.CI.*`; the executor part of `Fallout.Tooling` (`ProcessTasks`/`ToolExecutor`) + `Fallout.Utilities.IO`/`.Net`/`.Compression`/`.Globbing` → `.Infrastructure.*`; `Fallout.ProjectModel`/`Fallout.Solution(s)` → `.Infrastructure.ProjectModel`/`.Solutions` |

**The tool layer splits along purity** (decided after reviewing `DotNetBuild`'s shape — see ADR-0005's ports pattern applied to the deepest I/O): the **command vocabulary** (`DotNetBuildSettings : ToolOptions` is pure data; `DotNetBuild` just constructs argv) is Application; the **one impure step** — `ProcessTasks.StartProcess` spawning a real OS process (today a *static* call) — becomes an injectable **process/tool-execution port** with the OS adapter in Infrastructure. This is what lets `Fallout.Components` (e.g. `ICompile` calling `DotNetBuild`) live in Application without the inner ring depending on a concrete — and it makes builds unit-testable by faking the runner. There is deliberately **no** generic "abstract build" port: `ICompile` is irreducibly DotNet-specific, so abstracting it would be anemic or a re-spelling of DotNet — the wrong abstraction. The seam is *execution*, not *build semantics*.
| **`Fallout.Cli`** (composition root) | Entry points, host integration | `Fallout.Cli`, `Fallout.MSBuildTasks`, `Fallout.Migrate` — unchanged |

**Outside the runtime onion** (build-time tooling, kept as-is): `Fallout.SourceGenerators`, `Fallout.Tooling.Generator` (the `Fallout.CodeGeneration` codegen), `Fallout.Migrate.Analyzers`. The vendored `Fallout.Persistence.Solution` keeps its namespace (Microsoft code).

**Rules:**
1. **`namespace == project == layer`.** No project declares a namespace rooted outside its layer. `Fallout.Common` is **dissolved**.
2. **Onion dependency rule, fitness-enforced.** Domain references no other Fallout assembly; Application references only Domain; Infrastructure references Application/Domain; only the Cli composition root references Infrastructure. One architecture-test per ring, added as each ring lands (extends the ADR-0005 boundary-test pattern).
3. **Breaking → `experimental` → the next yearly major** (ADR-0004). The `2026` major is still unreleased, so rings ride the **2026** cut (`target/2026`) until it closes; anything after rolls to `2027`. A breaking PR targets `experimental` only and carries `target/<year>` + `breaking-change` + a `CHANGELOG.md` entry under that major.
4. **Ring-by-ring migration**, inner to outer — each ring is its own PR on `experimental`. The spike (0002) proves the mechanics on the Domain ring before the larger rings.
5. **Migration/shim strategy is deferred wholesale** — see below.

### Migration & shim strategy: deferred by design

The `Nuke.*` transition shims (and any native-`Fallout.*` migration aid) are **explicitly out of scope for the rearchitecture rings.** Rationale: there's no point re-pointing the existing `TransitionShimGenerator` ring-by-ring toward a target that's still moving. Instead — once the final layered shape has settled — we design a **fresh migration/shim strategy that fits whatever we ended up with**, as its own phase and its own ADR. The existing bridge is re-pointable (it's a prefix-remappable subclass generator, not raw `[TypeForwardedTo]`), so this deferral costs us no future optionality; it's a sequencing choice, not a capability loss.

Consequence during the work: on `experimental`, `Nuke.*` shim parity is **not maintained** while the rings land. That's acceptable — `experimental` is the unstable lane, and the only deadline that matters is the major cut the rings target; the new migration story is built before that cut. `Fallout.Migrate` likewise gets revisited then, not incrementally.

### Sequence (each step a PR on `experimental`, with its own ring fitness test)

1. **Domain ring** — extract `Fallout.Domain` (spike [0002](../spikes/0002-onion-domain-ring.md)). Innermost, smallest; proves the move → reference-fixup → fitness loop.
2. **Application ring** — rename `Fallout.Build` → `Fallout.Application` and dissolve the `Fallout.Common` core API into it (the user-facing `FalloutBuild`/`Target`/`[Parameter]` rename — the big one).
3. **Tooling/execution-port spike** — extract the process/tool-execution port; split `Fallout.Tooling` into pure vocabulary (Application) + executor adapter (Infrastructure). Unlocks tool wrappers as Application vocabulary and is a standalone testability win. *(Can run in parallel with step 2; must precede step 4.)*
4. **Tool vocabulary + `Components` → Application** — move the wrappers and the `Fallout.Components` recipes in, now that execution is behind the port.
5. **Infrastructure ring** — CI host adapters, IO/Net/compression/globbing, path/package resolvers, project/solution readers → `Fallout.Infrastructure.*`.

Migration/shim strategy is redesigned after step 5, before the cut (deferred, above).

## Judgment calls (flagged for review — defaults chosen, easily changed)

- **Public API under `.Infrastructure`.** Tool wrappers and CI attributes are consumer-facing yet land under `Fallout.Infrastructure.*` (the hexagonal reading: adapters are infrastructure even when public). Consequence: consumer `using` directives grow (`using Fallout.Infrastructure.Tools.DotNet;`). **Mitigation:** ship a curated set of global usings in the `dotnet fallout` project template so day-to-day build authoring isn't verbose. *Accepted by maintainer; recorded so the ergonomics cost is visible.*
- **`Fallout.Components` → Application (RESOLVED, not an open default).** The mixins call tool wrappers (`ICompile.Compile` → `DotNetBuild`). Rather than exile `Components` to an outer ring, we **invert the dependency**: the tool wrappers become Application *vocabulary* and the one impure step (process spawn) moves behind the process/tool-execution port (see the layer table). `Components` then depends only on Application vocabulary + that port and lives in `Application` cleanly. **Prerequisite:** the tooling/execution-port spike (step 3 of the Sequence) lands before the wrappers + `Components` move.
- **Utilities as Infrastructure vs shared kernel.** IO/Net/compression are genuinely infrastructure. Pure-algorithmic helpers (collections, reflection, string) are more a shared kernel; default is to keep a minimal shared-kernel that Domain may depend on, rather than forcing it through Infrastructure (which would violate the inner ring). To be pinned during the Application/Infrastructure rings.

## Consequences

### Positive
- The namespace finally tells the truth about the layer, and the onion dependency rule is enforced by construction (fitness tests), not convention.
- `Fallout.Common` — the catch-all — is gone; no more "five projects, one namespace."
- Sets the stage cleanly for the public plugin SDK (milestone #7): adapters live in a named Infrastructure layer behind the Application-owned ports.

### Negative
- **Large, breaking, multi-PR.** Touches nearly every file's `namespace`/`using`. Mitigated by ring-by-ring sequencing, re-pointed shims, the migrate codefix, and batching to one yearly major.
- **Consumer churn** for native `Fallout.*` users, and a window on `experimental` where `Nuke.*` shim parity lapses. Both are carried by the deferred migration phase (a fresh strategy + CHANGELOG migration guide), built before the target major cut — not during the rings.

## Alternatives considered

- **Concern-aligned names** (`Fallout.Core`/`Build`/`Tooling`/`Tools`/`CI`/`IO`), namespace=project but not layer-labelled. Lower consumer surprise (keeps short names), fits the repo's by-concern convention. **Not chosen** — maintainer wants explicit onion layer names so the architecture is legible from the namespace alone.
- **Keep `Fallout.Common` as a thin facade** re-exporting the layered types. Less churn. **Not chosen** — keeps a name we want gone ("don't drag dead weight").
- **Minimal — fix only the internal mismatches.** Smallest break. **Not chosen** — leaves the catch-all and doesn't achieve the onion goal.
- **Defer to "after shim sunset" (status quo per rebrand-plan).** **Not chosen** — see "Why now"; the work is wanted this year.

## Amendments

- **2026-06-01 — `Fallout.Core` + filesystem/external-IO is kernel-level (resolves the "Utilities as Infrastructure vs shared kernel" judgment call above, and amends the layer table's IO/Net/compression/globbing → Infrastructure entry).** During the Infrastructure ring (spike 0003 steps 5a/5d), the open default in *Judgment calls* — "IO/Net/compression are genuinely infrastructure" — was **reversed for the fluent vocabulary**. A new innermost shared ring **`Fallout.Core`** (Domain/Application/Infrastructure all depend on it; Domain stays zero-dep otherwise) holds the pure helpers **and** the fluent IO vocabulary. **Naming note:** the kernel ring deliberately reuses the `Fallout.Core` name that step 1 freed when the former domain project `Fallout.Core` became `Fallout.Domain` — distinct rings sharing a name across the realignment's history, not the same package; its sub-projects follow `Fallout.Core.IO[.Compression|.Globbing]` / `Fallout.Core.Net` / `Fallout.Core.Text.[Json|Yaml]` with `namespace = project`. It holds:
  - **Filesystem is a kernel-level capability** (like the BCL's `File`/`Directory`): `AbsolutePath` + all its fluent ops (`.ReadAllText`/`.GlobFiles`/`.CreateDirectory`/`.ZipTo`/…) stay in Core. Rationale: the fluent API is used pervasively across the Application ring; routing it through an `IFileSystem` port would be hundreds of call sites that kill the ergonomics, and placing it in Infrastructure would fail the Application-ring fitness gate.
  - **The same reasoning extends to the rest of external-IO *vocabulary* (5d):** the fluent HTTP client (`HttpClient.CreateRequest(...).Send()` extensions over the BCL `HttpClient`), compression (`.ZipTo()`/`.UnZipTo()` — themselves `AbsolutePath` extensions), and glob all stay in Core. Every one is consumed by *gated* Application-ring code (tool wrappers, `Components`, version-resolver attributes), so a clean move would break the gate, and they are thin ergonomic layers over BCL capabilities — not genuine external adapters worth a port. `HttpTasks`/`FtpTasks` (download-URL-to-file) are the one coarse "network task" category; kept in Core for now, with an optional `IHttpDownload`-style port → Infrastructure left as deferred future work (not required for the gate).
  - **What still goes to Infrastructure** is unchanged: process/tool execution (4b), CI host adapters (5b), tool-path/package resolvers (4b), and project/solution readers (5c, via ports). The seam is *genuine external side-effecting adapters behind Application-owned ports*, not "anything in a namespace called IO/Net."

- **2026-06-01 — project-file renames + splits + the `Fallout` meta-package (the realignment's final mechanical step).** With every namespace settled, the project files / assemblies / NuGet package IDs were brought to `ring = project = namespace = assembly = package`. Ring-pure projects were renamed (`Fallout.Build`→`.Application`, `Fallout.Components`→`.Application.Components`, `Fallout.ProjectModel`→`.Infrastructure.ProjectModel`, `Fallout.Utilities*`→`Fallout.Core*`); the three mixed projects were split into ring-pure assemblies (`Fallout.Tooling`→`Application.Tooling`+`Infrastructure.Tooling`; `Fallout.Solution`→`Application.Solutions`+`Infrastructure.Solutions`; the `Fallout.Common` catch-all dissolved into `Application.Tools`+`Infrastructure.CI`+`Application`+`Core`).
  - **Consumer anchor = a new thin `Fallout` meta-package** (resolves the open question of what replaces `Fallout.Common` as the "reference-one-package" entry point). It holds no code: it references every ring and carries the MSBuild integration (`Fallout.props`/`.targets`), the MSBuildTasks publish output, and the source-generator analyzer. `dotnet fallout :setup`/`:update` target it.
  - **Splitting ports from adapters required a module-init fix.** Co-hosting ports + their `[ModuleInitializer]` adapter registrations in one assembly had masked a latent bug: `Assembly.Load` loads metadata but does **not** run a `[ModuleInitializer]` (it fires lazily on first *use* of a type in the assembly). Once adapters moved to assemblies nothing references by type, their registration never ran (null `ToolingServices`/`SolutionServices` → swallowed NRE at build runtime, invisible to unit tests). Fix: the build's force-loader (`BuildManager.Initialize`) now calls `RuntimeHelpers.RunModuleConstructor` on each `Fallout.*` assembly; the source generator reads solutions via a direct `SolutionReader` call (a Roslyn host can't use the locator); test hosts force-run the adapter module ctors. This is the general pattern for any future ring split.
  - **Placement is dependency-driven, not namespace-driven.** Injection/extension attributes (`[Solution]`, `[Latest*]`, `[GitRepository]`, globbing attrs) and a few `ControlFlow`/`Configure`-coupled IO helpers (FtpTasks/HttpTasks) live in the `Fallout.Application` assembly despite their `Application.Solutions`/`Application.Tooling`/`Core.IO` namespaces — their base classes live there and the leaf ring projects are referenced *by* Application, so homing them in the leaves would cycle. Accepted residual: a handful of `Core.IO`-namespace files sit in the Application assembly (ring-safe — no Infra dependency; the gate stays green); a namespace-vs-assembly tidy-up is left for later.
  - **TFMs:** `net472` dropped (Fallout is `dotnet build`-only); `Fallout.Domain`→`net10.0`; `netstandard2.0` retained only on the Roslyn generators and the ring halves they consume; `Fallout.Infrastructure.ProjectModel` keeps `net8.0;net9.0;net10.0` (per-TFM Microsoft.Build pinning matches the evaluator to the SDK — load-bearing, not incidental).

- **2026-06-01 — `Nuke.*` shim + Migrate redesign (the deferred migration strategy, now resolved).** The realignment deliberately deferred the shim/migration rework "until the rings land" (above). Now landed: **one canonical `Nuke ↔ Fallout` namespace map** (`src/Shared/NukeNamespaceMap.cs`, linked into the generator + both migration projects) is the single source of truth for both directions — the transition-shim generator (Fallout→Nuke re-exports) and the `fallout-migrate` rewriters (Nuke→Fallout). This fixes the desync the onion caused (the migration's blind `Nuke.`→`Fallout.` swap had been producing dead `Fallout.Common.*` output).
  - **The 1:1 shim-package↔Fallout-project mapping is abandoned** (it assumed `Nuke.Common`↔`Fallout.Common`, which dissolving `Fallout.Common` broke). Shim-package count now tracks **NUKE's real consumer packages**: `Nuke.Common` (references the `Fallout` meta and re-exports every `Nuke.Common.*` sub-namespace wherever the source type now lives across the rings) + `Nuke.Components`. **`Nuke.Build` is deleted** — never a real consumer package.
  - **The generator is map-driven**: it emits the map rows whose `ShimPackage` equals the compiling assembly's name (replacing the per-assembly `[assembly: ShimAllPublicTypesUnder]` markers), with longest-Fallout-prefix-wins ownership so split namespaces (CI/ProjectModel/IO) don't double-emit.
  - **Cake migration support is dropped** (low demand; the original author's feature). Nuke→Fallout is the supported migration; even that is kept deliberately lightweight given current demand.
  - Validated by re-including the previously-excluded compat tests (`Nuke.Common.Shim.Tests`, `Nuke.Consumer`) — a pure NUKE consumer surface compiles through the 2 shims. **This closes the realignment**: the onion is structurally complete and NUKE-compat is restored.

## References

- Current-state map: see the structural survey summarized in the spike.
- Shim machinery: `src/Fallout.SourceGenerators/TransitionShimGenerator.cs`, `src/Shims/Nuke.*`
- Migrate codefix: `src/Fallout.Migrate`, `src/Fallout.Migrate.Analyzers`
- Deferral being amended: `docs/rebrand-plan.md` §"What's deliberately deferred"
- Spike: [docs/spikes/0002-onion-domain-ring.md](../spikes/0002-onion-domain-ring.md)
