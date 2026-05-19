// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;

namespace Nuke.CodeGeneration.Writers;

public interface IWriter
{
    void WriteLine(string text);
    void WriteBlock(Action action);
}
