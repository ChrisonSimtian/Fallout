using System;
using System.Linq;
using Fallout.Application;
using Fallout.Application.Tools.NerdbankGitVersioning;

namespace Fallout.Application.Components;

public interface IHasNerdbankGitVersioning : IFalloutBuild
{
    [NerdbankGitVersioning] [Required] NerdbankGitVersioning Versioning => TryGetValue(() => Versioning);
}
