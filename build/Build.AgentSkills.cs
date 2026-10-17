using System.Linq;
using Fallout.Common;
using Fallout.Common.IO;
using static Fallout.Common.Tools.Git.GitTasks;

partial class Build
{
    // The agent skills live in Fallout.Workbench, not in this repo. This target copies them into
    // .agents/skills/, the folder Cursor, Codex and Copilot read on their own. That folder is
    // git-ignored. To pick up newer skills, change the commit below in a PR.
    [Parameter("Fallout.Workbench commit to install the agent skills from")]
    readonly string WorkbenchCommit = "5ea71294626b9022127ab6fedcee81ccc910c5a6";

    AbsolutePath AgentSkillsDirectory => RootDirectory / ".agents" / "skills";

    Target InstallAgentSkills => _ => _
        .Executes(() =>
        {
            var workbench = TemporaryDirectory / "workbench";
            workbench.DeleteDirectory();
            workbench.CreateDirectory();

            Git("init --quiet", workbench);
            Git($"fetch --quiet --depth 1 https://github.com/Fallout-build/Fallout.Workbench.git {WorkbenchCommit}", workbench);
            Git("checkout --quiet FETCH_HEAD", workbench);

            var skillFolders = new[]
                {
                    workbench / "skills",
                    workbench / "plugins" / "fallout-core" / "skills"
                }
                .SelectMany(x => x.GetDirectories());

            // Replace only the skills installed here, so a contributor's own skills in the same folder survive.
            foreach (var skill in skillFolders)
            {
                var target = AgentSkillsDirectory / skill.Name;
                target.DeleteDirectory();
                skill.Copy(target);
            }
        });
}
