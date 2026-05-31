using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fallout.Common.Utilities;
using Fallout.Application.Execution;
using Fallout.Application;

namespace Fallout.Application.ValueInjection;

internal class InjectNonParameterValuesAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        ValueInjectionUtility.InjectValues(Build, (member, attribute) =>
        {
            if (attribute.GetType() == typeof(ParameterAttribute))
                return false;

            if (!Build.GetType().HasCustomAttribute<OnDemandValueInjectionAttribute>() &&
                !member.HasCustomAttribute<OnDemandAttribute>())
                return true;

            if (member.HasCustomAttribute<RequiredAttribute>())
                return false;

            var requiredMembers = executionPlan.SelectMany(x => x.DelegateRequirements)
                .Where(x => x is not Expression<Func<bool>>)
                .Select(x => x.GetMemberInfo()).ToList();

            return requiredMembers.Contains(member);
        });
    }
}
