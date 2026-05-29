# ADR-0004: Calendar versioning + dual-pace channels (edge / stable) + experimental APIs

## Status

Accepted (2026-05-29). **Supersedes the versioning section of [ADR-0001](0001-release-branch-model.md) and extends its channel model**; the release-branch + tag-triggered multi-channel CD machinery from ADR-0001 and the nuget.org-opt-in policy from [ADR-0002](0002-v11-off-nuget-by-default.md) remain in force. Discussion thread: [#302](https://github.com/ChrisonSimtian/Fallout/discussions/302).

## Context

Fallout's contributor velocity is **bimodal**. A subset of maintainers work AI-assisted and ship a large volume of change per week. Other contributors hand-write code and move deliberately. Left unstructured, the two paces collide: either the fast lane destabilises the surface everyone else depends on, or the slow lane becomes a gate the fast lane has to wait behind. Neither is acceptable, and the project would rather not force one tempo on everyone.

The maintainer's framing (paraphrased): *yearly breaking majors are fine — a year just ships more; the AI crowd should be able to push hard on the next version; and we want a deliberately-unstable channel to release experimental work from, feeding the stabilised parts back into the regular channels. The slower crowd stabilises the released version and takes their time on reviews.*

What we already have (and keep):

- **[ADR-0001](0001-release-branch-model.md)** — `main` integration trunk (merges don't publish), `release/vN` channels, **tag-triggered** multi-channel CD via three GitHub Environments (`nuget-org`, `github-packages`, `github-releases`), cherry-pick-to-release hotfix flow, per-branch Nerdbank.GitVersioning.
- **[ADR-0002](0002-v11-off-nuget-by-default.md)** — nuget.org publish is opt-in (`workflow_dispatch` flag + env approval); GitHub Packages is the faster default channel; the noise that drove this was specifically **nuget.org Dependabot fan-out** into consumer repos.

What's missing for the dual-pace model:

1. A **published fast channel** for the AI crowd to release intentionally-unstable work from.
2. A **versioning scheme** that matches a yearly-breaking cadence and reconciles with a contributor (Dennis) who advocated gitflow + strict semver.
3. A **per-feature opt-in** for unstable public APIs that can ride *any* channel — so "experimental" need not mean a divergent fork.
4. **Explicit review tiers** so the fast lane isn't gated and the slow lane isn't steamrolled.

Crucial timing fact: **v11 never cleanly shipped.** The `11.0.x` packages were contaminated auto-publishes, since unlisted from nuget.org (the ADR-0002 era + the #268/#294/#298 unlist batches). There is no stable v11 consumer base to renumber, so adopting calendar versioning *now* is near-zero-cost.

## Decision

### 1. Calendar versioning: `YYYY.MINOR.PATCH`

Adopt CalVer immediately, retiring the v11 numbering. `main` becomes `2026.x`.

- The version is **mechanically valid SemVer 2.0** — all three components are numeric, so Nerdbank.GitVersioning, NuGet, and ordering all keep working unchanged. It *is* semver; the major simply happens to be the year. This is the reconciliation with the semver camp.
- **`MAJOR` = the calendar year.** **`MINOR`** = a feature drop within the year. **`PATCH`** = fixes (git-height, as today).
- **Breaking changes are allowed only at the yearly major cut.** Mid-year stable releases are strictly non-breaking: minor adds features, patch fixes bugs. Breaking work accumulates on edge/`main` through the year and ships together as next year's `YYYY+1.0.0`. This is what gives the slow crowd a **stable API target for a whole year**, and it keeps the semver guarantee honest — a major bump always coincides with real breakage.

This replaces the old "any breaking change bumps the major in the same PR, any time" rule from ADR-0001 / `release-and-versioning.md`.

### 2. `main` *is* the edge channel

`main` stops being publish-silent and becomes the **edge** channel.

- Every push to `main` produces an `-edge` prerelease whose identifier lives in the prerelease segment (not the version core). The **core targets the *next* planned version**, so edge prereleases sort *above* the current stable line and consumers actually resolve them.
  > **Implementation note (2026-05-30):** as built, `main`'s `version.json` is `2026.1.0-edge.{height}` and `main` is a non-public NB.GV ref, so the actual edge version is **`2026.1.0-edge.<height>.g<commit>`** (e.g. `2026.1.0-edge.42.gfbb83ef`) — NB.GV-native height + commit, not a literal `<YYYYMMDD>` date. This satisfies the same goal (sortable, build-identifying, in the prerelease segment); a literal date stamp was not implemented because NB.GV does not produce one natively and the commit identifier is more precise. The original date-stamped examples in this ADR are illustrative of the *intent*, not the shipped string.
- Edge publishes to **GitHub Packages only — never nuget.org.** This is consistent with *why* `main` was made non-publishing in ADR-0001: the pain was nuget.org Dependabot fan-out into every consumer repo. GitHub Packages is opt-in (consumers add the feed), so edge causes none of that fan-out.
- Edge is **intentionally unstable.** This is the AI crowd's lane.

> The build identifier belongs in the **prerelease segment**, not the version core. A core of `2026.05.29` would parse as year 2026 / minor 5 / patch 29 — a *stable* release under this scheme, not a nightly. The prerelease-segment form is correct (see the implementation note above for the as-built `…-edge.<height>.g<commit>` string).

### 3. `release/YYYY` = the stable train

- Cut from `main`. The **slow crowd owns it**: hardening, `-rc.N` previews, then GA. After the cut it receives **non-breaking minors + patches only** — never a breaking change (those wait for next year's cut).
- Stable tags publish to nuget.org (still **opt-in** via the ADR-0002 flag + env approval) plus GitHub Packages + GitHub Releases.
- `release/2026` is cut now, even though `main` is still churning, so the slow crowd has something to own from day one.

### 4. Legacy lines coexist, unchanged

- **`release/v10`** (+ `hotfix/v10.1`, `hotfix/v10.2`) stay on **semver `10.x`** as a **legacy maintenance line — security and critical fixes only, no new features.** It is not renumbered into CalVer. This is a hard requirement: existing v10 consumers keep their line.
- **`release/v11`** is **retired** — nothing clean shipped under it, and the in-flight rebrand/plugin work it carried re-homes onto the `2026` line. The branch is kept for archaeology and marked EoL (per ADR-0001's "branches are cheap, don't delete" rule).
- `publicReleaseRefSpec` therefore covers **both** patterns: CalVer (`^refs/heads/release/\d{4}$`) and legacy semver (`^refs/heads/release/v\d+$`).

### 5. `[Experimental]` for opt-in unstable APIs

Use `System.Diagnostics.CodeAnalysis.ExperimentalAttribute` with a `FALLOUT0xx` diagnostic-ID scheme to mark public APIs that may change without notice — including APIs shipped in a **stable** release.

- The C# compiler forces consumers to explicitly suppress the diagnostic to use an experimental API, so opting in is a conscious choice. Fallout is a *framework* (a product devs build on), not an app — letting devs decide per-API whether to take the risk is exactly right.
- **Promoting an experimental feature to stable = deleting the attribute.** Because the feature already rode the trunk, there is no cross-branch cherry-pick — this is what lets us "feed stabilised work back into the regular channels" without a divergent fork.
- **Discipline differs by channel:** on edge/`main`, experimental churn is expected and the attribute is a courtesy. On the **stable train**, any risky-but-shipped surface **must** wear `[Experimental]` — that is the contract that lets the stable line stay trustworthy while still carrying new work.

### 6. Two-tier review

- **Edge / `main`:** light, fast review. Breakage is acceptable and cheap because no production consumer tracks edge.
- **Promotion to `release/YYYY` + the GA cut:** rigorous, unhurried, human review — **the slow crowd's domain and the project's quality gate.** There is no clock on it, because edge already served the impatient.

The net property: **the fast lane never blocks on slow review, and the slow lane is never steamrolled** — the thing they guard (stable) is theirs to pace. This is as much the social fix as the technical one.

### Channel summary

| Channel | Built from | Cadence | Version shape | Publishes to | Review tier |
|---|---|---|---|---|---|
| **edge** | `main` | per-commit | `2026.1.0-edge.<height>.g<commit>` (see §1 implementation note) | GitHub Packages | light/fast |
| **preview / rc** | `release/YYYY` pre-GA | per cut | `2026.0.0-rc.2` | GitHub Packages | rigorous |
| **stable** | `release/YYYY` tags | yearly major + non-breaking minor/patch | `2026.1.3` | nuget.org (opt-in) + GH Packages + GH Releases | rigorous |
| **legacy** | `release/v10` (+ `hotfix/v10.x`) | security/critical only | `10.x` (semver) | nuget.org (opt-in) + GH Packages | rigorous |
| **`[Experimental]` APIs** | any channel | per-feature | rides the package | (the package) | opt-in by consumer |

## Consequences

### Positive

- **Two paces without two divergent trunks.** `main` is the single trunk; the stable train is a short-lived-per-year branch off it. No long-lived `develop`/`experimental` branch, so no cherry-pick-back merge-hell. The dual pace lives in *channels + the `[Experimental]` attribute*, not in divergent history.
- **Social fix is structural.** Velocity mismatch stops being a source of friction: fast contributors ship to edge unblocked; deliberate contributors own stabilisation and review at their own tempo.
- **CalVer reconciles both camps.** It is real semver mechanically (Dennis keeps ordering + discipline), and breaking-batched-to-year-boundary keeps the major-signals-breaking guarantee — while delivering the maintainer's yearly-breaking cadence.
- **Near-zero adoption cost, taken now.** No clean v11 shipped, so renumbering to `2026` strands no one.
- **Legacy stays supportable.** v10 consumers keep their line; the existing release-branch CD handles it with no new machinery.

### Negative

- **Mid-year, a year bump with no breaking changes would "waste" a semver major signal.** Mitigated by the rule that breaking changes *are* batched to the year boundary, so a major bump always means real breakage. An emergency mid-year break is the rare exception and would be documented loudly.
- **GitHub Packages becomes load-bearing for edge** (on top of being load-bearing for the current default channel per ADR-0002). A GH Packages outage blocks edge publishing. Idempotent `--skip-duplicate` retries keep this cheap, but the dependency is real.
- **`publicReleaseRefSpec` now matches two patterns** (CalVer + legacy semver) and still needs per-branch tuning on each new `release/YYYY` cut. Documented in the runbook.
- **Contributors must learn `[Experimental]`.** A new convention to internalise; analyzer enforcement + docs mitigate.
- **Docs churn.** `branching-and-release.md`, `release-and-versioning.md`, and `AGENTS.md` all change in the wake of this ADR.

### Neutral

- **ADR-0001's branch/channel/CD model and ADR-0002's nuget.org-opt-in policy are unchanged.** This ADR changes the *versioning scheme*, adds the *edge channel* on top of `main`, and adds the *`[Experimental]` + review-tier* conventions. The three-environment fan-out, tag-triggered trigger, and cherry-pick hotfix flow all carry over.
- **`target/vN` labels become `target/YYYY`** (`target/2026`, …). Legacy work keeps `target/v10`. Milestones remain theme-based.

## Alternatives considered

### A. A separate long-lived `experimental` / `edge` branch

The literal reading of "experimental channel" — a standing branch where the AI crowd works, cherry-picking stabilised features back to `main`.

**Rejected because** a long-lived branch that hosts most development diverges from `main` continuously, and cherry-picking features back is the classic merge-hell that trunk-based development exists to avoid. Making `main` *itself* the edge channel — plus the `[Experimental]` attribute for opt-in risk — delivers the same "fast, unstable, feeds back to stable" property with **one** lineage.

### B. Keep semver `vN` majors

Stay on `11.x`, `12.x`, bumping major on any breaking change at any time.

**Rejected because** it doesn't deliver the yearly cadence the maintainer wants, and "bump major whenever" makes the major number arbitrary rather than a calendar landmark the whole community can plan around. CalVer gives a predictable annual breaking window.

### C. Gitflow with a permanent `develop`

Dennis's suggestion: `develop` for integration, `main` for stable, `release/*` + `hotfix/*`.

**Rejected** for the same divergence reason as (A): `develop` is a permanent second mainline. The train model takes gitflow's genuine benefit — insulating stable work from in-flight churn — and implements it with **short-lived `release/YYYY` branches cut from trunk** instead of a standing `develop`. Dennis keeps semver mechanics and the stable/dev separation; we keep trunk-based velocity.

### D. Date as the version core (`YYYY.MM.DD`)

Make daily builds literally `2026.05.29`.

**Rejected because** it collides with the CalVer core semantics — `2026.05.29` reads as a *stable* `MAJOR.MINOR.PATCH`, not a nightly — and there's no room left for minor/patch within a year. The build identifier lives in the **prerelease segment** instead (the `…-edge.…` form — see the §1 implementation note for the as-built string).

## References

- [ADR-0001: Release-branch model & multi-channel CD](0001-release-branch-model.md) — parent; versioning section superseded here, channel model extended.
- [ADR-0002: v11 off nuget.org by default](0002-v11-off-nuget-by-default.md) — nuget.org-opt-in policy, retained.
- [docs/branching-and-release.md](../branching-and-release.md) — maintainer runbook (updated for this model).
- [docs/agents/release-and-versioning.md](../agents/release-and-versioning.md) — agent-facing branching/versioning/PR-flow reference (updated for this model).
- Discussion thread: [#302 — Calendar versioning + dual-pace channels (feedback)](https://github.com/ChrisonSimtian/Fallout/discussions/302).
