using System;
using System.IO;
using System.Linq;

namespace Fallout.Core.IO;

partial class AbsolutePathExtensions
{
    public static bool IsDotDirectory(this AbsolutePath path)
    {
        return path.DirectoryExists() && path.Name.StartsWith(".");
    }
}
