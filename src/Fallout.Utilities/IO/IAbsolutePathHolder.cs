using System;
using System.Linq;

namespace Fallout.Common.IO;

public interface IAbsolutePathHolder
{
    AbsolutePath Path { get; }
}
