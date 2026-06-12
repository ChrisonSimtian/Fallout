import * as vscode from 'vscode';
import { BuildGraph, GraphSource, GraphTarget, checkCompatibility, findGraphFile, loadGraph } from './model';
import { GraphPanel } from './graphPanel';
import { goToTarget } from './goToTarget';

type Relation = 'dependsOn' | 'after' | 'triggeredBy' | 'triggers';

const RELATION_LABELS: Record<Relation, string> = {
    dependsOn: 'depends on',
    after: 'runs after',
    triggeredBy: 'triggered by',
    triggers: 'triggers',
};

const RELATION_ICONS: Record<Relation, string> = {
    dependsOn: 'arrow-right',
    after: 'list-ordered',
    triggeredBy: 'arrow-left',
    triggers: 'zap',
};

class TargetItem extends vscode.TreeItem {
    constructor(
        public readonly target: GraphTarget,
        relation: Relation | undefined,
        hasChildren: boolean,
    ) {
        super(
            target.name,
            hasChildren ? vscode.TreeItemCollapsibleState.Collapsed : vscode.TreeItemCollapsibleState.None,
        );

        this.contextValue = 'falloutTarget';
        this.description = relation
            ? RELATION_LABELS[relation]
            : [target.default ? 'default' : undefined, target.description ?? undefined]
                .filter(Boolean)
                .join(' — ');

        this.iconPath = relation
            ? new vscode.ThemeIcon(RELATION_ICONS[relation])
            : new vscode.ThemeIcon(
                target.default ? 'rocket' : target.listed ? 'circle-large-outline' : 'circle-slash',
                target.listed ? undefined : new vscode.ThemeColor('disabledForeground'),
            );

        const lines = [
            `**${target.name}**${target.default ? ' _(default target)_' : ''}${target.listed ? '' : ' _(unlisted)_'}`,
            target.description ?? undefined,
            ...(['dependsOn', 'after', 'triggeredBy', 'triggers'] as Relation[])
                .filter(r => target[r].length > 0)
                .map(r => `${RELATION_LABELS[r]}: ${target[r].join(', ')}`),
        ];
        this.tooltip = new vscode.MarkdownString(lines.filter(Boolean).join('\n\n'));
    }
}

class FalloutTargetsProvider implements vscode.TreeDataProvider<TargetItem> {
    constructor(private readonly extensionVersion: string) {}

    private readonly onDidChangeEmitter = new vscode.EventEmitter<void>();
    readonly onDidChangeTreeData = this.onDidChangeEmitter.event;

    private byName = new Map<string, GraphTarget>();
    source: GraphSource | undefined;
    graph: BuildGraph | undefined;

    refresh(): void {
        this.onDidChangeEmitter.fire();
    }

    getTreeItem(element: TargetItem): vscode.TreeItem {
        return element;
    }

    getChildren(element?: TargetItem): TargetItem[] {
        if (!element) {
            return this.loadRoots();
        }

        // Children: one entry per related target, recursively expandable.
        return (['dependsOn', 'after', 'triggeredBy', 'triggers'] as Relation[]).flatMap(relation =>
            element.target[relation]
                .map(name => this.byName.get(name))
                .filter((t): t is GraphTarget => t !== undefined)
                .map(t => new TargetItem(t, relation, this.hasRelations(t))),
        );
    }

    private loadRoots(): TargetItem[] {
        this.source = findGraphFile();
        if (!this.source) {
            this.byName.clear();
            this.graph = undefined;
            return []; // empty view -> viewsWelcome content kicks in
        }

        try {
            this.graph = loadGraph(this.source);
        } catch (e) {
            void vscode.window.showWarningMessage(`Fallout: could not parse ${this.source.file}: ${e}`);
            return [];
        }

        checkCompatibility(this.graph, this.extensionVersion);
        this.byName = new Map(this.graph.targets.map(t => [t.name, t]));
        return this.graph.targets
            .slice()
            .sort((a, b) => Number(b.listed) - Number(a.listed) || a.name.localeCompare(b.name))
            .map(t => new TargetItem(t, undefined, this.hasRelations(t)));
    }

    private hasRelations(target: GraphTarget): boolean {
        return target.dependsOn.length + target.after.length + target.triggeredBy.length + target.triggers.length > 0;
    }
}

function runInTerminal(root: string, args: string): void {
    const existing = vscode.window.terminals.find(t => t.name === 'Fallout');
    const terminal = existing ?? vscode.window.createTerminal({ name: 'Fallout', cwd: root });
    terminal.show();
    terminal.sendText(process.platform === 'win32' ? `./build.ps1 ${args}` : `./build.sh ${args}`);
}

export function activate(context: vscode.ExtensionContext): void {
    const extensionVersion: string = context.extension.packageJSON.version;
    const provider = new FalloutTargetsProvider(extensionVersion);

    const runTarget = (name: string) => {
        const root = provider.source?.root;
        if (root) {
            runInTerminal(root, name);
        }
    };

    const refreshAll = () => {
        provider.refresh();
        const source = findGraphFile();
        if (source) {
            try {
                GraphPanel.refresh(loadGraph(source));
            } catch {
                // transient: the build may be mid-write; the next watcher event re-reads it
            }
        }
    };

    const watcher = vscode.workspace.createFileSystemWatcher('**/build-graph.json');
    watcher.onDidChange(refreshAll);
    watcher.onDidCreate(refreshAll);
    watcher.onDidDelete(() => provider.refresh());

    context.subscriptions.push(
        vscode.window.registerTreeDataProvider('falloutTargets', provider),
        watcher,
        vscode.commands.registerCommand('fallout.refreshTargets', refreshAll),
        vscode.commands.registerCommand('fallout.runTarget', (item: TargetItem) => {
            if (item?.target) {
                runTarget(item.target.name);
            }
        }),
        vscode.commands.registerCommand('fallout.goToTarget', (item: TargetItem) => {
            if (item?.target) {
                void goToTarget(item.target);
            }
        }),
        vscode.commands.registerCommand('fallout.showGraph', () => {
            const source = findGraphFile();
            if (!source) {
                void vscode.window.showInformationMessage(
                    'Fallout: no build-graph.json found. Run the build once (e.g. ./build.ps1 --help) first.');
                return;
            }
            try {
                provider.source ??= source;
                const graph = loadGraph(source);
                checkCompatibility(graph, extensionVersion);
                GraphPanel.createOrShow(context.extensionUri, graph, runTarget);
            } catch (e) {
                void vscode.window.showWarningMessage(`Fallout: could not parse ${source.file}: ${e}`);
            }
        }),
        vscode.commands.registerCommand('fallout.planTarget', () => {
            const root = provider.source?.root ?? vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
            if (root) {
                runInTerminal(root, '--plan');
            }
        }),
    );
}

export function deactivate(): void {}
