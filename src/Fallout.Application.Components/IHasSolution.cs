using System;
using System.Linq;
using Fallout.Application;
using Fallout.Application.Solutions;

namespace Fallout.Application.Components;

public interface IHasSolution : IFalloutBuild
{
    [Solution] [Required] Solution Solution => TryGetValue(() => Solution);
}
