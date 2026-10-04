using System;
using System.Linq.Expressions;
using System.Reflection;
using Fallout.Common.Execution;
using Fallout.Common.Utilities;

namespace Fallout.Common;

internal partial class ParameterService
{
    /// <summary>Creates a service that reads the process command-line arguments and environment variables.</summary>
    internal static ParameterService CreateDefault()
        => new(() => EnvironmentInfo.ArgumentParser, () => EnvironmentInfo.Variables);

    // Used when no build run is active. Every caller outside a run shares this one service.
    private static readonly Lazy<ParameterService> ambientInstance = new(CreateDefault);

    /// <summary>
    /// The service for the current build run (<see cref="BuildContext.Parameters"/>). Outside a run,
    /// the shared service that <see cref="ambientInstance"/> holds.
    /// </summary>
    internal static ParameterService Instance => BuildContext.Current?.Parameters ?? ambientInstance.Value;

    public static T GetParameter<T>(string name, char? separator = null)
    {
        return (T)Instance.GetParameter(name, typeof(T), separator);
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
        return (T)GetFromMemberInfo(member, destinationType ?? typeof(T), Instance.GetParameter);
    }

    public static T GetNamedArgument<T>(string parameterName, char? separator = null)
    {
        return (T)Instance.GetCommandLineArgument(parameterName, typeof(T), separator);
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
        return (T)GetFromMemberInfo(member, destinationType ?? typeof(T), Instance.GetCommandLineArgument);
    }

    public static T GetPositionalArgument<T>(int position, char? separator = null)
    {
        return (T)Instance.GetCommandLineArgument(position, typeof(T), separator);
    }

    public static T[] GetAllPositionalArguments<T>(char? separator = null)
    {
        return (T[])Instance.GetPositionalCommandLineArguments(typeof(T), separator);
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
        return (T)GetFromMemberInfo(member, destinationType ?? typeof(T), Instance.GetEnvironmentVariable);
    }

    public static T GetVariable<T>(string parameterName, char? separator = null)
    {
        return (T)Instance.GetEnvironmentVariable(parameterName, typeof(T), separator);
    }

    public static bool HasArgument(MemberInfo member)
    {
        return Instance.HasCommandLineArgument(GetParameterMemberName(member));
    }
}
