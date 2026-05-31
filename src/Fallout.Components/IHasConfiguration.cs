using System;
using System.Linq;
using Fallout.Application;

namespace Fallout.Components;

public interface IHasConfiguration : IFalloutBuild
{
    [Parameter] Configuration Configuration => TryGetValue(() => Configuration) ??
                                               (IsLocalBuild ? Configuration.Debug : Configuration.Release);
}
