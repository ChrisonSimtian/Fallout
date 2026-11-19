# CI import (reverse generation)

Design for `fallout migrate <host>` — reading an existing CI definition and
emitting idiomatic Fallout C#. Tracks [#429](https://github.com/Fallout-build/Fallout/issues/429).

Fallout already generates CI config from C# (the forward path). This adds the
reverse, so a project with existing CI has a path *in*, and so Fallout becomes
the layer that moves a pipeline between hosts (GitHub Actions ⇄ Azure Pipelines)
without a per-host rewrite.

## Goal

Take an existing pipeline and produce a `Build.cs` that, run through the
existing generators, reproduces the pipeline — or runs the same work directly.
First cut targets **GitHub Actions** and **Azure Pipelines** (both have full
forward generators today; see [docs/05-cicd/](../05-cicd/)).

The end state for a migrated project: build logic lives in host-agnostic C#
targets, and each host's YAML collapses to a bootstrap that calls
`./build.ps1 <Target>`. Retargeting a new host is then a regenerated shim, not
a second migration.

## Why route through C#, not YAML→YAML

Direct host-to-host translation (a GitHub step rewritten as the equivalent Azure
task) fights both platforms' models forever — marketplace actions, expression
syntax, service containers, and matrix semantics do not line up. It never
reaches full coverage.

Routing through Fallout sidesteps that: lift the build logic into C# targets
that execute, and the per-host YAML becomes near-empty. Almost nothing
host-specific is left to translate. The remaining host-native surface is small,
explicit, and the only thing a human reviews when switching hosts.

### The asymmetry that makes it tractable

| | Import (this doc) | Export (exists) |
|---|---|---|
| Runs | once per host, per project | continuously |
| Fidelity | best-effort; a human finishes stubs | deterministic |
| Failure mode | a stub a person completes | none — it is the source of truth |

Import is allowed to be imperfect because it runs once and a person closes the
gap. Export is the robust, deterministic engine that already exists. You migrate
*in* once; from then on every host is a regenerated shim.

## Architecture: reuse `ConfigurationEntity` as a shared IR

The forward path is:

```
ExecutableTarget[]  →  IConfigurationGenerator.Generate  →  ConfigurationEntity tree  →  Write(CustomFileWriter)  →  YAML
```

Reverse mirrors it through the *same* intermediate model
(`src/Fallout.Build/CICD/ConfigurationEntity.cs`):

```
YAML  →  IConfigurationReader.Read  →  ConfigurationEntity tree  →  IBuildCodeEmitter  →  C# source
```

- **`IConfigurationReader`** — new, one per host, symmetric with
  `IConfigurationGenerator` (`src/Fallout.Build/CICD/IConfigurationGenerator.cs`).
  Parses host YAML into the host's existing configuration entities
  (`GitHubActionsConfiguration`, `AzurePipelinesConfiguration` and their
  job/step subtrees under `src/Fallout.Common/CI/`).
- **`IBuildCodeEmitter`** — new. Turns the entity tree plus synthesized targets
  into a `Build.cs` and the matching `[GitHubActions]` / `[AzurePipelines]`
  attribute. Mirror of the existing `Write` methods.

Reusing the entity model gives a round-trip test for free (see
[Validation](#validation)).

### Mapping

- `job` → `Target`; `needs:` / `dependsOn:` → `DependsOn()`.
- `matrix` → `Partition()`.
- triggers, runner/agent pools, env, secrets → the CI attribute + `[Parameter]` /
  secret declarations.
- step command → target body (see [Two buckets](#two-buckets)).

Target shape comes from `src/Fallout.Build/Execution/ExecutableTarget.cs`.

## Two buckets

Every step is classified **lift** or **stub**. Nothing is silently dropped.

**Lift** — recognized commands become idiomatic tool-wrapper calls:

```csharp
Target Test => _ => _
    .DependsOn(Compile)
    .Executes(() => DotNetTest(s => s
        .SetConfiguration(Configuration)
        .EnableNoBuild()));          // from `dotnet test --no-build -c Release`
```

**Stub** — anything unconvertible (marketplace actions, host-native deploy)
becomes a *compiling* target that fails loud, carrying the original inline:

```csharp
Target DeployWebApp => _ => _
    .DependsOn(Publish)
    // ── Fallout migrate: could not auto-convert ──────────────────────────
    // Original (.github/workflows/deploy.yml:42):
    //   uses: azure/webapps-deploy@v2
    //   with: { app-name: my-api, package: ./out }
    // Marketplace action — no CLI equivalent. Implement the deploy here.
    // ─────────────────────────────────────────────────────────────────────
    .Executes(() => throw new NotImplementedException("Migrated stub — see TODO above."));
```

Compiles, runs, and fails at exactly the unfinished step with the source in
front of the reader.

### The command recognizer

The lift bucket is driven by a recognizer that is a **reverse index over the
existing tool catalog** (`src/Fallout.Common/Tools/<Tool>/<Tool>.json`):

1. Tokenize the step command (`dotnet test --no-build -c Release`).
2. Match verb/subcommand against tool definitions → `DotNetTest`.
3. Map known flags back to fluent setters (`--no-build` → `EnableNoBuild()`).
4. Unmatched flags are kept as raw args with a `// TODO: unmapped flag` marker.

Coverage grows automatically as the tool catalog grows — no separate mapping
table. The recognizer can ship empty: without it, lift-eligible steps fall back
to shell `Executes`, and the rest still stub. So it is the quality dial, not a
blocker.

## Build order

1. **Readers** (`IConfigurationReader` for GitHub Actions + Azure Pipelines) →
   the existing IR. *The real new surface.*
2. **C# emitter** (`IBuildCodeEmitter`): IR → `Build.cs` + attributes.
3. **Stub emitter**: the boilerplate-with-reference format above.
4. **CLI command + migration report** (lifted N / stubbed M, per host).
5. **Command recognizer** (reverse `Tools/*.json` index) → idiomatic lift.

Steps 1–4 are a shippable MVP: everything converts structurally or stubs, and
the output compiles. Step 5 raises fidelity over time.

## Validation

Round-trip is the fidelity metric: import host YAML → emit C# → run the
*existing* forward generator → diff the regenerated YAML against the original.
The diff for lifted steps should be clean; the per-host residue is the stub set.
This runs as a test using the snapshot machinery already in `tests/`.

## CLI

```
fallout migrate github                 # detect .github/workflows/*, convert
fallout migrate azure                  # detect azure-pipelines.yml
fallout migrate github --dry-run       # report lift/stub counts, write nothing
```

## Scope

In: GitHub Actions, Azure Pipelines. Lift for common .NET commands; shell
fallback + stub for the rest.

Out (first cut): GitLab and other hosts (no forward generator yet — greenfield);
marketplace actions beyond checkout / cache / upload-artifact; full expression-language
coverage (`${{ }}`, `$(...)`, GitLab `rules:`) — map common cases, stub the rest.

Host-native surface that cannot be lifted and stays in the thin shim (be honest
that this is irreducible): checkout, runner/agent selection, OIDC / federated
auth, service connections, GitHub Environments and approvals, secret-store
wiring, host-specific deploy tasks. The migration report names this set per
host — it is the actual work product when moving hosts.

## Open questions

- Merge into an existing `Build.cs` vs. emit a new file when one is present?
- Reconciling two hosts that describe the same build: unify shared (recognized)
  steps into common targets, keep divergent steps per-host? Needs a rule for
  picking the canonical target when both define the same logic slightly
  differently.
- How much of the host-native residue is worth a curated mapping (e.g.
  `actions/checkout` → Fallout's built-in checkout) vs. always stubbing?
