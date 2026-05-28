using System;
using System.Linq;
using Fallout.Common.IO;

namespace Fallout.Common.Tooling;

public static class NpmToolPathResolver
{
    public static AbsolutePath NpmPackageJsonFile;

    public static string GetNpmExecutable(string npmExecutable)
    {
        Assert.FileExists(NpmPackageJsonFile);

        return ProcessTasks.StartProcess(
                toolPath: ToolPathResolver.GetPathExecutable("npx"),
                arguments: $"which {npmExecutable}",
                workingDirectory: NpmPackageJsonFile.Parent / "node_modules",
                logInvocation: false,
                logOutput: false)
            .AssertZeroExitCode()
            .Output.StdToText();
    }
}