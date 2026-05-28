using System.IO;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Common.IO;

partial class AbsolutePathExtensions
{
    /// <summary>
    /// Indicates whether the path ends with an extension.
    /// </summary>
    public static bool HasExtension(this AbsolutePath path, string extension, params string[] alternativeExtensions)
    {
        return path.ToString().EndsWithAnyOrdinalIgnoreCase(extension.Concat(alternativeExtensions));
    }

    /// <summary>
    /// Changes the extension of the path (with or without leading period).
    /// </summary>
    public static AbsolutePath WithExtension(this AbsolutePath path, string extension)
    {
        return path.Parent / Path.ChangeExtension(path.Name, extension);
    }
}
