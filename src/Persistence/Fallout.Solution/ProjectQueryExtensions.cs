using Fallout.Kernel.IO;

namespace Fallout.Solutions;

// Application-ring conveniences over a Project that need impure MSBuild evaluation. They route through the
// IProjectEditor port (implemented in Fallout.ProjectModel) so the inner ring stays free of Microsoft.Build.
// The richer query surface (GetTargetFrameworks/GetItems/GetProperty/...) lives on the Infrastructure-side
// ProjectExtensions and is for consumers/tests that can reference the outer ring directly.
public static class ProjectQueryExtensions
{
    /// <summary>Indicates whether the project references the given package ID (a <c>PackageReference</c> item).</summary>
    public static bool HasPackageReference(this Project project, string packageId)
    {
        return SolutionServices.Projects.HasPackageReference(project.Path, packageId);
    }
}
