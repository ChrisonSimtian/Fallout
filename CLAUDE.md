# CLAUDE.md

Guidance for Claude Code (and other AI assistants) when working in this repo.

## What this project is

**Fallout** — a build automation system for C#/.NET, hard-fork successor to [NUKE](https://github.com/nuke-build/nuke). The build is itself a C# console app (`build/_build.csproj`), so any change to the framework can be dogfooded by running `./build.ps1` (Windows) or `./build.sh`.

Originally NUKE by [matkoch](https://github.com/matkoch); under new maintenance as of 2026 and being renamed to Fallout. The codebase is mature, large, and has long-standing conventions — prefer matching existing patterns over introducing new ones.

**Rebrand status:** the structural rename has landed — namespaces (`Fallout.*`), package IDs (`Fallout.Common`, `Fallout.Build`, etc.), project filenames (`src/Fallout.*`), and the global tool name (`dotnet fallout`) are all in place. Legacy `Nuke.*` lives on only as the consumer transition shim at `src/Shims/Nuke.Common` (typed wrappers that inherit from the `Fallout.*` types). Remaining rebrand work — migration CLI polish, Roslyn codefix, docs site port, coordinated announcement — is rolled into v11 (see "Active major" below). See [docs/rebrand-plan.md](docs/rebrand-plan.md) for the locked namespace mapping and bridge strategy.

**Active major: v11 (milestone [#6](https://github.com/ChrisonSimtian/Fallout/milestone/6)) = rebrand completion + plugin-architecture foundation.** Two workstreams: finish the rebrand (the 9 issues moved over from the rebrand milestone), and lay internal groundwork for a plugin model (extract `Fallout.Core` domain, introduce DI, split the `FalloutBuild` god class, internal `IBuildMiddleware` pipeline, fitness tests, coverage uplift). **No public plugin SDK in v11** — that's v12 (milestone [#7](https://github.com/ChrisonSimtian/Fallout/milestone/7)). The internal middleware/listener interfaces stay `internal` in v11; do not expose them via `InternalsVisibleTo` to non-test assemblies. See [docs/roadmap.md](docs/roadmap.md) for the full picture and the five open RFCs ([#97](https://github.com/ChrisonSimtian/Fallout/issues/97)–[#101](https://github.com/ChrisonSimtian/Fallout/issues/101)) driving v12 design.

## Stack

- .NET SDK pinned in `global.json` (currently `10.0.100`, `rollForward: latestMinor`).
- Central package versions in `Directory.Packages.props` — never add a `Version=` to an individual `PackageReference`.
- xUnit + FluentAssertions + Verify.Xunit for tests.
- Solution file is `fallout.slnx` (new XML solution format, not `.sln`).

## Repository layout

Canonical top-level structure (see [docs/architecture.md](docs/architecture.md) for the long version):

| Path | What lives here |
|---|---|
| `src/` | All production library projects (`src/Fallout.<X>/Fallout.<X>.csproj`). The one exception is `src/Shims/Nuke.Common/` — the consumer transition shim. |
| `tests/` | All test projects (`tests/Fallout.<X>.Tests/Fallout.<X>.Tests.csproj`), plus `tests/Nuke.Common.Shim.Tests/` covering the shim. |
| `vendor/` | Vendored third-party source we maintain a fork of (currently the `vs-solutionpersistence` fork — packaged as `Fallout.VisualStudio.SolutionPersistence`). |
| `build/` | The build orchestrator (`_build.csproj` + `Build.*.cs` partial files). |
| `docs/` | Documentation site content. Architecture notes also live here. |
| `.assets/` | Images, icons, logos — anything binary and non-code. |
| Root | Solution file (`fallout.slnx`), shared MSBuild plumbing (`Directory.Build.props/targets`, `Directory.Packages.props`), `AssemblyInfo.cs`, project conventions (README/CONTRIBUTING/CODE_OF_CONDUCT/LICENSE/CHANGELOG/CLAUDE.md). |

Production project groupings under `src/`:

| Area | Projects |
|---|---|
| Core framework | `Fallout.Common`, `Fallout.Build`, `Fallout.Build.Shared`, `Fallout.Components`, `Fallout.Tooling` |
| Code generation | `Fallout.SourceGenerators`, `Fallout.Tooling.Generator` |
| Models | `Fallout.ProjectModel`, `Fallout.SolutionModel` |
| Tooling | `Fallout.Cli`, `Fallout.MSBuildTasks` |
| Migration | `Fallout.Migrate` (CLI for NUKE → Fallout repo migration) |
| Utilities | `Fallout.Utilities`, `Fallout.Utilities.IO.Compression`, `Fallout.Utilities.IO.Globbing`, `Fallout.Utilities.Net`, `Fallout.Utilities.Text.Json`, `Fallout.Utilities.Text.Yaml` |
| Vendored | `Fallout.VisualStudio.SolutionPersistence` (fork of upstream `Microsoft.VisualStudio.SolutionPersistence`, sources in `vendor/`) |
| Transition shim | `src/Shims/Nuke.Common` — typed wrappers in the `Nuke.*` namespace inheriting from the `Fallout.*` types. Lets pre-rename consumers compile against the new packages without source changes. |

Tool wrappers (the `.json` schemas Claude is most likely asked to extend) live under `src/Fallout.Common/Tools/<Tool>/<Tool>.json`.

## Common commands

```powershell
# Build the build
./build.ps1                          # default target = Pack
./build.ps1 Compile
./build.ps1 Test
./build.ps1 GenerateTools            # regenerate tool wrappers from JSON
./build.ps1 --help                   # list all targets and parameters

# Or via dotnet directly when iterating on a single project
dotnet build fallout.slnx
dotnet test tests/Fallout.Common.Tests/Fallout.Common.Tests.csproj
```

Do not commit code generated by `GenerateTools` — per `CONTRIBUTING.md`, generated code is regenerated manually once per release.

## Branching & releases

Trunk-based:
- `main` — single long-lived branch. Default branch on GitHub. Every merge can publish a release.
- `feature/<slug>`, `bugfix/<slug>`, `chore/<slug>`, `pr/<num>-<slug>` — short-lived branches, opened as PRs against `main`.
- No `develop`, `release/*`, `master`, or `hotfix/*` branches.

CI providers in use: **GitHub Actions only** (the other providers were dropped — see [#8](https://github.com/ChrisonSimtian/Fallout/issues/8) for the demand-driven revival roadmap).

Validation workflows: **`ubuntu-latest`** runs on every PR targeting `main` (with `paths-ignore` for `docs/**`, `.assets/**`, `**/*.md`). **`windows-latest`** and **`macos-latest`** run only on push to `main` — they're post-merge / release validation, not PR gates. This is a deliberate cost trade-off.

**Versioning:** [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) — configured in `version.json` at the repo root. Major+minor is hand-bumped; patch comes from git-height. `main` is the public-release ref (stable versions); everything else gets prerelease tags. GitVersion is still installed as a transitional helper for `MajorMinorPatchVersion` in `Build.cs`; full removal is a follow-up.

**Release pipeline:** `.github/workflows/release.yml` — triggered on push to `main`, runs the three-step shape (`actions/setup-dotnet` → `dotnet tool restore` → `dotnet fallout Test Pack Publish`). **Publishes to nuget.org** (`https://api.nuget.org/v3/index.json`) under the `Fallout.*` package ID prefix, using the `NUGET_API_KEY` repo secret (a nuget.org API key scoped to push `Fallout.*`). Prefix reservation tracked in [#33](https://github.com/ChrisonSimtian/Fallout/issues/33).

**Adding a new `Fallout.X` package — first-publish gotcha.** nuget.org's `Fallout.*` prefix reservation is per-ID, not per-prefix-wildcard: CI's first `nuget push` for any never-published `Fallout.X` package ID returns `403 (does not have permission to access the specified package)` until someone manually web-uploads one nupkg to register the ID. **Two traps when doing that upload:**

1. **Set the package owner to the org, not your personal account.** The nuget.org upload UI doesn't prompt you; ownership defaults to the uploading user's profile. If you forget, the package ID is reserved but the org's `NUGET_API_KEY` still 403s on subsequent pushes (the key is scoped to org-owned packages). Fix via `Manage Package → Owners → Add owner → <org>` then optionally remove your personal account. Or upload using credentials of the org's service account directly. See [#208](https://github.com/ChrisonSimtian/Fallout/issues/208) for what this looks like when it goes wrong.
2. **Validation can lag** the upload by 5–30 minutes. The package page may say "approved" while the API key permission hasn't propagated yet. Wait, then rerun the release pipeline (`gh run rerun <id> --failed`); `--skip-duplicate` makes the retry safe for already-published packages.

## Conventions worth respecting

- **Centralized package versions** — add new packages to `Directory.Packages.props`, never inline. Adding a *meaningful* library (not a tiny transitive helper)? Add a row to [docs/dependencies.md](docs/dependencies.md) in the same PR — reviewers will ask.
- **Tool wrappers**: copy/paste from neighbours; cover full commands; use `<c>`, `<a>`, `<ul>`/`<ol>`, `<em>`, `<para/>` in `help`; don't write `secret: false` or `default: xxx`.
- **Tests next to code, separate folder**: every `Foo` project under `src/` has a sibling `Foo.Tests` project under `tests/`. Mirror the namespace.
- **No IDE-specific style files committed.** `.editorconfig` and `*.DotSettings` were removed during the takeover — relying on `dotnet format` defaults and review.
- **Telemetry opt-out is set in test runs** (`FALLOUT_TELEMETRY_OPTOUT=true`). Keep it that way.
- **License header**: every source file starts with the 4-line `// Copyright … Maintainers of Fallout. // Originally based on NUKE …` block — copy from a neighbouring file when adding new ones.

## What not to do

- Don't reintroduce `source/` — production code lives under `src/`, tests under `tests/`. Same for `images/` (now `.assets/`).
- Don't add `secret`/`default` defaults to tool JSON files (see CONTRIBUTING.md).
- Don't introduce a new test framework or assertion library — stay on xUnit + FluentAssertions + Verify.
- Don't commit `output/` or any `bin/`/`obj/` directory.
- Don't commit `nuke-global.sln` or other `nuke-global.*` files — they're generated by `GenerateGlobalSolution`.
- Don't bypass `Directory.Packages.props` or `Directory.Build.targets`.
- Don't reintroduce `.editorconfig` or `*.DotSettings` without a maintainer-level decision — they were intentionally removed.

## When asked to add a tool wrapper

1. Find the closest existing tool under `src/Fallout.Common/Tools/<Tool>/<Tool>.json`.
2. Copy its shape; cover a full command with all arguments.
3. Run `./build.ps1 GenerateTools` to verify it generates cleanly.
4. Do **not** commit the generated `.cs` output.

## Useful pointers for AI assistants

- The `build/Build.*.cs` files are the canonical example of how to consume the framework — read these when reasoning about user-facing APIs.
- `src/Fallout.Common/Tools/<Tool>/<Tool>.json` files are the source of truth for tool wrappers; the `.cs` next to them is generated.
- Source generators (`src/Fallout.SourceGenerators`) produce per-target code at compile time — if a symbol seems missing, check whether it's generated.
- The Verify snapshots (`*.verified.txt`) under `tests/` are the contract for generator output; review carefully when they change.
