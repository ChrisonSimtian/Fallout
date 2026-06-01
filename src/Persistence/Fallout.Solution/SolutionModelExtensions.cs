using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Fallout.Persistence.Solution.Model;
using Fallout.Persistence.Solution.Serializer;
using Fallout.Common;
using Fallout.Kernel.IO;
using Fallout.Kernel;

namespace Fallout.Solutions;

public static class SolutionModelExtensions
{
    public static Solution ReadSolution(this AbsolutePath path)
    {
        return path.ReadSolution<Solution>();
    }

    public static Solution ReadSolution<T>(this AbsolutePath path)
        where T : Solution
    {
        var serializer = SolutionSerializers.GetSerializerByMoniker(path).NotNull();
        var model = AsyncHelper.RunSync(() => serializer.OpenAsync(path, CancellationToken.None));

        var solution = CreateSolution<T>(path, model);
        Populate(solution, model);
        return solution;
    }

    private static T CreateSolution<T>(AbsolutePath path, SolutionModel model)
        where T : Solution
    {
        // The (AbsolutePath, object) constructor is non-public on Solution and on the
        // generated strongly-typed subclasses, so bind explicitly to non-public ctors.
        return (T)System.Activator.CreateInstance(
            typeof(T),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [path, model],
            culture: null);
    }

    /// <summary>
    /// Walks the underlying serializer model and builds the Fallout solution tree
    /// (projects + folders + parent links). The Fallout model holds no reference to
    /// the vendored types beyond the opaque <see cref="Solution"/> handle.
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
