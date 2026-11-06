using System.Reflection;
using System.Runtime.ExceptionServices;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Compat;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitExtensionsTests;

/// <summary>
///  Keeps existing diff-tool menus synchronized without applying retired asynchronous loads.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CustomDiffMergeToolRefreshTests
{
    private const string OldDefaultTool = "old-default";
    private const string OldOtherTool = "old-other";
    private const string NewDefaultTool = "new-default";
    private const string NewOtherTool = "new-other";
    private static readonly FieldInfo ToolsField = typeof(CustomDiffMergeToolCache).GetField("_tools", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("The source-owned custom-tool cache must exist.");
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;
    private object? _previousDiffTools;
    private object? _previousMergeTools;
    private string? _showAvailableDiffTools;
    private string? _showAheadBehindData;
    private FormBrowse? _browse;

    [SetUp]
    public async Task SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
        _previousDiffTools = ToolsField.GetValue(CustomDiffMergeToolCache.DiffToolCache);
        _previousMergeTools = ToolsField.GetValue(CustomDiffMergeToolCache.MergeToolCache);
        _showAvailableDiffTools = AppSettings.GetString("difftools.showavailable", null);
        _showAheadBehindData = AppSettings.GetString("showaheadbehinddata", null);
        AppSettings.ShowAvailableDiffTools = true;
        AppSettings.ShowAheadBehindData = false;
        await CustomDiffMergeToolCache.DiffToolCache.ClearAsync();
        await CustomDiffMergeToolCache.MergeToolCache.ClearAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        ExceptionDispatchInfo? failure = null;
        try
        {
            if (_browse is not null)
            {
                try
                {
                    // Exercise the real cancellation boundary without persisting window settings.
                    MethodInfo closed = typeof(FormBrowse).GetMethod("OnFormClosed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                        ?? throw new AssertionException("The Browse close handler must exist.");
                    closed.Invoke(_browse, [EventArgs.Empty]);
                }
                catch (Exception exception)
                {
                    failure = ExceptionDispatchInfo.Capture(exception);
                    try
                    {
                        FieldInfo lifetimeField = typeof(FormBrowse).GetField("_loadOperationsCancellationTokenSource", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?? throw new AssertionException("The Browse load lifetime must exist.");
                        CancellationTokenSource lifetime = (CancellationTokenSource)lifetimeField.GetValue(_browse)!;
                        if (!lifetime.IsCancellationRequested)
                        {
                            await lifetime.CancelAsync();
                        }
                    }
                    catch (Exception cleanupException)
                    {
                        await TestContext.Error.WriteLineAsync(cleanupException.ToString());
                    }

                    try
                    {
                        _browse.RevisionGridControl.CancelBackgroundTasks();
                        _browse.GetTestAccessor().RevisionDiffControl.CancelBackgroundTasks();
                        _browse.GetTestAccessor().RevisionFileTreeControl.CancelBackgroundTasks();
                    }
                    catch (Exception cleanupException)
                    {
                        await TestContext.Error.WriteLineAsync(cleanupException.ToString());
                    }
                    finally
                    {
                        try
                        {
                            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(20));
                            await _browse.JoinLoadOperationsForTestAsync(cancellation.Token);
                            cancellation.Token.ThrowIfCancellationRequested();
                        }
                        catch (Exception cleanupException)
                        {
                            await TestContext.Error.WriteLineAsync(cleanupException.ToString());
                        }
                    }
                }
            }
        }
        finally
        {
            try
            {
                using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(20));
                await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                Dispatcher.UIThread.RunJobs();
            }
            catch (Exception cleanupException) when (failure is not null)
            {
                await TestContext.Error.WriteLineAsync(cleanupException.ToString());
            }
            finally
            {
                if (_browse is not null)
                {
                    ((IDisposable)_browse).Dispose();
                    _browse = null;
                }

                ToolsField.SetValue(CustomDiffMergeToolCache.DiffToolCache, _previousDiffTools);
                ToolsField.SetValue(CustomDiffMergeToolCache.MergeToolCache, _previousMergeTools);
                AppSettings.SetString("difftools.showavailable", _showAvailableDiffTools!);
                AppSettings.SetString("showaheadbehinddata", _showAheadBehindData!);
                ThreadHelper.JoinableTaskContext = _previousContext!;
                _context.Dispose();
            }
        }

        failure?.Throw();
    }

    [AvaloniaTest]
    public async Task Warm_cache_should_not_populate_menus_for_an_already_cancelled_load()
    {
        IGitModule module = CreateModule(OldDefaultTool, OldOtherTool);
        await CustomDiffMergeToolCache.DiffToolCache.GetToolsAsync(module, delay: 0, CancellationToken.None);
        MenuItem parent = new();
        List<GitUI.CustomDiffMergeTool> menus = [new(parent, (_, _) => { })];
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        CustomDiffMergeToolProvider provider = new();
        Func<Task> load = () => provider.GetTestAccessor().LoadCustomDiffMergeToolsAsync(
            module, menus, isDiff: true, cancellationToken: cancellation.Token);

        await load.Should().ThrowAsync<OperationCanceledException>();

        parent.Items.Should().BeEmpty("cached discovery must not bypass menu-owner cancellation");
    }

    [AvaloniaTest]
    public async Task Queued_retired_load_should_not_append_old_tools_to_the_current_menu()
    {
        IGitModule oldModule = CreateModule(OldDefaultTool, OldOtherTool);
        await CustomDiffMergeToolCache.DiffToolCache.GetToolsAsync(oldModule, delay: 0, CancellationToken.None);
        MenuItem parent = new();
        List<GitUI.CustomDiffMergeTool> menus = [new(parent, (_, _) => { })];
        CustomDiffMergeToolProvider provider = new();
        using CancellationTokenSource retiredCancellation = new();
        using ManualResetEventSlim queued = new();
        Task retired = Task.Run(async () =>
        {
            Task load = provider.GetTestAccessor().LoadCustomDiffMergeToolsAsync(
                oldModule, menus, isDiff: true, cancellationToken: retiredCancellation.Token);
            // A warm-cache call queues its UI mutation before returning the incomplete task.
            queued.Set();
            await load;
        });
        Task current = Task.CompletedTask;
        try
        {
            // Keep the UI thread here until the worker has really queued its callback.
            queued.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
            parent.Items.Should().BeEmpty();
            await retiredCancellation.CancelAsync();
            await CustomDiffMergeToolCache.DiffToolCache.ClearAsync();
            IGitModule currentModule = CreateModule(NewDefaultTool, NewOtherTool);
            current = provider.GetTestAccessor().LoadCustomDiffMergeToolsAsync(currentModule, menus, isDiff: true);
            try
            {
                await retired;
            }
            catch (OperationCanceledException)
            {
                // Observe the cancellation on the task created in this test's context.
            }

            retired.IsCanceled.Should().BeTrue("the queued callback belongs to the retired menu load");
            await current;

            ToolNames(parent).Should().Equal(NewDefaultTool, NewOtherTool);
        }
        finally
        {
            await retiredCancellation.CancelAsync();
            try
            {
                await retired;
            }
            catch (OperationCanceledException)
            {
                // The retired operation is deliberately cancelled, but must still be observed.
            }

            await current;
        }
    }

    [AvaloniaTest]
    public async Task Settings_refresh_should_reload_existing_diff_menus_and_invalidate_the_merge_cache()
    {
        IGitModule module = CreateModule(OldDefaultTool, OldOtherTool);
        await CustomDiffMergeToolCache.DiffToolCache.GetToolsAsync(module, delay: 0, CancellationToken.None);
        await CustomDiffMergeToolCache.MergeToolCache.GetToolsAsync(module, delay: 0, CancellationToken.None);
        _browse = CreateBrowse(module);
        _browse.GetTestAccessor().RevisionDiffControl.LoadCustomDifftools();
        _browse.RevisionGridControl.LoadCustomDifftools();
        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(30));
        await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
        cancellation.Token.ThrowIfCancellationRequested();
        MenuItem[] menus = DiffMenus(_browse);
        foreach (MenuItem menu in menus)
        {
            ToolNames(menu).Should().Equal(OldDefaultTool, OldOtherTool);
        }

        ConfigureTools(module, NewDefaultTool, NewOtherTool);
        MethodInfo settingsChanged = typeof(FormBrowse).GetMethod("HandleSettingsChanged", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new AssertionException("The source-shaped settings handler must exist.");
        settingsChanged.Invoke(_browse, [AppSettings.Translation, AppSettings.CommitInfoPosition]);
        await _browse.JoinLoadOperationsForTestAsync(cancellation.Token);
        cancellation.Token.ThrowIfCancellationRequested();
        ToolsField.GetValue(CustomDiffMergeToolCache.MergeToolCache).Should().BeNull("settings invalidate the independent merge-tool cache too");

        // Join the real delayed consumer loads; do not replace the product's startup delay.
        await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
        cancellation.Token.ThrowIfCancellationRequested();
        foreach (MenuItem menu in menus)
        {
            ToolNames(menu).Should().Equal(NewDefaultTool, NewOtherTool);
            menu.Items.OfType<MenuItem>().First().FontWeight.Should().Be(FontWeight.Bold);
        }

        IEnumerable<string> mergeTools = await CustomDiffMergeToolCache.MergeToolCache.GetToolsAsync(module, delay: 0, cancellation.Token);
        mergeTools.Should().Equal(NewDefaultTool, NewOtherTool);
    }

    private static IGitModule CreateModule(string defaultTool, string otherTool)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(true);
        ConfigureTools(module, defaultTool, otherTool);
        return module;
    }

    private static void ConfigureTools(IGitModule module, string defaultTool, string otherTool)
    {
        module.GetEffectiveSetting(Arg.Any<string>(), Arg.Any<string>()).Returns(defaultTool);
        string output = $"Available tools:\n\t\t{otherTool}\n\t\t{defaultTool}\n";
        module.GetCustomDiffMergeTools(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(output);
    }

    private static FormBrowse CreateBrowse(IGitModule module)
    {
        IHotkeySettingsLoader loader = Substitute.For<IHotkeySettingsLoader>();
        loader.LoadHotkeys(Arg.Any<string>()).Returns([]);
        IAppTitleGenerator title = Substitute.For<IAppTitleGenerator>();
        title.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        commands.GetService(typeof(IAppTitleGenerator)).Returns(title);
        commands.GetService(typeof(GitUI.Shells.IShellProvider)).Returns(new GitUI.Shells.ShellProvider());
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(loader);
        commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(Substitute.For<IRepositoryHistoryUIService>());
        commands.GetService(typeof(IUserRepositoriesListController)).Returns(Substitute.For<IUserRepositoriesListController>());
        commands.GetService(typeof(ILinkFactory)).Returns(Substitute.For<ILinkFactory>());
        return new FormBrowse(commands);
    }

    private static MenuItem[] DiffMenus(FormBrowse form)
    {
        string[] fileMenuNames =
        [
            "tsmiDiffFirstToSelected",
            "tsmiDiffSelectedToLocal",
            "tsmiDiffFirstToLocal",
            "tsmiDiffWithRemembered",
            "tsmiDiffTwoSelected",
        ];
        List<MenuItem> menus = [];
        foreach (string name in fileMenuNames)
        {
            menus.Add(form.fileStatusList.FindControl<MenuItem>(name)
                ?? throw new AssertionException($"The existing file difftool menu {name} must exist."));
        }

        menus.Add(form.RevisionGridControl.FindControl<MenuItem>("openCommitsWithDiffToolMenuItem")
            ?? throw new AssertionException("The existing revision difftool menu must exist."));
        return [.. menus];
    }

    private static IEnumerable<string> ToolNames(MenuItem menu)
        => menu.Items.OfType<MenuItem>().Select(item => item.Tag).OfType<string>();
}
