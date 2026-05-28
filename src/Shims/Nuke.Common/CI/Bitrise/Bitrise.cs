// Hand-written transition shim for the framework-injected CI host singleton.
// See src/Shims/Nuke.Common/CI/AppVeyor/AppVeyor.cs for the rationale shared
// across all CI host shims.

namespace Nuke.Common.CI.Bitrise;

public static class Bitrise
{
    public static global::Fallout.Common.CI.Bitrise.Bitrise Instance
        => global::Fallout.Common.CI.Bitrise.Bitrise.Instance;
}
