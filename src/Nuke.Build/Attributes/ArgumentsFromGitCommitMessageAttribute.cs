// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;

namespace Nuke.Common.Execution;

[PublicAPI]
public class ArgumentsFromGitCommitMessageAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    public string Prefix { get; set; } = "[nuke++]";

    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        if (BuildServerConfigurationGeneration.IsActive)
            return;

        var commit = GitRepository.GetCommitFromCI();
        if (commit == null)
            return;

        var git = ToolResolver.GetEnvironmentOrPathTool("git");
        var lastLine = git.Invoke($"show -s --format=%B {commit}", logInvocation: false, logOutput: false)
            .Select(x => x.Text)
            .LastOrDefault(x => !x.IsNullOrEmpty());
        if (!lastLine?.StartsWithOrdinalIgnoreCase(Prefix) ?? true)
            return;

        var arguments = lastLine[Prefix.Length..].TrimStart();
        ParameterService.Instance.ArgumentsFromCommitMessageService = new ArgumentParser(arguments);
    }
}
