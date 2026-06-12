using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fallout.Application.Execution;
using Fallout.Application;
using Fallout.Core.Collections;

namespace Fallout.Application.CI;

public class BuildServerConfigurationGenerationAttributeBase : BuildExtensionAttributeBase
{
    protected static IEnumerable<IConfigurationGenerator> GetGenerators(IFalloutBuild build)
    {
        return build.GetType().GetCustomAttributes<ConfigurationAttributeBase>()
            .ForEachLazy(x => x.Build = build);
    }
}
