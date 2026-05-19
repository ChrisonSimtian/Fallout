// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nuke.Common.Utilities.Collections;

public static partial class EnumerableExtensions
{
    /// <summary>
    /// Create an enumerable with the object as single element.
    /// </summary>
    public static IEnumerable<T> AsEnumerable<T>(this object obj)
    {
        return obj is IEnumerable enumerable ? enumerable.Cast<T>() : new[] { (T) obj };
    }
}
