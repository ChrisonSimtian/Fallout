using System.Linq;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Solutions;

internal class Build : FalloutBuild
{
    public static int Main() => Execute<Build>(x => x.Load);

    private Target Load => _ => _
        .Executes(() =>
        {
            var solution = (RootDirectory / "fallout.slnx").ReadSolution();
            var project = solution.Projects.Single(x => x.Name == "Fallout.ProjectModel");
            Assert.NotNull(project.GetMSBuildProject());
        });
}
