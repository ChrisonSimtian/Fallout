// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using static Nuke.Common.IO.PathConstruction;

namespace Nuke.Common.IO;

/// <summary>
/// Represents a relative path with the Windows separator (backward slash).
/// </summary>
[PublicAPI]
[Serializable]
public class WinRelativePath : RelativePath
{
    protected WinRelativePath(string path, char? separator)
        : base(path, separator)
    {
    }

    public static explicit operator WinRelativePath([CanBeNull] string path)
    {
        return new WinRelativePath(NormalizePath(path, WinSeparator), WinSeparator);
    }
}
