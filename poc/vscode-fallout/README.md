# Fallout Build View (PoC)

Proof-of-concept VS Code extension that renders the Fallout build's target graph as a
tree view — a structured alternative to the Mermaid HTML produced by `--plan`.

## How it works

The framework side ships an experimental graph export (`BuildGraphUtility`, diagnostic ID
`FALLOUT001`): every time a Fallout build starts from its project, `EmitBuildGraphAttribute`
writes `.fallout/temp/build-graph.json` containing every target with its description, default
flag, and all three dependency kinds (`dependsOn` = execution, `after` = order,
`triggeredBy`/`triggers` = trigger). This extension just reads that file.

The tree shows each target at the root (default target gets a rocket icon, unlisted targets
are dimmed at the bottom); expanding a target reveals its related targets labeled by relation
kind, recursively. A file watcher refreshes the view whenever the JSON is rewritten —
i.e. on every build run.

Besides the tree, **Fallout: Show Build Graph** (title-bar icon on the view) opens a webview
panel that renders the whole graph with Mermaid (bundled locally — webviews have no network),
themed for VS Code light/dark. Solid edges = execution dependencies, dashed = order, thick =
triggers; the default target gets a bold border, unlisted targets are dimmed. Clicking a node
runs that target. The panel re-renders automatically when `build-graph.json` changes, so it
supersedes the browser-based `--plan` HTML.

Commands:

- **inline ▶ on a tree item** — runs that target via `./build.ps1 <Target>` (or `build.sh`) in a `Fallout` terminal
- **right-click a tree item → Go to Definition** — jumps to the `Target <Name> =>` declaration
  in the C# source. Resolves via the C# language service first, falling back to a regex scan
  over workspace `.cs` files; the `declaredIn` field in `build-graph.json` (the declaring type,
  e.g. a component interface like `ICompile`) disambiguates same-named targets. Multiple
  surviving candidates open a quick pick.
- **Fallout: Show Build Graph** — Mermaid graph in a webview panel, click a node to run it
- **Fallout: Refresh Targets** — re-reads the JSON
- **Fallout: Show Execution Plan (legacy HTML)** — runs `--plan` for the classic browser view (overflow menu)

## Try it

```powershell
cd poc/vscode-fallout
npm install
npm run compile
```

Then open `poc/vscode-fallout` as a folder in VS Code and press **F5** — the Extension
Development Host opens the Fallout repo root with the **Fallout Targets** view in the
Explorer sidebar. If the view is empty, run the build once (`./build.ps1 --help` is enough)
to generate `.fallout/temp/build-graph.json`.

## Known PoC limitations

- Reads the first workspace folder that has a graph file; no multi-root build support.
- The JSON only exists after one build invocation (same lifecycle as `build.schema.json`).
- No live build progress — the view is static structure. Streaming target status during a
  running build (queued/running/succeeded/failed) is the obvious next step and would need a
  small execution-event emitter on the framework side.
- The webview loads Mermaid from `node_modules` — fine under F5; packaging with `vsce` would
  need the file whitelisted (or a bundler).
- Webview node-click parsing relies on Mermaid's `<prefix>-<TargetName>-<n>` DOM-id scheme.

## Tracked ideas

- Native task provider (`vscode.tasks.registerTaskProvider`) so targets appear in
  *Tasks: Run Task* with problem matchers.
- Go-to-definition could be made exact (no regex heuristics) if the framework emitted source
  locations into `build-graph.json` — e.g. captured via `[CallerFilePath]`/`[CallerLineNumber]`
  on the `Target` delegate plumbing.
