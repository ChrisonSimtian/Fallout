using System;
using System.Linq;
using Fallout.Solutions;
using Fallout.Application;

namespace Fallout.Components;

public interface IHasSolution : IFalloutBuild
{
    [Solution] [Required] Solution Solution => TryGetValue(() => Solution);
}
