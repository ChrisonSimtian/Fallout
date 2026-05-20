// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Execution;
using Fallout.Common.IO;

namespace Fallout.Common.CI;

public interface IConfigurationGenerator
{
    string Id { get; }
    string DisplayName { get; }
    string HostName { get; }

    bool AutoGenerate { get; }
    Type HostType { get; }
    IEnumerable<AbsolutePath> GeneratedFiles { get; }

    void Generate(IReadOnlyCollection<ExecutableTarget> executableTargets);
    void SerializeState();
}