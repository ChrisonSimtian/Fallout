using System;
using System.Diagnostics;
using System.Reflection;

namespace Fallout.Common.Utilities;

[DebuggerNonUserCode]
[DebuggerStepThrough]
public static class ExceptionExtensions
{
    /// <summary>
    /// Unwraps inner exceptions from <see cref="TypeInitializationException"/>, <see cref="TargetInvocationException"/>, and
    /// <see cref="AggregateException"/>.
    /// </summary>
    public static Exception Unwrap(this Exception exception)
    {
        return exception switch
        {
            TypeInitializationException typeInitializationException => typeInitializationException.InnerException.Unwrap(),
            TargetInvocationException targetInvocationException => targetInvocationException.InnerException.Unwrap(),
            AggregateException aggregateException => aggregateException.Flatten(),
            _ => exception
        };
    }
}
