// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using static Fallout.Common.IO.PathConstruction;

namespace Fallout.Common.IO;

/// <summary>
/// Represents a relative path with the separator of the current operating system.
/// </summary>
[PublicAPI]
[Serializable]
[DebuggerDisplay("{" + nameof(_path) + "}")]
public class RelativePath
{
    private readonly string _path;
    private readonly char? _separator;

    protected RelativePath(string path, char? separator = null)
    {
        _path = path;
        _separator = separator;
    }

    public static explicit operator RelativePath([CanBeNull] string path)
    {
        if (path is null)
            return null;

        return new RelativePath(NormalizePath(path));
    }

    public static implicit operator string([CanBeNull] RelativePath path)
    {
        return path?._path;
    }

#if NET6_0_OR_GREATER

    public static RelativePath operator /(RelativePath left, [CanBeNull] Range range)
    {
        Assert.True(range.Equals(Range.All));
        return left / "..";
    }

#endif

    public static RelativePath operator /(RelativePath left, [CanBeNull] string right)
    {
        var separator = left.NotNull()._separator;
        return new RelativePath(NormalizePath(Combine(left, (RelativePath) right, separator), separator), separator);
    }

    public static RelativePath operator +(RelativePath left, [CanBeNull] string right)
    {
        return new RelativePath(left.ToString() + right);
    }

    public override string ToString()
    {
        return _path;
    }
}
