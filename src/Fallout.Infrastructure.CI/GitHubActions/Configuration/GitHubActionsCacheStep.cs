using System;
using System.Linq;
using Fallout.Application.Utilities;
using Fallout.Kernel;
using Fallout.Kernel.Collections;

namespace Fallout.Infrastructure.CI.GitHubActions.Configuration;

// https://github.com/actions/cache
public class GitHubActionsCacheStep : GitHubActionsStep
{
    public string[] IncludePatterns { get; set; }
    public string[] ExcludePatterns { get; set; }
    public string[] KeyFiles { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: " + $"Cache: {IncludePatterns.JoinCommaSpace()}".SingleQuote());
        using (writer.Indent())
        {
            writer.WriteLine("uses: actions/cache@v4");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                writer.WriteLine("path: |");
                IncludePatterns.ForEach(x => writer.WriteLine($"  {x}"));
                ExcludePatterns.ForEach(x => writer.WriteLine($"  !{x}"));
                writer.WriteLine($"key: ${{{{ runner.os }}}}-${{{{ hashFiles({KeyFiles.Select(x => x.SingleQuote()).JoinCommaSpace()}) }}}}");
            }
        }
    }
}
