// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System.Collections.Generic;
using System.Linq;

namespace Nuke.Common.Utilities.Collections;

public static partial class EnumerableExtensions
{
    public static bool SequenceStartsWith<T>(this IEnumerable<T> enumerable, IEnumerable<T> other, IEqualityComparer<T> comparer = null)
    {
        var enumerableList = enumerable as List<T> ?? enumerable.ToList();
        var otherList = other as List<T> ?? other.ToList();
        return enumerableList.Take(otherList.Count).SequenceEqual(otherList, comparer);
    }
}
