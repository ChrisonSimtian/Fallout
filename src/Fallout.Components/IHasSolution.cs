using System;
using System.Linq;
using Fallout.Solutions;
using Fallout.Application;

namespace Fallout.Application.Components;

public interface IHasSolution : IFalloutBuild
{
    [Solution] [Required] Solution Solution => TryGetValue(() => Solution);
}
