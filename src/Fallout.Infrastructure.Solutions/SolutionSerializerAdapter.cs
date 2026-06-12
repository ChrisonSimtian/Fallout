using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Fallout.Persistence.Solution.Model;
using Fallout.Persistence.Solution.Serializer;
using Fallout.Core.IO;
using Fallout.Core;
using Fallout.Application.Solutions;

namespace Fallout.Infrastructure.Solutions;

// Infrastructure adapter for the ISolutionSerializer port (ADR-0006, step 5c). It owns the only references
// to the vendored, format-specific serializers (legacy .sln = SlnV12, XML .slnx = SlnXml — selected by
// moniker) and translates the vendored SolutionModel into the Fallout-owned Solution tree. The Fallout
// model never names a vendored type; the round-trip-fidelity carrier is the opaque Solution.Handle, which
// only this adapter casts back. Co-hosted with the model + port (like ToolingServices) so the module
// initializer always runs once this assembly loads.

internal sealed class SolutionSerializerAdapter : ISolutionSerializer
{
    public Solution Open(AbsolutePath path) => Open<Solution>(path);

    public T Open<T>(AbsolutePath path)
        where T : Solution
    {
        var serializer = SolutionSerializers.GetSerializerByMoniker(path).NotNull();
        var model = AsyncHelper.RunSync(() => serializer.OpenAsync(path, CancellationToken.None));

        var solution = Create<T>(path, model);
        Populate(solution, model);
        return solution;
    }

    public void Save(Solution solution, AbsolutePath path)
    {
        var model = (SolutionModel)solution.Handle.NotNull(
            "Solution was not read from a file and cannot be saved.");
        var serializer = SolutionSerializers.GetSerializerByMoniker(path).NotNull();
        AsyncHelper.RunSync(() => serializer.SaveAsync(path, model, CancellationToken.None));
    }

    private static T Create<T>(AbsolutePath path, SolutionModel model)
        where T : Solution
    {
        // The (AbsolutePath, object) constructor is non-public on Solution and on the generated
        // strongly-typed subclasses, so bind explicitly to non-public ctors.
        return (T)System.Activator.CreateInstance(
            typeof(T),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [path, model],
            culture: null);
    }

    /// <summary>
    /// Walks the vendored serializer model and builds the Fallout solution tree (projects + folders +
    /// parent links). After this the Fallout model holds no reference to the vendored types beyond the
    /// opaque <see cref="Solution.Handle"/>.
    /// </summary>
    private static void Populate(Solution solution, SolutionModel model)
    {
        var folders = new Dictionary<SolutionFolderModel, SolutionFolder>();
        foreach (var folderModel in model.SolutionFolders)
            folders[folderModel] = new SolutionFolder(folderModel.ActualDisplayName, solution);

        foreach (var folderModel in model.SolutionFolders)
        {
            var folder = folders[folderModel];
            folder.Parent = folderModel.Parent != null ? folders[folderModel.Parent] : solution;
            solution.AddSolutionFolder(folder);
        }

        foreach (var projectModel in model.SolutionProjects)
        {
            var project = new Project(projectModel.ActualDisplayName, projectModel.FilePath, solution)
            {
                Parent = projectModel.Parent != null ? folders[projectModel.Parent] : solution,
            };
            solution.AddProject(project);
        }
    }
}

internal static class SolutionSerializerRegistration
{
    // CA2255: the module initializer is the intended wiring point — registers the Infrastructure adapter
    // into the Application-ring SolutionServices when this assembly loads, before any model code runs.
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Register()
    {
        SolutionServices.Serializer ??= new SolutionSerializerAdapter();
    }
}
