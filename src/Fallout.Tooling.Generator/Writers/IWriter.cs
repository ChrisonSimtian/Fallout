using System;
using System.Linq;

namespace Fallout.CodeGeneration.Writers;

public interface IWriter
{
    void WriteLine(string text);
    void WriteBlock(Action action);
}
