using System;
using System.Linq;
using Fallout.Common.Tools.GitVersion;
using Fallout.Application;

namespace Fallout.Components;

public interface IHasGitVersion : IFalloutBuild
{
    [GitVersion(NoFetch = true, Framework = "net8.0")]
    [Required]
    GitVersion Versioning => TryGetValue(() => Versioning);
}
