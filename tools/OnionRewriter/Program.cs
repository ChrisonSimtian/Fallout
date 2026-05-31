using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// OnionRewriter — ADR-0006. Moves every type declared in a source project out of an old namespace
// prefix into a new one, and fixes references across the repo. The hard part it exists for: the old
// root namespace (Fallout.Common) is shared across several projects, so it disambiguates per file by
// the *types actually referenced* — adding/keeping/dropping `using`s accordingly. Biased to
// over-approximate (extra usings are harmless; the compiler catches any genuine miss).
//
// Usage:  dotnet run --project tools/OnionRewriter [-- --apply]
//   (default = dry run: report only, mutate nothing)

var apply = args.Contains("--apply");
var repo = Directory.GetCurrentDirectory();
var sourceProj = Path.Combine(repo, "src", "Fallout.Build") + Path.DirectorySeparatorChar;

const string OldRoot = "Fallout.Common";
const string NewRoot = "Fallout.Application";
const string OldExt = "Fallout.Build.Execution.Extensions";
const string NewExt = "Fallout.Application.Execution.Extensions";

bool IsMovable(string ns) => ns == OldRoot || ns.StartsWith(OldRoot + ".") || ns == OldExt || ns.StartsWith(OldExt + ".");
string MapNs(string ns) =>
    ns == OldRoot || ns.StartsWith(OldRoot + ".") ? NewRoot + ns[OldRoot.Length..]
    : ns == OldExt || ns.StartsWith(OldExt + ".") ? NewExt + ns[OldExt.Length..]
    : ns;

IEnumerable<string> CsFiles(params string[] roots) =>
    roots.Where(Directory.Exists)
         .SelectMany(r => Directory.EnumerateFiles(r, "*.cs", SearchOption.AllDirectories))
         .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                  && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                  && !p.Contains($"{Path.DirectorySeparatorChar}.claude{Path.DirectorySeparatorChar}")
                  && !p.Contains($"{Path.DirectorySeparatorChar}vendor{Path.DirectorySeparatorChar}"));

static string? NsOf(SyntaxNode n) =>
    n.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();

// True when a name appears in a type position (so an added `using` is warranted). Excludes the
// member name in `x.Foo` and the bare call target in `Foo(...)` — the main false-positive sources.
static bool IsTypePosition(SimpleNameSyntax n) =>
    !(n.Parent is MemberAccessExpressionSyntax ma && ma.Name == n)
    && !(n.Parent is InvocationExpressionSyntax inv && inv.Expression == n);

