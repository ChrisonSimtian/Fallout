// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Linq;
using Fallout.Common.CI.AzurePipelines;

namespace Fallout.Common.Tests.CI;

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
