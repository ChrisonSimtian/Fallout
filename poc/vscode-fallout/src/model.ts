import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';

/** Shape of .fallout/temp/build-graph.json, emitted by EmitBuildGraphAttribute (FALLOUT001). */
export interface GraphTarget {
    name: string;
    description: string | null;
    declaredIn?: string | null;
    default: boolean;
    listed: boolean;
    dependsOn: string[];
    after: string[];
    triggeredBy: string[];
    triggers: string[];
}

export interface BuildGraph {
    version: number;
    targets: GraphTarget[];
}

export interface GraphSource {
    root: string;
    file: string;
}

const GRAPH_LOCATIONS = ['.fallout/temp/build-graph.json', '.nuke/temp/build-graph.json'];

export function findGraphFile(): GraphSource | undefined {
    for (const folder of vscode.workspace.workspaceFolders ?? []) {
        for (const rel of GRAPH_LOCATIONS) {
            const file = path.join(folder.uri.fsPath, ...rel.split('/'));
            if (fs.existsSync(file)) {
                return { root: folder.uri.fsPath, file };
            }
        }
    }
    return undefined;
}

export function loadGraph(source: GraphSource): BuildGraph {
    return JSON.parse(fs.readFileSync(source.file, 'utf8'));
}

/**
 * Builds the Mermaid flowchart definition. Same edge semantics as the --plan HTML:
 * solid = execution dependency, dashed = order dependency, thick = trigger.
 * Arrows point in execution-flow direction (prerequisite --> dependent).
 */
export function toMermaid(graph: BuildGraph): string {
    const lines = ['flowchart TD'];

    for (const t of graph.targets) {
        lines.push(`  ${t.name}["${t.name}"]`);
    }
    for (const t of graph.targets) {
        t.dependsOn.forEach(d => lines.push(`  ${d} --> ${t.name}`));
        t.after.forEach(d => lines.push(`  ${d} -.-> ${t.name}`));
        // triggeredBy is the same edge seen from the other side - emitting triggers alone avoids duplicates
        t.triggers.forEach(d => lines.push(`  ${t.name} ==> ${d}`));
    }

    const defaults = graph.targets.filter(t => t.default).map(t => t.name);
    if (defaults.length > 0) {
        lines.push(`  class ${defaults.join(',')} defaultTarget`);
    }
    const unlisted = graph.targets.filter(t => !t.listed).map(t => t.name);
    if (unlisted.length > 0) {
        lines.push(`  class ${unlisted.join(',')} unlisted`);
    }
    lines.push('  classDef defaultTarget stroke-width:3px,font-weight:bold');
    lines.push('  classDef unlisted opacity:0.45,stroke-dasharray:3 3');

    return lines.join('\n');
}
