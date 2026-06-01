using System;
using System.Linq;
using Fallout.Application.Execution;

namespace Fallout.Application.CI;

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