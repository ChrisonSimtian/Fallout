using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Utilities;
using static Fallout.Common.Constants;

namespace Fallout.Cli;

partial class Program
{
    private static int Run(string[] forwardedArgs, AbsolutePath rootDirectory, AbsolutePath buildProjectFile)
    {
        var dotnet = ResolveDotnet(rootDirectory);

        var buildExitCode = StartDotnet(dotnet, GetBuildArguments(buildProjectFile));
        if (buildExitCode != 0)
            return buildExitCode;

        return StartDotnet(dotnet, GetRunArguments(buildProjectFile, forwardedArgs));
    }

    private static string ResolveDotnet(AbsolutePath rootDirectory)
    {
        var pathDotnet = TryGetDotnetFromPath();
        if (pathDotnet != null)
            return pathDotnet;

        var shimDirectoryName = EnvironmentInfo.IsWin ? "dotnet-win" : "dotnet-unix";
        var shimExecutableName = EnvironmentInfo.IsWin ? "dotnet.exe" : "dotnet";
        var shimPath = GetTemporaryDirectory(rootDirectory) / shimDirectoryName / shimExecutableName;
        Assert.True(File.Exists(shimPath),
            $"Could not locate 'dotnet'. Tried PATH and '{shimPath}'. " +
            $"Run './build.sh' (Unix) or './build.ps1' (Windows) once to provision .NET locally, then retry.");
        return shimPath;
    }

    private static string TryGetDotnetFromPath()
    {
        var executable = EnvironmentInfo.IsWin ? "dotnet.exe" : "dotnet";
        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        return pathVar
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(dir => Path.Combine(dir, executable))
            .FirstOrDefault(File.Exists);
    }

    private static int StartDotnet(string dotnet, IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = dotnet,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        startInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        startInfo.Environment["DOTNET_NOLOGO"] = "1";
        startInfo.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        startInfo.Environment["FALLOUT_TELEMETRY_OPTOUT"] = "1";
        startInfo.Environment[GlobalToolVersionEnvironmentKey] = typeof(Program).Assembly.GetVersionText();
        startInfo.Environment[GlobalToolStartTimeEnvironmentKey] = DateTime.Now.ToString("O");

        var process = Process.Start(startInfo).NotNull();
        process.WaitForExit();
        return process.ExitCode;
    }

    private static IEnumerable<string> GetBuildArguments(AbsolutePath buildProjectFile)
    {
        // Mirrors the dotnet build invocation in build.sh / build.ps1.
        return new[]
        {
            "build",
            buildProjectFile.ToString(),
            "/nodeReuse:false",
            "/p:UseSharedCompilation=false",
            "-nologo",
            "-clp:NoSummary"
        };
    }

    private static IEnumerable<string> GetRunArguments(AbsolutePath buildProjectFile, string[] forwardedArgs)
    {
        var args = new List<string>
        {
            "run",
            "--project",
            buildProjectFile.ToString(),
            "--no-build",
            "--"
        };
        args.AddRange(forwardedArgs);
        return args;
    }
}
