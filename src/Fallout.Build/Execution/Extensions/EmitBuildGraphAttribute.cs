using System.Collections.Generic;
using Fallout.Common.CI;
using Fallout.Common.IO;
using static Fallout.Common.Constants;

namespace Fallout.Common.Execution;

/// <summary>
/// Writes the target graph to <c>.fallout/temp/build-graph.json</c> whenever the build is
/// started from its project, keeping it fresh for IDE tooling (analogous to how
/// <see cref="HandleShellCompletionAttribute"/> maintains <c>build.schema.json</c>).
/// </summary>
internal class EmitBuildGraphAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        if (BuildServerConfigurationGeneration.IsActive || Build.BuildProjectFile == null)
            return;

#pragma warning disable FALLOUT001 // framework-internal consumption of the experimental graph export
        GetBuildGraphFile(Build.RootDirectory).WriteAllText(BuildGraphUtility.GetJsonString(executableTargets));
#pragma warning restore FALLOUT001
    }
}
