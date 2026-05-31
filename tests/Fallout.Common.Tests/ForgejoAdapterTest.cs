using System.Reflection;
using Fallout.Common.CI;
using Fallout.Common.CI.Forgejo;
using Fallout.Common.CI.GitHubActions;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Tests;

// ADR-0005: Forgejo is the first adapter built against the finished seam — both runtime-host ports,
// composition (not inheritance) for config generation, and the new IsActive detection style.
public class ForgejoAdapterTest
{
    [Fact]
    public void Forgejo_ImplementsBothRuntimeHostPorts()
    {
        typeof(IBuildHost).IsAssignableFrom(typeof(Forgejo)).Should().BeTrue();
        typeof(IBuildReporter).IsAssignableFrom(typeof(Forgejo)).Should().BeTrue();
    }

    [Fact]
    public void Forgejo_DetectsViaIsActiveOverride_NotTheLegacyConvention()
    {
        // No IsRunningForgejo static: detection comes from the IsActive override (ADR-0005 #3).
        // If it declared neither, discovery would throw — so the static's absence implies the override.
        typeof(Forgejo)
            .GetProperty("IsRunningForgejo", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Should().BeNull();
    }

    [Fact]
    public void Forgejo_DoesNotInheritTheGitHubActionsAdapter()
    {
        // Composition, not coupling: Forgejo reuses the GitHub config model + command dialect but is
        // not a GitHubActions.
        typeof(Forgejo).Should().NotBeAssignableTo<GitHubActions>();
        typeof(ForgejoAttribute).Should().NotBeAssignableTo<GitHubActionsAttribute>();
    }

    [Fact]
    public void ForgejoAttribute_TargetsTheForgejoHost()
    {
        new ForgejoAttribute("build", GitHubActionsImage.UbuntuLatest).HostType.Should().Be(typeof(Forgejo));
    }
}
