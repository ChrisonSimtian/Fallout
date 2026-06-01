#if NETSTANDARD2_0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fallout.Kernel.Collections;

public static partial class DictionaryExtensions
{
    public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        return new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}

#endif
