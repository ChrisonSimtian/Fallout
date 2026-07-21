import * as vscode from 'vscode';
import { BuildGraph } from './model';

/**
 * Singleton webview panel rendering the build graph with the Fallout graph
 * control (@fallout/graph-control). The control ships as one self-contained IIFE
 * in media/fallout-graph-control.js exposing `FalloutGraph.mount`; we post it the
 * raw BuildGraph and it lays out + renders. Re-posting on a file change reconciles
 * in place, which is the live refresh.
 */
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
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')],
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
        void this.panel.webview.postMessage({ type: 'graph', graph });
    }

    private getHtml(extensionUri: vscode.Uri): string {
        const webview = this.panel.webview;
        const controlUri = webview.asWebviewUri(
            vscode.Uri.joinPath(extensionUri, 'media', 'fallout-graph-control.js'),
        );
        const nonce = getNonce();

        // style-src 'unsafe-inline' is required: the control injects its CSS as a
        // runtime <style> element (vite-plugin-css-injected-by-js).
        return /* html */ `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta http-equiv="Content-Security-Policy"
          content="default-src 'none'; img-src ${webview.cspSource} data:; script-src 'nonce-${nonce}' ${webview.cspSource}; style-src ${webview.cspSource} 'unsafe-inline';">
    <style>
        html, body { height: 100%; margin: 0; padding: 0; }
        #graph { height: 100vh; }
    </style>
</head>
<body>
    <div id="graph"></div>
    <script nonce="${nonce}" src="${controlUri}"></script>
    <script nonce="${nonce}">
        const vscode = acquireVsCodeApi();
        const container = document.getElementById('graph');
        const runTarget = (name) => vscode.postMessage({ type: 'run', target: name });

        window.addEventListener('message', event => {
            if (event.data.type === 'graph') {
                FalloutGraph.mount(container, event.data.graph, { onRunTarget: runTarget });
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
