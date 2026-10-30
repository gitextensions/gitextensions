using System.Reflection;
using System.Runtime.ExceptionServices;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Compat;
using GitUI.Hotkey;
using GitUI.LeftPanel;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

/// <summary>
///  Refreshes existing Browse consumers when settings replace their hotkey definitions.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class FormBrowseSettingsRefreshTests
{
    private const WinFormsShims.Keys OldKey = WinFormsShims.Keys.Control | WinFormsShims.Keys.F20;
    private const WinFormsShims.Keys NewKey = WinFormsShims.Keys.Control | WinFormsShims.Keys.F21;
    private static readonly FieldInfo ToolsField = typeof(CustomDiffMergeToolCache).GetField("_tools", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("The source-owned custom-tool cache must exist.");
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;
    private string? _showAheadBehindData;
    private object? _previousDiffTools;
    private object? _previousMergeTools;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
        _showAheadBehindData = AppSettings.GetString("showaheadbehinddata", null);
        _previousDiffTools = ToolsField.GetValue(CustomDiffMergeToolCache.DiffToolCache);
        _previousMergeTools = ToolsField.GetValue(CustomDiffMergeToolCache.MergeToolCache);
        AppSettings.ShowAheadBehindData = false;
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
            await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            AppSettings.SetString("showaheadbehinddata", _showAheadBehindData!);
            ToolsField.SetValue(CustomDiffMergeToolCache.DiffToolCache, _previousDiffTools);
            ToolsField.SetValue(CustomDiffMergeToolCache.MergeToolCache, _previousMergeTools);
            ThreadHelper.JoinableTaskContext = _previousContext!;
            _context.Dispose();
        }
    }

    [AvaloniaTest]
    public void Settings_refresh_should_replace_the_revision_grid_command_routing_table()
    {
        string? originalGraphSetting = AppSettings.GetString("showrevisiongridgraphcolumn", null);
        bool showGraph = AppSettings.ShowRevisionGridGraphColumn;
        try
        {
            using BrowseFixture fixture = new();
            RevisionGridControl grid = fixture.Form.RevisionGridControl;
            grid.ReloadHotkeys();
            grid.ProcessHotkey(OldKey).Should().BeTrue();
            AppSettings.ShowRevisionGridGraphColumn.Should().Be(!showGraph);

            fixture.ApplyChangedSettings();

            grid.ProcessHotkey(OldKey).Should().BeFalse("the old cached shortcut must stop executing");
            AppSettings.ShowRevisionGridGraphColumn.Should().Be(!showGraph);
            grid.ProcessHotkey(NewKey).Should().BeTrue();
            AppSettings.ShowRevisionGridGraphColumn.Should().Be(showGraph);
        }
        finally
        {
            AppSettings.SetString("showrevisiongridgraphcolumn", originalGraphSetting!);
        }
    }

    [AvaloniaTest]
    public void Settings_refresh_should_replace_the_repository_tree_command_routing_table()
    {
        using BrowseFixture fixture = new();
        RepoObjectsTree tree = fixture.Form.GetTestAccessor().RepoObjectsTree;
        tree.ReloadHotkeys();
        tree.GetTestAccessor().Tree.SelectedItem.Should().BeNull();
        // Delete with no selected node exercises the real route without opening a dialog.
        tree.ProcessHotkey(OldKey).Should().BeTrue();

        fixture.ApplyChangedSettings();

        tree.ProcessHotkey(OldKey).Should().BeFalse("the old cached shortcut must stop executing");
        tree.ProcessHotkey(NewKey).Should().BeTrue();
        tree.GetTestAccessor().Tree.SelectedItem.Should().BeNull();
    }

    [AvaloniaTest]
    public void Settings_refresh_should_replace_the_commit_details_hotkey_and_add_notes_gesture()
    {
        using BrowseFixture fixture = new();
        GitUI.CommitInfo.CommitInfo details = fixture.Form.FindControl<GitUI.CommitInfo.CommitInfo>("RevisionInfo")
            ?? throw new AssertionException("The actual Browse commit-details control must exist.");
        details.ReloadHotkeys();
        MenuItem addNotes = details.GetTestAccessor().AddNoteMenuItem;
        addNotes.InputGesture.Should().NotBeNull();
        addNotes.InputGesture!.Matches(KeyEvent(Key.F20)).Should().BeTrue();
        addNotes.InputGesture.Matches(KeyEvent(Key.F21)).Should().BeFalse();

        fixture.ApplyChangedSettings();

        addNotes.InputGesture.Should().NotBeNull();
        addNotes.InputGesture!.Matches(KeyEvent(Key.F20)).Should().BeFalse();
        addNotes.InputGesture.Matches(KeyEvent(Key.F21)).Should().BeTrue();
        details.GetShortcutKeyDisplayString(FormBrowse.Command.AddNotes).Should().Be(
            new[] { new HotkeyCommand((int)FormBrowse.Command.AddNotes, nameof(FormBrowse.Command.AddNotes)) { KeyData = NewKey } }
                .GetShortcutDisplay(FormBrowse.Command.AddNotes));
    }

    private static KeyEventArgs KeyEvent(Key key) => new() { Key = key, KeyModifiers = KeyModifiers.Control };

    private sealed class BrowseFixture : IDisposable
    {
        private readonly Dictionary<string, IReadOnlyList<HotkeyCommand>> _hotkeys = [];

        public BrowseFixture()
        {
            SetKeys(OldKey);
            IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
            loader.LoadHotkeys(Arg.Any<string>()).Returns(call =>
                _hotkeys.TryGetValue(call.Arg<string>(), out IReadOnlyList<HotkeyCommand>? commands) ? commands : []);
            IGitModule module = Substitute.For<IGitModule>();
            module.WorkingDir.Returns(Path.GetTempPath());
            module.IsValidGitWorkingDir().Returns(true);
            IAppTitleGenerator title = Substitute.For<IAppTitleGenerator>();
            title.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
            commands.GetService(typeof(IAppTitleGenerator)).Returns(title);
            commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);
            commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(Substitute.For<IRepositoryHistoryUIService>());
            commands.GetService(typeof(IUserRepositoriesListController)).Returns(Substitute.For<IUserRepositoriesListController>());
            commands.GetService(typeof(ILinkFactory)).Returns(Substitute.For<ILinkFactory>());
            Form = new FormBrowse(commands);
        }

        public FormBrowse Form { get; }

        public void ApplyChangedSettings()
        {
            // Replace lists, not their entries: a stale consumer must retain the old key.
            SetKeys(NewKey);
            MethodInfo apply = typeof(FormBrowse).GetMethod("HandleSettingsChanged", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new AssertionException("The source-shaped settings refresh handler must exist.");
            apply.Invoke(Form, [AppSettings.Translation, AppSettings.CommitInfoPosition]);
        }

        public void Dispose()
        {
            ExceptionDispatchInfo? failure = null;
            try
            {
                // Cancel and join Browse-owned loads without persisting window settings.
                MethodInfo closed = typeof(FormBrowse).GetMethod("OnFormClosed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    ?? throw new AssertionException("The Browse close handler must exist.");
                closed.Invoke(Form, [EventArgs.Empty]);
            }
            catch (Exception exception)
            {
                failure = ExceptionDispatchInfo.Capture(exception);
                try
                {
                    ThreadHelper.JoinableTaskContext.Factory.Run(async () =>
                    {
                        try
                        {
                            FieldInfo lifetimeField = typeof(FormBrowse).GetField("_loadOperationsCancellationTokenSource", BindingFlags.Instance | BindingFlags.NonPublic)
                                ?? throw new AssertionException("The Browse load lifetime must exist.");
                            CancellationTokenSource lifetime = (CancellationTokenSource)lifetimeField.GetValue(Form)!;
                            if (!lifetime.IsCancellationRequested)
                            {
                                await lifetime.CancelAsync();
                            }

                            Form.RevisionGridControl.CancelBackgroundTasks();
                            Form.GetTestAccessor().RevisionDiffControl.CancelBackgroundTasks();
                            Form.GetTestAccessor().RevisionFileTreeControl.CancelBackgroundTasks();
                        }
                        finally
                        {
                            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(20));
                            await Form.JoinLoadOperationsForTestAsync(cancellation.Token);
                            cancellation.Token.ThrowIfCancellationRequested();
                        }
                    });
                }
                catch (Exception cleanupException)
                {
                    TestContext.Error.WriteLine(cleanupException);
                }
            }
            finally
            {
                ((IDisposable)Form).Dispose();
            }

            failure?.Throw();
        }

        private void SetKeys(WinFormsShims.Keys key)
        {
            _hotkeys[FormBrowse.HotkeySettingsName] =
            [new HotkeyCommand((int)FormBrowse.Command.AddNotes, nameof(FormBrowse.Command.AddNotes)) { KeyData = key }];
            _hotkeys[RevisionGridControl.HotkeySettingsName] =
            [new HotkeyCommand((int)RevisionGridControl.Command.ToggleRevisionGraph, nameof(RevisionGridControl.Command.ToggleRevisionGraph)) { KeyData = key }];
            _hotkeys[RepoObjectsTree.HotkeySettingsName] =
            [new HotkeyCommand((int)RepoObjectsTree.Command.Delete, nameof(RepoObjectsTree.Command.Delete)) { KeyData = key }];
        }
    }
}
