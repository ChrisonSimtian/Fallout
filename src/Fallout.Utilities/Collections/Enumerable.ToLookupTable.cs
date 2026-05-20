// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;

namespace Fallout.Common.Utilities.Collections;

public static partial class EnumerableExtensions
{
    public static LookupTable<TKey, TValue> ToLookupTable<TItem, TKey, TValue>(
        this IEnumerable<TItem> enumerable,
        Func<TItem, TKey> keySelector,
        Func<TItem, TValue> valueSelector)
    {
        return new LookupTable<TKey, TValue>(enumerable.ToLookup(keySelector, valueSelector));
    }
}
