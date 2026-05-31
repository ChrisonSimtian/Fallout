using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// OnionRewriter (semantic) — ADR-0006. Moves every type declared in a source project out of an old
// namespace prefix into a new one, and fixes references across the whole workspace by the symbol each
// reference actually BINDS to (so simple-name collisions across namespaces resolve correctly — no
// spurious usings, no CS0104). Rewrites: namespace declarations in the source project; qualified
// references to moved types; and `using` directives (add the new namespace, drop the old where no
// residual type is still used). Default = dry run (reports, mutates nothing). Pass --apply to write.

MSBuildLocator.RegisterDefaults();
return await Runner.RunAsync(args);

static class Runner
{
    // A move rule: types declared in any of `SourceAssemblies` under namespace `Old` (exact or
    // `.`-prefixed) move to the corresponding `New` namespace. The rule table is the multi-rule map
    // ADR-0006 steps 4–5 need — edit it per onion step (steps 1–2 + 4a are landed, so their rules are
    // gone). First matching rule wins for the namespace mapping. A single namespace that must split
    // across rings type-by-type uses `TypeOverrides` (full-name → target ns), which beats the prefix rule.
    record struct Rule(string Old, string New, string[] SourceAssemblies);

    // Step 4b — Tool vocabulary → Application; the impure executor that shares Fallout.Common.Tooling →
    // Infrastructure (user-chosen "true homes": ToolTasks(App) → ProcessTasks(Infra) is a tracked onion
    // violation until the resolver/process ports land). Tools.* and the Tooling vocabulary fold into the
    // Application ring; the 13 executor types below are carved out by name.
    static readonly Rule[] Rules =
    [
        new("Fallout.Common.Tools",   "Fallout.Application.Tools",   ["Fallout.Common"]),
        new("Fallout.Common.Tooling", "Fallout.Application.Tooling", ["Fallout.Tooling", "Fallout.Common"]),
    ];

    // Impure executor types (process start, filesystem/HTTP tool & package resolution) that live in the
    // Fallout.Common.Tooling namespace alongside the vocabulary — carved to Infrastructure by full name.
    // Ports (IProcess, IProcessRunner), the exception type, attributes, and the rest stay vocabulary →
    // Application; Infrastructure depending on Application is the correct (inward) onion direction.
    const string InfraTooling = "Fallout.Infrastructure.Tooling";
    static readonly Dictionary<string, string> TypeOverrides = new[]
    {
        "ProcessTasks", "SystemProcessRunner", "Process2", "ProcessExtensions", "ToolExecutor",
        "ToolPathResolver", "NuGetToolPathResolver", "NpmToolPathResolver",
        "NuGetVersionResolver", "NpmVersionResolver", "NuGetPackageResolver", "PaketPackageResolver",
        "ToolingExtensions",
    }.ToDictionary(n => $"Fallout.Common.Tooling.{n}", _ => InfraTooling);

    static bool Matches(Rule r, string ns) => ns == r.Old || ns.StartsWith(r.Old + ".");
    static bool IsMovable(string ns) => Rules.Any(r => Matches(r, ns));
    // New namespace for a moved type, by full name + declaring namespace. Per-type override beats the
    // prefix rule; returns null if nothing moves it.
    static string? TargetNs(string fullName, string ns)
    {
        if (TypeOverrides.TryGetValue(fullName, out var t)) return t;
        foreach (var r in Rules)
            if (Matches(r, ns)) return r.New + ns[r.Old.Length..];
        return null;
    }
    // Does assembly `asm` declare a type under namespace `ns` that a rule moves? (Gates moved-type
    // membership and namespace-decl rewriting to the rule's own source assemblies — a type with the same
    // namespace in another assembly is NOT moved.)
    static bool IsSourceFor(string asm, string ns) => Rules.Any(r => r.SourceAssemblies.Contains(asm) && Matches(r, ns));
    static readonly HashSet<string> SourceAssemblies = Rules.SelectMany(r => r.SourceAssemblies).ToHashSet();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<int> RunAsync(string[] args)
    {
        var apply = args.Contains("--apply");
        var repo = Directory.GetCurrentDirectory();

        using var ws = Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace.Create();
        ws.WorkspaceFailed += (_, e) => { if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure) Console.Error.WriteLine("  ws: " + e.Diagnostic.Message); };

        var csprojs = Directory.EnumerateFiles(Path.Combine(repo, "src"), "*.csproj", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(repo, "tests"), "*.csproj", SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(Path.Combine(repo, "build"), "*.csproj", SearchOption.AllDirectories))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .OrderBy(p => p).ToList();

        Console.WriteLine($"Loading {csprojs.Count} projects into the workspace…");
        foreach (var p in csprojs)
        {
            try { await ws.OpenProjectAsync(p); }
            catch (Exception ex) { Console.Error.WriteLine($"  open failed {Path.GetFileName(p)}: {ex.Message}"); }
        }
        var sln = ws.CurrentSolution;
        Console.WriteLine($"Loaded {sln.Projects.Count()} projects.");

