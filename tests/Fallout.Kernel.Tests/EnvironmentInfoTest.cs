using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using Fallout.Kernel.IO;
using Fallout.Kernel;

namespace Fallout.Kernel.Tests;

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
