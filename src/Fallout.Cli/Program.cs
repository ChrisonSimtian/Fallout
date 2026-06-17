using System;
using System.IO;
using System.Linq;
using System.Text;
using Fallout.Cli.Commands;
using Fallout.Cli.Commands.Navigation;
using Fallout.Cli.Prompts;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Fallout.Cli;

public class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            var rootDirectory = TryGetRootDirectory();

            var buildScript = rootDirectory != null
                ? rootDirectory.GetFiles(CliConventions.CurrentBuildScriptName, depth: 2)
                    .FirstOrDefault(x => Constants.TryGetRootDirectoryFrom(x.Parent) == rootDirectory)
                : null;

            using var services = BuildServiceProvider();
            return services.GetRequiredService<CommandDispatcher>().Dispatch(args, rootDirectory, buildScript);
        }
        catch (Exception exception)
        {
            Host.Error(exception.Unwrap().Message);
            return 1;
        }
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConsolePrompts, SpectreConsolePrompts>();
        services.AddSingleton<IConfigurationReader, ConfigurationReader>();
        services.AddSingleton<IBuildScaffolder, BuildScaffolder>();
        services.AddSingleton<IPackageManager, PackageManager>();
        services.AddSingleton<CommandDispatcher>();

        RegisterCommands(services);

        return services.BuildServiceProvider();
    }

    private static void RegisterCommands(IServiceCollection services)
    {
        services.AddSingleton<IFalloutCommand, RunCommand>();
        services.AddSingleton<IFalloutCommand, TriggerCommand>();
        services.AddSingleton<IFalloutCommand, CompleteCommand>();
        services.AddSingleton<IFalloutCommand, GetConfigurationCommand>();
        services.AddSingleton<IFalloutCommand, AddPackageCommand>();
        services.AddSingleton<IFalloutCommand, UpdateCommand>();
        services.AddSingleton<IFalloutCommand, SecretsCommand>();
        services.AddSingleton<IFalloutCommand, CakeConvertCommand>();
        services.AddSingleton<IFalloutCommand, CakeCleanCommand>();
        services.AddSingleton<IFalloutCommand, GetNextDirectoryCommand>();
        services.AddSingleton<IFalloutCommand, PopDirectoryCommand>();
        services.AddSingleton<IFalloutCommand, PushWithCurrentRootDirectoryCommand>();
        services.AddSingleton<IFalloutCommand, PushWithParentRootDirectoryCommand>();
        services.AddSingleton<IFalloutCommand, PushWithChosenRootDirectoryCommand>();

        // SetupCommand is also resolved directly by CakeConvertCommand, so register the concrete type
        // and expose the same instance as an IFalloutCommand.
        services.AddSingleton<SetupCommand>();
        services.AddSingleton<IFalloutCommand>(sp => sp.GetRequiredService<SetupCommand>());
    }

    private static AbsolutePath TryGetRootDirectory()
    {
        // TODO: copied in FalloutBuild.GetRootDirectory
        var parameterValue = EnvironmentInfo.GetNamedArgument<AbsolutePath>(Constants.RootDirectoryParameterName);
        if (parameterValue != null)
            return parameterValue;

        if (EnvironmentInfo.GetNamedArgument<bool>(Constants.RootDirectoryParameterName))
            return EnvironmentInfo.WorkingDirectory;

        return Constants.TryGetRootDirectoryFrom(Directory.GetCurrentDirectory());
    }
}
