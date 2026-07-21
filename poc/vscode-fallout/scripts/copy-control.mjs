// Vendors the Fallout graph control into media/ so the webview can load it as a
// single <script>, the same way this extension used to vendor mermaid.min.js.
//
// The control lives in the sibling poc/graph-control package. When it's present
// (both PoCs together on main, or a full checkout), build its library bundle and
// copy the artifact in. When it isn't (this branch alone, before #526 lands),
// skip gracefully if a previously-copied bundle already exists — otherwise fail
// loudly so packaging can't silently ship an extension with no graph.
import { execSync } from 'node:child_process';
import { existsSync, mkdirSync, copyFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const extensionRoot = join(here, '..');
const control = join(extensionRoot, '..', 'graph-control');
const artifact = join(control, 'dist-lib', 'fallout-graph-control.js');
const target = join(extensionRoot, 'media', 'fallout-graph-control.js');

mkdirSync(dirname(target), { recursive: true });

if (existsSync(join(control, 'package.json'))) {
    console.log('[copy-control] building @fallout/graph-control library bundle…');
    execSync('npm install --no-audit --no-fund', { cwd: control, stdio: 'inherit' });
    execSync('npm run build:lib', { cwd: control, stdio: 'inherit' });
    copyFileSync(artifact, target);
    console.log(`[copy-control] copied → ${target}`);
} else if (existsSync(target)) {
    console.warn('[copy-control] sibling graph-control not found; using the already-vendored bundle.');
} else {
    console.error(
        '[copy-control] sibling poc/graph-control not found and no vendored bundle present.\n' +
        'This branch depends on the graph-control package (PR #526). Check it out alongside, or\n' +
        'copy graph-control/dist-lib/fallout-graph-control.js into media/ manually.',
    );
    process.exit(1);
}
