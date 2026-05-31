using System;
using System.Linq;
using Fallout.Common.Git;
using Fallout.Application;
using Fallout.Application.Git;

namespace Fallout.Application.Components;

public interface IHasGitRepository : IFalloutBuild
{
    [GitRepository] [Required] GitRepository GitRepository => TryGetValue(() => GitRepository);
}
