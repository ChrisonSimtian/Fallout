using System.IO;

namespace Fallout.Kernel.IO;

partial class AbsolutePathExtensions
{
    /// <summary>
    /// Creates the correlating <see cref="FileInfo"/>.
    /// </summary>
    public static FileInfo ToFileInfo(this AbsolutePath path)
    {
        return path is not null ? new FileInfo(path) : null;
    }

    /// <summary>
    /// Creates the correlating <see cref="DirectoryInfo"/>.
    /// </summary>
    public static DirectoryInfo ToDirectoryInfo(this AbsolutePath path)
    {
        return path is not null ? new DirectoryInfo(path) : null;
    }
}
