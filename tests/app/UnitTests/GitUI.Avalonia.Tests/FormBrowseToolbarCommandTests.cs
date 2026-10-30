using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FormBrowseToolbarCommandTests
{
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext? _context;

    [SetUp]
    public void SetUp()
    {
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext();
        ThreadHelper.JoinableTaskContext = _context;
    }

    [TearDown]
    public void TearDown()
    {
        ThreadHelper.JoinableTaskContext = _previousContext!;
        _context?.Dispose();
    }

    [AvaloniaTest]
    public void Pull_toolbar_shortcut_should_respect_the_source_menu_action_availability(
        [Values(GitPullAction.None, GitPullAction.Merge, GitPullAction.Rebase,
            GitPullAction.Fetch, GitPullAction.FetchAll, GitPullAction.FetchPruneAll)] GitPullAction action,
        [Values] bool enabled,
        [Values] bool available)
    {
        using FormBrowse form = new();
        MenuItem source = action switch
        {
            GitPullAction.None => form.pullToolStripMenuItem1,
            GitPullAction.Merge => form.mergeToolStripMenuItem,
            GitPullAction.Rebase => form.rebaseToolStripMenuItem1,
            GitPullAction.Fetch => form.fetchToolStripMenuItem,
            GitPullAction.FetchAll => form.fetchAllToolStripMenuItem,
            GitPullAction.FetchPruneAll => form.fetchPruneAllToolStripMenuItem,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
        IconButton shortcut = form.ToolStripMain.Items.OfType<IconButton>()
            .Single(item => item.Name == FormBrowse.FetchPullToolbarShortcutsPrefix + source.Name);
        source.IsEnabled = enabled;
        source.IsVisible = available;
        int invocations = 0;
        object? actualSender = null;
        source.Click += (sender, _) =>
        {
            invocations++;
            actualSender = sender;
        };

        ((MenuFlyout)form.toolStripButtonPull.Flyout!).IsOpen.Should().BeFalse();
        TopLevel.GetTopLevel(source).Should().BeNull();
        shortcut.IsEnabled.Should().BeTrue();
        shortcut.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        invocations.Should().Be(enabled && available ? 1 : 0);
        actualSender.Should().BeSameAs(enabled && available ? source : null);
    }
}
