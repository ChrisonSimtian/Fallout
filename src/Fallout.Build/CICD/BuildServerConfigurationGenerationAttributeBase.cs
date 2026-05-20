// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fallout.Common.Execution;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.CI;

public class BuildServerConfigurationGenerationAttributeBase : BuildExtensionAttributeBase
{
    protected static IEnumerable<IConfigurationGenerator> GetGenerators(INukeBuild build)
    {
        return build.GetType().GetCustomAttributes<ConfigurationAttributeBase>()
            .ForEachLazy(x => x.Build = build);
    }
}
