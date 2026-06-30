using System.Text.Json;

namespace Fallout.MSBuildTasks.Protocol;

/// <summary>Worker verbs — the first CLI argument the bridge passes to the net10 worker.</summary>
public static class WorkerVerbs
{
    public const string Codegen = "codegen";
    public const string EmbedPackages = "embed-packages";
    public const string PackTools = "pack-tools";
}

/// <summary>Shared (de)serialization so both ends use identical options.</summary>
public static class WorkerJson
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}

public sealed class CodegenRequest
{
    public string[] SpecificationFiles { get; set; }
    public string BaseDirectory { get; set; }
    public bool UseNestedNamespaces { get; set; }
    public string BaseNamespace { get; set; }
    public bool UpdateReferences { get; set; }
}

public sealed class EmbedPackagesRequest
{
    public string ProjectAssetsFile { get; set; }
}

public sealed class PackToolsRequest
{
    public string ProjectAssetsFile { get; set; }
    public string NuGetPackageRoot { get; set; }
    public string TargetFramework { get; set; }
}

/// <summary>One discovered tool file plus the MSBuild item metadata the bridge re-attaches.</summary>
public sealed class PackagedToolFileDto
{
    public string File { get; set; }
    public string BuildAction { get; set; }
    public string PackagePath { get; set; }
}

/// <summary>
/// Item outputs the worker writes to its output JSON for the bridge to read back. Diagnostics and
/// success do NOT travel here — messages go to stdout, errors to stderr, and the exit code signals
/// success, so the bridge's ToolTask surfaces them natively.
/// </summary>
public sealed class WorkerResponse
{
    /// <summary>embed-packages output: package file paths.</summary>
    public string[] Files { get; set; } = [];

    /// <summary>pack-tools output: tool files with pack metadata.</summary>
    public PackagedToolFileDto[] ToolFiles { get; set; } = [];
}
