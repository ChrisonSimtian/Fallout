# tools/

Operational helper scripts that support repo maintenance but are **not** part of the build or release pipeline.

- **Not built.** They aren't referenced by `fallout.slnx` or any `.csproj`. The release pipeline doesn't pack or publish them.
- **Don't bump versions.** `version.json` has `pathFilters` excluding this directory — commits touching only `tools/` don't increment patch height. A typo fix here won't produce a fresh `11.0.x` release.
- **Cross-platform PowerShell Core 7+** by convention. Run with `pwsh` on macOS/Linux/Windows. Same script, no per-OS variants.

## Conventions for new tools

- One script per file. Filename matches the verb-noun PowerShell convention (`Unlist-NugetPackage.ps1`, not `unlist.ps1`).
- Comment-based help on every script. `Get-Help ./Foo.ps1 -Full` should explain inputs, outputs, examples.
- `[CmdletBinding(SupportsShouldProcess)]` so `-WhatIf` and `-Confirm` work for anything that mutates remote state.
- Secrets via explicit parameter first, fallback to an env var (`-ApiKey` → `$env:NUGET_API_KEY`), fail fast if neither.
- Idempotent where reasonable — re-running shouldn't corrupt state.

## Available tools

| Script | Purpose |
|---|---|
| [`Unlist-NugetPackage.ps1`](Unlist-NugetPackage.ps1) | Unlist one or many NuGet package versions from a feed. Single mode (Package + Version[]) or bulk JSON. API key from `-ApiKey` or `$env:NUGET_API_KEY`. |

## Bulk JSON shape (used by `Unlist-NugetPackage.ps1 -JsonPath`)

```json
[
  { "package": "Fallout.Common", "version": "10.3.45" },
  { "package": "Fallout.Common", "version": "10.3.47" },
  { "package": "Fallout.Cli",    "version": "10.3.45" }
]
```

Generate ad-hoc as needed; commit if the operation is worth keeping as a historical record.

- [`unlist-10.3.24-47.json`](unlist-10.3.24-47.json) — **the complete batch (256 entries).** Every published `10.3.x` patch that carries v11 breaking changes. The contamination boundary is exact: `10.3.24` (commit `ef837961`, the System.Text.Json migration that dropped the Newtonsoft surface) is the first breaking patch; everything `≥ 10.3.24` is contaminated, `≤ 10.3.23` is clean. These all shipped under a `10.3` label before the v11 semver-policy fix (#220, #222) caught up. `release/v10.3` is parked at `10.3.23` (the last clean commit) as the maintenance line.
- [`unlist-10.3.40-47.json`](unlist-10.3.40-47.json) — the original 102-entry batch (#224). **Incomplete** — it only covered the `10.3.40`–`10.3.47` tail (the CLI-rename churn) and missed `10.3.24`–`10.3.39`, including the entire STJ breaking migration. Superseded by the `24-47` file above; kept as the historical record of what was first identified.
