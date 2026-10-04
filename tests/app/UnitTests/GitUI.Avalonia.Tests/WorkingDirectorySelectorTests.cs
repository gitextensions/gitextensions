using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using GitUI.Hotkey;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class WorkingDirectorySelectorTests
{
    [SetUp]
    public void SetUp()
        => GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void FormBrowse_should_construct_with_the_same_named_working_directory_split_button()
    {
        FormBrowse form = new();

        WorkingDirectoryToolStripSplitButton selector =
            form.FindControl<WorkingDirectoryToolStripSplitButton>("_NO_TRANSLATE_WorkingDir")
            ?? throw new InvalidOperationException("The working-directory selector was not created.");

        selector.Icon.Should().BeSameAs(GitUI.Properties.Images.RepoOpen);
        selector.Flyout.Should().BeOfType<MenuFlyout>();
        selector.Height.Should().Be(22, "the original 96-DPI Designer specifies a 22-pixel toolbar item");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [NonParallelizable]
    public void WorkingDirectoryToolStripSplitButton_should_build_and_filter_favourites_and_recent_repositories()
    {
        int originalMaximum = AppSettings.MaxTopRepositories;
        bool originalHideTop = AppSettings.HideTopRepositoriesFromRecentList.Value;
        bool originalSortTop = AppSettings.SortTopRepos;
        bool originalSortRecent = AppSettings.SortRecentRepos;
        ShorteningRecentRepoPathStrategy originalShortening = AppSettings.ShorteningRecentRepoPathStrategy;
        try
        {
            AppSettings.MaxTopRepositories = 1;
            AppSettings.HideTopRepositoriesFromRecentList.Value = true;
            AppSettings.SortTopRepos = false;
            AppSettings.SortRecentRepos = false;
            AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.None;
            Repository favourite = new(@"C:\repos\favourite")
            {
                Category = "Team",
                Anchor = Repository.RepositoryAnchor.AnchoredInTop,
            };
            Repository alpha = new(@"C:\repos\alpha");
            Repository beta = new(@"C:\repos\beta") { Anchor = Repository.RepositoryAnchor.AnchoredInRecent };
            WorkingDirectoryToolStripSplitButton selector = new();
            WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();

            accessor.FillDropDown([favourite], [alpha, beta]);

            MenuItem[] repositoryItems = Flatten(accessor.Menu.Items)
                .Where(item => item.Tag is RecentRepoInfo)
                .ToArray();
            repositoryItems.Should().HaveCount(3);
            repositoryItems.Select(item => ((RecentRepoInfo)item.Tag!).Repo.Path)
                .Should().BeEquivalentTo(favourite.Path, alpha.Path, beta.Path);
            accessor.Menu.Items.OfType<MenuItem>()
                .Should().Contain(item => item.Header as string == "_Favorite repositories");
            MenuItem favouriteMenu = accessor.Menu.Items.OfType<MenuItem>()
                .Single(item => item.Header as string == "_Favorite repositories");
            favouriteMenu.Icon.Should().BeOfType<Image>().Which.Source.Should().BeSameAs(GitUI.Properties.Images.Star);
            repositoryItems.Single(item => ((RecentRepoInfo)item.Tag!).Repo.Path == favourite.Path)
                .Icon.Should().BeNull("native favourite children call AddRecentRepositories without its anchored opt-in");
            repositoryItems.Single(item => ((RecentRepoInfo)item.Tag!).Repo.Path == alpha.Path)
                .Icon.Should().BeNull("membership of the splitter's top group is not an anchor");
            repositoryItems.Single(item => ((RecentRepoInfo)item.Tag!).Repo.Path == beta.Path)
                .Icon.Should().BeOfType<Image>().Which.Source.Should().BeSameAs(GitUI.Properties.Images.Pin);
            MenuItem[] sizedItems = accessor.Menu.Items.OfType<MenuItem>()
                .Where(item => item.Header != accessor.Filter)
                .ToArray();
            sizedItems.Select(item => item.Width).Distinct().Should().ContainSingle(
                "WinForms gives every ordinary ToolStrip row the width of the widest item");
            accessor.Filter.Width.Should().Be(sizedItems[0].Width - 60,
                "the source reserves sixty pixels after the popup's trailing layout border is excluded");

            accessor.Filter.Text = "beta";
            accessor.ApplyFilterForTesting();

            repositoryItems.Single(item => ((RecentRepoInfo)item.Tag!).Repo.Path == beta.Path)
                .IsVisible.Should().BeTrue();
            repositoryItems.Where(item => ((RecentRepoInfo)item.Tag!).Repo.Path == alpha.Path)
                .Should().OnlyContain(item => !item.IsVisible);
            repositoryItems.Single(item => ((RecentRepoInfo)item.Tag!).Repo.Path == favourite.Path)
                .IsVisible.Should().BeTrue("the source excludes the favourite submenu and its children from filtering");
            accessor.Menu.Items.OfType<MenuItem>()
                .Where(item => item.Tag is not RecentRepoInfo)
                .Should().Contain(item => item.IsVisible && item.Header as string == "Open repository");
        }
        finally
        {
            AppSettings.MaxTopRepositories = originalMaximum;
            AppSettings.HideTopRepositoriesFromRecentList.Value = originalHideTop;
            AppSettings.SortTopRepos = originalSortTop;
            AppSettings.SortRecentRepos = originalSortRecent;
            AppSettings.ShorteningRecentRepoPathStrategy = originalShortening;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Working_directory_popup_should_use_source_alignment_insets_and_shortcut_spelling()
    {
        HotkeySettings browse = HotkeySettingsManager.CreateDefaultSettingsCore(scriptsManager: null)
            .Single(settings => settings.Name == FormBrowse.HotkeySettingsName);
        WorkingDirectoryToolStripSplitButton selector = new();
        selector.RefreshShortcutKeys(browse.Commands);
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        accessor.FillDropDown([], []);

        accessor.Menu.Placement.Should().Be(PlacementMode.BottomEdgeAlignedLeft);
        accessor.Menu.FlyoutPresenterClasses.Should().Contain("gitextensions-branch-menu");
        accessor.FilterHost.Height.Should().Be(25);
        MenuItem open = accessor.Menu.Items.OfType<MenuItem>()
            .Single(item => item.Header as string == "Open repository");
        MenuItem close = accessor.Menu.Items.OfType<MenuItem>()
            .Single(item => item.Header as string == "Close (go to Dashboard)");
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(open).Should().Be("Ctrl+O");
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(close).Should().Be("Ctrl+W");
        close.Icon.Should().BeOfType<Image>().Which.Source.Should().BeSameAs(GitUI.Properties.Images.DashboardFolderGit);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(12)]
    public void Working_directory_snapshot_should_keep_exact_top_group_boundary_numbers_and_source_icons(int topCount)
    {
        RepositoryHistoryEntry[] recent = Enumerable.Range(1, 12)
            .Select(number => new RepositoryHistoryEntry(
                new Repository($"/repos/repository-{number}"), $"repository-{number}", $"branch-{number}",
                IsFavourite: false, IsAnchored: number == 10))
            .ToArray();
        RepositoryHistoryEntry favourite = new(
            new Repository("/repos/favourite") { Category = "Team" }, "favourite", "main",
            IsFavourite: true, IsAnchored: true);
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();

        accessor.FillDropDown(new RepositoryHistorySnapshot(recent, [favourite]) { TopCount = topCount });

        MenuItem[] recentItems = accessor.Menu.Items.OfType<MenuItem>()
            .Where(item => item.Tag is RepositoryHistoryEntry)
            .ToArray();
        recentItems.Select(item => item.Tag).Should().Equal(recent);
        recentItems[0].Header.Should().Be("_1: repository-1");
        recentItems[9].Header.Should().Be("1_0: repository-10");
        recentItems[10].Header.Should().Be("11: repository-11");
        recentItems[0].Icon.Should().BeNull();
        recentItems[9].Icon.Should().BeOfType<Image>().Which.Source.Should().BeSameAs(GitUI.Properties.Images.Pin);
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(recentItems[9]).Should().Be("branch-10");
        accessor.Menu.Items.OfType<Separator>().Should().HaveCount(topCount is > 0 and < 12 ? 4 : 3);
        if (topCount is > 0 and < 12)
        {
            int boundary = accessor.Menu.Items.IndexOf(recentItems[topCount - 1]) + 1;
            accessor.Menu.Items[boundary].Should().BeOfType<Separator>();
            accessor.Menu.Items[boundary + 1].Should().BeSameAs(recentItems[topCount],
                "the service's source splitter boundary does not depend on recent-item anchor flags");
        }

        MenuItem favouriteMenu = accessor.Menu.Items.OfType<MenuItem>()
            .Single(item => item.Header as string == "_Favorite repositories");
        favouriteMenu.Icon.Should().BeOfType<Image>().Which.Source.Should().BeSameAs(GitUI.Properties.Images.Star);
        MenuItem favouriteItem = Flatten(favouriteMenu.Items).Single(item => item.Tag is RepositoryHistoryEntry);
        favouriteItem.Icon.Should().BeNull();
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(favouriteItem).Should().Be("main");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [NonParallelizable]
    [TestCase(false, "repo_main", "team_core", "repo__main", "team__core")]
    [TestCase(true, "repo_main", "team_core", "repo__main", "team__core")]
    [TestCase(false, "work_tree", "team_core", "work__tree", "team__core")]
    [TestCase(true, "work_tree", "team_core", "work__tree", "team__core")]
    [TestCase(false, "repo&main", "team&core", "repo_main", "team_core")]
    [TestCase(true, "repo&main", "team&core", "repo_main", "team_core")]
    [TestCase(false, "repo&&main", "team&&core", "repo&main", "team&core")]
    [TestCase(true, "repo&&main", "team&&core", "repo&main", "team&core")]
    public void Working_directory_raw_and_snapshot_captions_should_preserve_source_mnemonics_and_literal_underscores(
        bool snapshot, string caption, string category, string mappedCaption, string mappedCategory)
    {
        int originalMaximum = AppSettings.MaxTopRepositories;
        bool originalHideTop = AppSettings.HideTopRepositoriesFromRecentList.Value;
        bool originalSortTop = AppSettings.SortTopRepos;
        bool originalSortRecent = AppSettings.SortRecentRepos;
        ShorteningRecentRepoPathStrategy originalShortening = AppSettings.ShorteningRecentRepoPathStrategy;
        try
        {
            AppSettings.MaxTopRepositories = 0;
            AppSettings.HideTopRepositoriesFromRecentList.Value = true;
            AppSettings.SortTopRepos = false;
            AppSettings.SortRecentRepos = false;
            AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.None;
            Repository[] recent = Enumerable.Range(1, 10).Select(number => new Repository($"repository-{number}"))
                .Append(new Repository(caption))
                .ToArray();
            Repository favourite = new("favourite") { Category = category };
            WorkingDirectoryToolStripSplitButton selector = new();
            WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
            if (snapshot)
            {
                accessor.FillDropDown(new RepositoryHistorySnapshot(
                    recent.Select(repository => new RepositoryHistoryEntry(
                        repository, repository.Path, null, IsFavourite: false, IsAnchored: false)).ToArray(),
                    [new RepositoryHistoryEntry(favourite, "favourite", null, IsFavourite: true, IsAnchored: false)]));
            }
            else
            {
                accessor.FillDropDown([favourite], recent);
            }

            MenuItem[] items = accessor.Menu.Items.OfType<MenuItem>()
                .Where(item => item.Tag is RecentRepoInfo or RepositoryHistoryEntry)
                .ToArray();
            items.Should().HaveCount(11);
            items[0].Header.Should().Be("_1: repository-1");
            items[9].Header.Should().Be("1_0: repository-10");
            items[10].Header.Should().Be($"11: {mappedCaption}",
                "source number eleven supplies no numeric prefix, so literal underscores cannot become an accidental access key");
            MenuItem favourites = accessor.Menu.Items.OfType<MenuItem>()
                .Single(item => item.Header as string == "_Favorite repositories");
            favourites.Items.OfType<MenuItem>().Single().Header.Should().Be(mappedCategory);

            accessor.Filter.Text = caption;
            accessor.ApplyFilterForTesting();
            items[10].IsVisible.Should().BeTrue("source filtering searches the original caption, not its escaped AccessText Header");
            if (caption.Contains('_', StringComparison.Ordinal))
            {
                accessor.Filter.Text = "__";
                accessor.ApplyFilterForTesting();
                items[10].IsVisible.Should().BeFalse("escaping one literal underscore must not add searchable underscores");
            }
            else
            {
                accessor.Filter.Text = caption.Contains("&&", StringComparison.Ordinal) ? "repo&main" : "repo&&main";
                accessor.ApplyFilterForTesting();
                items[10].IsVisible.Should().BeFalse("native literal and mnemonic ampersands remain distinct in raw ToolStripItem.Text");
            }

            accessor.Filter.Text = "&1:";
            accessor.ApplyFilterForTesting();
            items[0].IsVisible.Should().BeTrue("the native numeric mnemonic marker remains part of searchable item text");
            accessor.Filter.Text = "_1:";
            accessor.ApplyFilterForTesting();
            items[0].IsVisible.Should().BeFalse("the Avalonia mnemonic marker is not source filter text");
        }
        finally
        {
            AppSettings.MaxTopRepositories = originalMaximum;
            AppSettings.HideTopRepositoriesFromRecentList.Value = originalHideTop;
            AppSettings.SortTopRepos = originalSortTop;
            AppSettings.SortRecentRepos = originalSortRecent;
            AppSettings.ShorteningRecentRepoPathStrategy = originalShortening;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false)]
    [TestCase(true)]
    public void Working_directory_shortcut_refresh_should_update_retained_fixed_items_without_rebuilding(bool openPopup)
    {
        HotkeySettings browse = HotkeySettingsManager.CreateDefaultSettingsCore(scriptsManager: null)
            .Single(settings => settings.Name == FormBrowse.HotkeySettingsName);
        WorkingDirectoryToolStripSplitButton selector = new();
        selector.RefreshShortcutKeys(browse.Commands);
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        accessor.PrepareDropDown([], []);
        Window window = new() { Width = 480, Height = 100, Content = selector };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            if (openPopup)
            {
                accessor.Menu.ShowAt(selector);
                Dispatcher.UIThread.RunJobs();
            }

            object?[] retainedItems = accessor.Menu.Items.ToArray();
            accessor.Filter.Text = "retained search";
            MenuItem open = accessor.Menu.Items.OfType<MenuItem>()
                .Single(item => item.Header as string == "Open repository");
            MenuItem close = accessor.Menu.Items.OfType<MenuItem>()
                .Single(item => item.Header as string == "Close (go to Dashboard)");
            HotkeyCommand openCommand = browse.Commands!.Single(command => command.CommandCode == (int)FormBrowse.Command.OpenRepo);
            HotkeyCommand closeCommand = browse.Commands!.Single(command => command.CommandCode == (int)FormBrowse.Command.CloseRepository);
            openCommand.KeyData = GitExtensions.Shims.WinForms.Keys.Control | GitExtensions.Shims.WinForms.Keys.Oemcomma;
            closeCommand.KeyData = GitExtensions.Shims.WinForms.Keys.Shift | GitExtensions.Shims.WinForms.Keys.W;

            selector.RefreshShortcutKeys(browse.Commands);
            Dispatcher.UIThread.RunJobs();

            accessor.Menu.Items.Should().Equal(retainedItems);
            accessor.Filter.Text.Should().Be("retained search");
            accessor.Menu.IsOpen.Should().Be(openPopup);
            open.InputGesture.Should().Be(KeysMapper.ToKeyGesture(openCommand.KeyData));
            close.InputGesture.Should().Be(new KeyGesture(Key.W, KeyModifiers.Shift));
            WinFormsToolStripMenuSizer.GetShortcutDisplayString(open).Should().Be("Ctrl+,");
            WinFormsToolStripMenuSizer.GetShortcutDisplayString(close).Should().Be("Shift+W");
            if (openPopup)
            {
                open.GetVisualDescendants().OfType<TextBlock>()
                    .Single(block => block.Name == "PART_InputGestureText").Text.Should().Be("Ctrl+,");
                close.GetVisualDescendants().OfType<TextBlock>()
                    .Single(block => block.Name == "PART_InputGestureText").Text.Should().Be("Shift+W");
            }
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Working_directory_selector_should_ignore_refresh_until_initialized()
    {
        WorkingDirectoryToolStripSplitButton selector = new() { Content = "WorkingDir" };

        selector.RefreshContent();

        selector.Content.Should().Be("WorkingDir");
    }

    [AvaloniaTest]
    public void Working_directory_selector_should_show_branch_hints_but_filter_only_source_captions()
    {
        Repository alpha = new(@"C:\repos\alpha");
        Repository beta = new(@"C:\repos\beta");
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        accessor.FillDropDown(new RepositoryHistorySnapshot(
            [
                new RepositoryHistoryEntry(alpha, "alpha", "main", IsFavourite: false, IsAnchored: false),
                new RepositoryHistoryEntry(beta, "beta", "feature", IsFavourite: false, IsAnchored: false),
            ],
            []));

        MenuItem[] repositoryItems = Flatten(accessor.Menu.Items)
            .Where(item => item.Tag is RepositoryHistoryEntry)
            .ToArray();
        repositoryItems.Should().HaveCount(2);
        MenuItem betaItem = repositoryItems.Single(
            item => ((RepositoryHistoryEntry)item.Tag!).Repository.Path == beta.Path);
        betaItem.Header.Should().Be("_2: beta");
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(betaItem).Should().Be("feature");

        accessor.Filter.Text = "feature";
        accessor.ApplyFilterForTesting();

        repositoryItems.Single(item => ((RepositoryHistoryEntry)item.Tag!).Repository.Path == alpha.Path)
            .IsVisible.Should().BeFalse();
        repositoryItems.Single(item => ((RepositoryHistoryEntry)item.Tag!).Repository.Path == beta.Path)
            .IsVisible.Should().BeFalse("branch hints are shortcut text, not the native item caption");

        accessor.Filter.Text = "beta";
        accessor.ApplyFilterForTesting();
        betaItem.IsVisible.Should().BeTrue();
        accessor.Filter.Text = " beta ";
        accessor.ApplyFilterForTesting();
        betaItem.IsVisible.Should().BeFalse("the source does not trim a nonempty search expression");
        accessor.Filter.Text = " ";
        accessor.ApplyFilterForTesting();
        repositoryItems.Should().OnlyContain(item => item.IsVisible);
    }

    [AvaloniaTest]
    public void WorkingDirectoryToolStripSplitButton_primary_and_arrow_clicks_should_open_repository_menu()
    {
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        Repository alpha = new(@"C:\repos\alpha");
        Repository beta = new(@"C:\repos\beta");
        accessor.PrepareDropDown([], [alpha, beta]);
        Window window = new()
        {
            Width = 320,
            Height = 80,
            Content = selector,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Button[] templateButtons = selector.GetVisualDescendants()
                .OfType<Button>()
                .Where(button => button.Name is "PART_PrimaryButton" or "PART_SecondaryButton")
                .ToArray();
            Button primaryButton = templateButtons.Single(button => button.Name == "PART_PrimaryButton");
            Button secondaryButton = templateButtons.Single(button => button.Name == "PART_SecondaryButton");

            Click(window, primaryButton, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.IsOpen.Should().BeTrue("the main button mirrors WinForms ButtonClick/ShowDropDown");

            TopLevel popup = TopLevel.GetTopLevel(accessor.Filter)
                ?? throw new InvalidOperationException("The repository flyout was not attached to a top level.");
            Click(popup, accessor.Filter, MouseButton.Left);
            popup.KeyTextInput("beta");
            Dispatcher.UIThread.RunJobs();

            accessor.Filter.Text.Should().Be("beta");
            MenuItem[] repositoryItems = Flatten(accessor.Menu.Items)
                .Where(item => item.Tag is RecentRepoInfo)
                .ToArray();
            repositoryItems.Single(item => ((RecentRepoInfo)item.Tag!).Repo.Path == alpha.Path)
                .IsVisible.Should().BeFalse();
            repositoryItems.Single(item => ((RecentRepoInfo)item.Tag!).Repo.Path == beta.Path)
                .IsVisible.Should().BeTrue();

            accessor.Menu.Hide();
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.IsOpen.Should().BeFalse();

            accessor.PrepareDropDown([], []);
            Click(window, secondaryButton, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.IsOpen.Should().BeTrue("the arrow is the split button's normal flyout trigger");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false)]
    [TestCase(true)]
    public void Working_directory_focused_filter_Escape_should_close_without_clearing_until_actual_reopen(bool rightToLeft)
    {
        WorkingDirectoryToolStripSplitButton selector = new()
        {
            FlowDirection = rightToLeft ? Avalonia.Media.FlowDirection.RightToLeft : Avalonia.Media.FlowDirection.LeftToRight,
        };
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
        history.LoadSnapshot().Returns(new RepositoryHistorySnapshot(
            [
                new RepositoryHistoryEntry(new Repository("/repos/alpha"), "alpha", null, IsFavourite: false, IsAnchored: false),
                new RepositoryHistoryEntry(new Repository("/repos/beta"), "beta", null, IsFavourite: false, IsAnchored: false),
            ], []));
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(string.Empty);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        selector.Initialize(() => commands, history, _ => { }, _ => { }, () => { }, () => { }, () => { });
        Window window = new() { Width = 480, Height = 100, Content = selector };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.ShowAt(selector);
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.IsOpen.Should().BeTrue();
            TextBox retainedFilter = accessor.Filter;
            TopLevel popup = TopLevel.GetTopLevel(retainedFilter)
                ?? throw new InvalidOperationException("The hosted search input must be in the actual open popup.");
            Click(popup, retainedFilter, MouseButton.Left);
            popup.KeyTextInput("beta");
            Dispatcher.UIThread.RunJobs();
            retainedFilter.IsFocused.Should().BeTrue();
            retainedFilter.Text.Should().Be("beta");

            popup.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
            window.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
            Dispatcher.UIThread.RunJobs();

            accessor.Menu.IsOpen.Should().BeFalse();
            retainedFilter.Text.Should().Be("beta", "the native focused input closes the menu without clearing its text");
            history.Received(1).LoadSnapshot();
            accessor.Menu.ShowAt(selector);
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.IsOpen.Should().BeTrue();
            accessor.Filter.Should().BeSameAs(retainedFilter);
            retainedFilter.Text.Should().BeEmpty("only the actual reopened FillDropDown clears the retained native input");
            history.Received(2).LoadSnapshot();
            accessor.Menu.Items.OfType<MenuItem>().Where(item => item.Tag is RepositoryHistoryEntry)
                .Should().OnlyContain(item => item.IsVisible);
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void WorkingDirectoryToolStripSplitButton_right_click_should_open_repository_picker_only()
    {
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        bool openedRepositoryPicker = false;
        accessor.SetOpenRepositoryAction(() => openedRepositoryPicker = true);
        accessor.PrepareDropDown([], []);
        Window window = new()
        {
            Width = 320,
            Height = 80,
            Content = selector,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Button primaryButton = selector.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => button.Name == "PART_PrimaryButton");

            Click(window, primaryButton, MouseButton.Right);
            Dispatcher.UIThread.RunJobs();

            openedRepositoryPicker.Should().BeTrue();
            accessor.Menu.IsOpen.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void WorkingDirectoryToolStripSplitButton_should_route_current_and_new_instance_actions()
    {
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        List<string> current = [];
        List<string> launched = [];
        accessor.SetRepositoryActions(current.Add, launched.Add);

        accessor.OpenRepository("current", openInNewInstance: false);
        accessor.OpenRepository("new", openInNewInstance: true);

        current.Should().Equal("current");
        launched.Should().Equal("new");
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(KeyModifiers.None, false)]
    [TestCase(KeyModifiers.Control, true)]
    [TestCase(KeyModifiers.Control | KeyModifiers.Shift, false)]
    [TestCase(KeyModifiers.Control | KeyModifiers.Alt, false)]
    [TestCase(KeyModifiers.Control | KeyModifiers.Meta, false)]
    [TestCase(KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Alt, false)]
    [TestCase(KeyModifiers.Shift, false)]
    [TestCase(KeyModifiers.Alt, false)]
    public void Working_directory_repository_key_route_should_use_only_the_exact_Control_modifier(
        KeyModifiers modifiers, bool openInNewInstance)
    {
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        List<string> current = [];
        List<string> launched = [];
        accessor.SetRepositoryActions(current.Add, launched.Add);
        accessor.FillDropDown(new RepositoryHistorySnapshot(
            [new RepositoryHistoryEntry(new Repository("/repos/route"), "Route", null, IsFavourite: false, IsAnchored: false)], []));
        MenuItem repository = accessor.Menu.Items.OfType<MenuItem>().Single(item => item.Tag is RepositoryHistoryEntry);

        repository.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.A,
            KeyModifiers = modifiers,
        });
        repository.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        current.Should().Equal(openInNewInstance ? Array.Empty<string>() : new[] { "/repos/route" });
        launched.Should().Equal(openInNewInstance ? new[] { "/repos/route" } : Array.Empty<string>());
        repository.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        current.Should().HaveCount(openInNewInstance ? 1 : 2, "the consumed modifier route must not leak into another click");
        launched.Should().HaveCount(openInNewInstance ? 1 : 0);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(RawInputModifiers.None, false)]
    [TestCase(RawInputModifiers.Control, true)]
    [TestCase(RawInputModifiers.Control | RawInputModifiers.Shift, false)]
    [TestCase(RawInputModifiers.Control | RawInputModifiers.Alt, false)]
    public void Working_directory_repository_pointer_route_should_use_only_the_exact_Control_modifier(
        RawInputModifiers modifiers, bool openInNewInstance)
    {
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        List<string> current = [];
        List<string> launched = [];
        accessor.SetRepositoryActions(current.Add, launched.Add);
        Repository repository = new("/repos/pointer-route");
        accessor.PrepareDropDown([], [repository]);
        Window window = new() { Width = 480, Height = 100, Content = selector };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.ShowAt(selector);
            Dispatcher.UIThread.RunJobs();
            MenuItem item = accessor.Menu.Items.OfType<MenuItem>().First(entry => entry.Tag is RecentRepoInfo);
            TopLevel popup = TopLevel.GetTopLevel(item)
                ?? throw new InvalidOperationException("The repository row must be attached to its actual popup.");

            Click(popup, item, MouseButton.Left, modifiers);
            Dispatcher.UIThread.RunJobs();

            current.Should().Equal(openInNewInstance ? Array.Empty<string>() : new[] { repository.Path });
            launched.Should().Equal(openInNewInstance ? new[] { repository.Path } : Array.Empty<string>());
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Working_directory_new_instance_should_launch_before_current_window_repository_validation()
    {
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
        history.CanOpenRepository(Arg.Any<string>()).Returns(false);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        List<string> current = [];
        List<string> launched = [];
        selector.Initialize(() => commands, history, current.Add, launched.Add, () => { }, () => { }, () => { });

        accessor.OpenRepository("/repos/new-instance", openInNewInstance: true);

        launched.Should().Equal("/repos/new-instance");
        history.DidNotReceive().CanOpenRepository(Arg.Any<string>());
        accessor.OpenRepository("/repos/current-instance", openInNewInstance: false);
        history.Received(1).CanOpenRepository("/repos/current-instance");
        current.Should().BeEmpty();
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void FormRecentReposSettings_should_roundtrip_settings_and_repository_actions()
    {
        int originalMaximum = AppSettings.MaxTopRepositories;
        int originalHistorySize = AppSettings.RecentRepositoriesHistorySize;
        int originalWidth = AppSettings.RecentReposComboMinWidth;
        bool originalHideTop = AppSettings.HideTopRepositoriesFromRecentList.Value;
        bool originalSortTop = AppSettings.SortTopRepos;
        bool originalSortRecent = AppSettings.SortRecentRepos;
        ShorteningRecentRepoPathStrategy originalShortening = AppSettings.ShorteningRecentRepoPathStrategy;
        FormRecentReposSettings? form = null;
        try
        {
            AppSettings.MaxTopRepositories = 1;
            AppSettings.RecentRepositoriesHistorySize = 20;
            AppSettings.RecentReposComboMinWidth = 100;
            AppSettings.HideTopRepositoriesFromRecentList.Value = true;
            AppSettings.SortTopRepos = false;
            AppSettings.SortRecentRepos = false;
            AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.None;
            Repository first = new(@"C:\repos\first");
            Repository second = new(@"C:\repos\second");
            IList<Repository>? saved = null;
            form = new(
                [first, second],
                repositories =>
                {
                    saved = [.. repositories];
                    return Task.CompletedTask;
                });
            FormRecentReposSettings.TestAccessor accessor = form.GetTestAccessor();
            accessor.TopRepositories.Items.Cast<RecentRepoInfo>().Should().ContainSingle();
            accessor.RecentRepositories.Items.Cast<RecentRepoInfo>().Should().ContainSingle();

            RecentRepoInfo top = accessor.TopRepositories.Items.Cast<RecentRepoInfo>().Single();
            accessor.TopRepositories.SelectedItem = top;
            accessor.SetContextList(accessor.TopRepositories);
            accessor.AnchorSelectedToRecent();
            first.Anchor.Should().Be(Repository.RepositoryAnchor.AnchoredInRecent);
            accessor.RecentRepositories.SelectedItem = accessor.RecentRepositories.Items
                .Cast<RecentRepoInfo>()
                .Single(item => item.Repo.Path == first.Path);
            accessor.SetContextList(accessor.RecentRepositories);
            accessor.RemoveSelectedAnchor();
            first.Anchor.Should().Be(Repository.RepositoryAnchor.None);

            RecentRepoInfo recent = accessor.RecentRepositories.Items.Cast<RecentRepoInfo>().Single();
            accessor.RecentRepositories.SelectedItem = recent;
            accessor.SetContextList(accessor.RecentRepositories);
            accessor.RemoveSelectedRecent();
            accessor.RecentRepositories.Items.Cast<RecentRepoInfo>()
                .Should().NotContain(item => item.Repo.Path == second.Path);

            accessor.MaximumTopRepositories.Value = 2;
            accessor.HistorySize.Value = 30;
            accessor.MinimumWidth.Value = 140;
            accessor.HideTopRepositories.IsChecked = false;
            accessor.SortTopRepositories.IsChecked = true;
            accessor.SortRecentRepositories.IsChecked = true;
            accessor.MiddleDots.IsChecked = true;
            accessor.SaveSettings();

            AppSettings.MaxTopRepositories.Should().Be(2);
            AppSettings.RecentRepositoriesHistorySize.Should().Be(30);
            AppSettings.RecentReposComboMinWidth.Should().Be(140);
            AppSettings.HideTopRepositoriesFromRecentList.Value.Should().BeFalse();
            AppSettings.SortTopRepos.Should().BeTrue();
            AppSettings.SortRecentRepos.Should().BeTrue();
            AppSettings.ShorteningRecentRepoPathStrategy.Should().Be(ShorteningRecentRepoPathStrategy.MiddleDots);
            saved.Should().NotBeNull();
        }
        finally
        {
            form?.Close();
            AppSettings.MaxTopRepositories = originalMaximum;
            AppSettings.RecentRepositoriesHistorySize = originalHistorySize;
            AppSettings.RecentReposComboMinWidth = originalWidth;
            AppSettings.HideTopRepositoriesFromRecentList.Value = originalHideTop;
            AppSettings.SortTopRepos = originalSortTop;
            AppSettings.SortRecentRepos = originalSortRecent;
            AppSettings.ShorteningRecentRepoPathStrategy = originalShortening;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void FormRecentReposSettings_should_ignore_detached_radio_group_transitions()
    {
        ShorteningRecentRepoPathStrategy originalShortening = AppSettings.ShorteningRecentRepoPathStrategy;
        FormRecentReposSettings? first = null;
        FormRecentReposSettings? second = null;
        try
        {
            AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.MiddleDots;
            first = new FormRecentReposSettings([]);

            AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.None;
            Action constructSecondForm = () => second = new FormRecentReposSettings([]);

            constructSecondForm.Should().NotThrow();
        }
        finally
        {
            first?.Close();
            second?.Close();
            AppSettings.ShorteningRecentRepoPathStrategy = originalShortening;
        }
    }

    [AvaloniaTest]
    public void Working_directory_selector_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        WorkingDirectoryToolStripSplitButton selector = new();

        ((ITranslate)selector).AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "_repositorySearchPlaceholder", "Text", "Search repositories...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "tsmiFavouriteRepositories", "Text", "&Favorite repositories");
        translation.DidNotReceive().AddTranslationItem(
            nameof(FormBrowse), "openToolStripMenuItem", "Text", Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(
            nameof(FormBrowse), "closeToolStripMenuItem", "Text", Arg.Any<string>());
    }

    [AvaloniaTest]
    public void FormRecentReposSettings_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        FormRecentReposSettings form = new([]);
        try
        {
            form.AddTranslationItems(translation);

            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "$this", "Text", "Recent repositories settings");
            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "maxRecentRepositories", "Text", "Maximum number of top repositories");
            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "shorteningGB", "Text", "Shortening strategy");
            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "anchorToTopReposToolStripMenuItem", "Text", "Anchor to top repositories");
            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "anchorToRecentReposToolStripMenuItem", "Text", "Anchor to recent repositories");
            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "removeAnchorToolStripMenuItem", "Text", "Remove anchor");
            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "removeRecentToolStripMenuItem", "Text", "Remove from recent repositories");
            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "chdrRepository", "Text", "Header");
            translation.Received(1).AddTranslationItem(
                nameof(FormRecentReposSettings), "chdrRepository1", "Text", "Header");
            translation.DidNotReceive().AddTranslationItem(
                nameof(FormRecentReposSettings), "panel2", "Text", Arg.Any<string>());
            translation.DidNotReceive().AddTranslationItem(
                nameof(FormRecentReposSettings), "panel3", "Text", Arg.Any<string>());
        }
        finally
        {
            form.Close();
        }
    }

    [Test]
    public void HotkeySettingsManager_should_restore_open_and_close_repository_defaults()
    {
        HotkeySettings browse = HotkeySettingsManager.CreateDefaultSettingsCore(scriptsManager: null)
            .Single(settings => settings.Name == FormBrowse.HotkeySettingsName);

        browse.Commands.Should().Contain(command =>
            command.CommandCode == (int)FormBrowse.Command.OpenRepo
            && command.KeyData == (GitExtensions.Shims.WinForms.Keys.Control | GitExtensions.Shims.WinForms.Keys.O));
        browse.Commands.Should().Contain(command =>
            command.CommandCode == (int)FormBrowse.Command.CloseRepository
            && command.KeyData == (GitExtensions.Shims.WinForms.Keys.Control | GitExtensions.Shims.WinForms.Keys.W));
    }

    private static IEnumerable<MenuItem> Flatten(IEnumerable<object?> items)
    {
        foreach (object? item in items)
        {
            if (item is not MenuItem menuItem)
            {
                continue;
            }

            yield return menuItem;
            foreach (MenuItem child in Flatten(menuItem.Items))
            {
                yield return child;
            }
        }
    }

    private static void Click(TopLevel topLevel, Control control, MouseButton button, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        Point clickPoint = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            topLevel) ?? throw new InvalidOperationException("The control position was not available.");
        topLevel.MouseDown(clickPoint, button, modifiers);
        topLevel.MouseUp(clickPoint, button, modifiers);
    }
}
