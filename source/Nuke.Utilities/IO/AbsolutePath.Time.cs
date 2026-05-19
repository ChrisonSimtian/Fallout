// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Linq;

namespace Nuke.Common.IO;

partial class AbsolutePathExtensions
{
    /// <summary>
    /// Returns the time the file or directory was last written to. For directories, the latest time for the whole content is returned.
    /// </summary>
    public static DateTime GetLastWriteTimeUtc(this AbsolutePath path)
    {
        Assert.True(Directory.Exists(path) || File.Exists(path));

        return path.DirectoryExists()
            ? path.ToDirectoryInfo()
                .GetFileSystemInfos("*", SearchOption.AllDirectories)
                .Max(x => x.LastWriteTimeUtc)
            : File.GetLastWriteTimeUtc(path);
    }
}
