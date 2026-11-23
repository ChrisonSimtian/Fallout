using System;
using System.Linq;
using System.Threading.Tasks;
using Fallout.Common.CI;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.CI.GitHubActions.Configuration;
using Fallout.Common.Execution;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Specs.CI;

public class GitHubActionsWorkflowRunSpecs
{
    private const string RequireSuccessCondition =
        "github.event_name != 'workflow_run' || github.event.workflow_run.conclusion == 'success'";

    [Fact]
    public void Unset_adds_no_workflow_run_trigger_and_no_job_condition()
    {
        // Act
        var configuration = GetConfiguration(a => a.OnPushBranches = new[] { "master" });

        // Assert
        configuration.DetailedTriggers.OfType<GitHubActionsWorkflowRunTrigger>().Should().BeEmpty();
        configuration.Jobs.Should().OnlyContain(x => x.Condition == null);
    }

    [Fact]
    public void A_workflow_run_trigger_needs_no_other_trigger()
    {
        // Act
        var configuration = GetConfiguration(a => a.OnWorkflowRunWorkflows = new[] { "Deploy" });

        // Assert
        var trigger = configuration.DetailedTriggers.Should().ContainSingle().Which
            .Should().BeOfType<GitHubActionsWorkflowRunTrigger>().Subject;
        trigger.Workflows.Should().Equal("Deploy");
        trigger.Types.Should().Equal("completed");
        configuration.Jobs.Should().OnlyContain(x => x.Condition == null);
    }

    [Fact]
    public void Workflow_run_combines_with_other_detailed_triggers()
    {
        // Act
        var configuration = GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnCronSchedule = "0 0 * * *";
        });

        // Assert
        configuration.DetailedTriggers.Should().HaveCount(2);
    }

    [Fact]
    public void Workflow_run_with_shorthand_triggers_throws()
    {
        // Act
        var act = () => GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.On = new[] { GitHubActionsTrigger.Push };
        });

        // Assert
        act.Should().Throw<Exception>().WithMessage("*shorthand*");
    }

    [Fact]
    public void Require_success_adds_job_condition()
    {
        // Act
        var configuration = GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnWorkflowRunRequireSuccess = true;
        });

        // Assert
        configuration.Jobs.Should().OnlyContain(x => x.Condition == RequireSuccessCondition);
    }

    [Fact]
    public void Require_success_condition_does_not_skip_other_event_types()
    {
        // Act
        var configuration = GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnCronSchedule = "0 0 * * *";
            a.OnWorkflowRunRequireSuccess = true;
        });

        // Assert
        configuration.Jobs.Should().OnlyContain(x => x.Condition.StartsWith("github.event_name != 'workflow_run' ||"));
    }

    [Fact]
    public void Require_success_without_the_completed_type_throws()
    {
        // Act
        var act = () => GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnWorkflowRunTypes = new[] { "requested" };
            a.OnWorkflowRunRequireSuccess = true;
        });

        // Assert
        act.Should().Throw<Exception>().WithMessage("*completed*");
    }

    [Fact]
    public void Null_types_throws()
    {
        // Act
        var act = () => GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnWorkflowRunTypes = null;
        });

        // Assert
        act.Should().Throw<Exception>().WithMessage("*must not be null*");
    }

    [Fact]
    public void Null_branches_throws()
    {
        // Act
        var act = () => GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnWorkflowRunBranches = null;
        });

        // Assert
        act.Should().Throw<Exception>().WithMessage("*must not be null*");
    }

    [Fact]
    public void Branches_without_workflows_throws()
    {
        // Act
        var act = () => GetConfiguration(a =>
        {
            a.On = new[] { GitHubActionsTrigger.Push };
            a.OnWorkflowRunBranches = new[] { "master" };
        });

        // Assert
        act.Should().Throw<Exception>().WithMessage("*OnWorkflowRunWorkflows*");
    }

    [Fact]
    public void Require_success_without_workflows_throws()
    {
        // Act
        var act = () => GetConfiguration(a =>
        {
            a.On = new[] { GitHubActionsTrigger.Push };
            a.OnWorkflowRunRequireSuccess = true;
        });

        // Assert
        act.Should().Throw<Exception>().WithMessage("*OnWorkflowRunWorkflows*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_workflow_name_throws(string bad)
    {
        // Act
        var act = () => GetConfiguration(a => a.OnWorkflowRunWorkflows = new[] { "Deploy", bad });

        // Assert
        act.Should().Throw<Exception>().WithMessage("*OnWorkflowRunWorkflows*");
    }

    [Fact]
    public Task Trigger_renders_workflows_types_and_branches()
        => ConfigurationEntityVerifier.Verify(new GitHubActionsWorkflowRunTrigger
        {
            Workflows = new[] { "Build: Deploy" },
            Types = new[] { "completed" },
            Branches = new[] { "master" }
        });

    private static GitHubActionsConfiguration GetConfiguration(Action<TestGitHubActionsAttribute> configure)
    {
        var attribute = new TestGitHubActionsAttribute(GitHubActionsImage.UbuntuLatest)
                        {
                            InvokedTargets = new[] { "Compile" }
                        };
        configure(attribute);
        var build = new ConfigurationGenerationSpecs.TestBuild();
        ((ConfigurationAttributeBase)attribute).Build = build;
        var targets = ExecutableTargetFactory.CreateAll(build, x => x.Compile);
        return (GitHubActionsConfiguration)attribute.GetConfiguration(targets);
    }
}
