# Branching and release flow

Maintainer reference for how Fallout branches, ships releases, hotfixes older majors, and uses GitHub Environments to gate publishes. Driven by [milestone #13](https://github.com/ChrisonSimtian/Fallout/milestone/13) / [RFC #267](https://github.com/ChrisonSimtian/Fallout/issues/267).

> **Audience.** Repository maintainers cutting releases or hotfixing older majors. Contributors filing PRs against `main` don't need to read this — see [CONTRIBUTING.md](../CONTRIBUTING.md) instead. AI coding tools should read both this file and [docs/agents/release-and-versioning.md](agents/release-and-versioning.md).

## Branches at a glance

| Branch | Purpose | Lifetime | Protected | Source of releases? |
|---|---|---|---|---|
| `main` | Integration trunk. Every PR lands here. | Long-lived | Yes | No (since milestone #13) |
| `release/vN` | Release channel for major version N (e.g. `release/v11`) | Long-lived per major | Yes | **Yes** — tags pushed here fire the release pipeline |
| `release/vN.M` | Optional minor channel for divergent patch lines | Cut on demand | Yes | Yes — same shape as `release/vN` |
| `feature/<slug>`, `bugfix/<slug>`, `chore/<slug>`, `docs/<slug>`, `pr/<num>-<slug>` | Working branches | Short-lived; PR-and-merge then deleted | No | No |

`develop`, `master`, `hotfix/*` are not used. Hotfixes flow through `main` first, then cherry-pick to `release/vN` — see the [hotfix flow](#hotfixing-an-older-major) below.

## Channel taxonomy

Releases fire to three independent channels, each with its own GitHub Environment:

| Tier | Channel | Cadence | Gating | Versioning |
|---|---|---|---|---|
| **1** | `nuget-org` env → nuget.org | Slow, deliberate | **Approval-gated** (maintainer reviewer) | Clean semver (`11.0.0`, `11.0.1`, ...) |
| **2** | `github-packages` env → GitHub Packages | Faster — betas, previews | None | Prereleases or NB.GV builds |
| **3** | (planned) Docker local NuGet server | Per-PR / per-commit | None (ephemeral) | PR-derived |
| (bundled) | `github-releases` env → GitHub Release with nupkgs attached | Same tag as the package publish | None | Same as the tag |

Tier 3 setup is tracked in [#279](https://github.com/ChrisonSimtian/Fallout/issues/279). See [`reference_release_channels` in agent memory](https://github.com/ChrisonSimtian/Fallout/issues/267#issuecomment-4570408325) for the design discussion.

## Cutting a release

The release pipeline fires on **tag push** to a `release/vN` branch. The tag must match `v*` and must point at a commit reachable from `release/v*` (the `validate-ref` job enforces this).

### Step-by-step

```bash
# 1. Make sure your local release/vN is up to date
git fetch
git switch release/v11
git pull --ff-only

# 2. (Optional) Verify what version NB.GV will compute
dotnet nbgv get-version   # should report 11.0.X clean, no -g<sha>

# 3. Create the tag + GitHub Release in one step
gh release create v11.0.X \
    --target release/v11 \
    --title "v11.0.X" \
    --generate-notes
```

`gh release create` publishes a tag + a GitHub Release in one call. That tag push triggers `.github/workflows/release.yml`, which:

1. **`validate-ref`** confirms the tag points at a commit reachable from `release/v*`.
2. **`test-and-pack`** runs `dotnet fallout Test Pack`, uploads `output/packages/*.nupkg` as an artifact.
3. **Three parallel publish jobs** consume the artifact and fan out per channel:
   - `publish-nuget-org` → pushes `Fallout.*.nupkg` to nuget.org (**pauses for your approval**)
   - `publish-github-packages` → pushes `Nuke.*.nupkg` transition shims to GitHub Packages
   - `publish-github-releases` → attaches all `*.nupkg` to the GitHub Release page

### Approving the `nuget-org` publish

When `publish-nuget-org` starts, it pauses for an environment-protection approval. You'll get a GitHub notification (and an entry on the run page). Click "Review deployments" → check `nuget-org` → "Approve and deploy". This is the production-grade gate — review what's about to publish before approving.

### If a publish fails partway through

Each `dotnet nuget push` uses `--skip-duplicate`, so re-running a publish job is idempotent on packages that already made it through. If nuget.org transient-fails on one of fifteen packages:

```bash
# Re-run the workflow against the same tag
gh workflow run release.yml -f tag=v11.0.X
```

The `workflow_dispatch` fallback re-checks out the tag and re-runs all three publish jobs. `--skip-duplicate` skips anything that already published.

## Hotfixing an older major

Workflow: fix lands on `main` first via a normal PR, then cherry-pick to `release/vN`, then tag.

```bash
# 1. Fix lands on main via standard PR flow
gh pr create --base main ...
# (review, merge to main)

# 2. Cherry-pick to release/vN
git fetch
git switch release/v11
git pull --ff-only
git cherry-pick <fix-sha-on-main>

# 3. Open a PR against release/v11 with the cherry-picked commit
# (yes, even a one-commit cherry-pick goes through a PR — branch protection
# blocks direct pushes and requires the ubuntu-latest status check)
git push origin HEAD:cherry-pick-XXXX-to-v11
gh pr create --base release/v11 ...

# 4. Once that PR merges, tag a new patch
gh release create v11.0.X+1 --target release/v11 --generate-notes
```

Cherry-pick-first guarantees forward compatibility: any fix in `release/v11` is also in `main` (and therefore in `release/v12` once cut).

### When direct-PR-to-release-branch is OK

The standard flow above is the default. In rare cases — security-incident fixes where main has diverged into incompatible v12 territory, or prod-down emergencies — a PR can target the release branch directly. Apply the `hotfix-direct` label and get explicit maintainer sign-off in the PR description. Not the default; not common.

## Cutting a new `release/vN`

When a new major version is about to ship from `main`:

```bash
# 1. Make sure main is at the commit you want to start the new major from
git fetch
git switch main
git pull --ff-only

# 2. Create the branch
git switch -c release/v12 main
git push -u origin release/v12

# 3. Apply branch protection (see docs/agents/release-and-versioning.md → Branch protection on release/vN
#    for the canonical settings)
gh api -X PUT repos/ChrisonSimtian/Fallout/branches/release/v12/protection \
    --input scripts/release-branch-protection.json   # mirror main's profile

# 4. Add the new branch to publicReleaseRefSpec on release/v12 (the branch itself),
#    so NB.GV produces clean versions instead of git-sha-suffixed ones:
#    version.json publicReleaseRefSpec += "^refs/heads/release/v12$"
#    Commit via PR targeting release/v12.

# 5. (Optional, when v12 is fully on-trunk) bump main's version.json major to 13.0
```

### Step 4 — why on `release/v12`, not `main`

`publicReleaseRefSpec` is per-branch. Putting `^refs/heads/release/v12$` in `main`'s `publicReleaseRefSpec` while `main` still has `"version": "12.0"` would create a patch-height collision (both branches public-mapped to the same major+minor). Editing on `release/v12` only avoids that, and the edit naturally stays on the release branch where it belongs.

When `main` later bumps to `13.0`, this concern goes away — `release/v12` keeps 12.0, `main` is 13.0, no overlap.

## Deprecating an old `release/vN`

Once a major hits end-of-life:

1. Final patch release (`v<N>.<last>.<last>`).
2. Announce EoL in the README + CHANGELOG.
3. Leave the branch in place — don't delete it. Future archaeology + historical hotfix-on-demand should remain possible.
4. Optionally apply a more restrictive protection profile (e.g. require admin approval on every merge) to make accidental tags less likely.

Branches are cheap. Deletion is destructive. Default to keeping.

## Tag protection

A repository ruleset blocks creation/deletion/update of tags matching `v*` for non-admins ([ruleset 17017817](https://github.com/ChrisonSimtian/Fallout/rules/17017817)). Bypass actors: repo admins (`RepositoryRole 5`). Combined with the `nuget-org` env approval gate, that's two layers of "who can fire a production release."

## See also

- [docs/agents/release-and-versioning.md](agents/release-and-versioning.md) — PR-creation flow, semver policy, release pipeline reference, branch protection settings.
- [docs/adr/0001-release-branch-model.md](adr/0001-release-branch-model.md) — Architecture Decision Record for the model documented here.
- [milestone #13](https://github.com/ChrisonSimtian/Fallout/milestone/13) — full work-breakdown of how this shape was implemented.
- [RFC #267](https://github.com/ChrisonSimtian/Fallout/issues/267) — original design discussion.
- [CONTRIBUTING.md](../CONTRIBUTING.md) — contributor-facing flow.
