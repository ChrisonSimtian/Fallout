using System;
using System.Threading.Tasks;
using Fallout.Common.IO;

namespace Fallout.Cli.Commands;

/// <summary>
/// Transitional adapter that exposes a not-yet-extracted <c>Program.X</c> handler as an
/// <see cref="IFalloutCommand"/>, so the dispatcher can route every command uniformly through the
/// registry while the per-command conversion (issue #392) lands one PR at a time. Each conversion
/// replaces one registration of this adapter with a real command type; the adapter is deleted once
/// the last legacy handler is gone.
/// </summary>
internal sealed class DelegateCommand : IFalloutCommand
{
    private readonly Func<string[], AbsolutePath, AbsolutePath, int> handler;

    public DelegateCommand(string name, Func<string[], AbsolutePath, AbsolutePath, int> handler)
    {
        Name = name;
        this.handler = handler;
    }

    public string Name { get; }

    // The adapted legacy handlers are still synchronous; each #392 conversion replaces one
    // registration of this adapter with a command type that overrides ExecuteAsync for real.
    public Task<int> ExecuteAsync(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
        => Task.FromResult(handler(args, rootDirectory, buildScript));
}
