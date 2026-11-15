using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;

namespace Fallout.Components;

/// <summary>
///     Builds the <c>dotnet test</c> command line that <see cref="ITest" /> uses when
///     <see cref="ITest.UseTestingPlatform" /> is on.
/// </summary>
internal static class TestingPlatformArguments
{
    private const string CoverageArguments =
        " --coverlet --coverlet-output-format cobertura --coverlet-exclude-by-file **/*.Generated.cs";

    public static string Create(
        AbsolutePath projectPath,
        string projectName,
        Configuration configuration,
        AbsolutePath resultsDirectory,
        bool noBuild,
        bool collectCoverage)
    {
        string noBuildArguments = noBuild ? " --no-build" : string.Empty;
        string coverage = collectCoverage ? CoverageArguments : string.Empty;

        ArgumentStringHandler arguments =
            $"test --project {projectPath} --configuration {configuration}" +
            $" --results-directory {resultsDirectory} --report-trx --report-trx-filename {projectName}.trx" +
            $"{noBuildArguments:nq}{coverage:nq}";

        return arguments.ToStringAndClear();
    }
}
