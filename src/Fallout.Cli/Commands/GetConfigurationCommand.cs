using System;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Collections;

namespace Fallout.Cli.Commands;

/// <summary>
/// <c>fallout :get-configuration</c>: prints the build configuration parsed from the build script.
/// </summary>
public sealed class GetConfigurationCommand : IFalloutCommand
{
    public string Name => "get-configuration";

    public int Execute(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
    {
        // Program.GetConfiguration(buildScript, evaluate) is shared with add-package/update/cake;
        // it moves into a configuration service in the final #392 collapse PR.
        var configuration = Program.GetConfiguration(buildScript.NotNull(), evaluate: false);

        Host.Information($"Configuration from {buildScript}:");
        configuration.ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));

        return 0;
    }
}
