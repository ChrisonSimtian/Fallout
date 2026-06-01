using System;
using System.Collections.Generic;
using System.Linq;
using static Fallout.Application.CI.BuildServerConfigurationGeneration;
using Fallout.Application.Execution;
using Fallout.Application;
using Fallout.Kernel.Collections;
using Fallout.Kernel;

namespace Fallout.Application.CI;

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
