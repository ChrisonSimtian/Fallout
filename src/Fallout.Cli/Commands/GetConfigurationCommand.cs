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
    private readonly IConfigurationReader _configuration;

    public GetConfigurationCommand(IConfigurationReader configuration) => _configuration = configuration;

    public string Name => "get-configuration";

    public int Execute(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
    {
        var configuration = _configuration.Read(buildScript.NotNull(), evaluate: false);

        Host.Information($"Configuration from {buildScript}:");
        configuration.ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));

        return 0;
    }
}
