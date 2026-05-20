// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Fallout.Common.Utilities.Collections;

[PublicAPI]
public static class LookupExtensions
{
    public static LookupTable<TKey, TValue> ToLookupTable<TKey, TValue>(this ILookup<TKey, TValue> lookup, IEqualityComparer<TKey> comparer)
    {
        return new LookupTable<TKey, TValue>(lookup, comparer);
    }
}