        // Moved types: declared in a rule's source assembly under that rule's movable namespace. Keyed by
        // full name. Collected across every source assembly (a step may move types out of several).
        var sourceProjects = sln.Projects.Where(p => SourceAssemblies.Contains(p.AssemblyName)).ToList();
        var missing = SourceAssemblies.Except(sourceProjects.Select(p => p.AssemblyName)).ToList();
        if (missing.Count > 0) { Console.Error.WriteLine($"FATAL: source assemblies not loaded: {string.Join(", ", missing)}"); return 1; }
        var movedFullNames = new HashSet<string>();
        foreach (var sp in sourceProjects)
        {
            var asm = sp.AssemblyName;
            var comp = await sp.GetCompilationAsync();
            void Collect(INamespaceSymbol nsSym)
            {
                var ns = nsSym.ToDisplayString();
                foreach (var t in nsSym.GetTypeMembers()) if (IsSourceFor(asm, ns)) movedFullNames.Add($"{ns}.{t.Name}");
                foreach (var child in nsSym.GetNamespaceMembers()) Collect(child);
            }
            Collect(comp!.Assembly.GlobalNamespace);
        }
        Console.WriteLine($"Moved types (declared in [{string.Join(", ", SourceAssemblies)}], movable ns): {movedFullNames.Count}");

        // New namespace for a moved type, or null if it doesn't move. Match by assembly NAME + the
        // moved-type set (resolves correctly across projects — a referenced project's assembly symbol is a
        // different instance per compilation, so identity comparison fails cross-project). Pinned package
        // consumers (Consumer.NuGet/Nuke.Consumer) may carry a source assembly via their published
        // package, but those projects are skipped entirely in the doc loop. A nested type's
        // ContainingNamespace is its enclosing NAMESPACE (not its outer type), so a member keyed by
        // `{ns}.{Name}` would miss the moved-set (top-level names only) and be misclassified as residual —
        // adding a dangling `using` to a namespace this move evacuates. A nested type moves iff its
        // OUTERMOST enclosing type moves (and to that type's target), so classify by that top-level type.
        string? TargetNsForType(INamedTypeSymbol t)
        {
            var top = t.OriginalDefinition;
            while (top.ContainingType is not null) top = top.ContainingType;
            var ns = top.ContainingNamespace?.ToDisplayString() ?? "";
            var asm = top.ContainingAssembly?.Name ?? "";
            var full = $"{ns}.{top.Name}";
            return IsSourceFor(asm, ns) && movedFullNames.Contains(full) ? TargetNs(full, ns) : null;
        }

        // Movable namespaces still declared OUTSIDE every source project (so they survive the move). A
        // movable `using` whose namespace is NOT here is evacuated and must be dropped even if the file
        // referenced its types only via inference (no explicit type name).
        var sourceDirs = sourceProjects
            .Where(p => p.FilePath is not null)
            .Select(p => Path.GetDirectoryName(p.FilePath)! + Path.DirectorySeparatorChar)
            .ToList();
        var survivingMovableNs = new HashSet<string>();
        foreach (var f in Directory.EnumerateFiles(Path.Combine(repo, "src"), "*.cs", SearchOption.AllDirectories))
        {
            if (f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") || f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")) continue;
            if (sourceDirs.Any(d => f.StartsWith(d, StringComparison.Ordinal))) continue;
            foreach (var n in CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetRoot().DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
            {
                var ns = n.Name.ToString();
                if (IsMovable(ns)) survivingMovableNs.Add(ns);
            }
        }

        int filesChanged = 0, refsRewritten = 0, usingsAdded = 0, usingsDropped = 0, nsDecls = 0;
        var samples = new List<string>();

        foreach (var project in sln.Projects)
        {
            var isSource = SourceAssemblies.Contains(project.AssemblyName);
            foreach (var doc in project.Documents)
            {
                if (doc.FilePath is null || !doc.FilePath.EndsWith(".cs") || doc.FilePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")) continue;
                // Package consumers (Consumer.NuGet, Nuke.Consumer) compile against the PUBLISHED package,
                // which still has the old namespaces — the local rename must not touch them. (Consumer.Local
                // references the local source and IS migrated.)
                if (doc.FilePath.Contains("Consumer.NuGet") || doc.FilePath.Contains("Nuke.Consumer")) continue;
                var model = await doc.GetSemanticModelAsync();
                var root = await doc.GetSyntaxRootAsync();
                if (model is null || root is null) continue;

                var (newRoot, c) = DocumentRewriter.Rewrite(root, model, isSource, IsMovable, TargetNsForType, survivingMovableNs);
                if (c.Changed)
                {
                    filesChanged++; refsRewritten += c.Refs; usingsAdded += c.Added; usingsDropped += c.Dropped; nsDecls += c.NsDecls;
                    if (samples.Count < 12) samples.Add(Path.GetRelativePath(repo, doc.FilePath));
                    if (apply) await File.WriteAllTextAsync(doc.FilePath, newRoot.ToFullString());
                }
            }
        }

        Console.WriteLine($"\nOnionRewriter ({(apply ? "APPLY" : "dry-run")})");
        Console.WriteLine($"  files changed   : {filesChanged}");
        Console.WriteLine($"  namespace decls : {nsDecls}");
        Console.WriteLine($"  qualified refs  : {refsRewritten}");
        Console.WriteLine($"  usings added    : {usingsAdded}");
        Console.WriteLine($"  usings dropped  : {usingsDropped}");
        Console.WriteLine($"  sample files    :\n    {string.Join("\n    ", samples)}");
        return 0;
    }
}
