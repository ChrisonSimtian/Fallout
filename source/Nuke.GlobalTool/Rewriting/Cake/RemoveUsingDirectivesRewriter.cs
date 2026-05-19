// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nuke.GlobalTool.Rewriting.Cake;

internal class RemoveUsingDirectivesRewriter : SafeSyntaxRewriter
{
    public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
    {
        return null;
    }
}
