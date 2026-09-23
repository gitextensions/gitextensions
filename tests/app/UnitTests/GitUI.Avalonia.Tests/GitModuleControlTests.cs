using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitExtensions.Extensibility.Git;
using GitUI;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class GitModuleControlTests
{
    [AvaloniaTest]
    public void Lazy_command_discovery_should_initialize_the_control_once_and_observe_repository_changes()
    {
        IGitUICommands initial = Substitute.For<IGitUICommands>();
        CommandHost host = new(initial);
        CommandControl control = new();
        host.Content = control;

        control.TryGetUICommandsDirect(out _).Should().BeFalse();
        control.UICommands.Should().BeSameAs(initial);
        control.UICommands.Should().BeSameAs(initial);
        control.Initializations.Should().Be(1);
        control.TryGetUICommandsDirect(out IGitUICommands? discovered).Should().BeTrue();
        discovered.Should().BeSameAs(initial);

        IGitUICommands replacement = Substitute.For<IGitUICommands>();
        host.ChangeCommands(replacement);
        control.RepositoryChanges.Should().Be(1);
        control.UICommands.Should().BeSameAs(replacement);
        control.Initializations.Should().Be(1);
    }

    [AvaloniaTest]
    public void Explicit_command_source_should_be_initialized_once_and_cannot_be_replaced()
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        CommandControl control = new() { UICommandsSource = source };

        control.UICommandsSource.Should().BeSameAs(source);
        control.Initializations.Should().Be(1);
        Action replace = () => control.UICommandsSource = source;
        replace.Should().Throw<InvalidOperationException>();
    }

    private sealed class CommandControl : GitModuleControl
    {
        public int Initializations { get; private set; }
        public int RepositoryChanges { get; private set; }

        protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            base.OnUICommandsSourceSet(source);
            Initializations++;
            source.UICommandsChanged += (_, _) => RepositoryChanges++;
        }
    }

    private sealed class CommandHost(IGitUICommands commands) : ContentControl, IGitUICommandsSource
    {
        public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged;

        public IGitUICommands UICommands { get; private set; } = commands;

        public void ChangeCommands(IGitUICommands replacement)
        {
            IGitUICommands previous = UICommands;
            UICommands = replacement;
            UICommandsChanged?.Invoke(this, new GitUICommandsChangedEventArgs(previous));
        }
    }
}
