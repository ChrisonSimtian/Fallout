---
title: Analyzing NuGet Packages
---

Over time, projects collect `PackageReference` entries that they no longer need. The global tool can find them for you. It reads the dependency graph that NuGet resolved, and reports three kinds of problems:

- **Redundant via project reference.** A package is referenced directly, but a project you reference already brings it in.
- **Redundant via package.** A package is referenced directly, but another package you reference already brings it in.
- **Version conflict.** The same package resolves to different versions in different projects.

## Usage

The command reads the `obj/project.assets.json` file of each project. Restore your projects first, for example with `dotnet restore`. Projects that are not restored are skipped, and the summary says how many.

```powershell
# terminal-command
fallout :analyze packages [<path>] [--tfm <moniker>] [--severity none|trace|normal|warning|error] [--format table|flat] [--conflicts] [--verbose] [--exclude <id>[,<id>...]]
```

| Argument | Meaning |
| --- | --- |
| `<path>` | A `.csproj` file, a directory, or a `.sln`/`.slnx` file. The default is the working directory. |
| `--tfm <moniker>` | Analyze only this target framework, for example `net10.0`. |
| `--format table\|flat` | `table` (default) prints a tree per project. `flat` prints one line per finding. |
| `--conflicts` | Also list version conflicts. They are only counted in the summary by default. |
| `--verbose`, `-v` | In the conflicts table, name the projects that use each version. |
| `--exclude <id>[,<id>...]` | Never report these packages. You can repeat the option. |
| `--severity <level>` | In the `flat` format, the log level of each finding. With `error`, the command also exits with code 1 when it reports a finding. The default is `warning`. |

## Reading the result

Each redundant reference is marked `remove` or `review`:

- **`remove`**: the package resolves to the same version without your direct reference.
- **`review`**: without your direct reference, the package would resolve to a lower version. Keep the reference if you need the higher version.

:::warning
The tool reads the dependency graph. It does not read your source code. A package can be redundant in the graph and still be one that your code uses directly. Referencing the packages you use is good practice, so check before you remove anything. This matters most in a library, where each direct reference is also a dependency of your package.
:::

The tool does not count these packages as the source of a redundancy, because they are not dependencies of what you build:

- packages with `PrivateAssets="all"`, such as build-time tools
- packages the SDK references for you (`autoReferenced`)

Version conflicts are found for each target framework separately. A project that uses different versions on `net8.0` and `net10.0` is not a conflict. Some differences are on purpose, for example a project that must stay on an older compiler package. Use `--exclude` for those packages.

## Use in CI

Use `--severity error` to fail a build when the analysis finds something:

```powershell
# terminal-command
fallout :analyze packages --severity error
```

The exit code follows what the output shows. Version conflicts only count when you also pass `--conflicts`.
