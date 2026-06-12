using System.Linq;
using System.Runtime.CompilerServices;
using Fallout.Core;
using Fallout.Core.IO;
using Fallout.Application.Solutions;

namespace Fallout.Infrastructure.ProjectModel;

// Infrastructure adapter for the IProjectEditor port (ADR-0006, step 5c). Owns the Microsoft.Build
// evaluation used to read/edit MSBuild project files, keeping the Application ring (e.g. Telemetry) free
// of any Microsoft.Build dependency. Registered into SolutionServices via a module initializer; the CLI
// composition root / MSBuild bootstrapping pulls this assembly in.

internal sealed class ProjectEditorAdapter : IProjectEditor
{
    public string GetProperty(AbsolutePath projectFile, params string[] propertyNames)
    {
        ProjectModelTasks.Initialize();
        var project = ProjectModelTasks.ParseProject(projectFile);
        foreach (var name in propertyNames)
        {
            var property = project.Properties.FirstOrDefault(x => x.Name.EqualsOrdinalIgnoreCase(name));
            if (property != null)
                return property.EvaluatedValue;
        }

        return null;
    }

    public void SetProperty(AbsolutePath projectFile, string name, string value)
    {
        ProjectModelTasks.Initialize();
        var project = ProjectModelTasks.ParseProject(projectFile);
        project.SetProperty(name, value);
        project.Save();
    }

    public bool HasPackageReference(AbsolutePath projectFile, string packageId)
    {
        ProjectModelTasks.Initialize();
        var project = ProjectModelTasks.ParseProject(projectFile);
        return project.GetItems("PackageReference").Any(x => x.EvaluatedInclude == packageId);
    }
}

internal static class ProjectEditorRegistration
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Register()
    {
        SolutionServices.Projects ??= new ProjectEditorAdapter();
    }
}
