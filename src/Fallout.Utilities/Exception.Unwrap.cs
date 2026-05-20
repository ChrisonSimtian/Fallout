// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Fallout.Common.Utilities;

[PublicAPI]
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
