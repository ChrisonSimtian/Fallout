using System;
using System.Linq;
using static Fallout.Core.IO.PathConstruction;

namespace Fallout.Core.IO;

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
