// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Fallout.Common.IO;

namespace Fallout.Common.Tooling;

[PublicAPI]
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