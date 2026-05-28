using System.Collections.Generic;

namespace Fallout.Common.Utilities.Collections;

public static partial class EnumerableExtensions
{
    public static IEnumerable<T> ToEmptyIfNull<T>(this IEnumerable<T> enumerable)
    {
        return enumerable ?? [];
    }

    public static T[] ToEmptyIfNull<T>(this T[] array)
    {
        return array ?? [];
    }

    public static IList<T> ToEmptyIfNull<T>(this IList<T> list)
    {
        return list ?? [];
    }
}
