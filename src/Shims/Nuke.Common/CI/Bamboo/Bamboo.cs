using Fallout.Infrastructure.CI.Bamboo;
// Hand-written transition shim for the framework-injected CI host singleton.
// See src/Shims/Nuke.Common/CI/AppVeyor/AppVeyor.cs for the rationale shared
// across all CI host shims.

namespace Nuke.Common.CI.Bamboo;

public static class Bamboo
{
    public static global::Fallout.Infrastructure.CI.Bamboo.Bamboo Instance
        => global::Fallout.Infrastructure.CI.Bamboo.Bamboo.Instance;
}
