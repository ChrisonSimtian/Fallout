// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;

namespace Fallout.Common.Utilities.Collections;

partial class EnumerableExtensions
{
    private static readonly Random s_randomNumberGenerator = new Random();

    public static T Random<T>(this IEnumerable<T> collection)
    {
        var array = collection.ToArray();
        return array[s_randomNumberGenerator.Next(array.Length)];
    }

    public static ICollection<T> Randomize<T>(this ICollection<T> collection)
    {
        var list = collection.ToList();
        var count = list.Count;
        while (count > 1) {
            count--;
            var k = s_randomNumberGenerator.Next(count + 1);
            (list[k], list[count]) = (list[count], list[k]);
        }

        return list;
    }
}
