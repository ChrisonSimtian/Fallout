using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;

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
    const string OldRoot = "Fallout.Common";
    const string NewRoot = "Fallout.Application";
    const string OldExt = "Fallout.Build.Execution.Extensions";
    const string NewExt = "Fallout.Application.Execution.Extensions";
    const string SourceAssembly = "Fallout.Build";

    static bool IsMovable(string ns) => ns == OldRoot || ns.StartsWith(OldRoot + ".") || ns == OldExt || ns.StartsWith(OldExt + ".");
    static string MapNs(string ns) =>
        ns == OldRoot || ns.StartsWith(OldRoot + ".") ? NewRoot + ns[OldRoot.Length..]
        : ns == OldExt || ns.StartsWith(OldExt + ".") ? NewExt + ns[OldExt.Length..]
        : ns;

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

        // Moved types: declared in the source assembly under a movable namespace. Keyed by full name.
        var sourceProject = sln.Projects.FirstOrDefault(p => p.AssemblyName == SourceAssembly);
        if (sourceProject is null) { Console.Error.WriteLine($"FATAL: source project {SourceAssembly} not loaded."); return 1; }
        var sourceComp = await sourceProject.GetCompilationAsync();
        var movedFullNames = new HashSet<string>();
        void Collect(INamespaceSymbol nsSym)
        {
            foreach (var t in nsSym.GetTypeMembers()) if (IsMovable(nsSym.ToDisplayString())) movedFullNames.Add($"{nsSym.ToDisplayString()}.{t.Name}");
            foreach (var child in nsSym.GetNamespaceMembers()) Collect(child);
        }
        Collect(sourceComp!.Assembly.GlobalNamespace);
        Console.WriteLine($"Moved types (declared in {SourceAssembly}, movable ns): {movedFullNames.Count}");

        bool IsMovedType(INamedTypeSymbol t)
        {
            var ns = t.OriginalDefinition.ContainingNamespace?.ToDisplayString() ?? "";
            return t.OriginalDefinition.ContainingAssembly?.Name == SourceAssembly && IsMovable(ns) && movedFullNames.Contains($"{ns}.{t.OriginalDefinition.Name}");
        }

        int filesChanged = 0, refsRewritten = 0, usingsAdded = 0, usingsDropped = 0, nsDecls = 0;
        var samples = new List<string>();

        foreach (var project in sln.Projects)
        {
            var isSource = project.AssemblyName == SourceAssembly;
            foreach (var doc in project.Documents)
            {
                if (doc.FilePath is null || !doc.FilePath.EndsWith(".cs") || doc.FilePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")) continue;
                var model = await doc.GetSemanticModelAsync();
                var root = await doc.GetSyntaxRootAsync();
                if (model is null || root is null) continue;

                var (newRoot, c) = DocumentRewriter.Rewrite(root, model, isSource, IsMovable, MapNs, IsMovedType);
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
