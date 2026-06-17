using System;
using System.Collections.Generic;
using System.IO;
using Fallout.Cli.Commands;
using Fallout.Cli.Prompts;
using Fallout.Common.IO;
using FluentAssertions;
using Xunit;

namespace Fallout.Cli.Specs;

public class CommandDispatcherSpecs
{
    private static readonly AbsolutePath SomeRoot = (AbsolutePath)Path.Combine(Path.GetTempPath(), "fallout-dispatch-root");
    private static readonly AbsolutePath SomeScript = SomeRoot / "build.sh";

    [Fact]
    public void Dispatch_ColonCommand_InvokesMatchingCommandWithForwardedArgs()
    {
        var run = new RecordingCommand("run");
        var setup = new RecordingCommand("setup");
        var dispatcher = new CommandDispatcher(new IFalloutCommand[] { run, setup }, new FakePrompts());

        var exitCode = dispatcher.Dispatch([":setup", "alpha", "beta"], SomeRoot, SomeScript);

        exitCode.Should().Be(0);
        setup.WasCalled.Should().BeTrue();
        setup.ReceivedArgs.Should().Equal("alpha", "beta");
        setup.ReceivedRoot.Should().Be(SomeRoot);
        setup.ReceivedBuildScript.Should().Be(SomeScript);
        run.WasCalled.Should().BeFalse();
    }

    [Theory]
    [InlineData(":setup")]
    [InlineData(":SETUP")]
    [InlineData(":Setup")]
    public void Dispatch_MatchesNameCaseInsensitively(string token)
    {
        var setup = new RecordingCommand("setup");
        var dispatcher = new CommandDispatcher(new IFalloutCommand[] { setup }, new FakePrompts());

        dispatcher.Dispatch([token], SomeRoot, SomeScript);

        setup.WasCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData(":add-package")]
    [InlineData(":addpackage")]
    [InlineData(":ADD-PACKAGE")]
    public void Dispatch_MatchesNameIgnoringDashes(string token)
    {
        // Preserves every spelling the historical reflection dispatch accepted.
        var addPackage = new RecordingCommand("add-package");
        var dispatcher = new CommandDispatcher(new IFalloutCommand[] { addPackage }, new FakePrompts());

        dispatcher.Dispatch([token], SomeRoot, SomeScript);

        addPackage.WasCalled.Should().BeTrue();
    }

    [Fact]
    public void Dispatch_ReturnsCommandExitCode()
    {
        var trigger = new RecordingCommand("trigger", exitCode: 42);
        var dispatcher = new CommandDispatcher(new IFalloutCommand[] { trigger }, new FakePrompts());

        dispatcher.Dispatch([":trigger"], SomeRoot, SomeScript).Should().Be(42);
    }

    [Fact]
    public void Dispatch_UnknownCommand_ThrowsWithAvailableCommandListing()
    {
        var dispatcher = new CommandDispatcher(
            new IFalloutCommand[] { new RecordingCommand("run"), new RecordingCommand("setup") },
            new FakePrompts());

        var action = () => dispatcher.Dispatch([":bogus"], SomeRoot, SomeScript);

        action.Should().Throw<Exception>()
            .WithMessage("*'bogus' is not supported*")
            .And.Message.Should().ContainAll("- run", "- setup");
    }

    [Fact]
    public void Dispatch_EmptyCommandToken_Fails()
    {
        var dispatcher = new CommandDispatcher(new IFalloutCommand[] { new RecordingCommand("run") }, new FakePrompts());

        var action = () => dispatcher.Dispatch([":"], SomeRoot, SomeScript);

        action.Should().Throw<Exception>().WithMessage("*No command specified*");
    }

    [Fact]
    public void Dispatch_NoCommand_NullRoot_Confirmed_InvokesSetup()
    {
        var setup = new RecordingCommand("setup");
        var dispatcher = new CommandDispatcher(
            new IFalloutCommand[] { new RecordingCommand("run"), setup },
            new FakePrompts(confirm: true));

        dispatcher.Dispatch(["whatever"], rootDirectory: null, buildScript: null);

        setup.WasCalled.Should().BeTrue();
        setup.ReceivedArgs.Should().BeEmpty();
        setup.ReceivedRoot.Should().BeNull();
    }

    [Fact]
    public void Dispatch_NoCommand_NullRoot_Declined_ReturnsZeroWithoutSetup()
    {
        var setup = new RecordingCommand("setup");
        var dispatcher = new CommandDispatcher(
            new IFalloutCommand[] { new RecordingCommand("run"), setup },
            new FakePrompts(confirm: false));

        var exitCode = dispatcher.Dispatch(["whatever"], rootDirectory: null, buildScript: null);

        exitCode.Should().Be(0);
        setup.WasCalled.Should().BeFalse();
    }

    [Fact]
    public void Dispatch_NoCommand_WithRoot_InvokesRunWithResolvedBuildProject()
    {
        using var root = TempRoot.Create();
        var buildProject = root.WriteBuildProjectAtConvention();
        var run = new RecordingCommand("run");
        var dispatcher = new CommandDispatcher(
            new IFalloutCommand[] { run, new RecordingCommand("setup") },
            new FakePrompts());

        dispatcher.Dispatch(["compile", "--verbose"], root.Path, buildScript: null);

        run.WasCalled.Should().BeTrue();
        run.ReceivedArgs.Should().Equal("compile", "--verbose");
        run.ReceivedBuildScript.Should().Be(buildProject);
    }

    private sealed class RecordingCommand : IFalloutCommand
    {
        private readonly int _exitCode;

        public RecordingCommand(string name, int exitCode = 0)
        {
            Name = name;
            _exitCode = exitCode;
        }

        public string Name { get; }
        public bool WasCalled { get; private set; }
        public string[] ReceivedArgs { get; private set; }
        public AbsolutePath ReceivedRoot { get; private set; }
        public AbsolutePath ReceivedBuildScript { get; private set; }

        public int Execute(string[] args, AbsolutePath rootDirectory, AbsolutePath buildScript)
        {
            WasCalled = true;
            ReceivedArgs = args;
            ReceivedRoot = rootDirectory;
            ReceivedBuildScript = buildScript;
            return _exitCode;
        }
    }

    private sealed class FakePrompts : IConsolePrompts
    {
        private readonly bool _confirm;

        public FakePrompts(bool confirm = false) => _confirm = confirm;

        public bool PromptForConfirmation(string question) => _confirm;

        public void ShowInput(string emoji, string title, string value) { }
        public void ShowCompletion(string title) { }
        public void ClearPreviousLine() { }
        public string PromptForInput(string question, string defaultValue = null) => defaultValue;
        public string PromptForSecret(string title, int? minLength = null) => string.Empty;
        public T PromptForChoice<T>(string question, params (T Value, string Description)[] choices) => default;
        public void ConfirmExecution(string title, Action action) => action();
    }

    private sealed class TempRoot : IDisposable
    {
        public AbsolutePath Path { get; }

        private TempRoot(AbsolutePath path)
        {
            Path = path;
            (path / ".fallout").CreateDirectory();
        }

        public static TempRoot Create()
        {
            var dir = (AbsolutePath)System.IO.Path.Combine(System.IO.Path.GetTempPath(), "fallout-dispatch-" + Guid.NewGuid().ToString("N"));
            dir.CreateDirectory();
            return new TempRoot(dir);
        }

        public AbsolutePath WriteBuildProjectAtConvention()
        {
            var buildDir = Path / "build";
            buildDir.CreateDirectory();
            var projectFile = buildDir / "_build.csproj";
            File.WriteAllText(projectFile, string.Empty);
            return projectFile;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
