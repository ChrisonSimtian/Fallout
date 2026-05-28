using System;
using System.Linq;
using FluentAssertions;
using Fallout.Common.IO;
using Xunit;

namespace Fallout.Common.Tests;

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
