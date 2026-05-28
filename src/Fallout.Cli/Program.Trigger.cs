using System;
using System.Linq;
using Fallout.Common;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Common.Tools.Git;
using Fallout.Common.Utilities;

namespace Fallout.Cli;

partial class Program
{
    public static int Trigger(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
    {
        var repository = GitRepository.FromLocalDirectory(rootDirectory.NotNull()).NotNull("No Git repository");
        Assert.NotNull(repository.Branch, "Git repository must not be detached");
        Assert.NotEmpty(args);

        try
        {
            var messageBody = args.JoinSpace();
            GitTasks.Git($"commit --allow-empty -m {messageBody.DoubleQuote()}");
            GitTasks.Git($"push {repository.RemoteName} {repository.Head}:{repository.RemoteBranch}");
            return 0;
        }
        catch
        {
            return 1;
        }
    }
}
