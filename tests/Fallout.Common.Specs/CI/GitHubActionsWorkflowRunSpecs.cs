using System;
using System.IO;
using System.Linq;
using Fallout.Common.CI;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.CI.GitHubActions.Configuration;
using Fallout.Common.Execution;
using Fallout.Common.Utilities;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Specs.CI;

public class GitHubActionsWorkflowRunSpecs
{
    [Fact]
    public void Unset_adds_no_workflow_run_trigger_and_no_job_condition()
    {
        var configuration = GetConfiguration(a => a.OnPushBranches = new[] { "master" });

        configuration.DetailedTriggers.OfType<GitHubActionsWorkflowRunTrigger>().Should().BeEmpty();
        configuration.Jobs.Should().OnlyContain(x => x.Condition == null);
    }

    [Fact]
    public void Workflow_run_alone_satisfies_the_trigger_requirement()
    {
        var configuration = GetConfiguration(a => a.OnWorkflowRunWorkflows = new[] { "Deploy" });

        var trigger = configuration.DetailedTriggers.Should().ContainSingle().Which
            .Should().BeOfType<GitHubActionsWorkflowRunTrigger>().Subject;
        trigger.Workflows.Should().Equal("Deploy");
        trigger.Types.Should().Equal("completed");
        configuration.Jobs.Should().OnlyContain(x => x.Condition == null);
    }

    [Fact]
    public void Workflow_run_combines_with_other_detailed_triggers()
    {
        var configuration = GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnCronSchedule = "0 0 * * *";
        });

        configuration.DetailedTriggers.Should().HaveCount(2);
    }

    [Fact]
    public void Workflow_run_with_shorthand_triggers_throws()
    {
        var act = () => GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.On = new[] { GitHubActionsTrigger.Push };
        });

        act.Should().Throw<Exception>().WithMessage("*shorthand*");
    }

    [Fact]
    public void Require_success_adds_job_condition()
    {
        var configuration = GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnWorkflowRunRequireSuccess = true;
        });

        configuration.Jobs.Should().OnlyContain(x => x.Condition == "github.event_name != 'workflow_run' || github.event.workflow_run.conclusion == 'success'");
    }

    [Fact]
    public void Require_success_condition_does_not_skip_other_event_types()
    {
        var configuration = GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            a.OnCronSchedule = "0 0 * * *";
            a.OnWorkflowRunRequireSuccess = true;
        });

        configuration.Jobs.Should().OnlyContain(x => x.Condition.StartsWith("github.event_name != 'workflow_run' ||"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Null_types_or_branches_throws(bool nullTypes)
    {
        var act = () => GetConfiguration(a =>
        {
            a.OnWorkflowRunWorkflows = new[] { "Deploy" };
            if (nullTypes)
                a.OnWorkflowRunTypes = null;
            else
                a.OnWorkflowRunBranches = null;
        });

        act.Should().Throw<Exception>().WithMessage("*must not be null*");
    }

    [Fact]
    public void Branches_without_workflows_throws()
    {
        var act = () => GetConfiguration(a =>
        {
            a.On = new[] { GitHubActionsTrigger.Push };
            a.OnWorkflowRunBranches = new[] { "master" };
        });

        act.Should().Throw<Exception>().WithMessage("*OnWorkflowRunWorkflows*");
    }

    [Fact]
    public void Require_success_without_workflows_throws()
    {
        var act = () => GetConfiguration(a =>
        {
            a.On = new[] { GitHubActionsTrigger.Push };
            a.OnWorkflowRunRequireSuccess = true;
        });

        act.Should().Throw<Exception>().WithMessage("*OnWorkflowRunWorkflows*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_workflow_name_throws(string bad)
    {
        var act = () => GetConfiguration(a => a.OnWorkflowRunWorkflows = new[] { "Deploy", bad });

        act.Should().Throw<Exception>().WithMessage("*OnWorkflowRunWorkflows*");
    }

    [Fact]
    public void Trigger_renders_workflows_types_and_branches()
    {
        var trigger = new GitHubActionsWorkflowRunTrigger
                      {
                          Workflows = new[] { "Build: Deploy" },
                          Types = new[] { "completed" },
                          Branches = new[] { "master" }
                      };

        var yaml = Render(trigger);

        yaml.Should().Be(string.Join(Environment.NewLine,
            "workflow_run:",
            "  workflows:",
            "    - 'Build: Deploy'",
            "  types:",
            "    - completed",
            "  branches:",
            "    - master",
            string.Empty));
    }

    private static string Render(GitHubActionsDetailedTrigger trigger)
    {
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        {
            trigger.Write(new CustomFileWriter(writer, indentationFactor: 2, commentPrefix: "#"));
        }

        stream.Seek(offset: 0, SeekOrigin.Begin);
        return new StreamReader(stream).ReadToEnd();
    }

    private static GitHubActionsConfiguration GetConfiguration(Action<TestGitHubActionsAttribute> configure)
    {
        var attribute = new TestGitHubActionsAttribute(GitHubActionsImage.UbuntuLatest) { InvokedTargets = new[] { "Compile" } };
        configure(attribute);
        var build = new ConfigurationGenerationSpecs.TestBuild();
        ((ConfigurationAttributeBase)attribute).Build = build;
        var targets = ExecutableTargetFactory.CreateAll(build, x => x.Compile);
        return (GitHubActionsConfiguration)attribute.GetConfiguration(targets);
    }
}
