using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Fallout.Common.Execution;
using Fallout.Common.Utilities;

namespace Fallout.Common;

internal partial class ParameterService
{
    // FT-4 (#309): the active instance is the per-run one held on BuildContext, so prod uses the
    // same instance form as tests and nothing leaks across runs. The lazy fallback covers the rare
    // access outside a build run (no cross-run concern there); it can retire once that's confirmed dead.
    private static readonly Lazy<ParameterService> s_ambient = new(
        () => new ParameterService(() => EnvironmentInfo.ArgumentParser, () => EnvironmentInfo.Variables));

    internal static ParameterService Instance => BuildContext.Current?.Parameters ?? s_ambient.Value;

    public static T GetParameter<T>(string name, char? separator = null)
    {
        return (T) Instance.GetParameter(name, typeof(T), separator);
    }

    public static T GetParameter<T>(Expression<Func<T>> expression)
    {
        return GetParameter<T>(expression.GetMemberInfo());
    }

    public static T GetParameter<T>(Expression<Func<object>> expression)
    {
        return GetParameter<T>(expression.GetMemberInfo());
    }

    public static T GetParameter<T>(MemberInfo member, Type destinationType = null)
    {
        return (T) GetFromMemberInfo(member, destinationType ?? typeof(T), Instance.GetParameter);
    }

    public static T GetNamedArgument<T>(string parameterName, char? separator = null)
    {
        return (T) Instance.GetCommandLineArgument(parameterName, typeof(T), separator);
    }

    public static T GetNamedArgument<T>(Expression<Func<T>> expression)
    {
        return GetNamedArgument<T>(expression.GetMemberInfo());
    }

    public static T GetNamedArgument<T>(Expression<Func<object>> expression)
    {
        return GetNamedArgument<T>(expression.GetMemberInfo());
    }

    public static T GetNamedArgument<T>(MemberInfo member, Type destinationType = null)
    {
        return (T) GetFromMemberInfo(member, destinationType ?? typeof(T), Instance.GetCommandLineArgument);
    }

    public static T GetPositionalArgument<T>(int position, char? separator = null)
    {
        return (T) Instance.GetCommandLineArgument(position, typeof(T), separator);
    }

    public static T[] GetAllPositionalArguments<T>(char? separator = null)
    {
        return (T[]) Instance.GetPositionalCommandLineArguments(typeof(T), separator);
    }

    public static T GetVariable<T>(Expression<Func<T>> expression)
    {
        return GetVariable<T>(expression.GetMemberInfo());
    }

    public static T GetVariable<T>(Expression<Func<object>> expression)
    {
        return GetVariable<T>(expression.GetMemberInfo());
    }

    public static T GetVariable<T>(MemberInfo member, Type destinationType = null)
    {
        return (T) GetFromMemberInfo(member, destinationType ?? typeof(T), Instance.GetEnvironmentVariable);
    }

    public static T GetVariable<T>(string parameterName, char? separator = null)
    {
        return (T) Instance.GetEnvironmentVariable(parameterName, typeof(T), separator);
    }

    public static bool HasArgument(MemberInfo member)
    {
        return Instance.HasCommandLineArgument(GetParameterMemberName(member));
    }
}
