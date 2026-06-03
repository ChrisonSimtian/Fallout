using System;
using System.IO;
using System.Linq;
using Fallout.Infrastructure.CI.SpaceAutomation;

namespace Fallout.Infrastructure.CI.Tests;

public class TestSpaceAutomationAttribute : SpaceAutomationAttribute, ITestConfigurationGenerator
{
    public TestSpaceAutomationAttribute(string jobName, string image)
        : base(jobName, image)
    {
    }

    public StreamWriter Stream { get; set; }

    protected override StreamWriter CreateStream()
    {
        return Stream;
    }
}
