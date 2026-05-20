// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;

namespace Fallout.Common.Tooling;

partial class ToolOptions
{
    internal Func<ToolOptions, IProcess, object> ProcessExitHandler { get; set; }
}
