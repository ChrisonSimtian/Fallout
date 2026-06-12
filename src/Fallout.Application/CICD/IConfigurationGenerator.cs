using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Application.Execution;
using Fallout.Core.IO;

namespace Fallout.Application.CI;

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