// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

#if NETSTANDARD
using System;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
#endif
