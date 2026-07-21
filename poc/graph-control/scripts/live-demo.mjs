// Emits a self-contained live-run demo: the control's IIFE inlined, driven by a
// scripted build run through FalloutGraph.mountLive. Proves the live animation
// path (queued → running → succeeded, edges flowing into active targets) with no
// server and no C# side — a stand-in for the real BuildManager status stream.
//
//   node scripts/live-demo.mjs [out.html]
import { execSync } from 'node:child_process';
import { existsSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, '..');
const bundle = join(root, 'dist-lib', 'fallout-graph-control.js');
const outArg = process.argv[2] ?? join(root, 'dist-lib', 'live.html');

if (!existsSync(bundle)) {
    console.log('[live-demo] building library bundle…');
    execSync('npm run build:lib', { cwd: root, stdio: 'inherit' });
}
const bundleJs = readFileSync(bundle, 'utf8');

const html = `<!doctype html>
<html lang="en" data-theme="dark">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Fallout — Live Build Graph</title>
<style>html, body { height: 100%; margin: 0; padding: 0; } #graph { height: 100vh; }</style>
</head>
<body>
<div id="graph"></div>
<script>${bundleJs}</script>
<script>
    // Structure with every target queued — the run drives the statuses.
    var initial = {
        version: 1, falloutVersion: '2026.1.0-preview.412.g8f3a1c',
        targets: [
            { name:'Clean',   description:'Wipe artifacts',     default:false, listed:false, dependsOn:[],                 after:[],         triggeredBy:[],        triggers:[],         status:'skipped' },
            { name:'Restore', description:'dotnet restore',      default:false, listed:true,  dependsOn:[],                 after:['Clean'],  triggeredBy:[],        triggers:[],         status:'queued' },
            { name:'Compile', description:'Build all projects',  default:false, listed:true,  dependsOn:['Restore'],        after:[],         triggeredBy:[],        triggers:[],         status:'queued' },
            { name:'Test',    description:'xUnit suite',         default:false, listed:true,  dependsOn:['Compile'],        after:[],         triggeredBy:[],        triggers:[],         status:'queued' },
            { name:'Pack',    description:'NuGet pack (default)',default:true,  listed:true,  dependsOn:['Compile'],        after:[],         triggeredBy:[],        triggers:['Canary'], status:'queued' },
            { name:'Publish', description:'Push to GH Packages', default:false, listed:true,  dependsOn:['Test','Pack'],    after:[],         triggeredBy:[],        triggers:[],         status:'queued' },
            { name:'Canary',  description:'Smoke-test package',  default:false, listed:true,  dependsOn:['Publish'],        after:[],         triggeredBy:['Pack'],  triggers:[],         status:'queued' }
        ]
    };

    // Scripted run: each step is a status patch pushed at { at } ms into the run.
    var script = [
        { at: 500,  s: { Restore:'running' } },
        { at: 1600, s: { Restore:'succeeded', Compile:'running' } },
        { at: 3000, s: { Compile:'succeeded', Test:'running', Pack:'running' } },
        { at: 5200, s: { Test:'succeeded', Pack:'succeeded', Publish:'running' } },
        { at: 6600, s: { Publish:'succeeded', Canary:'running' } },
        { at: 7800, s: { Canary:'succeeded' } }
    ];
    var LOOP_GAP = 2000, RUN_END = 7800;

    // Looping simulator: resets to queued, replays the script, repeats — so the
    // graph is animating whenever you look. Stand-in for a real status stream.
    function simulate(push) {
        var timers = [];
        function reset() {
            var t = initial.targets.map(function (x) {
                return Object.assign({}, x, { status: x.name === 'Clean' ? 'skipped' : 'queued' });
            });
            push(Object.assign({}, initial, { targets: t }));
        }
        function cycle() {
            reset();
            script.forEach(function (step) { timers.push(setTimeout(function () { push(step.s); }, step.at)); });
            timers.push(setTimeout(cycle, RUN_END + LOOP_GAP));
        }
        cycle();
        return function () { timers.forEach(clearTimeout); };
    }

    FalloutGraph.mountLive(document.getElementById('graph'), initial, { subscribe: simulate });
</script>
</body>
</html>
`;

writeFileSync(resolve(outArg), html);
console.log('[live-demo] wrote ' + resolve(outArg) + ' (' + Math.round(Buffer.byteLength(html) / 1024) + ' KB, self-contained)');
