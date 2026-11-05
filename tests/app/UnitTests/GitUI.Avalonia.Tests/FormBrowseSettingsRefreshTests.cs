using System.Reflection;
using System.Runtime.ExceptionServices;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Extensibility.Translations.Xliff;
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
using SourceControls = GitUI.Compat.WinFormsControls;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

/// <summary>
///  Refreshes existing Browse consumers when settings replace hotkeys, translations or typography.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class FormBrowseSettingsRefreshTests
{
    private const WinFormsShims.Keys OldKey = WinFormsShims.Keys.Control | WinFormsShims.Keys.F20;
    private const WinFormsShims.Keys NewKey = WinFormsShims.Keys.Control | WinFormsShims.Keys.F21;
    private const string TranslationCommandName = "ShowRemoteBranches";
    private static readonly FieldInfo ToolsField = typeof(CustomDiffMergeToolCache).GetField("_tools", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("The source-owned custom-tool cache must exist.");
    private static readonly FieldInfo TranslationCacheField = typeof(Translator).GetField("_translation", BindingFlags.Static | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("The source-owned translation cache must exist.");
    private static readonly FieldInfo TranslationNameField = typeof(Translator).GetField("_name", BindingFlags.Static | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("The source-owned translation cache name must exist.");
    private static readonly FieldInfo CurrentTranslationField = typeof(AppSettings).GetField("_currentTranslation", BindingFlags.Static | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("The current translation override must exist.");
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;
    private string? _showAheadBehindData;
    private string? _showAvailableDiffTools;
    private string? _font;
    private string? _translation;
    private object? _previousTranslationCache;
    private object? _previousTranslationName;
    private object? _previousCurrentTranslation;
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
        _showAvailableDiffTools = AppSettings.GetString("difftools.showavailable", null);
        _font = AppSettings.GetString("font", null);
        _translation = AppSettings.GetString("translation", null);
        _previousTranslationCache = TranslationCacheField.GetValue(null);
        _previousTranslationName = TranslationNameField.GetValue(null);
        _previousCurrentTranslation = CurrentTranslationField.GetValue(null);
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
            AppSettings.SetString("difftools.showavailable", _showAvailableDiffTools!);
            AppSettings.SetString("font", _font!);
            AppSettings.SetString("translation", _translation!);
            TranslationCacheField.SetValue(null, _previousTranslationCache);
            TranslationNameField.SetValue(null, _previousTranslationName);
            CurrentTranslationField.SetValue(null, _previousCurrentTranslation);
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
        tree.GetTestAccessor().TreeView.SelectedItem.Should().BeNull();
        // Delete with no selected node exercises the real route without opening a dialog.
        tree.ProcessHotkey(OldKey).Should().BeTrue();

        fixture.ApplyChangedSettings();

        tree.ProcessHotkey(OldKey).Should().BeFalse("the old cached shortcut must stop executing");
        tree.ProcessHotkey(NewKey).Should().BeTrue();
        tree.GetTestAccessor().TreeView.SelectedItem.Should().BeNull();
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

    [AvaloniaTest]
    public void Settings_refresh_should_retranslate_the_existing_grid_and_shared_main_menu_items()
    {
        const string firstLanguage = "Browse-settings-first";
        const string secondLanguage = "Browse-settings-second";
        const string firstCaption = "First remote &branches";
        const string secondCaption = "Second remote &branches";
        const string firstToolTip = "First branch help";
        const string secondToolTip = "Second branch help";
        AppSettings.ShowAvailableDiffTools = false;
        using BrowseFixture fixture = new();
        RevisionGridControl grid = fixture.Form.RevisionGridControl;
        InstallTranslation(firstLanguage, firstCaption, firstToolTip);
        grid.ReloadTranslation();
        MenuItem contextItem = MenuItems(grid.ViewMenuItem).Single(item => Equals(item.Tag, TranslationCommandName));
        Menu mainMenu = fixture.Form.FindControl<Menu>("mainMenuStrip")
            ?? throw new AssertionException("The actual Browse menu must exist.");
        MenuItem mainItem = MenuItems(mainMenu).Single(item => Equals(item.Tag, TranslationCommandName));
        contextItem.Header.Should().Be(AvaloniaTranslationUtils.ToAvaloniaMnemonics(firstCaption));
        mainItem.Header.Should().Be(contextItem.Header);
        ToolTip.GetTip(contextItem).Should().Be(firstToolTip);
        ToolTip.GetTip(mainItem).Should().Be(firstToolTip);

        InstallTranslation(secondLanguage, secondCaption, secondToolTip);
        fixture.ApplyChangedSettings(firstLanguage);

        fixture.Form.RevisionGridControl.Should().BeSameAs(grid);
        MenuItems(grid.ViewMenuItem).Single(item => Equals(item.Tag, TranslationCommandName)).Should().BeSameAs(contextItem);
        MenuItems(mainMenu).Single(item => Equals(item.Tag, TranslationCommandName)).Should().BeSameAs(mainItem);
        contextItem.Header.Should().Be(AvaloniaTranslationUtils.ToAvaloniaMnemonics(secondCaption));
        mainItem.Header.Should().Be(contextItem.Header);
        ToolTip.GetTip(contextItem).Should().Be(secondToolTip);
        ToolTip.GetTip(mainItem).Should().Be(secondToolTip);
    }

    [AvaloniaTest]
    public void Settings_refresh_should_rebuild_existing_dashboard_links_and_layout_for_the_changed_font()
    {
        AppSettings.ShowAvailableDiffTools = false;
        string fontFamily = AppSettings.Font.Name;
        AppSettings.Font = new WinFormsShims.Font(fontFamily, 9);
        using BrowseFixture fixture = new(validRepository: false);
        FieldInfo dashboardField = typeof(FormBrowse).GetField("_dashboard", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new AssertionException("The existing Browse Dashboard owner must exist.");
        Dashboard dashboard = dashboardField.GetValue(fixture.Form).Should().BeOfType<Dashboard>().Which;
        dashboard.IsVisible.Should().BeTrue();
        Border start = dashboard.FindControl<Border>("flpnlStart")
            ?? throw new AssertionException("The actual Dashboard start-links panel must exist.");
        StackPanel links = start.Child.Should().BeOfType<StackPanel>().Which;
        SourceControls.LinkLabel before = links.Children.OfType<SourceControls.LinkLabel>().First();
        double oldLinkHeight = before.Height;
        double oldPanelMinimum = start.MinHeight;
        before.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(9));
        before.FontWeight.Should().Be(FontWeight.Normal);
        before.FontStyle.Should().Be(FontStyle.Normal);

        AppSettings.Font = new WinFormsShims.Font(fontFamily, 13, WinFormsShims.FontStyle.Bold | WinFormsShims.FontStyle.Italic);
        fixture.ApplyChangedSettings();

        dashboardField.GetValue(fixture.Form).Should().BeSameAs(dashboard);
        dashboard.IsVisible.Should().BeTrue();
        SourceControls.LinkLabel after = links.Children.OfType<SourceControls.LinkLabel>().First();
        after.Should().NotBeSameAs(before, "the source rebuilds anonymous links when settings change");
        after.FontFamily.Name.Should().Be(fontFamily);
        after.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(13));
        after.FontWeight.Should().Be(FontWeight.Bold);
        after.FontStyle.Should().Be(FontStyle.Italic);
        after.Height.Should().BeGreaterThan(oldLinkHeight);
        start.MinHeight.Should().BeGreaterThan(oldPanelMinimum);
        TextBlock heading = dashboard.FindControl<TextBlock>("lblContribute")
            ?? throw new AssertionException("The actual contribution heading must exist.");
        heading.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(13 + 5.5F));
        heading.FontWeight.Should().Be(FontWeight.Normal, "the source keeps its enlarged heading regular");
    }

    private static KeyEventArgs KeyEvent(Key key) => new() { Key = key, KeyModifiers = KeyModifiers.Control };

    private static IEnumerable<MenuItem> MenuItems(ItemsControl owner)
        => owner.Items.OfType<MenuItem>().SelectMany(item => MenuItems(item).Prepend(item));

    private static void InstallTranslation(string name, string caption, string toolTip)
    {
        TranslationFile translation = new("en", "test");
        TranslationCategory category = translation.FindOrAddTranslationCategory("RevisionGrid");
        category.Body.AddTranslationItem(new TranslationItem(TranslationCommandName, "Text", "Show remote &branches", caption));
        category.Body.AddTranslationItem(new TranslationItem(TranslationCommandName, "ToolTipText", string.Empty, toolTip));
        TranslationCacheField.SetValue(null, new Dictionary<string, TranslationFile> { [string.Empty] = translation });
        TranslationNameField.SetValue(null, name);
        AppSettings.Translation = name;
        AppSettings.CurrentTranslation = name;
    }

    private sealed class BrowseFixture : IDisposable
    {
        private readonly Dictionary<string, IReadOnlyList<HotkeyCommand>> _hotkeys = [];

        public BrowseFixture(bool validRepository = true)
        {
            SetKeys(OldKey);
            IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
            loader.LoadHotkeys(Arg.Any<string>()).Returns(call =>
                _hotkeys.TryGetValue(call.Arg<string>(), out IReadOnlyList<HotkeyCommand>? commands) ? commands : []);
            IGitModule module = Substitute.For<IGitModule>();
            module.WorkingDir.Returns(Path.GetTempPath());
            module.IsValidGitWorkingDir().Returns(validRepository);
            IAppTitleGenerator title = Substitute.For<IAppTitleGenerator>();
            title.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
            commands.GetService(typeof(IAppTitleGenerator)).Returns(title);
            commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);
            IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
            history.LoadSnapshot().Returns(new RepositoryHistorySnapshot([], []));
            IUserRepositoriesListController controller = Substitute.For<IUserRepositoriesListController>();
            IReadOnlyList<RecentRepoInfo> emptyRepositories = [];
            controller.PreRenderRepositories(Arg.Any<string>()).Returns((emptyRepositories, emptyRepositories));
            commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(history);
            commands.GetService(typeof(IUserRepositoriesListController)).Returns(controller);
            commands.GetService(typeof(ILinkFactory)).Returns(Substitute.For<ILinkFactory>());
            Form = new FormBrowse(commands);
        }

        public FormBrowse Form { get; }

        public void ApplyChangedSettings(string? oldTranslation = null)
        {
            // Replace lists, not their entries: a stale consumer must retain the old key.
            SetKeys(NewKey);
            MethodInfo apply = typeof(FormBrowse).GetMethod("HandleSettingsChanged", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new AssertionException("The source-shaped settings refresh handler must exist.");
            apply.Invoke(Form, [oldTranslation ?? AppSettings.Translation, AppSettings.CommitInfoPosition]);
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
