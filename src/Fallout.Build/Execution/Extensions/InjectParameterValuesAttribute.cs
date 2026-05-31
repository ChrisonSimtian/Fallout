using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Application.Execution;
using Fallout.Application;

namespace Fallout.Application.ValueInjection;

internal class InjectParameterValuesAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        ValueInjectionUtility.InjectValues(Build, (_, attribute) => attribute.GetType() == typeof(ParameterAttribute));
    }
}
