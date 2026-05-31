using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

static class DocumentRewriter
{
    public record struct Counts(bool Changed, int Refs, int Added, int Dropped, int NsDecls);

    // `targetNs(type)` returns the NEW namespace a moved type lands in, or null if the type does not move.
    // It subsumes the old (isMoved + mapNs) pair and — crucially — is per TYPE, not per namespace, so a
    // single source namespace can split across rings (e.g. Fallout.Common.Tooling vocab → Application,
    // executor → Infrastructure). `isMovable(ns)` stays namespace-level (drives residual/using-drop logic).
    public static (SyntaxNode Root, Counts Counts) Rewrite(
        SyntaxNode root, SemanticModel model, bool isSource,
        Func<string, bool> isMovable, Func<INamedTypeSymbol, string?> targetNs,
        HashSet<string> surviving)
    {
        var cu = (CompilationUnitSyntax)root;

        // --- Semantic pre-scan: which movable-namespace types are USED here, as types? ---
        var usedMovedNewNs = new HashSet<string>();        // new namespaces we must import
        var usedMovedOldNs = new HashSet<string>();        // old namespaces a MOVED type was used from
        var usedResidualMovableNs = new HashSet<string>(); // old movable namespaces still needed (residual types)
        var usedUnmappedNs = new HashSet<string>();        // namespaces of used types that do NOT move
        // descendIntoTrivia: also scan `<see cref="…"/>` names inside doc comments, so a type referenced
        // ONLY from a cref still gets its `using` added (else its doc link dangles → CS1574/CS1580).
        foreach (var name in cu.DescendantNodes(descendIntoTrivia: true).OfType<SimpleNameSyntax>())
        {
            // Type-position binds to a type symbol; an attribute/`new` binds to its constructor; an
            // extension call (x.NotNull()) binds to the reduced method — take the declaring type in each
            // case so attributes AND extension-method imports (often same-namespace siblings a moved file
            // loses) are counted.
            var sym = model.GetSymbolInfo(name).Symbol;
            var t = sym as INamedTypeSymbol
                ?? (sym is IMethodSymbol { MethodKind: MethodKind.Constructor } ctor ? ctor.ContainingType : null)
                ?? (sym is IMethodSymbol { IsExtensionMethod: true } ext ? (ext.ReducedFrom ?? ext).ContainingType : null);
            if (t is null) continue;
            var ns = t.OriginalDefinition.ContainingNamespace?.ToDisplayString() ?? "";
            var newNs = targetNs(t);
            if (newNs != null) { usedMovedNewNs.Add(newNs); usedMovedOldNs.Add(ns); }
            else { usedUnmappedNs.Add(ns); if (isMovable(ns)) usedResidualMovableNs.Add(ns); }
        }

        // Map a SOURCE namespace declaration to its target by the moved types it declares (first moved type
        // wins — a declaration block is single-ring in practice, since each type sits in its own file).
        // Returns null when the block declares no moved type (leave it as-is).
        string? MapDecl(BaseNamespaceDeclarationSyntax n)
        {
            foreach (var m in n.Members)
            {
                var sym = m switch
                {
                    BaseTypeDeclarationSyntax btd => model.GetDeclaredSymbol(btd) as INamedTypeSymbol,
                    DelegateDeclarationSyntax dd => model.GetDeclaredSymbol(dd) as INamedTypeSymbol,
                    _ => null,
                };
                if (sym is not null && targetNs(sym) is { } target) return target;
            }
            return null;
        }

        // The file's own namespaces (mapped) — never import those.
        var ownNs = cu.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>()
            .Select(n => (isSource ? MapDecl(n) : null) ?? n.Name.ToString())
            .ToHashSet();

        // A file in namespace `A.B.C` implicitly sees types in its ANCESTOR namespaces (`A.B`, `A`) with no
        // `using`. When a source file changes namespace (`Fallout.Common.Tooling` → `Fallout.Application.
        // Tooling`), it loses that implicit access to old ancestors that aren't also ancestors of the new
        // namespace (here `Fallout.Common` — which holds `EnvironmentInfo`, `NotNull`, `Assert`, …). Add an
        // explicit `using` for each lost ancestor it actually uses. (Generalises lesson #6 from movable
        // residual siblings to non-movable ancestor namespaces.)
        var lostAncestorNs = new HashSet<string>();
        if (isSource)
        {
            static IEnumerable<string> StrictAncestors(string ns)
            {
                for (var i = ns.LastIndexOf('.'); i > 0; i = ns.LastIndexOf('.', i - 1)) yield return ns[..i];
            }
            static bool IsAncestorOrSelf(string anc, string ns) => ns == anc || ns.StartsWith(anc + ".");
            foreach (var n in cu.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
            {
                var oldName = n.Name.ToString();
                if (MapDecl(n) is not { } newName || newName == oldName) continue;
                foreach (var anc in StrictAncestors(oldName))
                    if (!IsAncestorOrSelf(anc, newName) && usedUnmappedNs.Contains(anc)) lostAncestorNs.Add(anc);
            }
        }

        // --- Stage 1: rewrite namespace declarations (source files) + qualified refs to moved types. ---
        var rewriter = new SyntaxFixer(model, isSource, targetNs, MapDecl);
        cu = (CompilationUnitSyntax)rewriter.Visit(cu)!;

        // --- Stage 2: reconcile namespace `using` directives. ---
        var keep = new List<UsingDirectiveSyntax>();
        int dropped = 0;
        foreach (var u in cu.Usings)
        {
            var name = u.Name?.ToString();
            if (u.StaticKeyword.IsKind(SyntaxKind.None) && u.Alias is null && name != null && isMovable(name))
            {
                // Drop a movable `using` when its namespace is fully evacuated (no residual declarations
                // anywhere — would otherwise dangle), OR when a moved type was imported from here and no
                // residual type still is. Otherwise keep (residual still used, or a pre-existing unused
                // using of a surviving namespace — not our concern).
                var evacuated = !surviving.Contains(name);
                if (evacuated || (usedMovedOldNs.Contains(name) && !usedResidualMovableNs.Contains(name))) dropped++;
                else keep.Add(u);
            }
            else keep.Add(u);  // static/alias usings get their type names remapped in Stage 1
        }
        int added = 0;
        // Moved types → new-namespace usings. Plus, for SOURCE files that change namespace: residual
        // movable-namespace types they used to see as same-namespace siblings now need an explicit
        // `using` (e.g. ParameterService leaving Fallout.Common still uses Utilities' ArgumentParser).
        // Never re-import a namespace this move fully evacuates (not in `surviving`) — it would dangle.
        var toAdd = isSource ? usedMovedNewNs.Concat(usedResidualMovableNs.Where(surviving.Contains)).Concat(lostAncestorNs) : usedMovedNewNs;
        foreach (var ns in toAdd.Distinct())
        {
            if (ownNs.Contains(ns)) continue;
            if (keep.Any(u => u.StaticKeyword.IsKind(SyntaxKind.None) && u.Alias is null && u.Name?.ToString() == ns)) continue;
            keep.Add(ParseCompilationUnit($"using {ns};\n").Usings[0]);
            added++;
        }
        cu = cu.WithUsings(List(keep));

        var changed = added > 0 || dropped > 0 || rewriter.Refs > 0 || rewriter.NsDecls > 0;
        return (cu, new Counts(changed, rewriter.Refs, added, dropped, rewriter.NsDecls));
    }

    // visitIntoStructuredTrivia: also rewrite qualified names inside doc-comment crefs (the generated tool
    // wrappers embed fully-qualified `cref` parameter types like `Fallout.Common.Tools.X.XSettings`).
    sealed class SyntaxFixer(SemanticModel model, bool isSource, Func<INamedTypeSymbol, string?> targetNs, Func<BaseNamespaceDeclarationSyntax, string?> mapDecl)
        : CSharpSyntaxRewriter(visitIntoStructuredTrivia: true)
    {
        public int Refs, NsDecls;

        public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) => MapNamespace(node, base.VisitNamespaceDeclaration(node));
        public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) => MapNamespace(node, base.VisitFileScopedNamespaceDeclaration(node));

        SyntaxNode? MapNamespace(BaseNamespaceDeclarationSyntax original, SyntaxNode? visited)
        {
            if (!isSource || visited is not BaseNamespaceDeclarationSyntax n || mapDecl(original) is not { } target) return visited;
            NsDecls++;
            return n.WithName(ParseName(target).WithTriviaFrom(original.Name));
        }

        public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
        {
            var symbol = model.GetSymbolInfo(node).Symbol as INamedTypeSymbol;
            var visited = (QualifiedNameSyntax)base.VisitQualifiedName(node)!;
            if (symbol != null && targetNs(symbol) is { } newNs)
            {
                var ns = symbol.OriginalDefinition.ContainingNamespace!.ToDisplayString();
                // Only remap when Left is EXACTLY the namespace (direct `Namespace.Type`). For a nested
                // ref (`Namespace.Outer.Nested`), Left includes the outer type — leave it to the inner
                // QualifiedName visit, which remaps just the namespace and preserves the outer qualifier.
                if (node.Left.ToString() == ns)
                {
                    Refs++;
                    return visited.WithLeft(ParseName(newNs).WithTriviaFrom(visited.Left));
                }
            }
            return visited;
        }
    }
}
