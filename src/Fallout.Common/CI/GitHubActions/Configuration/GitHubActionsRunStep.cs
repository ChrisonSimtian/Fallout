using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.GitHubActions.Configuration;

public class GitHubActionsRunStep : GitHubActionsStep
{
    /// <summary>
    /// The SDK-setup action to reference — this step emits the setup, the tool restore, and the build run,
    /// so only the first of the three is configurable. Accepts a complete <c>owner/repo@ref</c> or a bare
    /// ref that gets appended to <c>actions/setup-dotnet</c>. Defaults to the version the generator pins;
    /// setting null or whitespace restores it.
    /// </summary>
    public string SetupDotNetAction
    {
        get;
        set => field = GitHubActionsActionReference.Resolve(
            GitHubActionsDefaults.SetupDotNetAction, value, $"{nameof(GitHubActionsRunStep)}.{nameof(SetupDotNetAction)}");
    } = GitHubActionsDefaults.SetupDotNetAction;

    /// <summary>
    /// SDK versions to install, emitted as <c>dotnet-version</c> in place of <c>global-json-file</c>. One value
    /// is written inline, several as a <c>|</c> block. Null, empty, or only blank entries keep
    /// <c>global-json-file: global.json</c>.
    /// </summary>
    public string[] SetupDotNetVersions { get; set; }

    public string[] InvokedTargets { get; set; }

    public Dictionary<string, string> Imports { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: 'Setup: .NET SDK'");
        using (writer.Indent())
        {
            writer.WriteLine($"uses: {SetupDotNetAction}");
            writer.WriteLine("with:");
            using (writer.Indent())
            {
                var versions = (SetupDotNetVersions ?? [])
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToArray();

                if (versions.Length == 0)
                    writer.WriteLine("global-json-file: global.json");
                else if (versions.Length == 1)
                    writer.WriteLine($"dotnet-version: {versions[0]}");
                else
                {
                    writer.WriteLine("dotnet-version: |");
                    using (writer.Indent())
                        versions.ForEach(x => writer.WriteLine(x));
                }
            }
        }

        writer.WriteLine("- name: 'Restore: dotnet tools'");
        using (writer.Indent())
        {
            writer.WriteLine("run: dotnet tool restore");
        }

        writer.WriteLine("- name: " + $"Run: {InvokedTargets.JoinCommaSpace()}".SingleQuoteYaml());
        using (writer.Indent())
        {
            writer.WriteLine($"run: dotnet fallout {InvokedTargets.JoinSpace()}");

            if (Imports.Count > 0)
            {
                writer.WriteLine("env:");
                using (writer.Indent())
                {
                    Imports.ForEach(x => writer.WriteLine($"{x.Key}: {x.Value}"));
                }
            }
        }
    }
}
