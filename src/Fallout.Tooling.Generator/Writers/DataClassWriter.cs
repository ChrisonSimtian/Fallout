// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Fallout.CodeGeneration.Model;

namespace Fallout.CodeGeneration.Writers;

public class DataClassWriter : IWriterWrapper
{
    public DataClassWriter(DataClass dataClass, ToolWriter writer)
    {
        DataClass = dataClass;
        Writer = writer;
    }

    public DataClass DataClass { get; }
    public IWriter Writer { get; }
}
