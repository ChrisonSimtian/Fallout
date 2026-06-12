using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using Fallout.Core.IO;
using Fallout.Core;

namespace Fallout.Core.Tests;

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
