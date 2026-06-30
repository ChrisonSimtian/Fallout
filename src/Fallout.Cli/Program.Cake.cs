using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Fallout.Common;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using Fallout.Cli.Rewriting.Cake;
using static Fallout.Common.Constants;
using static Fallout.Common.EnvironmentInfo;

namespace Fallout.Cli;

partial class Program
{
    public const string CAKE_FILE_PATTERN = "*.cake";

    // Residual after the :cake-convert/:cake-clean commands moved to CakeConvertCommand/
    // CakeCleanCommand: these .cake syntax-rewriting helpers stay on Program until the #392 collapse
    // PR (GetCakeConvertedContent/GetCakePackages are exercised directly by tests).
    internal static IEnumerable<AbsolutePath> GetCakeFiles()
    {
        return (TryGetRootDirectoryFrom(WorkingDirectory) ?? WorkingDirectory).GlobFiles($"**/{CAKE_FILE_PATTERN}");
    }

    internal static string GetCakeConvertedContent(string content)
    {
        var options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None, SourceCodeKind.Script);
        var syntaxTree = CSharpSyntaxTree.ParseText(content, options);
        return new CSharpSyntaxRewriter[]
               {
                   new RemoveUsingDirectivesRewriter(),
                   new RenameFieldIdentifierRewriter(),
                   new ParameterRewriter(),
                   new AbsolutePathRewriter(),
                   new RegularFieldRewriter(),
                   new TargetDefinitionRewriter(),
                   new InvocationRewriter(),
                   new MemberAccessRewriter(),
                   new IdentifierNameRewriter(),
                   new ToolInvocationRewriter(),
                   new ClassRewriter(),
                   new FormattingRewriter()
               }.Aggregate(syntaxTree.GetRoot(), (root, rewriter) => rewriter.Visit(root.NormalizeWhitespace(elasticTrivia: true)))
            .ToFullString();
    }

    internal static IEnumerable<(string Type, string Id, string Version)> GetCakePackages(string content)
    {
        IEnumerable<(string Type, string Id, string Version)> GetPackages(
            string packageType,
            string regexPattern)
        {
            var regex = new Regex(regexPattern);
            foreach (Match match in regex.Matches(content))
            {
                var packageId = match.Groups["packageId"].Value;
                var packageVersion = match.Groups["version"].Value;
                if (packageVersion.IsNullOrEmpty())
                    packageVersion = AsyncHelper.RunSync(() => NuGetVersionResolver.GetLatestVersion(packageId, includePrereleases: false));
                yield return new(packageType, packageId, packageVersion);
            }
        }

        return GetPackages(PACKAGE_TYPE_DOWNLOAD, @"#tool ""nuget:\?package=(?'packageId'[\w\d\.]+)(&version=(?'version'[\w\d\.]+))?S*""")
            .Concat(GetPackages(PACKAGE_TYPE_REFERENCE, @"#addin ""nuget:\?package=(?'packageId'[\w\d\.]+)(&version=(?'version'[\w\d\.]+))?S*"""))
            .Where(x => !x.Id.ContainsOrdinalIgnoreCase("Cake"));
    }
}
