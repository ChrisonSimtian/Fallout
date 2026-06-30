using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Execution;

namespace Fallout.Common.ValueInjection;

internal class InjectParameterValuesAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    public void OnBuildCreated(IReadOnlyCollection<ITargetModel> executableTargets)
    {
        ValueInjectionUtility.InjectValues(Build, (_, attribute) => attribute.GetType() == typeof(ParameterAttribute));
    }
}
