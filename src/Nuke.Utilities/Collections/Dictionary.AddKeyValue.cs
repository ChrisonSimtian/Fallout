// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Nuke.Common.Utilities.Collections;

public static partial class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> AddPair<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        [CanBeNull] TValue value = default)
    {
        dictionary.Add(key, value);
        return dictionary;
    }

    public static Dictionary<TKey, string> AddPair<TKey, TValue>(
        this Dictionary<TKey, string> dictionary,
        TKey key,
        [CanBeNull] TValue value = default)
    {
        dictionary.Add(key, value.ToString());
        return dictionary;
    }

    public static Dictionary<TKey, TValue> AddPairWhenKeyNotNull<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        [CanBeNull] TKey key,
        [CanBeNull] TValue value = default)
        where TKey : class
    {
        return key != null
            ? dictionary.AddPair(key, value)
            : dictionary;
    }

    public static Dictionary<TKey, TValue> AddPairWhenValueNotNull<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        [CanBeNull] TKey key,
        [CanBeNull] TValue value)
        where TValue : class
    {
        return value != null
            ? dictionary.AddPair(key, value)
            : dictionary;
    }
}
