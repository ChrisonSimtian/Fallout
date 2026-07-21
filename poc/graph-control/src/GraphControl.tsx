import { useEffect, useMemo, useState } from 'react';
import {
    ReactFlow,
    Background,
    BackgroundVariant,
    Controls,
    type Edge,
    type Node,
    type NodeMouseHandler,
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';
import './theme.css';
import './control.css';
import { TargetNode } from './TargetNode';
import { layoutGraph, type TargetNodeData } from './layout';
import type { BuildGraph } from './model';

const nodeTypes = { target: TargetNode };

export interface GraphControlProps {
    graph: BuildGraph;
    /** Fired when a target card is clicked — the host decides what "run" means. */
    onRunTarget?: (target: string) => void;
}

// Structural signature: names + relations + flags, but NOT status. Layout depends
// only on structure, so a status-only change (the live case) reuses the existing
// positions instead of triggering an async relayout — the key to smooth animation.
function structureSignature(graph: BuildGraph): string {
    return graph.targets
        .map((t) => `${t.name}|${t.dependsOn}|${t.after}|${t.triggers}|${t.triggeredBy}|${t.default}|${t.listed}`)
        .join('\n');
}

/**
 * The reusable Fallout graph control. Same component drives the VS Code webview,
 * the static --plan HTML report, and the live CI run graph — only the data source
 * and the onRunTarget handler change. Layout (positions) is computed from the
 * structure; per-target status is patched in on top, so a live run animates
 * without re-laying-out.
 */
export function GraphControl({ graph, onRunTarget }: GraphControlProps) {
    // Base layout — positions + edges — recomputed only when the structure changes.
    const [base, setBase] = useState<{ nodes: Node<TargetNodeData>[]; edges: Edge[] } | null>(null);
    const structureKey = useMemo(() => structureSignature(graph), [graph]);

    useEffect(() => {
        let cancelled = false;
        void layoutGraph(graph).then((laid) => {
            if (!cancelled) setBase(laid);
        });
        return () => {
            cancelled = true;
        };
        // graph is read for its structure only; structureKey gates the relayout.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [structureKey]);

    const statusByName = useMemo(() => {
        const m = new Map<string, string | undefined>();
        for (const t of graph.targets) m.set(t.name, t.status);
        return m;
    }, [graph]);

    // Patch current status onto the laid-out nodes (new object only when it changed,
    // so unchanged nodes keep referential identity).
    const nodes = useMemo<Node<TargetNodeData>[]>(() => {
        if (!base) return [];
        return base.nodes.map((n) => {
            const status = statusByName.get(n.id);
            return status === n.data.status ? n : { ...n, data: { ...n.data, status } };
        });
    }, [base, statusByName]);

    // Animate edges feeding a currently-running target (the "flow" into active work).
    const edges = useMemo<Edge[]>(() => {
        if (!base) return [];
        return base.edges.map((e) => {
            const animated = statusByName.get(e.target) === 'running';
            return animated === e.animated ? e : { ...e, animated };
        });
    }, [base, statusByName]);

    const onNodeClick = useMemo<NodeMouseHandler>(
        () => (_event, node) => onRunTarget?.(node.id),
        [onRunTarget],
    );

    return (
        <div className="fallout-graph">
            <div className="graph-header">
                <span className="mark" aria-hidden="true">
                    ☢
                </span>
                <span className="graph-title">Build graph</span>
                {graph.falloutVersion && <span className="graph-version">Fallout {graph.falloutVersion}</span>}
                <span className="graph-count">{graph.targets.length} targets</span>
            </div>
            <div className="graph-canvas">
                {base && (
                    <ReactFlow
                        nodes={nodes}
                        edges={edges}
                        nodeTypes={nodeTypes}
                        onNodeClick={onNodeClick}
                        fitView
                        fitViewOptions={{ padding: 0.2 }}
                        minZoom={0.2}
                        maxZoom={2}
                        proOptions={{ hideAttribution: true }}
                        nodesDraggable={false}
                        nodesConnectable={false}
                        elementsSelectable
                    >
                        <Background variant={BackgroundVariant.Dots} gap={22} size={1} className="graph-bg" />
                        <Controls showInteractive={false} />
                    </ReactFlow>
                )}
            </div>
            <div className="graph-legend">
                <span><i className="k-depends" /> depends on</span>
                <span><i className="k-after" /> runs after</span>
                <span><i className="k-trigger" /> triggers</span>
                {onRunTarget && <span className="legend-hint">click a target to run it</span>}
            </div>
        </div>
    );
}
