using System;
using System.Linq;
using Fallout.Common.Tools.NerdbankGitVersioning;
using Fallout.Application;

namespace Fallout.Components;

public interface IHasNerdbankGitVersioning : IFalloutBuild
{
    [NerdbankGitVersioning] [Required] NerdbankGitVersioning Versioning => TryGetValue(() => Versioning);
}
