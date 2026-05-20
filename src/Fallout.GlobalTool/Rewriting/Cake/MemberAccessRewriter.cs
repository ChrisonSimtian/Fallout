// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Fallout.Common;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Fallout.GlobalTool.Rewriting.Cake;

internal class MemberAccessRewriter : SafeSyntaxRewriter
{
    private Dictionary<string, string> Replacements =>
        new()
        {
            ["BuildSystem"] = null,
        };

    public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        node = (MemberAccessExpressionSyntax) base.VisitMemberAccessExpression(node).NotNull();
        return node.Expression is not IdentifierNameSyntax identifierNameSyntax ||
               !Replacements.TryGetValue(identifierNameSyntax.Identifier.Text, out var newName)
            ? node
            : newName != null
                ? node.WithExpression(identifierNameSyntax.WithIdentifier(Identifier(newName)))
                : node.Name;
    }
}
