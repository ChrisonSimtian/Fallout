// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Linq;
using Fallout.Common.CI.AppVeyor;

namespace Fallout.Common.Tests.CI;

public class TestAppVeyorAttribute : AppVeyorAttribute, ITestConfigurationGenerator
{
    public TestAppVeyorAttribute(AppVeyorImage image, params AppVeyorImage[] images)
        : base(image, images)
    {
    }

    public StreamWriter Stream { get; set; }

    protected override StreamWriter CreateStream()
    {
        return Stream;
    }
}
