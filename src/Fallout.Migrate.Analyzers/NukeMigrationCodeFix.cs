using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Fallout.Migration.Shared;

namespace Fallout.Migrate.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NukeMigrationCodeFix))]
public sealed class NukeMigrationCodeFix : CodeFixProvider
{
    private const string Title = "Migrate to Fallout.*";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(DiagnosticIds.NukeNamespaceMigration);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            if (node is null)
                continue;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: ct => ApplyFixAsync(context.Document, node, ct),
                    equivalenceKey: Title),
                diagnostic);
        }
    }

    private static async Task<Document> ApplyFixAsync(Document document, SyntaxNode offendingNode, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        var rewriter = new NukeToFalloutRewriter();
        var rewritten = rewriter.Visit(offendingNode);
        var newRoot = root.ReplaceNode(offendingNode, rewritten);
        return document.WithSyntaxRoot(newRoot);
    }

    private sealed class NukeToFalloutRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
        {
            // Map the whole dotted name by longest Nuke prefix (e.g. Nuke.Common.Tools.DotNet →
            // Fallout.Application.Tools.DotNet), via the shared canonical map. Replacing the bare `Nuke`
            // token alone would yield the dead `Fallout.Common.*` namespace (post-onion).
            var mapped = MapNamespacePrefix(node.ToString());
            return mapped is null
                ? base.VisitQualifiedName(node)
                : SyntaxFactory.ParseName(mapped).WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            var replacement = node.Identifier.ValueText switch
            {
                "NukeBuild" => "FalloutBuild",
                "INukeBuild" => "IFalloutBuild",
                "Nuke" => "Fallout", // lone Nuke identifier (not part of a qualified name)
                _ => null,
            };

            if (replacement is null)
                return base.VisitIdentifierName(node);

            return node.WithIdentifier(SyntaxFactory.Identifier(
                node.Identifier.LeadingTrivia,
                replacement,
                node.Identifier.TrailingTrivia));
        }

        /// <summary>Applies the canonical Nuke→Fallout prefix map (longest first); null if no prefix matches.</summary>
        private static string? MapNamespacePrefix(string dottedName)
        {
            foreach (var pair in NukeNamespaceMap.MigrationPairsLongestFirst)
            {
                if (dottedName == pair.Key)
                    return pair.Value;
                if (dottedName.StartsWith(pair.Key + ".", System.StringComparison.Ordinal))
                    return pair.Value + dottedName.Substring(pair.Key.Length);
            }
            return null;
        }
    }
}
