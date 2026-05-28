# Release and versioning

Branching, semver policy, the PR-creation procedure, and the release pipeline.

## Branching

Trunk-based:

- `main` — single long-lived branch. Default branch on GitHub. Every merge can publish a release.
- `feature/<slug>`, `bugfix/<slug>`, `chore/<slug>`, `docs/<slug>`, `pr/<num>-<slug>` — short-lived branches, opened as PRs against `main`.
- No `develop`, `release/*`, `master`, or `hotfix/*` branches.

CI providers in use: **GitHub Actions only** (others were dropped — see [#8](https://github.com/ChrisonSimtian/Fallout/issues/8) for the demand-driven revival roadmap).

**Validation workflows.** `ubuntu-latest` runs on every PR targeting `main` (with `paths-ignore` for `docs/**`, `.assets/**`, `**/*.md`). `windows-latest` and `macos-latest` run only on push to `main` — they're post-merge / release validation, not PR gates. This is a deliberate cost trade-off.

**Merging.** Both squash and rebase merge are enabled (plain merge commits are disabled by repo setting and would fail linear-history protection anyway). Squash is the default; rebase is opt-in for curated commit sequences. See [CONTRIBUTING.md → Merging](../../CONTRIBUTING.md#merging) for the convention.

## Versioning

[Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) — configured in `version.json` at the repo root. Major+minor is hand-bumped; patch comes from git-height. `main` is the public-release ref (stable versions); everything else gets prerelease tags. GitVersion is still installed as a transitional helper for `MajorMinorPatchVersion` in `Build.cs`; full removal is a follow-up.

## Semver policy

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) per [CHANGELOG.md](../../CHANGELOG.md). **Any breaking change must bump the major in `version.json` in the same PR before it can merge to `main`.** A "breaking change" is any of:

- A conventional-commit subject with the `!` suffix (e.g. `feat(globaltool)!: …`, `fix(security)!: …`).
- A `BREAKING CHANGE:` footer in the commit body.
- A change a reviewer reasonably flags as breaking even without the marker (renamed/removed public API, package ID change, on-disk format change, CI/CD shape change consumers depend on).

Patch increments from git-height are reserved for non-breaking fixes; carrying breaking changes under a patch series ships them to consumers silently.

**Reviewer responsibility:** if a PR carries `!` (or a flagged breaking change) and `version.json`'s major is unchanged, block the merge until the bump is in the same PR. Record the breaking change under `[Unreleased] — <next-major>` in `CHANGELOG.md` as part of the PR.

## Milestones and version targeting

Milestones are **theme-based** (e.g. "Plugin Architecture Foundation & Rebrand Completion", "Public Plugin SDK", "Continuous Delivery Vision") and carry across releases; version targeting uses **`target/vN`** labels (`target/v11`, `target/v12`, `target/v13`). A breaking change forces a new major via the semver policy above — so its PR carries `target/v<next-major>` instead of `target/v<current-major>`.

## PR-creation flow

At PR-creation time — not after, not as a follow-up — every PR gets:

1. **A `target/vN` label** matching where it will release. Default to `target/v<current-major>` (read `version.json`'s `version` field). If the PR bumps the major (i.e. it carries a breaking change), use `target/v<new-major>` instead. Pass via `--label target/v11` to `gh pr create`.

If the PR includes a **breaking change** (any commit uses `!`, has a `BREAKING CHANGE:` footer, or otherwise meets the breaking-change definition above), additionally:

2. **Add the `breaking-change` label.** `gh pr create --label target/v<new-major> --label breaking-change …`.
3. **Open the PR body with a `⚠️ Breaking change` callout** that names the affected surface (public API, package ID, CLI flag, on-disk format, CI/CD shape, etc.) and the consumer-side impact in one sentence. This is what reviewers and downstream consumers read first.
4. **Confirm `version.json`'s major is already bumped** for the target release. If it isn't, stop — bump it in the same branch before opening the PR. Don't open a breaking-change PR against an unchanged-major `version.json`.
5. **Add a `CHANGELOG.md` entry** under the existing `[Unreleased] — <next-major>` heading, in the same PR, describing the breaking change and the migration path (one paragraph minimum).

If you only discover the breaking nature mid-review, apply all relevant steps before requesting re-review.

## Release pipeline

`.github/workflows/release.yml` — triggered on push to `main`, runs the three-step shape (`actions/setup-dotnet` → `dotnet tool restore` → `dotnet fallout Test Pack Publish`). **Publishes to nuget.org** (`https://api.nuget.org/v3/index.json`) under the `Fallout.*` package ID prefix, using the `NUGET_API_KEY` repo secret (a nuget.org API key scoped to push `Fallout.*`). Prefix reservation tracked in [#33](https://github.com/ChrisonSimtian/Fallout/issues/33).

## Adding a new `Fallout.X` package — first-publish gotcha

nuget.org's `Fallout.*` prefix reservation is per-ID, not per-prefix-wildcard: CI's first `nuget push` for any never-published `Fallout.X` package ID returns `403 (does not have permission to access the specified package)` until someone manually web-uploads one nupkg to register the ID. **Two traps when doing that upload:**

1. **Set the package owner to the org, not your personal account.** The nuget.org upload UI doesn't prompt you; ownership defaults to the uploading user's profile. If you forget, the package ID is reserved but the org's `NUGET_API_KEY` still 403s on subsequent pushes (the key is scoped to org-owned packages). Fix via `Manage Package → Owners → Add owner → <org>` then optionally remove your personal account. Or upload using credentials of the org's service account directly. See [#208](https://github.com/ChrisonSimtian/Fallout/issues/208) for what this looks like when it goes wrong.
2. **Validation can lag** the upload by 5–30 minutes. The package page may say "approved" while the API key permission hasn't propagated yet. Wait, then rerun the release pipeline (`gh run rerun <id> --failed`); `--skip-duplicate` makes the retry safe for already-published packages.
