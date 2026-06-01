using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Fallout.Common;
using Fallout.Kernel.IO;
using Fallout.Kernel;

namespace Fallout.Application.Solutions;

public interface IProjectContainer
{
    IProjectContainer Parent { get; }
    IReadOnlyCollection<Project> Projects { get; }
    IReadOnlyCollection<SolutionFolder> SolutionFolders { get; }
}

public static class ProjectContainerExtensions
{
    public static SolutionFolder GetSolutionFolder(this IProjectContainer container, string name)
    {
        return container.SolutionFolders.SingleOrDefault(x => name.Equals(x.Name, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets a project by its name.
    /// </summary>
    public static Project GetProject(this IProjectContainer container, string name)
    {
        return container.Projects.SingleOrDefault(x => name.Equals(x.Name, StringComparison.Ordinal));
    }
}

public class Solution : IProjectContainer, IAbsolutePathHolder
{
    private readonly List<Project> _allProjects = new();
    private readonly List<SolutionFolder> _allSolutionFolders = new();

    /// <summary>
    /// Constructs an (empty) solution. The project/folder tree is populated by the
    /// reader (<see cref="SolutionModelExtensions.ReadSolution(AbsolutePath)"/>); the
    /// <paramref name="handle"/> is an opaque carrier for the underlying serializer
    /// model so the solution can be saved back without the inner ring naming the
    /// concrete (vendored) model type.
    /// </summary>
    protected internal Solution(AbsolutePath path = null, object handle = null)
    {
        Path = path;
        Handle = handle;
    }

    /// <summary>The opaque serializer model this solution was read from (null if constructed in-memory).</summary>
    internal object Handle { get; }

    internal void AddProject(Project project) => _allProjects.Add(project);
    internal void AddSolutionFolder(SolutionFolder folder) => _allSolutionFolders.Add(folder);

    public AbsolutePath Path { get; set; }
    public string Name => Path?.NameWithoutExtension;
    public string FileName => Path?.Name;
    public AbsolutePath Directory => Path?.Parent;

    public IReadOnlyCollection<Project> AllProjects => _allProjects;
    public IReadOnlyCollection<SolutionFolder> AllSolutionFolders => _allSolutionFolders;

    IProjectContainer IProjectContainer.Parent => null;
    public IReadOnlyCollection<Project> Projects => AllProjects.Where(x => x.Parent == this).ToList();
    public IReadOnlyCollection<SolutionFolder> SolutionFolders => AllSolutionFolders.Where(x => x.Parent == this).ToList();

    public static implicit operator string(Solution solution) => solution.Path;
    public static implicit operator AbsolutePath(Solution solution) => solution.Path;

    public IEnumerable<Project> GetAllProjects(string wildcardPattern)
    {
        wildcardPattern = $"^{wildcardPattern}$";
        var regex = new Regex(wildcardPattern
            .Replace(".", "\\.")
            .Replace("*", ".*"));
        return AllProjects.Where(x => regex.IsMatch(x.Name));
    }

    public void Save(AbsolutePath path = null)
    {
        Path = (path ?? Path).NotNull();
        SolutionServices.Serializer.Save(this, Path);
    }
}

public class SolutionItem
{
    private protected SolutionItem(string name, Solution solution)
    {
        Name = name;
        Solution = solution;
    }

    public string Name { get; }

    public Solution Solution { get; }
    public IProjectContainer Parent { get; internal set; }

    public override string ToString() => Name;
}

public class SolutionFolder : SolutionItem, IProjectContainer
{
    internal SolutionFolder(string name, Solution solution)
        : base(name, solution)
    {
    }

    // Vestigial: strongly-typed folder subclasses (generated, in consumer assemblies) are
    // reached via Unsafe.As and never actually constructed, but their declarations still
    // need an accessible (cross-assembly => protected) base ctor to compile.
    protected SolutionFolder()
        : base(name: null, solution: null)
    {
    }

    public IReadOnlyCollection<Project> Projects => Solution.AllProjects.Where(x => x.Parent == this).ToList();
    public IReadOnlyCollection<SolutionFolder> SolutionFolders => Solution.AllSolutionFolders.Where(x => x.Parent == this).ToList();
}

public class Project : SolutionItem, IAbsolutePathHolder
{
    internal Project(string name, string relativePath, Solution solution)
        : base(name, solution)
    {
        RelativePath = relativePath;
    }

    public string RelativePath { get; }
    public AbsolutePath Path => Solution.Directory.NotNull() / RelativePath;
    public string FileName => System.IO.Path.GetFileName(RelativePath);
    public AbsolutePath Directory => Path?.Parent;

    public static implicit operator string(Project project) => project.Path;
    public static implicit operator AbsolutePath(Project project) => project.Path;
}
