// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System.IO;
using JetBrains.Annotations;

namespace Nuke.Common.IO;

partial class AbsolutePathExtensions
{
    /// <summary>
    /// Creates the correlating <see cref="FileInfo"/>.
    /// </summary>
    [Pure]
    [ContractAnnotation("null => null; => notnull")]
    public static FileInfo ToFileInfo(this AbsolutePath path)
    {
        return path is not null ? new FileInfo(path) : null;
    }

    /// <summary>
    /// Creates the correlating <see cref="DirectoryInfo"/>.
    /// </summary>
    [Pure]
    [ContractAnnotation("null => null; => notnull")]
    public static DirectoryInfo ToDirectoryInfo(this AbsolutePath path)
    {
        return path is not null ? new DirectoryInfo(path) : null;
    }
}
