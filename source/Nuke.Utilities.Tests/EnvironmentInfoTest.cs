// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using FluentAssertions;
using Nuke.Common.IO;
using Xunit;

namespace Nuke.Common.Tests;

public class EnvironmentInfoTest
{
    [Fact]
    public void TestPaths()
    {
        var paths = EnvironmentInfo.Paths;
        paths.Should().HaveCountGreaterThan(1);
        paths.Should().OnlyContain(x => PathConstruction.HasPathRoot(x));
    }
}
