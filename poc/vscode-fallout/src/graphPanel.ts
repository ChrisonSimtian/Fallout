import * as vscode from 'vscode';
import { BuildGraph, toMermaid } from './model';

/** Singleton webview panel rendering the build graph with Mermaid. */
export class GraphPanel {
    private static current: GraphPanel | undefined;

    static createOrShow(extensionUri: vscode.Uri, graph: BuildGraph, onRunTarget: (name: string) => void): void {
        if (GraphPanel.current) {
            GraphPanel.current.panel.reveal();
            GraphPanel.current.update(graph);
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            'falloutGraph',
            'Fallout Build Graph',
            vscode.ViewColumn.Active,
            {
                enableScripts: true,
                retainContextWhenHidden: true,
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'node_modules', 'mermaid', 'dist')],
            },
        );
        GraphPanel.current = new GraphPanel(panel, extensionUri, graph, onRunTarget);
    }

    /** Pushes a new graph into the panel if it is open (no-op otherwise). */
    static refresh(graph: BuildGraph): void {
        GraphPanel.current?.update(graph);
    }

    private constructor(
        private readonly panel: vscode.WebviewPanel,
        extensionUri: vscode.Uri,
        private graph: BuildGraph,
        onRunTarget: (name: string) => void,
    ) {
        panel.webview.html = this.getHtml(extensionUri);

        panel.webview.onDidReceiveMessage((message: { type: string; target?: string }) => {
            if (message.type === 'ready') {
                this.update(this.graph);
            } else if (message.type === 'run' && message.target) {
                onRunTarget(message.target);
            }
        });

        panel.onDidDispose(() => {
            GraphPanel.current = undefined;
        });
    }

    private update(graph: BuildGraph): void {
        this.graph = graph;
        void this.panel.webview.postMessage({ type: 'graph', definition: toMermaid(graph) });
    }

    private getHtml(extensionUri: vscode.Uri): string {
        const webview = this.panel.webview;
        const mermaidUri = webview.asWebviewUri(
            vscode.Uri.joinPath(extensionUri, 'node_modules', 'mermaid', 'dist', 'mermaid.min.js'),
        );
        const nonce = getNonce();

        // style-src 'unsafe-inline' is required: Mermaid injects its own <style> elements.
        return /* html */ `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Security-Policy"
          content="default-src 'none'; img-src ${webview.cspSource} data:; script-src 'nonce-${nonce}' ${webview.cspSource}; style-src ${webview.cspSource} 'unsafe-inline';">
    <style>
        body { padding: 0.5em 1em; }
        #legend { font-size: 0.85em; opacity: 0.8; margin-bottom: 0.5em; }
        #graph .node { cursor: pointer; }
        #graph svg { max-width: 100%; height: auto; }
    </style>
</head>
<body>
    <div id="legend">
        solid &rarr; depends on &nbsp;&bull;&nbsp; dashed &rarr; runs after &nbsp;&bull;&nbsp;
        thick &rarr; triggers &nbsp;&bull;&nbsp; bold border = default target &nbsp;&bull;&nbsp;
        <b>click a target to run it</b>
    </div>
    <div id="graph"></div>
    <script nonce="${nonce}" src="${mermaidUri}"></script>
    <script nonce="${nonce}">
        const vscode = acquireVsCodeApi();
        const container = document.getElementById('graph');
        const dark = document.body.classList.contains('vscode-dark')
                  || document.body.classList.contains('vscode-high-contrast');
        mermaid.initialize({ startOnLoad: false, securityLevel: 'strict', theme: dark ? 'dark' : 'default' });

        let renderCount = 0;
        async function render(definition) {
            const { svg } = await mermaid.render('falloutGraph' + renderCount++, definition);
            container.innerHTML = svg;
            for (const node of container.querySelectorAll('g.node')) {
                node.addEventListener('click', () => {
                    // Mermaid node DOM ids look like "flowchart-<TargetName>-<n>";
                    // target names are C# identifiers, so they never contain dashes.
                    const name = node.id.split('-')[1];
                    if (name) {
                        vscode.postMessage({ type: 'run', target: name });
                    }
                });
            }
        }

        window.addEventListener('message', event => {
            if (event.data.type === 'graph') {
                render(event.data.definition);
            }
        });
        vscode.postMessage({ type: 'ready' });
    </script>
</body>
</html>`;
    }
}

function getNonce(): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    return Array.from({ length: 32 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
}
