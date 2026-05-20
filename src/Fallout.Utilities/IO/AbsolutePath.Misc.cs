// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Linq;

namespace Fallout.Common.IO;

partial class AbsolutePathExtensions
{
    public static bool IsDotDirectory(this AbsolutePath path)
    {
        return path.DirectoryExists() && path.Name.StartsWith(".");
    }
}
