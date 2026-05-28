using System;
using System.Linq;

namespace Fallout.CodeGeneration.Writers;

public interface IWriterWrapper
{
    IWriter Writer { get; }
}
