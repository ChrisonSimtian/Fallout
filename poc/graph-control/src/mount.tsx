import { createRoot, type Root } from 'react-dom/client';
import { GraphControl } from './GraphControl';
import type { BuildGraph } from './model';

// Library entry for host embedding (VS Code webview, static HTML report).
// The IIFE build exposes these as `window.FalloutGraph`.

export interface MountOptions {
    onRunTarget?: (target: string) => void;
}

// One React root per container, reused across graph updates so re-rendering with
// a fresh graph reconciles in place (drives the extension's live refresh).
const roots = new WeakMap<HTMLElement, Root>();

export function mount(el: HTMLElement, graph: BuildGraph, options: MountOptions = {}): void {
    let root = roots.get(el);
    if (!root) {
        root = createRoot(el);
        roots.set(el, root);
    }
    root.render(<GraphControl graph={graph} onRunTarget={options.onRunTarget} />);
}

export function unmount(el: HTMLElement): void {
    roots.get(el)?.unmount();
    roots.delete(el);
}

// ---- Live run graph ----------------------------------------------------------

/** A status update: either a {targetName: status} patch, or a whole replacement graph. */
export type StatusUpdate = Record<string, string> | BuildGraph;

/** Wires a status source. Called once with a `push` callback; returns a teardown. */
export type Subscribe = (push: (update: StatusUpdate) => void) => (() => void) | void;

export interface LiveOptions {
    onRunTarget?: (target: string) => void;
    /** The status source — e.g. `pollStatus(url)` or `sseStatus(url)`, or a custom fn. */
    subscribe: Subscribe;
}

function isGraph(update: StatusUpdate): update is BuildGraph {
    return Array.isArray((update as BuildGraph).targets);
}

/**
 * Renders the graph and keeps it live: each update from `subscribe` patches
 * per-target status (or replaces the whole graph) and re-renders. Because layout
 * is cached on structure, a status patch only animates the affected nodes/edges.
 * Returns a dispose function that tears down the source and unmounts.
 */
export function mountLive(el: HTMLElement, initialGraph: BuildGraph, options: LiveOptions): () => void {
    let graph = initialGraph;
    const render = () => mount(el, graph, { onRunTarget: options.onRunTarget });
    render();

    const apply = (update: StatusUpdate) => {
        if (isGraph(update)) {
            graph = update;
        } else {
            graph = {
                ...graph,
                targets: graph.targets.map((t) =>
                    update[t.name] ? { ...t, status: update[t.name] } : t,
                ),
            };
        }
        render();
    };

    const teardown = options.subscribe(apply);
    return () => {
        teardown?.();
        unmount(el);
    };
}

/** Status source that polls a JSON URL (a status map or a full graph) on an interval. */
export function pollStatus(url: string, intervalMs = 1000): Subscribe {
    return (push) => {
        let stopped = false;
        const tick = async () => {
            if (stopped) return;
            try {
                const res = await fetch(url, { cache: 'no-store' });
                if (res.ok) push(await res.json());
            } catch {
                // transient — the next tick retries
            }
        };
        const id = setInterval(tick, intervalMs);
        void tick();
        return () => {
            stopped = true;
            clearInterval(id);
        };
    };
}

/** Status source backed by Server-Sent Events; each event's data is a JSON update. */
export function sseStatus(url: string): Subscribe {
    return (push) => {
        const source = new EventSource(url);
        source.onmessage = (event) => {
            try {
                push(JSON.parse(event.data));
            } catch {
                // ignore malformed frames
            }
        };
        return () => source.close();
    };
}
