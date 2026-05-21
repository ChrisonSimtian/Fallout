// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using static Fallout.Common.IO.PathConstruction;

namespace Fallout.Common.IO;

/// <summary>
/// Represents a relative path with the Windows separator (backward slash).
/// </summary>
[Serializable]
public class WinRelativePath : RelativePath
{
    protected WinRelativePath(string path, char? separator)
        : base(path, separator)
    {
    }

    public static explicit operator WinRelativePath(string path)
    {
        return new WinRelativePath(NormalizePath(path, WinSeparator), WinSeparator);
    }
}
