// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Fallout.Common;

namespace Fallout.GlobalTool.Rewriting.Cake;

internal class SafeSyntaxRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode Visit(SyntaxNode node)
    {
        try
        {
            return base.Visit(node);
        }
        catch (Exception)
        {
            Host.Warning($"Could not handle fragment '{node.ToFullString().Trim()}', skipping ...");
            return node;
        }
    }
}
