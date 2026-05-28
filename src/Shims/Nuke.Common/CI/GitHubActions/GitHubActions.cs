// Hand-written transition shim for the framework-injected CI host singleton.
// See src/Shims/Nuke.Common/CI/AppVeyor/AppVeyor.cs for the rationale shared
// across all CI host shims.

namespace Nuke.Common.CI.GitHubActions;

public static class GitHubActions
{
    public static global::Fallout.Common.CI.GitHubActions.GitHubActions Instance
        => global::Fallout.Common.CI.GitHubActions.GitHubActions.Instance;
}
