using Xunit;
using Arch = Fallout.Architecture.Specs.FalloutArchitecture;

namespace Fallout.Architecture.Specs;

/// <summary>
/// Dependency-direction rules. These lock in the layering the <c>ProjectReference</c> graph already enforces at
/// build time, so that future work (new types, the onion refactor, AI-assisted changes) cannot quietly invert an
/// edge that still compiles. Most carry an empty baseline — they are strict invariants today.
///
/// Subjects and targets are scoped by <b>assembly</b>, never namespace: the legacy NUKE namespaces
/// (<c>Fallout.Common.*</c>, …) span several assemblies, so namespace-based layering would be meaningless here.
/// </summary>
public class LayeringSpecs
{
    [Fact]
    public void Core_is_a_foundation_depending_on_nothing_else_in_the_repo() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.Core)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.AllAssembliesExcept(Arch.Core))),
            "Fallout.Core is the pure reactor core and must reference no other Fallout/Nuke assembly",
            KnownViolations.None);

    [Fact]
    public void Utilities_is_a_foundation_depending_on_nothing_else_in_the_repo() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.Utilities)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.AllAssembliesExcept(Arch.Utilities))),
            "Fallout.Utilities is a foundation library and must not depend on any other Fallout/Nuke assembly",
            KnownViolations.None);

    [Theory]
    [InlineData(Arch.UtilitiesIoCompression)]
    [InlineData(Arch.UtilitiesIoGlobbing)]
    [InlineData(Arch.UtilitiesNet)]
    [InlineData(Arch.UtilitiesTextJson)]
    [InlineData(Arch.UtilitiesTextYaml)]
    public void Utility_satellite_depends_only_on_Fallout_Utilities(string satellite) =>
        Ratchet.Enforce(
            Arch.TypesIn(satellite)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.AllAssembliesExcept(satellite, Arch.Utilities))),
            $"{satellite} is a utility satellite and may depend only on Fallout.Utilities",
            KnownViolations.None);

    [Theory]
    [InlineData(Arch.ToolingExecution)]
    [InlineData(Arch.ToolingRequirements)]
    public void Tooling_contract_leaf_is_a_foundation(string leaf) =>
        Ratchet.Enforce(
            Arch.TypesIn(leaf)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.AllAssembliesExcept(leaf))),
            $"{leaf} is a pure Application-layer tooling-contract leaf and must reference no other Fallout/Nuke assembly",
            KnownViolations.None);

    [Theory]
    [InlineData(Arch.ToolsNotifications)]
    public void Tool_family_does_not_depend_on_upper_layers(string family) =>
        Ratchet.Enforce(
            Arch.TypesIn(family)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.Build, Arch.Components, Arch.Cli)),
            $"{family} is an Application-layer tool family and must not depend on the Build/Components/Cli layers above it",
            KnownViolations.None);

    [Fact]
    public void Tooling_does_not_depend_on_upper_layers() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.Tooling)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.Build, Arch.Common, Arch.Cli, Arch.Components, Arch.ProjectModel, Arch.BuildShared)),
            "Fallout.Tooling sits below the engine and must not depend on Build/Common/Cli/Components/ProjectModel/Build.Shared",
            KnownViolations.None);

    [Fact]
    public void ProjectModel_does_not_depend_on_upper_layers() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.ProjectModel)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.Build, Arch.Common, Arch.Cli, Arch.Components, Arch.BuildShared)),
            "Fallout.ProjectModel must not depend on the Build/Common/Cli/Components/Build.Shared layers above it",
            KnownViolations.None);

    [Fact]
    public void Solution_facade_depends_only_on_Utilities_and_the_vendored_parser() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.Solution)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.AllAssembliesExcept(Arch.Solution, Arch.Utilities, "Fallout.Persistence.Solution"))),
            "Fallout.Solution is a thin facade over the vendored parser and may depend only on Fallout.Utilities (+ the parser)",
            KnownViolations.None);

    [Fact]
    public void BuildShared_depends_only_on_Utilities() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.BuildShared)
                .Should().NotDependOnAny(Arch.TypesIn(Arch.Build, Arch.Common, Arch.Cli, Arch.Components, Arch.ProjectModel, Arch.Tooling, Arch.Solution)),
            "Fallout.Build.Shared is a low-level helper and must not depend on the layers above Fallout.Utilities",
            KnownViolations.None);

    [Fact]
    public void Nothing_depends_on_the_Cli_composition_root() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.AllAssembliesExcept(Arch.Cli))
                .Should().NotDependOnAny(Arch.TypesIn(Arch.Cli)),
            "Fallout.Cli is the composition root (the dotnet tool) — nothing may depend back on it",
            KnownViolations.None);

    [Fact]
    public void Fallout_never_depends_on_the_Nuke_transition_shims() =>
        Ratchet.Enforce(
            Arch.TypesIn(Arch.FalloutAssemblies())
                .Should().NotDependOnAny(Arch.TypesIn(Arch.NukeCommon, Arch.NukeBuild, Arch.NukeComponents)),
            "The Nuke.* shims wrap Fallout.* for NUKE-era consumers; dependencies point inward (Nuke.* -> Fallout.*), never the reverse",
            KnownViolations.None);
}
