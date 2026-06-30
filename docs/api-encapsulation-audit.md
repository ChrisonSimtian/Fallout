# API encapsulation audit

Companion to the `[PublicAPI]` annotation pass (PR #6). Stamping `[assembly: PublicAPI]`
on the outer (consumer-facing) layer declares each assembly's **entire** public surface as
intentional API. This audit records public types that are `public` by accident and the
decisions taken. (GitHub issues are disabled on this fork, so this doc is the tracker.)

> **Caveat that drives every call here:** repo-only grep is insufficient for a framework.
> A type with zero in-repo references can be internal plumbing **or** an extensibility
> surface only external consumers touch. Verify intent (NUKE history, `public`/`protected
> virtual` reachability) before flipping anything in an outer assembly.

## Tier 1 — done (non-breaking, inner-layer assemblies)

Flipped to `internal` in PR #6 (no consumer-facing API in these assemblies):

| Type | Assembly | Note |
|---|---|---|
| `TaskItemExtensions` | `Fallout.MSBuildTasks` | not packed; in-assembly helper |
| `CodeAnalysisExtensions` | `Fallout.SourceGenerators` | not packed; in-assembly helper |
| `Migration` + nested `Summary` | `Fallout.Migrate` | tool exe; used only by `Program` + tests (IVT). `Migration` internalized with `Summary` to keep accessibility consistent (CS0050) |

## Tier 2 — deferred (breaking; batch to the yearly major cut)

High-confidence implementation details, but in **outer/packed** assemblies — flipping is
breaking, so it follows the breaking-change flow (gate behind `[Experimental(...)]` or hold
on a topic branch; CHANGELOG + `breaking-change` label at PR time).

**`Fallout.Build`**
- `InMemorySink` (`Logging.cs`) — nested log sink, in-assembly only
- `ConsoleUtility` (`Utilities/ConsoleUtility.cs`) — console I/O helper, in-assembly only
- `Terminal` + `Terminal.Rider` / `Terminal.VSCode` / `Terminal.VisualStudio` (`Terminal.cs`) — env-detection markers

**`Fallout.Persistence.Solution`** (csproj already documents intent to narrow these)
- `StringTable`, `PathShim`, `Extensions`

Before flipping: confirm no external consumer usage and no extensibility reachability for each.

## Tier 3 — keep public (intentional extensibility; not candidates)

The ~49 CI Configuration types (`*Step` / `*Trigger` / `*Job` / `*Parameter` / `*Dependency`
+ abstract bases under `Fallout.Common/CI/**/Configuration/`) and abstract attribute bases
(`FileSystemGlobbingAttributeBase`, `AzureKeyVaultAttributeBase`). These are unreferenced
in-repo precisely *because* only external consumers use them: consumers subclass e.g.
`GitHubActionsAttribute`, override `public override ConfigurationEntity GetConfiguration(...)`,
and build these objects. The blanket `[assembly: PublicAPI]` is correct for them — it stops
Rider flagging them as unused. **No action.**

## Clean

`Fallout.Components`, `Fallout.Core`, `Fallout.Tooling`, `Fallout.ProjectModel`, all
`Fallout.Utilities*`, and the `Fallout.Solution` facade audited clean — no accidental publics.
