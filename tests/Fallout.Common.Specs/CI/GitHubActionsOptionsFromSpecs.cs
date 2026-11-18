using System;
using System.IO;
using System.Linq;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.Execution;
using FluentAssertions;
using Xunit;

namespace Fallout.Common.Specs.CI;

// Spec: GitHubActionsInputAttribute.OptionsFrom takes an enum type; options are the enum names in
// declaration order; requires Type = Choice; Default must be an enum name; no OptionsFrom leaves output unchanged.
public class GitHubActionsOptionsFromSpecs
{
    private static string Render(string workflowName, string[] workflowNames, params GitHubActionsInputAttribute[] inputs)
    {
        var build = new ConfigurationGenerationSpecs.TestBuild();
        var relevantTargets = ExecutableTargetFactory.CreateAll(build, x => x.Compile);

        var stream = new MemoryStream();
        var attribute = new TestGitHubActionsAttribute(workflowName, GitHubActionsImage.UbuntuLatest)
        {
            InvokedTargets = new[] { nameof(ConfigurationGenerationSpecs.TestBuild.Test) },
            Inputs = inputs,
            WorkflowNames = workflowNames
        };
        attribute.Build = build;
        attribute.Stream = new StreamWriter(stream, leaveOpen: true);
        attribute.Generate(relevantTargets);

        stream.Seek(offset: 0, SeekOrigin.Begin);
        return new StreamReader(stream).ReadToEnd();
    }

    private static string Render(params GitHubActionsInputAttribute[] inputs) => Render("test", null, inputs);

    private static GitHubActionsInputAttribute Choice(Type optionsFrom, string @default = null) =>
        new("Pick") { Type = GitHubActionsInputType.Choice, OptionsFrom = optionsFrom, Default = @default };

    // Option names in the order they appear in the rendered YAML.
    private static string[] OptionOrder(string yaml, params string[] names) =>
        names.Where(n => yaml.Contains($"- \"{n}\"")).OrderBy(n => yaml.IndexOf($"- \"{n}\"", StringComparison.Ordinal)).ToArray();

    [Fact]
    public void Enum_with_non_ascending_values_lists_options_in_declaration_order()
    {
        // Act
        var yaml = Render(Choice(typeof(Scrambled)));

        // Assert
        OptionOrder(yaml, "Zeta", "Alpha", "Mid").Should().Equal("Zeta", "Alpha", "Mid");
    }

    [Fact]
    public void Enum_with_non_ascending_values_matches_explicit_options_yaml()
    {
        // Act
        var fromEnum = Render(Choice(typeof(Scrambled), "Mid"));
        var explicitOptions = Render(new GitHubActionsInputAttribute("Pick")
        {
            Type = GitHubActionsInputType.Choice, Options = new[] { "Zeta", "Alpha", "Mid" }, Default = "Mid"
        });

        // Assert
        fromEnum.Should().Be(explicitOptions);
    }

    [Fact]
    public void Empty_enum_throws_because_a_choice_needs_options()
    {
        // Act
        var act = () => Render(Choice(typeof(Empty)));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Enum_members_with_duplicate_values_all_appear_in_declaration_order()
    {
        // Act
        var yaml = Render(Choice(typeof(Aliased)));

        // Assert
        OptionOrder(yaml, "Second", "First", "Third", "Alias").Should().Equal("Second", "First", "Alias", "Third");
    }

    [Fact]
    public void Flags_enum_lists_each_member_name_in_declaration_order()
    {
        // Act
        var yaml = Render(Choice(typeof(Permissions)));

        // Assert
        OptionOrder(yaml, "Write", "Read", "None", "Admin").Should().Equal("Write", "Read", "None", "Admin");
    }

    [Fact]
    public void Nullable_enum_type_is_not_an_enum_and_throws()
    {
        // Act
        var act = () => Render(Choice(typeof(Scrambled?)));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*enum*");
    }

    [Fact]
    public void Valid_default_renders_in_yaml()
    {
        // Act
        var yaml = Render(Choice(typeof(Scrambled), "Alpha"));

        // Assert
        yaml.Should().Contain("default: Alpha");
    }

    [Fact]
    public void Default_that_is_an_enum_value_not_a_name_throws()
    {
        // Act
        var act = () => Render(Choice(typeof(Scrambled), "5"));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Default_with_different_case_than_the_enum_name_throws()
    {
        // Act
        var act = () => Render(Choice(typeof(Scrambled), "alpha"));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Options_from_an_input_scoped_to_this_workflow_render()
    {
        // Arrange
        var input = Choice(typeof(Scrambled));
        input.Workflows = new[] { "publish" };

        // Act
        var yaml = Render("publish", new[] { "publish", "build" }, input);

        // Assert
        OptionOrder(yaml, "Zeta", "Alpha", "Mid").Should().Equal("Zeta", "Alpha", "Mid");
    }

    [Fact]
    public void Options_from_an_input_scoped_to_another_workflow_do_not_render()
    {
        // Arrange
        var input = Choice(typeof(Scrambled));
        input.Workflows = new[] { "build" };

        // Act
        var yaml = Render("publish", new[] { "publish", "build" }, input, new GitHubActionsInputAttribute("Other"));

        // Assert
        yaml.Should().Contain("Other:");
        yaml.Should().NotContain("Pick:");
        yaml.Should().NotContain("\"Zeta\"");
    }

    [Fact]
    public void Explicit_options_without_options_from_render_unchanged()
    {
        // Act
        var yaml = Render(new GitHubActionsInputAttribute("Pick")
        {
            Type = GitHubActionsInputType.Choice, Options = new[] { "b", "a" }, Default = "a"
        });

        // Assert
        yaml.Should().Contain("type: choice");
        yaml.Should().Contain("default: a");
        OptionOrder(yaml, "b", "a").Should().Equal("b", "a");
    }

    private enum Scrambled { Zeta = 5, Alpha = 1, Mid = 3 }

    private enum Empty { }

    private enum Aliased { Second = 2, First = 1, Alias = 1, Third = 3 }

    [Flags]
    private enum Permissions { Write = 4, Read = 2, None = 0, Admin = 8 }
}
