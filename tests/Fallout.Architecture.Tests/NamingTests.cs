using System.Text.RegularExpressions;
using ArchUnitNET.Fluent;
using Xunit;
using Arch = Fallout.Architecture.Tests.FalloutArchitecture;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Naming invariants. The headline one — a type's namespace should be rooted at its assembly name — is the main
/// debt-bearing rule in the suite: today's legacy NUKE namespaces span assemblies, so it starts with a sizeable
/// baseline (<see cref="KnownViolations.NamespaceAssemblyDrift"/>) that the onion refactor will whittle down.
/// </summary>
public class NamingTests
{
    [Fact]
    public void Types_reside_in_a_namespace_rooted_at_their_assembly_name() =>
        Ratchet.Enforce(
            BuildNamespaceAlignmentRule(),
            "A type's namespace should be rooted at its assembly name; mismatches are the legacy NUKE namespace sprawl",
            KnownViolations.NamespaceAssemblyDrift);

    // One ArchUnitNET rule per assembly (the expected namespace differs per assembly), AND-combined into a single
    // rule so the ratchet sees one flat set of offending type names across the whole repo.
    private static IArchRule BuildNamespaceAlignmentRule()
    {
        IArchRule? combined = null;
        foreach (var assembly in Arch.RuntimeLibraries)
        {
            var expectedRoot = "^" + Regex.Escape(assembly);
            var rule = Arch.TypesIn(assembly)
                .Should().ResideInNamespaceMatching(expectedRoot);
            combined = combined is null ? rule : combined.And(rule);
        }

        return combined!;
    }
}
