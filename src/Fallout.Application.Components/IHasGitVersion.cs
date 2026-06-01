using System;
using System.Linq;
using Fallout.Application;
using Fallout.Application.Tools.GitVersion;

namespace Fallout.Application.Components;

public interface IHasGitVersion : IFalloutBuild
{
    [GitVersion(NoFetch = true, Framework = "net8.0")]
    [Required]
    GitVersion Versioning => TryGetValue(() => Versioning);
}
