// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Fallout.Common.Utilities;

[PublicAPI]
[DebuggerNonUserCode]
[DebuggerStepThrough]
public static class Lazy
{
    /// <summary>
    /// Creates a <see cref="Lazy{T}"/> from a delegate.
    /// </summary>
    public static Lazy<T> Create<T>(Func<T> provider)
    {
        return new Lazy<T>(provider);
    }
}
