using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.CI.GitHubActions.Configuration;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI.Forgejo;

/// <summary>
/// Generates a Forgejo Actions workflow at <c>.forgejo/workflows/{name}.yml</c>. Forgejo Actions speaks
/// the GitHub Actions workflow dialect, so this <strong>composes the GitHub Actions configuration
/// model</strong> (<see cref="GitHubActionsConfiguration"/> + jobs/steps) rather than inheriting
/// <see cref="GitHubActionsAttribute"/> — keeping the two adapters decoupled (ADR-0005). Inheriting it
/// is also blocked by design: <see cref="ConfigurationAttributeBase.Build"/> has an <c>internal set</c>,
/// so an attribute outside Fallout.Build cannot wire up a composed inner attribute. Reusing the shared
/// model side-steps that.
/// <para/>
/// First-cut scaffold: covers the common case (short triggers + invoked targets + image). Fuller parity
/// with <see cref="GitHubActionsAttribute"/> (caching, artifacts, secrets, detailed triggers) is tracked
/// in <c>docs/spikes/0001-ci-ports-and-adapters.md</c>; the clean way to get there is to extract the GH
/// generator's model-building from its file placement so both adapters compose one builder.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ForgejoAttribute : ConfigurationAttributeBase
{
    private readonly string _name;
    private readonly GitHubActionsImage[] _images;

    public ForgejoAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images)
    {
        _name = name.Replace(oldChar: ' ', newChar: '_');
        _images = new[] { image }.Concat(images).ToArray();
    }

    public override string IdPostfix => _name;
    public override Type HostType => typeof(Forgejo);
    public override AbsolutePath ConfigurationFile => Build.RootDirectory / ".forgejo" / "workflows" / $"{_name}.yml";
    public override IEnumerable<AbsolutePath> GeneratedFiles => new[] { ConfigurationFile };

    public override IEnumerable<string> RelevantTargetNames => InvokedTargets;
    public override IEnumerable<string> IrrelevantTargetNames => new string[0];

    public GitHubActionsTrigger[] On { get; set; } = new GitHubActionsTrigger[0];
    public string[] InvokedTargets { get; set; } = new string[0];

    public override CustomFileWriter CreateWriter(StreamWriter streamWriter)
    {
        return new CustomFileWriter(streamWriter, indentationFactor: 2, commentPrefix: "#");
    }

    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        Assert.True(On.Length > 0, $"Forgejo workflow must define at least one '{nameof(On)}' trigger");

        return new GitHubActionsConfiguration
               {
                   Name = _name,
                   ShortTriggers = On,
                   DetailedTriggers = new GitHubActionsDetailedTrigger[0],
                   Permissions = new (GitHubActionsPermissions, string)[0],
                   Jobs = _images.Select(image => new GitHubActionsJob
                                                  {
                                                      Name = image.GetValue().Replace(".", "_"),
                                                      Image = image,
                                                      Steps = new GitHubActionsStep[]
                                                              {
                                                                  new GitHubActionsCheckoutStep(),
                                                                  new GitHubActionsRunStep
                                                                  {
                                                                      InvokedTargets = InvokedTargets,
                                                                      Imports = new Dictionary<string, string>()
                                                                  }
                                                              }
                                                  }).ToArray()
               };
    }
}
