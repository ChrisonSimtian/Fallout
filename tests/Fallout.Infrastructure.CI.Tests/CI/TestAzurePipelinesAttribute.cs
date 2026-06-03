using System;
using System.IO;
using System.Linq;
using Fallout.Infrastructure.CI.AzurePipelines;

namespace Fallout.Infrastructure.CI.Tests;

public class TestAzurePipelinesAttribute : AzurePipelinesAttribute, ITestConfigurationGenerator
{
    public TestAzurePipelinesAttribute(AzurePipelinesImage image, params AzurePipelinesImage[] images)
        : base(image, images)
    {
    }

    public StreamWriter Stream { get; set; }

    protected override StreamWriter CreateStream()
    {
        return Stream;
    }
}
