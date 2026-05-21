// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using static Fallout.Common.IO.PathConstruction;

namespace Fallout.Common.IO;

/// <summary>
/// Represents a relative path with the UNIX separator (forward slash).
/// </summary>
[Serializable]
public class UnixRelativePath : RelativePath
{
    protected UnixRelativePath(string path, char? separator)
        : base(path, separator)
    {
    }

    public static explicit operator UnixRelativePath(string path)
    {
        return new UnixRelativePath(NormalizePath(path, UnixSeparator), UnixSeparator);
    }
}
