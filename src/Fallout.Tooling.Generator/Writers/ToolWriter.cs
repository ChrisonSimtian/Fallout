using System;
using System.IO;
using System.Linq;
using Fallout.CodeGeneration.Model;
using Fallout.Common.Utilities;

namespace Fallout.CodeGeneration.Writers;

public class ToolWriter : IDisposable, IWriter, IWriterWrapper
{
    private readonly StreamWriter _streamWriter;
    private int _indention;

    public ToolWriter(Tool tool, StreamWriter streamWriter)
    {
        Tool = tool;
        _streamWriter = streamWriter;
    }

    public Tool Tool { get; }
    public IWriter Writer => this;

    public void Dispose()
    {
        _streamWriter.Dispose();
    }

    void IWriter.WriteLine(string text)
    {
        _streamWriter.WriteLine($"{' '.Repeat(_indention * 4)}{text}");
    }

    void IWriter.WriteBlock(Action action)
    {
        this.WriteLine("{");
        _indention++;
        action();
        _indention--;
        this.WriteLine("}");
    }
}
