// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Nuke.Common.IO;

/// <summary>
/// Indicates the case sensitivity used for globbing.
/// </summary>
public enum GlobbingCaseSensitivity
{
    /// <summary>
    /// Automatically determines whether to use case-sensitive or case-insensitive matching when globbing. This
    /// means using case-insensitive matching when running on Windows, and case-sensitive otherwise.
    /// </summary>
    Auto,

    /// <summary>
    /// Globbing patterns will be case-sensitive.
    /// </summary>
    CaseSensitive,

    /// <summary>
    /// Globbing patterns will be case-insensitive.
    /// </summary>
    CaseInsensitive
}