IEnumerable<(string Name, string Ns)> TopLevelTypes(SyntaxNode root)
{
    foreach (var t in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        if (t.Parent is BaseNamespaceDeclarationSyntax && NsOf(t) is { } ns)
            yield return (t.Identifier.Text, ns);
    foreach (var d in root.DescendantNodes().OfType<DelegateDeclarationSyntax>())
        if (d.Parent is BaseNamespaceDeclarationSyntax && NsOf(d) is { } ns)
            yield return (d.Identifier.Text, ns);
}

// Pass A — types declared in the source project under a movable namespace (these MOVE).
var movedTypes = new Dictionary<string, string>();          // simpleName -> new namespace
var movedNamespaces = new HashSet<string>();                // old movable namespaces actually present
foreach (var f in CsFiles(sourceProj))
    foreach (var (name, ns) in TopLevelTypes(CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetRoot()))
        if (IsMovable(ns)) { movedTypes[name] = MapNs(ns); movedNamespaces.Add(ns); }

// Pass B — types under Fallout.Common.* declared OUTSIDE the source project (these STAY = residual).
var residualByNs = new Dictionary<string, HashSet<string>>();
foreach (var f in CsFiles(Path.Combine(repo, "src")))
{
    if (f.StartsWith(sourceProj, StringComparison.Ordinal)) continue;
    foreach (var (name, ns) in TopLevelTypes(CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetRoot()))
        if (ns == OldRoot || ns.StartsWith(OldRoot + "."))
            (residualByNs.TryGetValue(ns, out var s) ? s : residualByNs[ns] = new()).Add(name);
}

// Pass C — rewrite references repo-wide.
int filesChanged = 0, usingsAdded = 0, usingsDropped = 0, nsDecls = 0;
var samples = new List<string>();

foreach (var f in CsFiles(Path.Combine(repo, "src"), Path.Combine(repo, "tests"), Path.Combine(repo, "build")))
{
    var original = File.ReadAllText(f);
    var cu = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(original).GetRoot();
    var isSource = f.StartsWith(sourceProj, StringComparison.Ordinal);

    // (1) Source-project files: rewrite their namespace declarations.
    if (isSource)
    {
        var before = nsDecls;
        cu = (CompilationUnitSyntax)new NsRewriter(IsMovable, MapNs, () => nsDecls++).Visit(cu);
        if (nsDecls > before) { /* counted */ }
    }

    // (1b) Rewrite fully-qualified references to moved types (e.g. Fallout.Common.Execution.Foo).
    cu = (CompilationUnitSyntax)new QualNameRewriter(IsMovable, MapNs, movedTypes).Visit(cu);

    // (2) All files: fix usings by type identity.
    //  - DROP decision uses a BROAD reference set (every identifier) so we only ever over-keep — safe.
    //  - ADD decision uses a TYPE-POSITION-filtered set so we don't add usings for method/property
    //    names that merely collide with a moved type's simple name.
    var referencedAll = cu.DescendantNodes().OfType<SimpleNameSyntax>().Select(n => n.Identifier.Text).ToHashSet();
    var referencedTypes = cu.DescendantNodes().OfType<SimpleNameSyntax>().Where(IsTypePosition).Select(n => n.Identifier.Text).ToHashSet();
    var fileNs = cu.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();

    var keep = new List<UsingDirectiveSyntax>();
    foreach (var u in cu.Usings)
    {
        var name = u.Name?.ToString();
        if (name != null && movedNamespaces.Contains(name))
        {
            var residualUsed = residualByNs.TryGetValue(name, out var set) && set.Overlaps(referencedAll);
            if (residualUsed) keep.Add(u); else usingsDropped++;   // drop usings now empty of residual types
        }
        else keep.Add(u);
    }

    var needed = referencedTypes.Where(movedTypes.ContainsKey).Select(n => movedTypes[n]).Distinct();
    foreach (var ns in needed)
    {
        if (ns == fileNs) continue;
        if (keep.Any(u => u.Name?.ToString() == ns)) continue;
        keep.Add(ParseCompilationUnit($"using {ns};\n").Usings[0]);
        usingsAdded++;
    }

    cu = cu.WithUsings(List(keep));
    var updated = cu.ToFullString();
    if (updated != original)
    {
        filesChanged++;
        if (samples.Count < 12) samples.Add(Path.GetRelativePath(repo, f));
        if (apply) File.WriteAllText(f, updated);
    }
}

Console.WriteLine($"OnionRewriter ({(apply ? "APPLY" : "dry-run")})");
Console.WriteLine($"  moved types        : {movedTypes.Count}");
Console.WriteLine($"  moved namespaces   : {string.Join(", ", movedNamespaces.OrderBy(x => x))}");
Console.WriteLine($"  residual namespaces: {string.Join(", ", residualByNs.Keys.OrderBy(x => x))}");
Console.WriteLine($"  files changed      : {filesChanged}");
Console.WriteLine($"  namespace decls    : {nsDecls}");
Console.WriteLine($"  usings added       : {usingsAdded}");
Console.WriteLine($"  usings dropped     : {usingsDropped}");
Console.WriteLine($"  sample files       :\n    {string.Join("\n    ", samples)}");

sealed class NsRewriter(Func<string, bool> isMovable, Func<string, string> map, Action onHit) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) => Rewrite(node, base.VisitNamespaceDeclaration(node));
    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) => Rewrite(node, base.VisitFileScopedNamespaceDeclaration(node));

    private SyntaxNode? Rewrite(BaseNamespaceDeclarationSyntax original, SyntaxNode? visited)
    {
        var name = original.Name.ToString();
        if (!isMovable(name) || visited is not BaseNamespaceDeclarationSyntax n) return visited;
        onHit();
        return n.WithName(SyntaxFactory.ParseName(map(name)).WithTriviaFrom(original.Name));
    }
}

// Rewrites fully-qualified references like `Fallout.Common.Execution.Foo` → `Fallout.Application.Execution.Foo`,
// but only when `Foo` is genuinely a moved type that lived in that namespace (guards against a same-named
// type from a different moved namespace).
sealed class QualNameRewriter(Func<string, bool> isMovable, Func<string, string> map, IReadOnlyDictionary<string, string> movedTypes) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
    {
        var visited = (QualifiedNameSyntax)base.VisitQualifiedName(node)!;
        var left = node.Left.ToString();
        var right = node.Right.Identifier.Text;
        if (isMovable(left) && movedTypes.TryGetValue(right, out var newNs) && newNs == map(left))
            return visited.WithLeft(SyntaxFactory.ParseName(map(left)).WithTriviaFrom(node.Left));
        return visited;
    }
}
