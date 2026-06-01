using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;
using Xunit;

namespace Fallout.SourceGenerators.Tests;

public class TransitionShimGeneratorTest
{
    // The generator is map-driven: it emits shims when the COMPILING assembly's name is a shim package
    // (Nuke.Common / Nuke.Components in NukeNamespaceMap), mirroring public types from referenced
    // Fallout.* assemblies under the package's mapped Fallout prefixes. These tests fake a referenced
    // canonical Fallout.Application assembly (a Nuke.Common-mapped prefix) and compile "as" Nuke.Common.

    // Each kind in the Easy tier (regular class, abstract class, interface, attribute, generic class,
    // nested type) emits a representative shim. The Hard tier kinds (sealed class, static class, enum,
    // class with no public/protected ctor) emit SHIM001 diagnostics instead. Captures both as a snapshot.
    [Fact]
    public Task EmitsShimsForEachKindAndSkipsHardTier()
    {
        var canonical = CompileCanonicalAssembly("""
            namespace Fallout.Application
            {
                // Easy tier
                public class Regular { public Regular(string a) {} public Regular() {} }
                public abstract class Abstr { protected Abstr() {} }
                public interface IFoo { }
                [System.AttributeUsage(System.AttributeTargets.Class)]
                public class MyAttr : System.Attribute { public MyAttr(int n) {} }
                public class Generic<T> where T : class { public Generic(T item) {} }
                public class WithNested
                {
                    public WithNested() {}
                    public class Nested { public Nested() {} }
                }

                // Hard tier — sealed-class still skipped (deferred to session 2b)
                public sealed class SealedThing { public SealedThing() {} }
                public enum MyEnum { A, B }
                public class PrivateCtorOnly { private PrivateCtorOnly(string x) {} }

                // Static-class with the various method shapes that need delegation
                public static class StaticHelpers
                {
                    public static int Plain(int a) { return a; }
                    public static void VoidReturn(string s) { }
                    public static T Generic<T>(T input) where T : class { return input; }
                    public static int WithOptional(int a, int b = 7, string s = "hello") { return a + b; }
                    public static int Sum(params int[] nums) { return 0; }
                    public static int TryParse(string s, out int value) { value = 0; return 0; }
                    public static string AsHex(this byte b) { return b.ToString("x2"); }
                }
            }
            """);

        var result = RunForShimPackage("Nuke.Common", canonical);
        return Verifier.Verify(result);
    }

    // Hand-bridge suppression: when the consuming compilation declares a type at the target shim FQN, the
    // generator treats that as the authoritative bridge — no emission, no SHIM001 (even for kinds that
    // would otherwise be skipped). Mirrors the CI host accessors hand-written in Nuke.Common.
    [Fact]
    public void SkipsCanonicalTypesAlreadyHandBridgedByConsumer()
    {
        var canonical = CompileCanonicalAssembly("""
            namespace Fallout.Application
            {
                public sealed class HandBridgedSealed { public HandBridgedSealed() {} }
                public class HandBridgedRegular { public HandBridgedRegular() {} }
            }
            """);

        var shimCompilation = CSharpCompilation.Create("Nuke.Common",
            new[]
            {
                CSharpSyntaxTree.ParseText("""
                    namespace Nuke.Common
                    {
                        public static class HandBridgedSealed { }
                        public static class HandBridgedRegular { }
                    }
                    """),
            },
            Basic.Reference.Assemblies.NetStandard20.References.All.Concat(new[] { canonical }),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var result = CSharpGeneratorDriver.Create(new TransitionShimGenerator())
            .RunGenerators(shimCompilation).GetRunResult();

        result.GeneratedTrees.Should().BeEmpty();
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void EmitsNothingWhenAssemblyIsNotAShimPackage()
    {
        var canonical = CompileCanonicalAssembly("""
            namespace Fallout.Application
            {
                public class Whatever { public Whatever() {} }
            }
            """);

        // A normal consumer assembly (not a shim package) → no map rows → no emission.
        var result = RunForShimPackage("SomeConsumer.Build", canonical);

        result.GetRunResult().GeneratedTrees.Should().BeEmpty();
    }

    private static GeneratorDriver RunForShimPackage(string assemblyName, MetadataReference canonical)
    {
        var shimCompilation = CSharpCompilation.Create(assemblyName,
            new[] { CSharpSyntaxTree.ParseText("// shim package — types come from referenced Fallout.* assemblies") },
            Basic.Reference.Assemblies.NetStandard20.References.All.Concat(new[] { canonical }),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return CSharpGeneratorDriver.Create(new TransitionShimGenerator()).RunGenerators(shimCompilation);
    }

    private static MetadataReference CompileCanonicalAssembly(string source)
    {
        var compilation = CSharpCompilation.Create("Fallout.TestCanonical",
            new[] { CSharpSyntaxTree.ParseText(source) },
            Basic.Reference.Assemblies.NetStandard20.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var stream = new MemoryStream();
        var emit = compilation.Emit(stream);
        emit.Success.Should().BeTrue(
            because: "canonical test compilation should compile: {0}",
            string.Join("; ", emit.Diagnostics.Select(d => d.GetMessage())));
        stream.Position = 0;
        return MetadataReference.CreateFromStream(stream);
    }
}
