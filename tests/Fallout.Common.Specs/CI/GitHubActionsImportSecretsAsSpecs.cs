using System;
using Fallout.Common.CI;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.Execution;
using Fallout.Common.Tooling;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Specs.CI;

public class GitHubActionsImportSecretsAsSpecs
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("MISSING_COLON")]
    [InlineData(": SECRET")]
    [InlineData("KEY WITH SPACE: SECRET")]
    [InlineData("KEY : SECRET")]
    [InlineData("KEY:SECRET")]
    [InlineData("KEY:")]
    [InlineData("KEY:   ")]
    [InlineData("KEY: SECRET WITH SPACE")]
    public void Malformed_entry_throws(string badEntry)
    {
        var act = () => GetConfiguration(importSecretsAs: new[] { badEntry });

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Apis__NzPost__FunctionKey: OPS_API_TESTS_NZPOST_KEY")]
    [InlineData("KEY:  SECRET")]
    public void Well_formed_entry_does_not_throw(string goodEntry)
    {
        var act = () => GetConfiguration(importSecretsAs: new[] { goodEntry });

        act.Should().NotThrow();
    }

    [Fact]
    public void Duplicate_env_name_within_the_property_throws()
    {
        var act = () => GetConfiguration(importSecretsAs: new[] { "KEY: A", "KEY: B" });

        act.Should().Throw<ArgumentException>().WithMessage("*Duplicate env names*KEY*");
    }

    [Fact]
    public void Env_name_that_repeats_an_import_secrets_parameter_throws()
    {
        var act = () => GetConfiguration(importSecrets: new[] { "ApiKey" }, importSecretsAs: new[] { "ApiKey: OTHER_SECRET" });

        act.Should().Throw<ArgumentException>().WithMessage("*Duplicate env names*ApiKey*");
    }

    [Fact]
    public void Same_secret_under_different_env_names_does_not_throw()
    {
        var act = () => GetConfiguration(importSecretsAs: new[] { "ONE: SHARED_SECRET", "TWO: SHARED_SECRET" });

        act.Should().NotThrow();
    }

    private static void GetConfiguration(string[] importSecrets = null, string[] importSecretsAs = null)
    {
        var build = new ConfigurationGenerationSpecs.TestBuild();
        var relevantTargets = ExecutableTargetFactory.CreateAll(build, x => x.Compile);

        var attribute = new TestGitHubActionsAttribute(GitHubActionsImage.UbuntuLatest)
                        {
                            On = new[] { GitHubActionsTrigger.Push },
                            InvokedTargets = new[] { nameof(ConfigurationGenerationSpecs.TestBuild.Test) },
                            ImportSecrets = importSecrets ?? new string[0],
                            ImportSecretsAs = importSecretsAs ?? new string[0]
                        };
        ((ConfigurationAttributeBase)attribute).Build = build;

        attribute.GetConfiguration(relevantTargets);
    }
}
