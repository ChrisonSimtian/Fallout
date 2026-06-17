using System;
using System.IO;
using Fallout.Cli.Commands;
using Fallout.Common.IO;
using FluentAssertions;
using Xunit;

namespace Fallout.Cli.Tests.Commands;

public class GetConfigurationCommandTests
{
    [Fact]
    public void Name_IsGetConfiguration()
        => new GetConfigurationCommand(new ConfigurationReader()).Name.Should().Be("get-configuration");

    [Fact]
    public void Execute_ParsesConfigurationBlock_ReturnsZero()
    {
        var dir = (AbsolutePath)Path.Combine(Path.GetTempPath(), "fallout-getcfg-" + Guid.NewGuid().ToString("N"));
        dir.CreateDirectory();
        var buildScript = dir / "build.sh";
        try
        {
            File.WriteAllText(buildScript, string.Join("\n",
                "# CONFIGURATION",
                "##############",
                "",
                "BUILD_PROJECT_FILE=\"build/_build.csproj\"",
                "TEMP_DIRECTORY=\"$SCRIPT_DIR/.fallout/temp\"",
                "",
                "# EXECUTION",
                "dotnet run"));

            new GetConfigurationCommand(new ConfigurationReader()).Execute([], dir, buildScript).Should().Be(0);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
