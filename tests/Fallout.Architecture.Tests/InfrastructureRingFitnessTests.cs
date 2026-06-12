using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Onion fitness for the Infrastructure ring (ADR-0006). <c>Fallout.Infrastructure.*</c> holds the adapters
/// that touch the outside world (process/tool execution, CI hosts, project/solution readers). It is allowed to
/// depend inward — on Application, Domain, and Core — but must not depend on the <c>Fallout.Cli</c> composition
/// root, which is the outermost layer that wires everything together. This is the outer-ring counterpart to the
/// Application ⊥ Infrastructure guard, and was missing from the original fitness suite.
/// </summary>
public class InfrastructureRingFitnessTests
{
    // One public anchor per Infrastructure assembly (compile-checked); the namespace clause below scopes the
    // scan to Fallout.Infrastructure.* across all four.
    private static readonly Assembly[] InfrastructureAssemblies =
    [
        typeof(global::Fallout.Infrastructure.Tooling.NpmToolPathResolver).Assembly,
        typeof(global::Fallout.Infrastructure.CI.GitLab.GitLabProjectVisibility).Assembly,
        typeof(global::Fallout.Infrastructure.Solutions.SolutionReader).Assembly,
        typeof(global::Fallout.Infrastructure.ProjectModel.ProjectModelTasks).Assembly,
    ];

    [Fact]
    public void Infrastructure_ring_does_not_depend_on_the_Cli_composition_root()
    {
        var result = Types.InAssemblies(InfrastructureAssemblies)
            .That().ResideInNamespaceStartingWith("Fallout.Infrastructure")
            .Should()
            .NotHaveDependencyOn("Fallout.Cli")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Infrastructure adapters may depend inward (Application/Domain/Core) but never on the " +
                     "Fallout.Cli composition root; offending types: " + FailingTypes(result));
    }

    private static string FailingTypes(TestResult result) =>
        result.FailingTypeNames is null ? "(none reported)" : string.Join(", ", result.FailingTypeNames);
}
