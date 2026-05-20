// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

namespace Fallout.Common.ProjectModel;

public static partial class ProjectExtensions
{
    /// <summary>
    /// Loads the project through the <a href="https://github.com/dotnet/msbuild">Microsoft Build Engine</a>.
    /// </summary>
    public static Microsoft.Build.Evaluation.Project GetMSBuildProject(
        this Project project,
        string configuration = null,
        string targetFramework = null)
    {
        return ProjectModelTasks.ParseProject(project.Path, configuration, targetFramework);
    }
}
