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
    falloutVersion?: string | null;
    targets: GraphTarget[];
}

/** Schema version of build-graph.json this extension understands. */
export const SUPPORTED_SCHEMA_VERSION = 1;

const warned = new Set<string>();

/**
 * Versioning contract: the extension's major.minor track the Fallout framework
 * (calendar versioning, major = year); the patch moves independently. The schema
 * `version` is the hard gate, the major.minor comparison a drift warning.
 */
export function checkCompatibility(graph: BuildGraph, extensionVersion: string): void {
    let message: string | undefined;
    if (graph.version !== SUPPORTED_SCHEMA_VERSION) {
        message = `Fallout: build-graph.json uses schema v${graph.version}, but this extension understands v${SUPPORTED_SCHEMA_VERSION}. ` +
            'Update the extension or the Fallout package — the view may be incomplete.';
    } else if (graph.falloutVersion) {
        const [exMajor, exMinor] = extensionVersion.split('.');
        const [fwMajor, fwMinor] = graph.falloutVersion.split('-')[0].split('.');
        if (exMajor !== fwMajor || exMinor !== fwMinor) {
            message = `Fallout: this workspace builds with Fallout ${graph.falloutVersion}, but the extension is versioned ${extensionVersion} ` +
                `(coupled to Fallout ${exMajor}.${exMinor}). Things may not line up.`;
        }
    }

    if (message && !warned.has(message)) {
        warned.add(message);
        void vscode.window.showWarningMessage(message);
    }
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
