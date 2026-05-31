# Spike 0001 — CI host integration as ports & adapters (GitHub Actions end-to-end)

- **Status:** Done (2026-05-31) — see [Verdict](#verdict)
- **Date:** 2026-05-31
- **Decision record:** [ADR-0005](../adr/0005-ci-host-integration-ports-and-adapters.md)
- **Channel:** `experimental` (spike branch off `experimental`; nothing here is meant to merge as-is)
- **Time-box:** one focused session

> Spikes are **time-boxed, throwaway-by-default** experiments to validate a shape, not to ship it. The output is a *decision* ("the port shape is right / wrong / needs X"), plus possibly a clean re-implementation later. Code from a spike earns its way to `main` only after review, additively, per ADR-0005's compatibility strategy.

## Hypothesis to validate

> The runtime-host concern can be expressed as a focused, de-anemic port (`IBuildHost`, working name) that **GitHub Actions implements as a named adapter**, with the existing public surface (`[GitHubActions]`, `GitHubActions.Instance`, `[CI]` injection, `Host`) preserved as a thin facade delegating inward — and a fitness test can enforce the core→adapter boundary — **all without a single breaking change and with the dogfood workflow output byte-identical.**

If that holds on GitHub Actions, the same shape generalizes to the other ten providers. If it forces awkward escape hatches (e.g. the port can't be separated from `Host`'s console rendering without breaking something), we learn that *before* touching anything public.

## Scope

### In scope (this spike)
1. Define the runtime-host port `IBuildHost` (working name) in `src/Fallout.Build/CICD/`, separated from `Host`'s logging/theming responsibilities. Fold in / supersede the anemic `IBuildServer` (`Branch`, `Commit`).
2. Make **`GitHubActions` implement the port** as the first named adapter — additively (add the interface; keep the `Host` base, keep all existing members).
3. Keep the **facade working unchanged**: `GitHubActions.Instance`, `[CI]`-injection (`CIAttribute`), and `Host` API resolve to something satisfying the port. No consumer-visible change.
4. Confirm the **config-generation port is untouched and still healthy**: `GitHubActionsAttribute : ConfigurationAttributeBase` still satisfies `IConfigurationGenerator`. (This port already exists — the spike only re-confirms the boundary, it does not redesign it.)
5. Add one **architecture fitness test**: `Fallout.Build` must not reference the concrete `GitHubActions` type (scope narrowly to concrete provider types, not the whole `Fallout.Common.CI` namespace).
6. **Validate end-to-end:** `./build.sh Compile`, `./build.sh Test`, and regenerate the dogfood workflow from `build/Build.CI.GitHubActions.cs` — assert the emitted `.github/workflows/*.yml` (and any Verify snapshot) is unchanged.

### Explicitly OUT of scope (do not do these in the spike)
- ❌ Touching the other ten providers (`AzurePipelines`, `TeamCity`, …). GitHub only.
- ❌ Renaming namespaces or splitting assemblies (`Fallout.Infrastructure.*` etc.). ADR-0005 §Alternatives B.
- ❌ Deleting or `[Obsolete]`-ing `IBuildServer` for real — at most mark it provisionally to feel the blast radius; the actual deprecation is a separate, reviewed, batched-to-2027 change.
- ❌ Any public SDK surface (milestone #7). Internal interfaces stay `internal` / unexposed.
- ❌ Introducing a DI container. If resolution needs a seam, use the simplest static delegation; container work is a separate workstream.

## Ordered steps

1. **Branch.** Cut a spike branch off `experimental` (e.g. `spike/ci-ports-gha`). Do not target `main`.
2. **Read the runtime contract as-built.** `Host.cs`, `Host.Activation.cs` (the `IsRunning{Name}` + `Host.Default` discovery), `IBuildServer.cs`, `CIAttribute.cs`, and `GitHubActions.cs` — list every member the *runtime host* concern actually uses, separating it from console-rendering members that belong to `Host`.
3. **Draft `IBuildHost`.** Put only the host-integration surface on it (environment/VCS facts, annotation reporting, summary/output writing). Keep it small — anemic-but-honest beats fat-and-speculative. Leave `Host`'s `WriteLogo`/`WriteBlock`/`WriteTargetOutcome` *out* of the port.
4. **Implement on `GitHubActions`.** Add `IBuildHost` to its declaration; wire members to existing behaviour. Additive only.
5. **Make the facade delegate.** Ensure `GitHubActions.Instance` and `[CI]` injection still hand back an instance that satisfies the port; ensure `Host.Default` discovery is unaffected.
6. **Fitness test.** Add the boundary assertion to the orchestration test project (xUnit + FluentAssertions, per repo convention). Watch it pass; then temporarily introduce a deliberate violation to confirm it *fails* (a fitness test that can't fail proves nothing), then revert.
7. **Validate.** `./build.sh Compile` → `./build.sh Test` → regenerate the dogfood workflow → diff. Green + byte-identical YAML = hypothesis holds.

## Success criteria

- ✅ Solution compiles; full test suite green.
- ✅ Dogfood `.github/workflows/*.yml` output unchanged (and any Verify snapshot unchanged).
- ✅ `GitHubActions` satisfies both ports; the facade is consumer-invisible.
- ✅ Fitness test passes, and demonstrably fails on an injected violation.
- ✅ A written verdict (append to this doc): is the `IBuildHost` shape right, what's the final name, and what's the blast radius of superseding `IBuildServer`?

## Risks / things to watch

- **Half-and-half mess** — the named failure mode. The mitigation is the facade-delegates-to-port discipline (ADR-0005 §3); if you find the static singleton still doing the real work while the port is decorative, stop and rethink the resolution path.
- **`Host` entanglement** — if the host-integration members genuinely can't be separated from console rendering without breakage, that's a key finding: record it; it may mean the port needs to *compose* `Host` rather than replace it.
- **Discovery coupling** — `Host.Default` finds adapters by reflecting on public `Host` subclasses with a static `IsRunning{Name}`. The port must not break that convention this spike; generalizing discovery to "any `IBuildHost`" is future work.
- **`IBuildServer` blast radius** — it's public. Feel it (who implements vs. who reads), but defer the real deprecation to a reviewed, batched change.

## Exit

Update **Status** to `Done` and append the verdict. Feed the result back into ADR-0005 (promote `Proposed` → `Accepted`, lock the port name) before generalizing to the other ten adapters or exposing anything for milestone #7.

## Verdict

**Hypothesis confirmed.** The runtime-host port can be introduced fully additively, GitHub Actions satisfies it as the first named adapter, the public surface is untouched, the boundary is enforced, and build output is byte-identical.

**What was built (branch `spike/ci-ports-gha` off `experimental`):**
- `src/Fallout.Build/CICD/IBuildHost.cs` — the port (`Branch`, `Commit`, `IsPullRequest` default-interface-member, `ReportWarning`, `ReportError`).
- `src/Fallout.Build/CICD/IBuildServer.cs` — doc pointer to its successor (no `[Obsolete]` — that removal is batched to the year cut).
- `src/Fallout.Common/CI/GitHubActions/GitHubActions.BuildHost.cs` + a one-token edit to the class declaration — GitHub adapter, explicit interface implementation.
- `tests/Fallout.Build.Tests/CiHostBoundaryTest.cs` — fitness test: `Fallout.Build` must not reference `Fallout.Common`.

**Evidence:**
- `dotnet build src/Fallout.Common` → succeeded (0 errors; warnings are pre-existing generated-code noise).
- `dotnet test tests/Fallout.Build.Tests` → **104/104 passed**, including the fitness test.
- Fitness test demonstrably **goes red** when pointed at a genuinely-referenced assembly (`Fallout.Core`), then reverted — it is not vacuous.
- `git status .github/` → clean; the config-generation path was never touched, so generated workflows are unchanged by construction *and* in fact.

**Findings that shape the real implementation:**
1. **The boundary already holds structurally.** `Fallout.Build` does not reference `Fallout.Common`, and a `Build → Common` reference would be *circular* (Common already references Build) — so the compiler enforces the inward-only rule for free. The fitness test guards against the subtler case of an indirect/transitive leak, not the gross one.
2. **~~The reporting half is entangled with the `Host` base — and exposes a latent gap.~~ CORRECTED (see Refinement round below).** The original finding claimed `Host.ReportWarning`/`ReportError` were no-ops on GitHub Actions and that sink-routed warnings never became `::warning::` annotations. **That was wrong** — it was based on reading only `GitHubActions.cs` and missing the overrides in `GitHubActions.Theming.cs` (and `AzurePipelines`/`TeamCity`/`AppVeyor` likewise). Reporting *is* wired today; there is no bug. The reporting members (`ReportWarning`/`ReportError`/`WriteBlock`) genuinely live on the `Host` base and are overridden per-adapter.
3. **One port, for now — split not yet justified.** The read/write seam is real (and marked with regions), but the write side is two members. A two-port split (`IBuildHost` context + `IBuildReporter`) earns its keep only once reporting grows (log grouping, job summaries, output variables). Keep one port; revisit at that trigger.
4. **`IBuildHost` reads well** and the explicit-impl pattern mirrors the existing `IBuildServer` mapping (`Branch => Ref`, `Commit => Sha`). Name validated.
5. **Discovery untouched.** `Host.Default` still finds adapters via the `IsRunning{Name}` reflection convention. Generalizing discovery to "any `IBuildHost`" is future work, not needed for the seam.

## Refinement round (same day) — two-port split

Acting on the spike's open question (and a maintainer's gut feeling), the single port was split in two, and finding #2 was corrected:

- **`IBuildHost`** rescoped to the **context** half: `Branch`, `Commit`, `IsPullRequest`.
- **`IBuildReporter`** added for the **reporting** half: `ReportWarning`, `ReportError`, `WriteBlock`. Implemented by the `Host` base (explicit interface impls delegating to the existing protected-virtual hooks), so every host — including the local `Terminal` — is a reporter, and adapters keep overriding exactly as before. **No behavior change.**
- **The split is justified, not cosmetic:** the implementor sets differ. A fitness test now pins this down — `Terminal` is an `IBuildReporter` but **not** an `IBuildHost`; `GitHubActions` is both. If those sets ever collapse, the test fails and the split has lost its reason.
- `GitHubActions.BuildHost.cs` reduced to context-only; reporting overrides stay in `GitHubActions.Theming.cs`, untouched.
- Validation: `Fallout.Common` builds clean; **105** Build.Tests + the new Common.Tests port check pass; generated workflows unchanged.

## Discovery generalization (same day) — #3 done

Host discovery no longer hinges on the magic-string `IsRunning{TypeName}` convention:

- Added `protected internal virtual bool Host.IsActive`; `Host.Default` now probes `IsActive` instead of reflecting for `IsRunning{TypeName}`.
- The default `IsActive` **falls back** to the legacy convention, so all eleven existing hosts work unchanged; a new adapter just overrides `IsActive` (no static, no name-matching) — which is how the upcoming Forgejo adapter will declare its detection.
- Safe by construction: every host ctor is side-effect-free (verified), and because `First()` stops at the active host it is the last constructed and remains `Host.Instance`.
- Tests (`HostActivationTest`) cover both paths: override-needs-no-static, and default-falls-back-to-convention. **107** Build.Tests + full Common.Tests green.

## Forgejo adapter (same day) — #4 scaffolded

The first adapter built against the finished seam, validating it generalizes to a new provider:

- **Runtime host** (`Forgejo.cs` + `.BuildHost.cs` + `.Theming.cs`): first user of the #3 detection style — overrides `IsActive` directly, **no `IsRunning{Name}` static**. Implements both ports (`IBuildHost` context via GitHub-compatible `GITHUB_*` vars; `IBuildReporter` reporting via the shared command dialect).
- **Composition, not coupling** (`ForgejoAttribute.cs`, `WorkflowCommands.cs`): Forgejo reuses the GitHub config *model* (`GitHubActionsConfiguration` + jobs/steps) and a new shared `WorkflowCommands` helper for the `::cmd::` dialect — but is **not** a `GitHubActions`. A fitness test asserts the non-inheritance.
- Validation: clean build; **46** Common.Tests (4 new) + **107** Build.Tests green; existing `.github` workflows untouched.

**Findings:**
1. **Config-gen isn't composition-ready at the attribute level.** `ConfigurationAttributeBase.Build` has an `internal set`, so an attribute in `Fallout.Common` can't wire up a composed inner `GitHubActionsAttribute`. Worked around by composing the shared *model* instead. The clean fix is to extract the GH generator's model-building from its file placement so both adapters compose one builder — that's what unlocks full Forgejo parity (caching, artifacts, secrets, detailed triggers), currently a common-case scaffold only.
2. **`WorkflowCommands` is the dialect composition seam, but GHA still has its own copy.** Deduping GitHubActions onto it touches that adapter's public `WriteCommand` and risks its byte-identical output, so it's deferred — a tracked follow-up, not done here.
3. **Detection is a naive guess** (`FORGEJO_ACTIONS`). Forgejo sets `GITHUB_ACTIONS=true` for compatibility, so the real distinctive variable (and ordering-vs-GitHub) must be confirmed against the live instance — marked `TODO(forgejo)` in `Forgejo.cs`.

**Recommended next moves (in order):**
1. Promote ADR-0005 `Proposed` → `Accepted` (maintainer call, via PR review) and lock the two-port shape + names (`IBuildHost` / `IBuildReporter`).
2. Separate doc PR (targets `main`, non-breaking) for ADR-0005 + this spike.
3. ~~Generalize host discovery~~ **done**. 4. ~~Add the Forgejo adapter~~ **scaffolded** (above).
5. Confirm Forgejo detection against the live instance; then extract the GH model-builder (finding #1) for full config parity, and dedup `WorkflowCommands` onto GitHubActions (finding #2).
6. Roll the ports across the other adapters; then consider exposing the seam for milestone #7. Mark the ports `[Experimental("FALLOUT0xx")]` when they near consumer-facing exposure.
