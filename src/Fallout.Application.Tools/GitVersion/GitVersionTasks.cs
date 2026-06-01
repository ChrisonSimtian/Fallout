using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Fallout.Application.Tooling;
using Fallout.Kernel.Collections;
using Fallout.Kernel;

namespace Fallout.Application.Tools.GitVersion;

partial class GitVersionTasks
{
    protected override object GetResult<T>(ToolOptions options, IReadOnlyCollection<Output> output)
    {
        try
        {
            return output.EnsureOnlyStd().StdToJson<GitVersion>();
        }
        catch (Exception exception)
        {
            throw new Exception($"Cannot parse {nameof(GitVersion)} output:".Concat(output.Select(x => x.Text)).JoinNewLine(), exception);
        }
    }
}

public record GitVersion(
    int Major,
    int Minor,
    int Patch,
    string PreReleaseTag,
    string PreReleaseTagWithDash,
    string PreReleaseLabel,
    string PreReleaseLabelWithDash,
    [property: JsonConverter(typeof(NumberToStringJsonConverter))] 
    string PreReleaseNumber,
    [property: JsonConverter(typeof(NumberToStringJsonConverter))] 
    string WeightedPreReleaseNumber,
    [property: JsonConverter(typeof(NumberToStringJsonConverter))] 
    string BuildMetaData,
    string BuildMetaDataPadded,
    string FullBuildMetaData,
    string MajorMinorPatch,
    string SemVer,
    string LegacySemVer,
    string LegacySemVerPadded,
    string AssemblySemVer,
    string AssemblySemFileVer,
    string FullSemVer,
    string InformationalVersion,
    string BranchName,
    string EscapedBranchName,
    string Sha,
    string ShortSha,
    string NuGetVersionV2,
    string NuGetVersion,
    string NuGetPreReleaseTagV2,
    string NuGetPreReleaseTag,
    string VersionSourceSha,
    [property: JsonConverter(typeof(NumberToStringJsonConverter))] 
    string CommitsSinceVersionSource,
    string CommitsSinceVersionSourcePadded,
    int? UncommittedChanges,
    string CommitDate);
