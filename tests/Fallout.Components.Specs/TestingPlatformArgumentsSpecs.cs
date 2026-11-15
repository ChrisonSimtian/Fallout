using Fallout.Common;
using Fallout.Common.IO;
using FluentAssertions;
using Xunit;

namespace Fallout.Components.Specs;

public class TestingPlatformArgumentsSpecs
{
    private static readonly AbsolutePath Root = AbsolutePath.Create(System.IO.Path.GetTempPath()) / "root";

    [Fact]
    public void Passes_project_configuration_and_trx_report_options()
    {
        // Act
        string arguments = TestingPlatformArguments.Create(
            Root / "A.Specs.csproj", "A.Specs", Configuration.Debug, Root / "results", noBuild: false, collectCoverage: false);

        // Assert
        arguments.Should().Be(
            $"test --project {Root / "A.Specs.csproj"} --configuration Debug" +
            $" --results-directory {Root / "results"} --report-trx --report-trx-filename A.Specs.trx");
    }

    [Fact]
    public void Adds_no_build_without_quoting_it()
    {
        // Act
        string arguments = TestingPlatformArguments.Create(
            Root / "A.csproj", "A", Configuration.Release, Root / "results", noBuild: true, collectCoverage: false);

        // Assert
        arguments.Should().EndWith(" --no-build").And.NotContain("\"");
    }

    [Fact]
    public void Adds_coverage_options_that_exclude_generated_files()
    {
        // Act
        string arguments = TestingPlatformArguments.Create(
            Root / "A.csproj", "A", Configuration.Debug, Root / "results", noBuild: false, collectCoverage: true);

        // Assert
        arguments.Should().EndWith(
            " --coverlet --coverlet-output-format cobertura --coverlet-exclude-by-file **/*.Generated.cs");
    }

    [Fact]
    public void Quotes_paths_that_contain_spaces()
    {
        // Act
        string arguments = TestingPlatformArguments.Create(
            Root / "my project" / "A.csproj", "A", Configuration.Debug, Root / "my results", noBuild: false, collectCoverage: false);

        // Assert
        arguments.Should().Contain($"\"{Root / "my project" / "A.csproj"}\"")
            .And.Contain($"\"{Root / "my results"}\"");
    }
}
