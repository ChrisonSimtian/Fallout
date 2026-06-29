# Architecture-fitness tests

`tests/Fallout.Architecture.Specs` is a solution-wide [ArchUnitNET](https://github.com/TNG/ArchUnitNET) suite
that asserts Fallout's intended shape so it can't drift as new code lands. It's an ordinary xUnit project — the
rules run inside `[Fact]`/`[Theory]` and fail the build like any other test (no separate tool to run).

It tracks issue [#95](https://github.com/ChrisonSimtian/Fallout/issues/95). The earlier `Fallout.Core` purity
guard from #88 was migrated here off the (unmaintained) `NetArchTest.Rules`.

## Why scoping is by *assembly*, not namespace

Layering in this repo is a project/assembly concern. The legacy NUKE namespaces deliberately span several
assemblies — `Fallout.Common.*` types live in `Fallout.Build`, `Fallout.Build.Shared` and `Fallout.Common`;
the `Fallout.Utilities.Net` assembly still emits `Fallout.Common.Utilities.Net.*` — so a namespace-based layering
rule would be meaningless. Every rule scopes its subject and target with `ResideInAssembly(...)`.

The shared helper `FalloutArchitecture.TypesIn(...)` also constrains every subject/target to first-party
(`Fallout.*` / `Nuke.*`) namespaces. That strips the no-namespace, tooling-generated types injected into every
assembly (`ThisAssembly` from Nerdbank.GitVersioning, `RefSafetyRulesAttribute`, coverlet instrumentation), which
would otherwise read as spurious cross-assembly dependencies.

## What's governed

The reference set in `Fallout.Architecture.Specs.csproj` **is the contract** — an assembly is scanned only if it's
referenced (so it lands in the test output and gets loaded). Adding a production assembly without adding it there
silently drops it from the gate. Deliberately out of scope: the Roslyn build-time tooling
(`Fallout.SourceGenerators`, `Fallout.Tooling.Generator`, `Fallout.Migrate[.Analyzers]`, `Fallout.MSBuildTasks`)
and the vendored `Fallout.Persistence.Solution` parser.

Current rules:

| File | Rule(s) |
|---|---|
| `LayeringSpecs` | `Core` / `Utilities` are foundations (no in-repo deps); utility satellites depend only on `Utilities`; `Tooling` / `ProjectModel` / `Build.Shared` don't reach upward; `Solution` is a thin facade; nothing depends on the `Cli` composition root; `Fallout.*` never depends on the `Nuke.*` shims. |
| `PuritySpecs` | `Fallout.Core` takes no dependency on `System.IO`, `System.Diagnostics.Process`, `System.Console`, or Serilog (issue #88). |
| `NamingSpecs` | A type's namespace should be rooted at its assembly name. The main debt-bearing rule. |

## The ratchet

The architecture is known to be partially broken, so rules aren't all strict. Each rule is enforced by
`Ratchet.Enforce(rule, because, baseline)` against a baseline of known violations in `KnownViolations`:

- a violation **not** in the baseline fails the test — a new regression;
- a baseline entry that **no longer** violates also fails the test — the architecture improved, so the stale
  entry must be deleted to lock the gain in.

The baseline can therefore only shrink. Invariants that already hold pass `KnownViolations.None` (zero tolerance).
The naming rule ships with a ~220-entry baseline (`NamespaceAssemblyDrift`) capturing today's `Fallout.Common.*`
sprawl; it shrinks as the onion-architecture refactor (ADR-0006, on `refactor/architecture`) renames things.

## Working with it

**A rule went red on your change.** Don't reflexively add the offender to `KnownViolations`. First decide:
is the new dependency *wrong* (fix the code — the rule just did its job) or *intended* (add it to the relevant
list with a one-line justification)? Baseline keys are type full names, exactly as ArchUnitNET reports them — including nested types
(`Fallout.Common.CI.Partition+TypeConverter`) and generic arity (`RequiresAttribute` + backtick + `1`).

**You fixed some debt.** The test will now fail telling you which baseline entries are stale — delete them.

**Regenerating the drift baseline wholesale** (e.g. after a large rename):

```powershell
dotnet test tests/Fallout.Architecture.Specs/Fallout.Architecture.Specs.csproj
```

The failure message for `Types_reside_in_a_namespace_rooted_at_their_assembly_name` lists every current offender
(the `Offenders:` block). Replace `KnownViolations.NamespaceAssemblyDrift` with that sorted, de-duplicated set.

## Adding a rule

Write the ArchUnitNET rule with `FalloutArchitecture.TypesIn(...)` for the subject/target and pass it through
`Ratchet.Enforce` with a `because` string and a baseline (`KnownViolations.None` if it should be strict). Use the
assembly-name constants on `FalloutArchitecture` rather than string literals.
