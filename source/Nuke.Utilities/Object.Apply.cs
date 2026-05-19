// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Nuke.Common.Utilities;

partial class ObjectExtensions
{
    public static TOutput Apply<TInput, TOutput>(this TInput input, Func<TInput, TOutput> transform)
    {
        return transform.Invoke(input);
    }
}
