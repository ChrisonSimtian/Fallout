using Fallout.Kernel.IO;

namespace Fallout.Application.Solutions;

// Ports for the impure solution/project I/O (ADR-0006, onion realignment step 5c). The Application ring
// (the Solution/Project model, [Solution] injection, build orchestration) depends only on these
// abstractions; the concrete adapters — the vendored .sln/.slnx serializers and the MSBuild project
// evaluator — live behind them and register into SolutionServices via module initializers. This is the
// inversion that keeps the model free of any concrete persistence/MSBuild dependency (the format-specific
// implementations are the "outside" of the onion; the abstract concept is here, inside).

/// <summary>
/// Reads/writes a <see cref="Solution"/> across the supported file formats (legacy <c>.sln</c> and the
/// XML <c>.slnx</c>). The concrete, format-specific serializers live in the Infrastructure adapter.
/// </summary>
public interface ISolutionSerializer
{
    /// <summary>Reads a solution file into the Fallout model.</summary>
    Solution Open(AbsolutePath path);

    /// <summary>Reads a solution file into a strongly-typed <typeparamref name="T"/> solution subclass.</summary>
    T Open<T>(AbsolutePath path) where T : Solution;

    /// <summary>Writes a previously-read solution back to <paramref name="path"/>.</summary>
    void Save(Solution solution, AbsolutePath path);
}

/// <summary>
/// Reads/edits MSBuild project files (property evaluation + mutation). The concrete implementation lives in
/// Fallout.ProjectModel (the Microsoft.Build evaluator) — the impure "outside" of the onion.
/// </summary>
public interface IProjectEditor
{
    /// <summary>Evaluated value of the first of <paramref name="propertyNames"/> that is set, or null.</summary>
    string GetProperty(AbsolutePath projectFile, params string[] propertyNames);

    /// <summary>Sets <paramref name="name"/> to <paramref name="value"/> on the project and saves it.</summary>
    void SetProperty(AbsolutePath projectFile, string name, string value);

    /// <summary>Indicates whether the project references the given package ID (a <c>PackageReference</c> item).</summary>
    bool HasPackageReference(AbsolutePath projectFile, string packageId);
}

/// <summary>
/// Service locator holding the registered Infrastructure adapters for the solution/project ports. The
/// solution serializer is registered by a module initializer co-hosted in this assembly (so it is always
/// set once the model assembly loads). The project editor is registered from Fallout.ProjectModel when
/// that assembly loads (the CLI composition root / MSBuild bootstrapping pulls it in). Tests may swap
/// implementations directly.
/// </summary>
public static class SolutionServices
{
    public static ISolutionSerializer Serializer { get; set; }
    public static IProjectEditor Projects { get; set; }
}
