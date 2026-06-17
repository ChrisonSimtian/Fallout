using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Utilities;

namespace Fallout.Cli.Commands.Navigation;

/// <summary><c>fallout :PushWithCurrentRootDirectory</c>: queues the current root directory.</summary>
public sealed class PushWithCurrentRootDirectoryCommand : IFalloutCommand
{
    public string Name => "PushWithCurrentRootDirectory";

    public int Execute(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
    {
        return NavigationSession.PushAndSetNext(() => rootDirectory.NotNull("No root directory"));
    }
}
