using System.Collections.Generic;
using Fallout.Application;

namespace Fallout.Application.CI;

// Application-ring ports for the CI host providers (ADR-0006). Build components and version/coverage
// attributes need provider-specific capabilities (publish test results, push artifacts, set the build
// number, …) but must not depend on the concrete provider classes, which live in the outer
// Fallout.Infrastructure.CI ring. Each provider (AppVeyor, AzurePipelines, TeamCity, GitHubActions —
// subclasses of Host) implements the matching port; the Application ring reaches the current one through
// CiHost, which casts the already-detected Host.Instance to the port (null when not running on that host).
// No registration needed: Host.Instance is the existing detection seam.

public interface IAppVeyor
{
    string AccountName { get; }
    string ProjectSlug { get; }
    int BuildId { get; }
    int BuildNumber { get; }
    string BuildVersion { get; }
    string JobId { get; }
    void UpdateBuildVersion(string version);
    void PushArtifact(string path, string name = null);
}

public interface IAzurePipelines
{
    string StageDisplayName { get; }
    void UpdateBuildNumber(string buildNumber);
    void PublishCodeCoverage(AzurePipelinesCodeCoverageToolType coverageTool, string summaryFile, string reportDirectory, params string[] additionalCodeCoverageFiles);
    void PublishTestResults(string title, AzurePipelinesTestResultsType type, IEnumerable<string> files, bool? mergeResults = null, string platform = null, string configuration = null, bool? publishRunAttachments = null);
}

public interface ITeamCity
{
    IReadOnlyDictionary<string, string> ConfigurationProperties { get; }
    bool IsPullRequest { get; }
    void SetBuildNumber(string number);
    void SetConfigurationParameter(string name, string value);
}

public interface IGitHubActions
{
    string Token { get; }
}

/// <summary>
/// Typed access to the current CI host as an Application-ring port (null when not running on that host).
/// Casts the detected <see cref="Host.Instance"/> to the port — the concrete provider lives in
/// Fallout.Infrastructure.CI and implements it.
/// </summary>
public static class CiHost
{
    public static IAppVeyor AppVeyor => Host.Instance as IAppVeyor;
    public static IAzurePipelines AzurePipelines => Host.Instance as IAzurePipelines;
    public static ITeamCity TeamCity => Host.Instance as ITeamCity;
    public static IGitHubActions GitHubActions => Host.Instance as IGitHubActions;
}
