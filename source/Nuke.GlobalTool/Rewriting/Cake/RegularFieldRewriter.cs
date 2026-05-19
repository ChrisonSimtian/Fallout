// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nuke.Common;

namespace Nuke.GlobalTool.Rewriting.Cake;

internal class RegularFieldRewriter : SafeSyntaxRewriter
{
    public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        node = (FieldDeclarationSyntax) base.VisitFieldDeclaration(node).NotNull();
        var initializerValue = node.GetSingleDeclarator().Initializer?.Value;
        if (initializerValue != null)
            node = node.WithDeclaration(node.Declaration
                .WithType(initializerValue.GetExpressionType()));

        return node;
    }
}
