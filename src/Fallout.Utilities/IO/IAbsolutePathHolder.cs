using System;
using System.Linq;

namespace Fallout.Kernel.IO;

public interface IAbsolutePathHolder
{
    AbsolutePath Path { get; }
}
