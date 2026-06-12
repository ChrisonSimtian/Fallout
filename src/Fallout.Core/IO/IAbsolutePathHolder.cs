using System;
using System.Linq;

namespace Fallout.Core.IO;

public interface IAbsolutePathHolder
{
    AbsolutePath Path { get; }
}
