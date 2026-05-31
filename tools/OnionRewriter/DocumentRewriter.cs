using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

static class DocumentRewriter
{
    public record struct Counts(bool Changed, int Refs, int Added, int Dropped, int NsDecls);

    public static (SyntaxNode Root, Counts Counts) Rewrite(
        SyntaxNode root, SemanticModel model, bool isSource,
        Func<string, bool> isMovable, Func<string, string> mapNs, Func<INamedTypeSymbol, bool> isMoved,
        HashSet<string> surviving)
    {
        var cu = (CompilationUnitSyntax)root;

        // --- Semantic pre-scan: which movable-namespace types are USED here, as types? ---
        var usedMovedNewNs = new HashSet<string>();        // new namespaces we must import
        var usedMovedOldNs = new HashSet<string>();        // old namespaces a MOVED type was used from
        var usedResidualMovableNs = new HashSet<string>(); // old movable namespaces still needed (residual types)
        foreach (var name in cu.DescendantNodes().OfType<SimpleNameSyntax>())
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
            if (!isMovable(ns)) continue;
            if (isMoved(t)) { usedMovedNewNs.Add(mapNs(ns)); usedMovedOldNs.Add(ns); }
            else usedResidualMovableNs.Add(ns);
        }

        // The file's own namespaces (mapped) — never import those.
        var ownNs = cu.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>()
            .Select(n => n.Name.ToString())
            .Select(n => isSource && isMovable(n) ? mapNs(n) : n)
            .ToHashSet();

        // --- Stage 1: rewrite namespace declarations (source files) + qualified refs to moved types. ---
        var rewriter = new SyntaxFixer(model, isSource, isMovable, mapNs, isMoved);
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
        var toAdd = isSource ? usedMovedNewNs.Concat(usedResidualMovableNs) : usedMovedNewNs;
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

    sealed class SyntaxFixer(SemanticModel model, bool isSource, Func<string, bool> isMovable, Func<string, string> mapNs, Func<INamedTypeSymbol, bool> isMoved)
        : CSharpSyntaxRewriter
    {
        public int Refs, NsDecls;

        public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) => MapNamespace(node, base.VisitNamespaceDeclaration(node));
        public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) => MapNamespace(node, base.VisitFileScopedNamespaceDeclaration(node));

        SyntaxNode? MapNamespace(BaseNamespaceDeclarationSyntax original, SyntaxNode? visited)
        {
            var name = original.Name.ToString();
            if (!isSource || !isMovable(name) || visited is not BaseNamespaceDeclarationSyntax n) return visited;
            NsDecls++;
            return n.WithName(ParseName(mapNs(name)).WithTriviaFrom(original.Name));
        }

        public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
        {
            var symbol = model.GetSymbolInfo(node).Symbol as INamedTypeSymbol;
            var visited = (QualifiedNameSyntax)base.VisitQualifiedName(node)!;
            if (symbol != null && isMoved(symbol))
            {
                var ns = symbol.OriginalDefinition.ContainingNamespace!.ToDisplayString();
                // Only remap when Left is EXACTLY the namespace (direct `Namespace.Type`). For a nested
                // ref (`Namespace.Outer.Nested`), Left includes the outer type — leave it to the inner
                // QualifiedName visit, which remaps just the namespace and preserves the outer qualifier.
                if (node.Left.ToString() == ns)
                {
                    Refs++;
                    return visited.WithLeft(ParseName(mapNs(ns)).WithTriviaFrom(visited.Left));
                }
            }
            return visited;
        }
    }
}
