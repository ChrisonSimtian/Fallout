import * as vscode from 'vscode';
import { GraphTarget } from './model';

/**
 * Jumps to the C# declaration of a target: `Target <Name> => _ => _ ...`.
 * Tries the C# language service first (precise, needs Roslyn warmed up), then falls back
 * to a regex scan over workspace .cs files. `declaredIn` (from build-graph.json) breaks
 * ties when several types declare a same-named target (e.g. component interfaces).
 */
export async function goToTarget(target: GraphTarget): Promise<void> {
    let locations = await viaSymbolProvider(target);
    if (locations.length === 0) {
        locations = await viaRegexScan(target);
    }

    if (locations.length === 0) {
        void vscode.window.showInformationMessage(
            `Fallout: no declaration of target '${target.name}' found in the workspace.`);
        return;
    }

    const location = locations.length === 1 ? locations[0] : await pickLocation(target.name, locations);
    if (location) {
        await vscode.window.showTextDocument(location.uri, { selection: location.range });
    }
}

async function viaSymbolProvider(target: GraphTarget): Promise<vscode.Location[]> {
    let symbols: vscode.SymbolInformation[];
    try {
        symbols = await vscode.commands.executeCommand<vscode.SymbolInformation[]>(
            'vscode.executeWorkspaceSymbolProvider', target.name) ?? [];
    } catch {
        return [];
    }

    let candidates = symbols.filter(s =>
        s.name === target.name &&
        (s.kind === vscode.SymbolKind.Property || s.kind === vscode.SymbolKind.Field) &&
        s.location.uri.fsPath.endsWith('.cs'));

    if (target.declaredIn) {
        // containerName is e.g. "Build" or "Fallout.Components.ICompile" depending on the provider
        const scoped = candidates.filter(s => s.containerName?.split('.').pop() === target.declaredIn);
        if (scoped.length > 0) {
            candidates = scoped;
        }
    }
    return candidates.map(s => s.location);
}

async function viaRegexScan(target: GraphTarget): Promise<vscode.Location[]> {
    const files = await vscode.workspace.findFiles('**/*.cs', '{**/bin/**,**/obj/**,**/node_modules/**}');
    // Matches plain and explicit-interface declarations: "Target Foo =>", "Target IComponent.Foo =>"
    const declaration = new RegExp(`(?:^|\\s)Target\\s+(?:[\\w.]+\\.)?${target.name}\\s*=>`);
    const declaringType = target.declaredIn
        ? new RegExp(`(?:class|interface)\\s+${target.declaredIn}\\b`)
        : undefined;

    const matches: { location: vscode.Location; inDeclaringType: boolean }[] = [];
    for (const file of files) {
        const text = Buffer.from(await vscode.workspace.fs.readFile(file)).toString('utf8');
        const match = declaration.exec(text);
        if (!match) {
            continue;
        }
        const offset = match.index + match[0].indexOf('Target');
        const before = text.slice(0, offset);
        const line = (before.match(/\n/g) ?? []).length;
        const character = offset - (before.lastIndexOf('\n') + 1);
        matches.push({
            location: new vscode.Location(file, new vscode.Position(line, character)),
            inDeclaringType: declaringType?.test(text) ?? false,
        });
    }

    const preferred = matches.filter(m => m.inDeclaringType);
    return (preferred.length > 0 ? preferred : matches).map(m => m.location);
}

async function pickLocation(targetName: string, locations: vscode.Location[]): Promise<vscode.Location | undefined> {
    const picked = await vscode.window.showQuickPick(
        locations.map(location => ({
            label: targetName,
            description: `${vscode.workspace.asRelativePath(location.uri)}:${location.range.start.line + 1}`,
            location,
        })),
        { placeHolder: `Multiple declarations of '${targetName}' found` },
    );
    return picked?.location;
}
