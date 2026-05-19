# Architecture

Canonical reference for how the Fallout repo is laid out and how the pieces fit together.

## Top-level layout

```
.
├── .assets/                  Images, icons, logos — anything binary and non-code
│   ├── icon.png              Package icon (referenced by Directory.Build.props)
│   └── images/               README / marketing imagery
├── .github/                  GitHub Actions workflows
├── .nuke/                    Build orchestrator runtime state (committed: schema, parameters)
├── build/                    The build orchestrator project (consumes Fallout itself — dogfooding)
│   ├── _build.csproj
│   └── Build.*.cs            Partial classes split by concern (CI, Licenses, etc.)
├── docs/                     Documentation site content + architecture notes (this file)
├── src/                      All production library projects
│   └── Nuke.<X>/Nuke.<X>.csproj
├── tests/                    All test projects
│   └── Nuke.<X>.Tests/Nuke.<X>.Tests.csproj
├── AssemblyInfo.cs           Shared InternalsVisibleTo declarations (included by Directory.Build.props)
├── Directory.Build.props     Shared MSBuild properties + ItemGroups applied to every project
├── Directory.Build.targets   Smart PackageReference → ProjectReference logic
├── Directory.Packages.props  Central package version management — never put Version= inline
├── nuke-common.slnx          Solution file (new XML format, not .sln)
├── global.json               Pinned .NET SDK
├── version.json              Nerdbank.GitVersioning config
├── nuget.config              Restricts package sources to nuget.org with explicit mapping
└── build.{ps1,sh,cmd}        Bootstrap entry points
```

## Why the layout looks like this

### `src/` vs `tests/` split

Production code and tests live in separate top-level directories so:

- Project filters in IDEs map cleanly to "what ships" vs "what verifies."
- CI can target `tests/**` patterns without writing per-project exclusions.
- `IsPackable` is name-based (`MSBuildProjectName.EndsWith('Tests')` → false) — no manual opt-out per project.

The previous monorepo style under `source/` mixed both, and `source/Directory.Build.props` had to special-case the `*.Tests` projects. After the split, the split is structural.

### `.assets/` for binary content

Images, icons, logos, and other non-code binary content live under `.assets/`. The leading dot keeps it out of most CI path filters and signals "not source." The package icon (`.assets/icon.png`) is referenced via `$(MSBuildThisFileDirectory).assets\icon.png` in `Directory.Build.props` — independent of project depth.

### `build/` consumes the rest of the repo (dogfooding)

`build/_build.csproj` `ProjectReference`s `src/Nuke.Components`, `src/Nuke.Tooling.Generator`, and `src/Nuke.SourceGenerators`. Any change to the framework can be exercised by running `./build.ps1` — if the build itself breaks, you'd notice immediately.

### Shared build files hoisted to root

`Directory.Build.props` and `AssemblyInfo.cs` live at the repo root rather than under `src/` or `tests/`. Reason: both `src/<Project>/` and `tests/<Project>/` projects need to inherit them, and hoisting to root means MSBuild's directory walk finds them once without per-tree duplication.

## Project groupings under `src/`

| Area | Projects | Purpose |
|---|---|---|
| Core framework | `Nuke.Common`, `Nuke.Build`, `Nuke.Build.Shared`, `Nuke.Components`, `Nuke.Tooling` | The API consumers reference and the host runtime that executes targets. |
| Code generation | `Nuke.SourceGenerators`, `Nuke.Tooling.Generator` | Roslyn source generators that produce per-target code at compile time, plus the `.cs`-from-`.json` tool-wrapper generator. |
| Models | `Nuke.ProjectModel`, `Nuke.SolutionModel` | Strongly-typed wrappers over `.csproj` / `.sln` / `.slnx`. |
| Tooling | `Nuke.GlobalTool`, `Nuke.MSBuildTasks` | The `dotnet nuke` global tool and the MSBuild tasks layer it builds on. |
| Utilities | `Nuke.Utilities` + sub-packages (`IO.Compression`, `IO.Globbing`, `Net`, `Text.Json`, `Text.Yaml`) | Standalone helpers reusable outside the build context. |

Every project under `src/` has a sibling under `tests/` (e.g. `src/Nuke.Common/` → `tests/Nuke.Common.Tests/`).

## Build conventions

- **Central package versions.** All `PackageReference` versions live in `Directory.Packages.props`. Never inline `Version=` on a `PackageReference` — the build will error.
- **Smart `PackageReference`.** `Directory.Build.targets` rewrites `PackageReference`s that match a project in the current solution into `ProjectReference`s. Lets us reference our own packages by ID across the dev/release boundary.
- **`AssemblyInfo.cs` at root.** Shared `InternalsVisibleTo` declarations. Included automatically via `Directory.Build.props`.
- **License header.** Every `.cs` file starts with the 4-line Fallout/NUKE attribution block. See `CLAUDE.md` for the exact shape.

## CI layout

| Workflow | When it runs | What it does |
|---|---|---|
| `ubuntu-latest.yml` | Every PR targeting `main` (with `paths-ignore` for docs/.assets/markdown) | Test + Pack. Fast Linux feedback loop. |
| `windows-latest.yml` | Push to `main` only | Test + Pack on Windows after merge. |
| `macos-latest.yml` | Push to `main` only | Test + Pack on macOS after merge. |
| `release.yml` | Push to `main` | Test + Pack + Publish to GitHub Packages. |

Linux runs on PRs because it's cheap and fast; Windows and macOS are reserved for post-merge validation. If either breaks post-merge, it surfaces as a red commit on `main` and we fix forward.

## What this doc deliberately does NOT cover

- API design decisions inside individual projects — read the project's tests for those.
- Rebrand status and migration strategy — see `CLAUDE.md` and the [Fallout rebrand milestone](https://github.com/ChrisonSimtian/Fallout/milestone/1).
- Contribution workflow — see `CONTRIBUTING.md`.

When in doubt, the structure is whatever this file says it is. If you change the layout, update this file in the same PR.
