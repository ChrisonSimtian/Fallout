// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using FluentAssertions;
using Xunit;

namespace Fallout.Migrate.Tests;

public class CsprojRewriterTest
{
    [Fact]
    public void RewritesPackageReferenceNamespace()
    {
        const string input = """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Nuke.Common" Version="9.0.0" />
                <PackageReference Include="Nuke.Components" />
              </ItemGroup>
            </Project>
            """;

        var result = CsprojRewriter.Rewrite(input);

        result.EditCount.Should().Be(2);
        result.Content.Should().Contain(@"Include=""Fallout.Common""");
        result.Content.Should().Contain(@"Include=""Fallout.Components""");
        result.Content.Should().NotContain(@"Include=""Nuke.");
    }

    [Fact]
    public void RewritesNukeRootDirectoryProperty()
    {
        const string input = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <NukeRootDirectory>.\..</NukeRootDirectory>
                <NukeTelemetryVersion>1</NukeTelemetryVersion>
              </PropertyGroup>
            </Project>
            """;

        var result = CsprojRewriter.Rewrite(input);

        result.EditCount.Should().Be(4);  // 2 opening + 2 closing tags
        result.Content.Should().Contain("<FalloutRootDirectory>");
        result.Content.Should().Contain("</FalloutRootDirectory>");
        result.Content.Should().Contain("<FalloutTelemetryVersion>");
        result.Content.Should().NotContain("<NukeRootDirectory>");
    }

    [Fact]
    public void LeavesUnrelatedNukePrefixedIdentifiersAlone()
    {
        const string input = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <NukeSomeRandomConsumerProp>x</NukeSomeRandomConsumerProp>
              </PropertyGroup>
            </Project>
            """;

        var result = CsprojRewriter.Rewrite(input);

        result.EditCount.Should().Be(0);
        result.Content.Should().Be(input);
    }

    [Fact]
    public void ReturnsZeroEditsForUnchangedContent()
    {
        const string input = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

        var result = CsprojRewriter.Rewrite(input);

        result.EditCount.Should().Be(0);
        result.Content.Should().Be(input);
    }
}
