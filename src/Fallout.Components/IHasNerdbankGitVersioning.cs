using System;
using System.Linq;
using Fallout.Common.Tools.NerdbankGitVersioning;
using Fallout.Application;

namespace Fallout.Application.Components;

public interface IHasNerdbankGitVersioning : IFalloutBuild
{
    [NerdbankGitVersioning] [Required] NerdbankGitVersioning Versioning => TryGetValue(() => Versioning);
}
