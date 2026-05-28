using System;
using System.IO;

namespace Fallout.Migrate;

public static class Program
{
    public static int Main(string[] args)
    {
        var dryRun = Array.Exists(args, a => a is "--dry-run" or "-n");
        var helpRequested = Array.Exists(args, a => a is "--help" or "-h" or "/?");
        var rootArg = Array.Find(args, a => !a.StartsWith('-') && !a.StartsWith('/'));

        if (helpRequested)
        {
            PrintHelp();
            return 0;
        }

        var rootDirectory = ResolveRootDirectory(rootArg);
        if (rootDirectory == null)
        {
            Console.Error.WriteLine("error: could not locate a repository root containing a build orchestrator project (_build.csproj) under the working directory.");
            Console.Error.WriteLine("       pass an explicit path: fallout-migrate <path>");
            return 1;
        }

        Console.WriteLine($"fallout-migrate — migrating: {rootDirectory}");
        if (dryRun)
            Console.WriteLine("(dry-run — no files will be modified)");
        Console.WriteLine();

        var migration = new Migration(rootDirectory, dryRun, Console.Out);
        var summary = migration.Run();

        Console.WriteLine();
        Console.WriteLine($"Files changed:   {summary.FilesChanged}");
        Console.WriteLine($"Edits made:      {summary.EditCount}");
        Console.WriteLine($"Directories:     {summary.DirectoriesRenamed} renamed");
        if (summary.Warnings.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Warnings:");
            foreach (var w in summary.Warnings)
                Console.WriteLine($"  - {w}");
        }

        Console.WriteLine();
        Console.WriteLine(dryRun
            ? "Dry-run complete. Re-run without --dry-run to apply changes."
            : "Migration complete. Verify the build:  ./build.ps1   (or ./build.sh on unix)");
        Console.WriteLine("Migration guide: https://fallout.build  (see #37 for the full guide)");
        return 0;
    }

    private static string ResolveRootDirectory(string explicitArg)
    {
        if (explicitArg != null)
            return Path.GetFullPath(explicitArg);

        var current = new DirectoryInfo(Environment.CurrentDirectory);
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "build")) ||
                Directory.Exists(Path.Combine(current.FullName, ".nuke")) ||
                Directory.Exists(Path.Combine(current.FullName, ".fallout")))
            {
                return current.FullName;
            }

            // Heuristic: any _build.csproj or build.cmd / build.ps1 anywhere below.
            if (current.GetFiles("build.cmd", SearchOption.TopDirectoryOnly).Length > 0 ||
                current.GetFiles("build.ps1", SearchOption.TopDirectoryOnly).Length > 0 ||
                current.GetFiles("build.sh", SearchOption.TopDirectoryOnly).Length > 0)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            fallout-migrate — migrate a NUKE consumer repo to Fallout.

            Usage:
              fallout-migrate [path] [options]

            Arguments:
              path        Repository root. Defaults to walking up from the working
                          directory to find one (looking for build.cmd / build.ps1 /
                          build.sh, .nuke/, or build/).

            Options:
              --dry-run, -n      Show what would change without writing.
              --help, -h, /?     Show this message.

            What it does:
              - Rewrites Nuke.* PackageReferences and MSBuild properties in .csproj
              - Rewrites `using Nuke.*` directives and qualified type references in .cs
              - Rewrites `dotnet nuke` → `dotnet fallout` and legacy NUKE_* env vars
                in build.cmd / build.ps1 / build.sh
              - Renames .nuke/ to .fallout/
              - Prints a summary of files changed and warnings to address manually
            """);
    }
}
