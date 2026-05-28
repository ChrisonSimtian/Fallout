using System;
using System.Linq;

namespace Fallout.Common.Tooling;

partial class OptionsExtensions
{
    public static T When<T>(this T options, Func<T, bool> condition, Configure<T> configurator)
        where T : Options, new()
    {
        return condition.Invoke(options) ? options.Apply(configurator) : options;
    }

    public static T[] When<T>(this T[] options, Func<T, bool> condition, Configure<T> configurator)
        where T : Options, new()
    {
        return options.Select(x => condition(x) ? x.Apply(configurator) : x).ToArray();
    }
}
