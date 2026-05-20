// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Fallout.CodeGeneration.Model;

public interface IDeprecatable
{
    [CanBeNull]
    string DeprecationMessage { get; }

    [CanBeNull]
    IDeprecatable Parent { get; }
}

public static class DeprecatableExtensions
{
    [Pure]
    public static bool IsDeprecated(this IDeprecatable deprecatable)
    {
        return deprecatable.DeprecationMessage != null || deprecatable.Parent != null && deprecatable.Parent.IsDeprecated();
    }

    [Pure]
    [CanBeNull]
    public static string GetDeprecationMessage(this IDeprecatable deprecatable)
    {
        var message = deprecatable.DeprecationMessage;
        if (!string.IsNullOrEmpty(message))
            return message;
        return deprecatable.Parent?.GetDeprecationMessage();
    }
}
