// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Nuke.Common.Execution;

namespace Nuke.Common.CI;

internal class SerializeBuildServerStateAttribute : BuildServerConfigurationGenerationAttributeBase, IOnBuildFinished
{
    public void OnBuildFinished()
    {
        GetGenerators(Build)
            // TODO: bool IsRunning
            .FirstOrDefault(x => x.HostType == Build.Host.GetType())
            ?.SerializeState();
    }
}