using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class FormBrowseSourceActionTests
{
    private static readonly MethodInfo SettingsContainerSetter = typeof(AppSettings).GetProperty(
            nameof(AppSettings.SettingsContainer), BindingFlags.Public | BindingFlags.Static)?.GetSetMethod(nonPublic: true)
        ?? throw new InvalidOperationException("The existing settings container setter must exist.");
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;
    private DistributedSettings? _originalSettingsContainer;
    private Lazy<string?> _originalApplicationDataPath = null!;
    private GitExtSettingsCache? _isolatedSettingsCache;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext();
        ThreadHelper.JoinableTaskContext = _context;
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.BrowseActions.Tests-{Guid.NewGuid():N}");
        _serviceContainer = null!;
        try
        {
            Directory.CreateDirectory(_workingDirectory);
            AppSettings.TestAccessor accessor = AppSettings.GetTestAccessor();
            _originalSettingsContainer = AppSettings.SettingsContainer;
            _originalApplicationDataPath = accessor.ApplicationDataPath;
            _isolatedSettingsCache = GitExtSettingsCache.Create(
                Path.Combine(_workingDirectory, AppSettings.SettingsFileName), useSharedCache: false);
            accessor.ApplicationDataPath = new Lazy<string?>(() => _workingDirectory);
            SetSettingsContainer(new DistributedSettings(lowerPriority: null, _isolatedSettingsCache, SettingLevel.Unknown));
            AppSettings.RevisionGraphShowArtificialCommits = false;
            AppSettings.BranchFilterEnabled.Value = false;
            AppSettings.ShowCurrentBranchOnly.Value = false;
            _serviceContainer = new ServiceContainer();
            GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);
            System.IO.Abstractions.FileSystem fileSystem = new();
            GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
            RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
            _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
            _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
            _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
            _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
            _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
            GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
            GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        }
        catch
        {
            DisposeFixture();
            throw;
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(15));
            await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            DisposeFixture();
        }
    }

    [AvaloniaTest]
    public async Task Push_pointer_activation_should_use_release_modifiers(
        [Values] bool toolbar, [Values] bool shiftAtPress, [Values] bool shiftAtRelease)
    {
        (FormBrowse form, IGitUICommands commands) = await OpenBrowseAsync();
        using (form)
        {
            try
            {
                Control target = GetPushTarget(form, toolbar);
                TopLevel root = GetRoot(target);
                Point point = GetClickPoint(root, target);
                root.MouseDown(point, MouseButton.Left, shiftAtPress ? RawInputModifiers.Shift : RawInputModifiers.None);
                root.MouseUp(point, MouseButton.Left, shiftAtRelease ? RawInputModifiers.Shift : RawInputModifiers.None);

                commands.Received(1).StartPushDialog(form, pushOnShow: shiftAtRelease);
                commands.ClearReceivedCalls();
                target = GetPushTarget(form, toolbar);
                Click(GetRoot(target), target);
                commands.Received(1).StartPushDialog(form, pushOnShow: false);
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    [TestCase(Key.Enter, false, false, false)]
    [TestCase(Key.Enter, true, true, true)]
    [TestCase(Key.Space, false, false, false)]
    [TestCase(Key.Space, false, true, true)]
    [TestCase(Key.Space, true, false, false)]
    [TestCase(Key.Space, true, true, true)]
    public async Task Push_toolbar_keyboard_activation_should_use_the_dispatching_key_modifiers(
        Key key, bool shiftAtPress, bool shiftAtRelease, bool pushOnShow)
    {
        (FormBrowse form, IGitUICommands commands) = await OpenBrowseAsync();
        using (form)
        {
            try
            {
                form.toolStripButtonPush.Focus();
                PhysicalKey physicalKey = key == Key.Enter ? PhysicalKey.Enter : PhysicalKey.Space;
                form.KeyPress(key, shiftAtPress ? RawInputModifiers.Shift : RawInputModifiers.None, physicalKey, keySymbol: null);
                if (key == Key.Space)
                {
                    commands.DidNotReceive().StartPushDialog(form, Arg.Any<bool>());
                }

                form.KeyRelease(key, shiftAtRelease ? RawInputModifiers.Shift : RawInputModifiers.None, physicalKey, keySymbol: null);
                commands.Received(1).StartPushDialog(form, pushOnShow: pushOnShow);
                commands.ClearReceivedCalls();
                form.toolStripButtonPush.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                commands.Received(1).StartPushDialog(form, pushOnShow: false);
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public async Task Push_menu_keyboard_activation_should_keep_framework_Enter_dispatch()
    {
        (FormBrowse form, IGitUICommands commands) = await OpenBrowseAsync();
        using (form)
        {
            try
            {
                Control target = GetPushTarget(form, toolbar: false);
                target.Focus();
                GetRoot(target).KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);

                commands.Received(1).StartPushDialog(form, pushOnShow: false);
                form.commandsToolStripMenuItem.IsSubMenuOpen.Should().BeFalse();
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public async Task Push_hotkeys_should_scope_Shift_and_preserve_QuickPush([Values] bool direct)
    {
        WinFormsShims.Keys normal = WinFormsShims.Keys.Control | WinFormsShims.Keys.Up;
        WinFormsShims.Keys shifted = normal | WinFormsShims.Keys.Shift;
        (FormBrowse form, IGitUICommands commands) = await OpenBrowseAsync(
            new HotkeyCommand((int)FormBrowse.Command.Push, nameof(FormBrowse.Command.Push)) { KeyData = normal },
            new HotkeyCommand((int)FormBrowse.Command.Push, nameof(FormBrowse.Command.Push)) { KeyData = shifted },
            new HotkeyCommand((int)FormBrowse.Command.QuickPush, nameof(FormBrowse.Command.QuickPush)) { KeyData = WinFormsShims.Keys.F6 });
        using (form)
        {
            try
            {
                if (direct)
                {
                    form.ProcessHotkey(shifted).Should().BeTrue();
                    form.ProcessHotkey(normal).Should().BeTrue();
                    form.ProcessHotkey(WinFormsShims.Keys.F6).Should().BeTrue();
                }
                else
                {
                    form.KeyPress(Key.Up, RawInputModifiers.Control | RawInputModifiers.Shift, PhysicalKey.ArrowUp, keySymbol: null);
                    form.KeyRelease(Key.Up, RawInputModifiers.Control | RawInputModifiers.Shift, PhysicalKey.ArrowUp, keySymbol: null);
                    form.KeyPress(Key.Up, RawInputModifiers.Control, PhysicalKey.ArrowUp, keySymbol: null);
                    form.KeyRelease(Key.Up, RawInputModifiers.Control, PhysicalKey.ArrowUp, keySymbol: null);
                    form.KeyPress(Key.F6, RawInputModifiers.None, PhysicalKey.F6, keySymbol: null);
                }

                commands.Received(2).StartPushDialog(form, pushOnShow: true);
                commands.Received(1).StartPushDialog(form, pushOnShow: false);
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public async Task Canceled_Push_input_should_not_leave_Shift_for_a_later_Click([Values] bool toolbar)
    {
        (FormBrowse form, IGitUICommands commands) = await OpenBrowseAsync();
        using (form)
        {
            try
            {
                Control target = GetPushTarget(form, toolbar);
                Click(GetRoot(target), target, MouseButton.Right, RawInputModifiers.Shift);
                commands.DidNotReceive().StartPushDialog(form, Arg.Any<bool>());
                target.RaiseEvent(new RoutedEventArgs(toolbar ? Button.ClickEvent : MenuItem.ClickEvent));
                commands.Received(1).StartPushDialog(form, pushOnShow: false);

                if (!toolbar)
                {
                    target.Focus();
                    GetRoot(target).KeyPress(Key.Escape, RawInputModifiers.Shift, PhysicalKey.Escape, keySymbol: null);
                    form.commandsToolStripMenuItem.IsSubMenuOpen.Should().BeFalse();
                    commands.ClearReceivedCalls();
                    target.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    commands.Received(1).StartPushDialog(form, pushOnShow: false);
                }
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public async Task Explicit_Pull_dialog_routes_should_use_the_remembered_dialog_action(
        [Values("menu", "hotkey", "dropdown")] string route,
        [Values(GitPullAction.None, GitPullAction.Merge, GitPullAction.Rebase,
            GitPullAction.Fetch, GitPullAction.FetchAll, GitPullAction.FetchPruneAll)] GitPullAction action)
    {
        AppSettings.DefaultPullAction = action == GitPullAction.Fetch ? GitPullAction.Merge : GitPullAction.Fetch;
        AppSettings.FormPullAction = action;
        (FormBrowse form, IGitUICommands commands) = await OpenBrowseAsync(
            new HotkeyCommand((int)FormBrowse.Command.PullOrFetch, nameof(FormBrowse.Command.PullOrFetch))
            {
                KeyData = WinFormsShims.Keys.F6,
            });
        using (form)
        {
            try
            {
                if (route == "menu")
                {
                    OpenCommandsMenu(form);
                    Click(GetRoot(form.pullToolStripMenuItem), form.pullToolStripMenuItem);
                }
                else if (route == "hotkey")
                {
                    form.KeyPress(Key.F6, RawInputModifiers.None, PhysicalKey.F6, keySymbol: null);
                }
                else
                {
                    form.toolStripButtonPull.ShowDropDown();
                    Dispatcher.UIThread.RunJobs();
                    Click(GetRoot(form.pullToolStripMenuItem1), form.pullToolStripMenuItem1);
                }

                commands.Received(1).StartPullDialog(form, pullAction: action);
                commands.DidNotReceive().StartPullDialogAndPullImmediately(form, pullAction: Arg.Any<GitPullAction>());
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    [TestCase(false, GitPullAction.Fetch, GitPullAction.Merge)]
    [TestCase(true, GitPullAction.Fetch, GitPullAction.Merge)]
    [TestCase(false, GitPullAction.None, GitPullAction.Rebase)]
    [TestCase(true, GitPullAction.None, GitPullAction.Rebase)]
    public async Task Pull_toolbar_and_quick_action_should_keep_the_configured_default(
        bool hotkey, GitPullAction defaultAction, GitPullAction dialogAction)
    {
        AppSettings.DefaultPullAction = defaultAction;
        AppSettings.FormPullAction = dialogAction;
        (FormBrowse form, IGitUICommands commands) = await OpenBrowseAsync(
            new HotkeyCommand((int)FormBrowse.Command.QuickPullOrFetch, nameof(FormBrowse.Command.QuickPullOrFetch))
            {
                KeyData = WinFormsShims.Keys.F6,
            });
        using (form)
        {
            try
            {
                if (hotkey)
                {
                    form.KeyPress(Key.F6, RawInputModifiers.None, PhysicalKey.F6, keySymbol: null);
                }
                else
                {
                    Click(form, form.toolStripButtonPull);
                }

                if (defaultAction == GitPullAction.None)
                {
                    commands.Received(1).StartPullDialog(form, pullAction: dialogAction);
                    commands.DidNotReceive().StartPullDialogAndPullImmediately(form, pullAction: Arg.Any<GitPullAction>());
                }
                else
                {
                    commands.Received(1).StartPullDialogAndPullImmediately(form, pullAction: defaultAction);
                    commands.DidNotReceive().StartPullDialog(form, pullAction: Arg.Any<GitPullAction>());
                }
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public async Task Branch_filter_suggestions_should_refresh_even_when_the_left_panel_is_hidden([Values] bool hidden)
    {
        GitModule module = CreateRepository("refresh");
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module)) { Width = 1600, Height = 850 };
        form.GetGitExtensionsFormTestAccessor().WindowPositionManager = new PositionManager();
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision is not null);
            ShowBranchSuggestions(form).Should().NotContain("new-branch");
            if (hidden)
            {
                Click(form, form.toggleLeftPanel);
            }

            form.FindControl<Control>("leftPanel")!.IsVisible.Should().Be(!hidden);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "new-branch" });
            bool reloaded = false;
            form.RevisionGrid.RevisionsLoaded += (_, _) => reloaded = true;
            Click(form, form.RefreshButton);
            await WaitUntilAsync(() => reloaded);

            ShowBranchSuggestions(form).Should().Contain("new-branch");
            form.FindControl<Control>("leftPanel")!.IsVisible.Should().Be(!hidden);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task Hidden_left_panel_repository_switch_should_replace_filter_refs_and_branch_validation()
    {
        GitModule first = CreateRepository("first");
        GitModule second = CreateRepository("second");
        first.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "only-first" });
        second.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "only-second" });
        using FormBrowse form = new(new GitUICommands(_serviceContainer, first)) { Width = 1600, Height = 850 };
        form.GetGitExtensionsFormTestAccessor().WindowPositionManager = new PositionManager();
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision is not null);
            ShowBranchSuggestions(form).Should().Contain("only-first");
            Click(form, form.toggleLeftPanel);
            form.FindControl<Control>("leftPanel")!.IsVisible.Should().BeFalse();

            form.SetWorkingDir(second.WorkingDir);
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == second.GetCurrentCheckout());

            ShowBranchSuggestions(form).Should().Contain("only-second").And.NotContain("only-first");
            FilterToolBar.TestAccessor filter = form.GetTestAccessor().ToolStripFilters.GetTestAccessor();
            List<string> invalid = [];
            filter.SetInvalidReferenceHandler(invalid.Add);
            filter.tscboBranchFilter.Text = "only-first";
            filter.tscboBranchFilter.Focus();
            GetRoot(filter.tscboBranchFilter).KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);
            GetRoot(filter.tscboBranchFilter).KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, keySymbol: null);

            invalid.Should().Equal("only-first");
            form.RevisionGrid.CurrentFilter.BranchFilter.Should().BeEmpty();
            form.FindControl<Control>("leftPanel")!.IsVisible.Should().BeFalse();
        }
        finally
        {
            form.Close();
        }
    }

    private async Task<(FormBrowse Form, IGitUICommands Commands)> OpenBrowseAsync(params HotkeyCommand[] hotkeys)
    {
        GitModule module = CreateRepository("actions");
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        loader.LoadHotkeys(Arg.Any<string>()).Returns([]);
        loader.LoadHotkeys(FormBrowse.HotkeySettingsName).Returns(hotkeys);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        commands.GetService(Arg.Any<Type>()).Returns(call => call.Arg<Type>() == typeof(IHotkeySettingsLoader)
            ? loader : _serviceContainer.GetService(call.Arg<Type>()));
        FormBrowse form = new(commands) { Width = 1600, Height = 850 };
        // Action tests must not restore or publish geometry through the real production position manager.
        form.GetGitExtensionsFormTestAccessor().WindowPositionManager = new PositionManager();
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision is not null);
            return (form, commands);
        }
        catch
        {
            form.Close();
            ((IDisposable)form).Dispose();
            throw;
        }
    }

    private GitModule CreateRepository(string name)
    {
        string path = Path.Combine(_workingDirectory, name);
        Directory.CreateDirectory(path);
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), path);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(path, "tracked.txt"), name);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", name });
        return module;
    }

    private static Control GetPushTarget(FormBrowse form, bool toolbar)
    {
        if (toolbar)
        {
            return form.toolStripButtonPush;
        }

        OpenCommandsMenu(form);
        return form.pushToolStripMenuItem;
    }

    private static void OpenCommandsMenu(FormBrowse form)
    {
        if (!form.commandsToolStripMenuItem.IsSubMenuOpen)
        {
            Click(form, form.commandsToolStripMenuItem);
        }

        Dispatcher.UIThread.RunJobs();
        form.commandsToolStripMenuItem.IsSubMenuOpen.Should().BeTrue();
    }

    private static string[] ShowBranchSuggestions(FormBrowse form)
    {
        FilterToolBar.TestAccessor filter = form.GetTestAccessor().ToolStripFilters.GetTestAccessor();
        filter.tscboBranchFilter.IsDropDownOpen = false;
        filter.tscboBranchFilter.Text = string.Empty;
        filter.tscboBranchFilter.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        string[] suggestions = filter.tscboBranchFilter.Items.OfType<string>().ToArray();
        filter.tscboBranchFilter.IsDropDownOpen = false;
        return suggestions;
    }

    private static TopLevel GetRoot(Control control)
        => TopLevel.GetTopLevel(control) ?? throw new InvalidOperationException("The input target must be attached to its actual top level.");

    private static Point GetClickPoint(TopLevel root, Control control)
    {
        using WriteableBitmap? frame = root.CaptureRenderedFrame();
        return control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), root)
            ?? throw new InvalidOperationException("The rendered input target must have a position.");
    }

    private static void Click(TopLevel root, Control control, MouseButton button = MouseButton.Left, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        Point point = GetClickPoint(root, control);
        root.MouseDown(point, button, modifiers);
        root.MouseUp(point, button, modifiers);
        Dispatcher.UIThread.RunJobs();
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the repository reload should complete before the timeout");
    }

    private void DisposeFixture()
    {
        try
        {
            _serviceContainer?.Dispose();
        }
        finally
        {
            try
            {
                if (_originalSettingsContainer is not null)
                {
                    SetSettingsContainer(_originalSettingsContainer);
                    AppSettings.SettingsContainer.Should().BeSameAs(_originalSettingsContainer);
                    AppSettings.GetTestAccessor().ApplicationDataPath = _originalApplicationDataPath;
                    AppSettings.GetTestAccessor().ApplicationDataPath.Should().BeSameAs(_originalApplicationDataPath);
                }

                _isolatedSettingsCache?.Dispose();
                _isolatedSettingsCache = null;
            }
            finally
            {
                ThreadHelper.JoinableTaskContext = _previousContext!;
                _context.Dispose();
            }
        }

        TestDirectory.Delete(_workingDirectory);
    }

    private static void SetSettingsContainer(DistributedSettings settings)
    {
        // UsingContainer holds the previous cache lock across its synchronous action;
        // this async fixture must replace only the reference, as the assembly isolation does.
        SettingsContainerSetter.Invoke(obj: null, [settings]);
    }

    private sealed class PositionManager : IWindowPositionManager
    {
        public WindowPosition? LoadPosition(Window form)
            => null;

        public void SavePosition(Window form)
        {
            // Source-action tests intentionally do not persist window geometry.
        }
    }
}
