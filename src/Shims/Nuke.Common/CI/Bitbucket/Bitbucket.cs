// Hand-written transition shim for the framework-injected CI host singleton.
// See src/Shims/Nuke.Common/CI/AppVeyor/AppVeyor.cs for the rationale shared
// across all CI host shims.

namespace Nuke.Common.CI.Bitbucket;

public static class Bitbucket
{
    public static global::Fallout.Common.CI.Bitbucket.Bitbucket Instance
        => global::Fallout.Common.CI.Bitbucket.Bitbucket.Instance;
}
