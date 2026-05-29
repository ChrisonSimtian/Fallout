# Branching and release flow

Maintainer reference for how Fallout branches, ships releases, hotfixes older lines, and uses GitHub Environments to gate publishes. Model defined by [ADR-0004](adr/0004-calendar-versioning-and-dual-pace-channels.md) (calendar versioning + dual-pace channels), amending [ADR-0001](adr/0001-release-branch-model.md) / [milestone #13](https://github.com/ChrisonSimtian/Fallout/milestone/13) / [RFC #267](https://github.com/ChrisonSimtian/Fallout/issues/267).

> **Audience.** Repository maintainers cutting releases or hotfixing older lines. Contributors filing PRs against `main` don't need to read this — see [CONTRIBUTING.md](../CONTRIBUTING.md) instead. AI coding tools should read both this file and [docs/agents/release-and-versioning.md](agents/release-and-versioning.md).

## Branches at a glance

| Branch | Purpose | Lifetime | Protected | Source of releases? |
|---|---|---|---|---|
| `main` | Integration trunk **+ `edge` channel**. Every PR lands here; pushes publish `…-edge` prereleases to GitHub Packages. | Long-lived | Yes | **Edge only** (GitHub Packages, no nuget.org / no GH Release) |
| `release/YYYY` | Stable train for the calendar year (e.g. `release/2026`). Non-breaking minors/patches only after the cut. | Long-lived per year | Yes | **Yes** — tags pushed here fire the full release pipeline |
| `release/v10` (+ `hotfix/v10.1`, `hotfix/v10.2`) | **Legacy** semver `10.x` maintenance line — security/critical fixes only. | Long-lived | Yes | Yes — tags fire the pipeline (nuget.org opt-in) |
| `release/v11` | **Retired** — nothing clean shipped; work re-homed onto `2026`. Kept for archaeology, marked EoL. | Frozen | Yes | No |
| `feature/<slug>`, `bugfix/<slug>`, `chore/<slug>`, `docs/<slug>`, `pr/<num>-<slug>` | Working branches | Short-lived; PR-and-merge then deleted | No | No |

`develop` and `master` are not used. Breaking changes land on `main` only and are batched to the yearly major cut. Fixes that apply to both `main` and a stable train land on `main` first, then cherry-pick to `release/YYYY` — see the [hotfix flow](#hotfixing-an-older-line) below.

## Channel taxonomy

Releases fire to multiple channels, each with its own GitHub Environment:

| Channel | Built from | Cadence | Gating | Version shape |
|---|---|---|---|---|
| **edge** → `github-packages` env | `main` | Per-commit | None | `2026.1.0-edge.<height>.g<commit>` |
| **stable** → `nuget-org` env | `release/YYYY` tags | Slow, deliberate | **Flag opt-in + approval-gated** | `2026.1.3` (CalVer) |
| **stable/legacy** → `github-packages` env | `release/YYYY`, `release/v10` tags | Every tag | None | CalVer / `10.x` |
| **legacy** → `nuget-org` env | `release/v10` tags | Security/critical only | **Flag opt-in + approval-gated** | `10.x` (semver) |
| `github-releases` env (bundled) | `release/*` tags | Same tag as the package publish | None | Same as the tag |
| Docker local NuGet server | Per-PR / per-commit | None (local) | PR-derived | Available via `tests/integration/docker-compose.yml` |

**Defaults:** edge publishes from `main` to GitHub Packages only (no nuget.org, no GH Release). Stable/legacy tag pushes publish to GitHub Packages + GitHub Releases. nuget.org is **always opt-in** via the `workflow_dispatch` `publish-to-nugetorg` flag — used when a `release/YYYY` is stabilised enough for the broader consumer audience, or for a `release/v10` security patch. See [`project_release_channels` in agent memory](https://github.com/ChrisonSimtian/Fallout/issues/267#issuecomment-4570408325) and [ADR-0004](adr/0004-calendar-versioning-and-dual-pace-channels.md).

## Cutting a release

### Routine stable release (GitHub Packages only)

The default path. Pushing a `v2026.1.X` tag to `release/2026` publishes to GitHub Packages + GitHub Releases. nuget.org is **not** touched. (Git tags keep the `v` prefix — `v2026.1.3` — so the `v*` tag-protection ruleset and `validate-ref` apply; the package version core is `2026.1.3`.)

```bash
# 1. Make sure your local release/YYYY is up to date
git fetch
git switch release/2026
git pull --ff-only

# 2. (Optional) Verify what version NB.GV will compute
dotnet nbgv get-version   # should report 2026.1.X clean, no -g<sha>

# 3. Create the tag + GitHub Release in one step
gh release create v2026.1.X \
    --target release/2026 \
    --title "v2026.1.X" \
    --generate-notes
```

That tag push triggers `.github/workflows/release.yml`:

1. **`validate-ref`** confirms the tag points at a commit reachable from `release/v*`.
2. **`test-and-pack`** runs `dotnet fallout Test Pack`, uploads `output/packages/*.nupkg` as an artifact.
3. Three parallel publish jobs consume the artifact:
   - `publish-nuget-org` — **skipped** (not opt-in by default)
   - `publish-github-packages` — pushes **all** `*.nupkg` (Fallout.* + Nuke.*) to GitHub Packages
   - `publish-github-releases` — attaches all `*.nupkg` to the GitHub Release page

### Stabilised release (nuget.org publish)

When a `release/2026` release is stabilised enough for nuget.org, or for cutting a `release/v10` legacy security patch, use `workflow_dispatch` with the opt-in flag:

```bash
# Option A: via gh CLI
gh workflow run release.yml \
    -f tag=v2026.1.X \
    -f publish-to-nugetorg=true

# Option B: via Actions UI → release → "Run workflow" → set publish-to-nugetorg to true
```

The workflow:

1. Skips `validate-ref` (workflow_dispatch doesn't auto-validate the ref; you took the action consciously).
2. Re-runs `test-and-pack` against the named tag.
3. **`publish-nuget-org` fires** — pauses for approval at the `nuget-org` env gate (notification + entry on the run page; click "Review deployments" → check `nuget-org` → "Approve and deploy"). Then pushes Fallout.* to nuget.org.
4. `publish-github-packages` re-runs idempotently (`--skip-duplicate` skips what's already there).
5. `publish-github-releases` re-runs idempotently (uses `--clobber` for asset replacement if the GH Release already exists).

Two layers of safety on the nuget.org path: the flag opt-in + the env approval. You can also test the wiring without burning a release — set the flag, get the approval prompt, then cancel without approving.

### If a publish fails partway through

Each `dotnet nuget push` uses `--skip-duplicate`. Re-running a publish job is idempotent on packages already pushed. For a transient failure mid-publish:

```bash
# Routine re-run — leave publish-to-nugetorg false
gh workflow run release.yml -f tag=v2026.1.X

# Stabilised re-run — include the flag if you want to retry the nuget.org push
gh workflow run release.yml -f tag=v2026.1.X -f publish-to-nugetorg=true
```

## Hotfixing an older line

Two cases: a fix for the **current stable train** (`release/2026`), or a security/critical fix for the **legacy line** (`release/v10`).

For a fix that also applies to `main`: it lands on `main` first via a normal PR, then is cherry-picked to the target release branch, then tagged.

```bash
# 1. Fix lands on main via standard PR flow
gh pr create --base main ...
# (review, merge to main)

# 2. Cherry-pick to the target release branch (release/2026 here; release/v10 for legacy)
git fetch
git switch release/2026
git pull --ff-only
git cherry-pick <fix-sha-on-main>

# 3. Open a PR against the release branch with the cherry-picked commit
# (yes, even a one-commit cherry-pick goes through a PR — branch protection
# blocks direct pushes and requires the ubuntu-latest status check)
git push origin HEAD:cherry-pick-XXXX-to-2026
gh pr create --base release/2026 ...

# 4. Once that PR merges, tag a new patch
gh release create v2026.1.X+1 --target release/2026 --generate-notes
```

Cherry-pick-first guarantees forward compatibility: any fix in `release/2026` is also in `main`. A **legacy `release/v10` security fix** that doesn't apply to `main` (the code has moved on) lands directly on `release/v10` (or the relevant `hotfix/v10.x`) via PR — that's the expected path for a frozen line, not the exception. Such a release is the nuget.org case (use the opt-in flag).

### When direct-PR-to-release-branch is OK

For `release/2026`, the cherry-pick-first flow is the default. In rare cases — security-incident fixes where `main` has diverged, or prod-down emergencies — a PR can target the stable train directly. Apply the `hotfix-direct` label and get explicit maintainer sign-off in the PR description. (For the frozen `release/v10` legacy line, direct-PR is the *normal* path, since `main` no longer carries v10 code.)

## Cutting a new `release/YYYY`

At the yearly major cut, when the next year's line is about to ship from `main`:

```bash
# 1. Make sure main is at the commit you want to start the new year from
git fetch
git switch main
git pull --ff-only

# 2. Create the branch
git switch -c release/2027 main
git push -u origin release/2027

# 3. Apply branch protection (see docs/agents/release-and-versioning.md → Branch protection on release/YYYY
#    for the canonical settings)
gh api -X PUT repos/ChrisonSimtian/Fallout/branches/release/2027/protection \
    --input scripts/release-branch-protection.json   # mirror main's profile

# 4. On release/2027 (the branch itself), set version.json "version": "2027.0" and add
#    itself to publicReleaseRefSpec so NB.GV produces clean versions, not git-sha-suffixed:
#    publicReleaseRefSpec already matches "^refs/heads/release/\\d{4}$" — confirm it resolves.
#    Commit via PR targeting release/2027.

# 5. Bump main's version.json to the next year's edge target (e.g. "2027.1") so edge
#    builds sort above the new stable line.
```

### Step 4 — why on `release/2027`, not `main`

`publicReleaseRefSpec` is per-branch. The CalVer ref pattern (`^refs/heads/release/\d{4}$`) matches `release/2027` automatically, but the `"version"` field is per-branch: `release/2027` pins `"2027.0"` while `main` moves on to the next edge target. Editing the version pin on `release/2027` keeps the stable line's number stable and avoids a patch-height collision with `main`.

## Deprecating an old `release/YYYY` (or the legacy line)

Once a year's line (or `release/v10`) hits end-of-life:

1. Final patch release.
2. Announce EoL in the README + CHANGELOG.
3. Leave the branch in place — don't delete it. Future archaeology + historical hotfix-on-demand should remain possible (this is why `release/v11` stays around despite being retired).
4. Optionally apply a more restrictive protection profile (e.g. require admin approval on every merge) to make accidental tags less likely.

Branches are cheap. Deletion is destructive. Default to keeping.

## Tag protection

A repository ruleset blocks creation/deletion/update of tags matching `v*` for non-admins ([ruleset 17017817](https://github.com/ChrisonSimtian/Fallout/rules/17017817)). Bypass actors: repo admins (`RepositoryRole 5`). Combined with the `nuget-org` env approval gate, that's two layers of "who can fire a production release."

## See also

- [docs/agents/release-and-versioning.md](agents/release-and-versioning.md) — PR-creation flow, semver policy, release pipeline reference, branch protection settings.
- [docs/adr/0004-calendar-versioning-and-dual-pace-channels.md](adr/0004-calendar-versioning-and-dual-pace-channels.md) — the current versioning + channel decision.
- [docs/adr/0001-release-branch-model.md](adr/0001-release-branch-model.md) — the release-branch + multi-channel CD model (versioning amended by 0004).
- [milestone #13](https://github.com/ChrisonSimtian/Fallout/milestone/13) — full work-breakdown of how this shape was implemented.
- [RFC #267](https://github.com/ChrisonSimtian/Fallout/issues/267) — original design discussion.
- [CONTRIBUTING.md](../CONTRIBUTING.md) — contributor-facing flow.
