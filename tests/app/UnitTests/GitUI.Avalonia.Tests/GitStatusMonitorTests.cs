using Avalonia.Headless.NUnit;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs.BrowseDialog;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class GitStatusMonitorTests
{
    [AvaloniaTest]
    public void Disposed_monitor_should_release_command_events_and_ignore_late_refresh_requests()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);
        GitStatusMonitor monitor = new(source, () => false);
        int changes = 0;
        monitor.GitStatusMonitorStateChanged += (_, _) => changes++;

        monitor.Dispose();
        monitor.Dispose();
        monitor.RequestRefresh();
        commands.PreCheckoutBranch += Raise.EventWith(new GitUIEventArgs(null, commands));
        commands.PostCheckoutBranch += Raise.EventWith(new GitUIPostActionEventArgs(null, commands, actionDone: true));
        commands.PreCheckoutRevision += Raise.EventWith(new GitUIEventArgs(null, commands));
        commands.PostCheckoutRevision += Raise.EventWith(new GitUIPostActionEventArgs(null, commands, actionDone: true));
        commands.PostRepositoryChanged += Raise.EventWith(new GitUIEventArgs(null, commands));
        source.UICommandsChanged += Raise.EventWith(new GitUICommandsChangedEventArgs(commands));

        changes.Should().Be(0);
        monitor.Active.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Changing_repository_should_detach_the_old_commands_and_observe_the_new_commands()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        IGitUICommands oldCommands = Substitute.For<IGitUICommands>();
        IGitUICommands newCommands = Substitute.For<IGitUICommands>();
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(oldCommands);
        using GitStatusMonitor monitor = new(source, () => false);
        source.UICommands.Returns(newCommands);
        source.UICommandsChanged += Raise.EventWith(source, new GitUICommandsChangedEventArgs(oldCommands));
        int changes = 0;
        monitor.GitStatusMonitorStateChanged += (_, _) => changes++;

        oldCommands.PreCheckoutBranch += Raise.EventWith(new GitUIEventArgs(null, oldCommands));
        changes.Should().Be(0);

        newCommands.PreCheckoutBranch += Raise.EventWith(new GitUIEventArgs(null, newCommands));
        changes.Should().Be(1);
    }
}
