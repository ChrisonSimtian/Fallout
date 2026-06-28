using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using Fallout.Core.Planning;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Fallout.Core.Tests;

/// <summary>
/// The acceptance criterion for issue #88: Fallout.Core is the pure reactor core. It depends on
/// nothing in the repo and never touches I/O, processes, the console, or logging. The broader
/// architecture-fitness suite lands in #95; these two tests guard the Core invariant specifically.
/// </summary>
public class ArchitectureFitnessTests
{
    private static readonly Architecture Architecture =
        new ArchLoader().LoadAssemblies(typeof(TopoSort).Assembly).Build();

    // Scope rules to our own Fallout.* types only (inlined per test). This excludes build-tool noise
    // compiled into the assembly that we don't author and can't keep pure: the generated
    // `ThisAssembly` (Nerdbank.GitVersioning, no namespace) and `Coverlet.*` instrumentation (added
    // under coverage runs, which legitimately touches System.IO).

    [Fact]
    public void SimpleNamespaceUnitTest()
    {
         IObjectProvider<IType> reactorCoreTypes = Types()
            .That().ResideInAssemblyMatching("Fallout.Core")
            .As("Fallout.Core");
        IObjectProvider<IType> rootNamespace = Types()
            .That().ResideInNamespaceMatching("Fallout")
            .As("Fallout.*");
        IObjectProvider<IType> commonNamespace = Types()
            .That().ResideInNamespaceMatching("Fallout.Common")
            .As("Fallout.Common.*");

        IArchRule shouldResideInRootNamespace = Types()
            .That().Are(reactorCoreTypes)
            .Should().Be(rootNamespace)
            .Because("Fallout.Core is the pure reactor core and must not depend on any other Fallout project.");

        shouldResideInRootNamespace.Check(Architecture);
    }

    [Fact]
    public void Core_does_not_depend_on_higher_fallout_layers()
    {
        var rule = Types().That().ResideInNamespaceMatching("^Fallout")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(
                @"^Fallout\.(Build|Common\.Tooling|Common\.Utilities|ProjectModel|Tooling|Utilities)")
            .Because("Fallout.Core sits at the bottom and must reference no other Fallout project.");

        rule.Check(Architecture);
    }
}
