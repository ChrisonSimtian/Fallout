using System;
using System.Linq;
using Fallout.Common;
using Fallout.Application.Tools.Versioning.NerdbankGitVersioning;

namespace Fallout.Components;

public interface IHasNerdbankGitVersioning : IFalloutBuild
{
    [NerdbankGitVersioning] [Required] NerdbankGitVersioning Versioning => TryGetValue(() => Versioning);
}
