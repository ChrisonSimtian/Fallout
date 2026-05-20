// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Execution;

namespace Fallout.Common.ValueInjection;

internal class InjectParameterValuesAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        ValueInjectionUtility.InjectValues(Build, (_, attribute) => attribute.GetType() == typeof(ParameterAttribute));
    }
}
