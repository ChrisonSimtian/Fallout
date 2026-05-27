<p align="center">
  <img width="320" src=".assets/fallout-logo.svg" alt="Fallout — .NET build system" />
</p>

<p align="center">
  <strong>📖 Documentation: <a href="https://docs.fallout.build/">docs.fallout.build</a></strong>
</p>

> 📦 **Fallout is the successor to NUKE.** [Migrating from NUKE →](docs/migration/from-nuke.md)

# Fallout

> Build automation for C#/.NET — the hard-fork successor to NUKE.

[![Docs](https://img.shields.io/badge/docs-docs.fallout.build-blue?logo=readthedocs&logoColor=white)](https://docs.fallout.build/)
[![CI](https://github.com/ChrisonSimtian/Fallout/actions/workflows/ubuntu-latest.yml/badge.svg)](https://github.com/ChrisonSimtian/Fallout/actions/workflows/ubuntu-latest.yml)
[![NuGet](https://img.shields.io/nuget/v/Fallout.Common?label=Fallout.Common)](https://www.nuget.org/packages/Fallout.Common)
[![NuGet downloads](https://img.shields.io/nuget/dt/Fallout.Common?label=downloads)](https://www.nuget.org/packages/Fallout.Common)
[![Latest release](https://img.shields.io/github/v/release/ChrisonSimtian/Fallout?label=release)](https://github.com/ChrisonSimtian/Fallout/releases/latest)
[![GitHub last commit](https://img.shields.io/github/last-commit/ChrisonSimtian/Fallout)](https://github.com/ChrisonSimtian/Fallout/commits/main)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dot.net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Open issues](https://img.shields.io/github/issues/ChrisonSimtian/Fallout)](https://github.com/ChrisonSimtian/Fallout/issues)
[![Open PRs](https://img.shields.io/github/issues-pr/ChrisonSimtian/Fallout)](https://github.com/ChrisonSimtian/Fallout/pulls)
[![GitHub Sponsors](https://img.shields.io/github/sponsors/ChrisonSimtian?label=sponsor&logo=githubsponsors&color=EA4AAA)](https://github.com/sponsors/ChrisonSimtian)

> [!IMPORTANT]
> **Rebrand in progress + v11/v12 roadmap published.** This repository is being renamed from **NUKE** to **Fallout** as part of a hard fork. URLs, package names, and namespaces are migrating in stages.
>
> **What's next:** v11 finishes the rebrand and lays the internal foundation for a plugin architecture; v12 ships the public `Fallout.Plugin.Sdk`. The full plan is in [**docs/roadmap.md**](docs/roadmap.md). Five RFCs are open now to shape the SDK — your input matters most before v12 firms up.
>
> Track v11 in [milestone #6](https://github.com/ChrisonSimtian/Fallout/milestone/6) and v12 in [milestone #7](https://github.com/ChrisonSimtian/Fallout/milestone/7).

## Based on NUKE

Fallout is the successor to **[NUKE](https://github.com/nuke-build/nuke)**, originally created by **Matthias Koch** ([@matkoch](https://github.com/matkoch)) and many contributors. Fallout continues NUKE's mission as a C#-first build automation framework for .NET — under new maintenance, with an enterprise-CI/CD focus.

The original NUKE code is preserved here under the MIT License with attribution. Major version 10.x was the last NUKE release; everything from this fork forward carries the Fallout identity.

### Migrating from NUKE

If you maintain a NUKE-based build, **[docs/migration/from-nuke.md](docs/migration/from-nuke.md)** walks you through it. The short version:

```sh
dotnet tool install -g Fallout.Migrate
cd path/to/your-nuke-repo
fallout-migrate
```

## Install

```sh
dotnet tool install -g Fallout.Cli
```

The CLI installs as `fallout`. Verify with `fallout --help`.

> [!NOTE]
> **Upgrading from `Fallout.GlobalTool`?** The package was renamed to `Fallout.Cli` for install ergonomics — same `fallout` command, friendlier ID. Uninstall the old one first so you don't end up with two tools claiming the same command:
>
> ```sh
> dotnet tool uninstall -g Fallout.GlobalTool
> ```

For per-repo manifest pinning (`.config/dotnet-tools.json`), project setup, and shell completion, see the [Installation guide on docs.fallout.build](https://docs.fallout.build/getting-started/installation).

## Table of Contents

- [Elevator Pitch](#elevator-pitch)
- [Build Status](#build-status)
- [Activity](#activity)
- [Sponsorship](#sponsorship)

## Elevator Pitch

Solid and scalable CI/CD pipelines are an essential pillar for being competitive and creating a great product. But why are most of us a little afraid of touching YAML files and don't even dare to look at build scripts? Much of this is because C# developers are spoiled with a great language and smart IDEs, and they don't like missing their buddy for code-completion, ease of debugging, refactorings, and code formatting.

Fallout (NUKE's successor) brings your build automation to an even level with every other .NET project. How? It's a regular console application allowing all the OOP goodness! Besides, it solves many common problems in build automation, like parameter injection, path separator abstraction, access to solution and project models, and build step sharing across repositories. Fallout can also generate CI/CD configurations (YAML, etc.) that automatically parallelize build steps on multiple agents to optimize throughput!

## Build Status

CI runs on every push to non-`main` branches and every PR targeting `main` across `ubuntu-latest` (with `windows-latest` and `macos-latest` as post-merge validation). Releases publish from `main` to **nuget.org** under the reserved `Fallout.*` prefix, via `.github/workflows/release.yml`. Docs-only pushes (`docs/**`, `.assets/**`, `**/*.md`) skip the release workflow.

| Build Server   | Status                                                                                                                                                                                                                                                  |       Platform       | Configuration                                                                                       |
|----------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:--------------------:|-----------------------------------------------------------------------------------------------------|
| GitHub Actions | [![GitHub Actions](https://img.shields.io/github/actions/workflow/status/ChrisonSimtian/Fallout/ubuntu-latest.yml?branch=main&label=build&style=flat-square&logo=github&logoColor=white)](https://github.com/ChrisonSimtian/Fallout/actions)             | Win / Ubuntu / macOS | [`.github/workflows/`](https://github.com/ChrisonSimtian/Fallout/tree/main/.github/workflows)       |

Multi-provider CI support (Azure Pipelines, GitLab, TeamCity, AppVeyor) was removed during the takeover and is being revived demand-driven — see [#8](https://github.com/ChrisonSimtian/Fallout/issues/8).

## Activity

![Repobeats analytics image](https://repobeats.axiom.co/api/embed/13bc62ac87b75cea7be4bdbbcba90af620665333.svg "Repobeats analytics image")

Generated by [Repobeats](https://repobeats.axiom.co) — 30-day rolling view of commits, issues, PRs, and contributor activity.

## Sponsorship

Fallout is volunteer-run. There's no donation channel yet, but we want to be transparent about what running the project costs — see [`costs.md`](costs.md) for the full list. If you or your organisation would like to help offset those costs, open an issue and we'll work out the details.

## Credits

- [Matthias Koch](https://github.com/matkoch) and the [NUKE contributors](https://github.com/nuke-build/nuke/graphs/contributors) — for creating and maintaining NUKE through version 10.x.

If you maintained or contributed to NUKE and want to be credited differently here, please open an issue.
