// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Execution;
using Fallout.Common.Utilities.Collections;
using static Fallout.Common.CI.BuildServerConfigurationGeneration;

namespace Fallout.Common.CI;

public class GenerateBuildServerConfigurationsAttribute
    : BuildServerConfigurationGenerationAttributeBase, IOnBuildCreated
{
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        var configurationId = ParameterService.GetParameter<string>(ConfigurationParameterName);
        if (configurationId == null)
            return;

        Assert.NotNull(Build.RootDirectory);

        var generator = GetGenerators(Build)
            .Where(x => x.Id == configurationId)
            .SingleOrDefaultOrError($"Found multiple {nameof(IConfigurationGenerator)} with same ID '{configurationId}'.")
            .NotNull("generator != null");

        generator.Generate(executableTargets);

        Environment.Exit(0);
    }
}
