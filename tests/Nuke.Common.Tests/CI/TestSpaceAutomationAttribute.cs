// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Linq;
using Nuke.Common.CI.SpaceAutomation;

namespace Nuke.Common.Tests.CI;

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
