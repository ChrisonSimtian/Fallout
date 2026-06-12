using System.Threading.Tasks;
using Fallout.Common.Execution;
using VerifyXunit;
using Xunit;

#pragma warning disable FALLOUT002 // opting into the experimental build-graph export

namespace Fallout.Common.Tests;

public class BuildGraphUtilityTest
{
    [Fact]
    public Task TestEmptyGraph()
    {
        var json = BuildGraphUtility.GetJsonString(new ExecutableTarget[0]);
        return Verifier.Verify(json, "json");
    }

    [Fact]
    public Task TestGraph()
    {
        var clean = new ExecutableTarget { Name = "Clean", Listed = true, Member = typeof(SampleComponent).GetProperty(nameof(SampleComponent.Clean)) };
        var restore = new ExecutableTarget { Name = "Restore", Listed = true };
        var compile = new ExecutableTarget { Name = "Compile", Description = "Compiles the solution", Listed = true, IsDefault = true };
        var report = new ExecutableTarget { Name = "Report", Listed = false };

        restore.OrderDependencies.Add(clean);
        compile.ExecutionDependencies.Add(restore);
        compile.Triggers.Add(report);
        report.TriggerDependencies.Add(compile);

        var json = BuildGraphUtility.GetJsonString(new[] { clean, restore, compile, report }, falloutVersion: "2026.1.0-test");
        return Verifier.Verify(json, "json");
    }

    private class SampleComponent
    {
        public object Clean => null;
    }
}
