using System;
using System.Linq;
using static Fallout.Kernel.IO.PathConstruction;

namespace Fallout.Kernel.IO;

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
