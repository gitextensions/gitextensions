using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.Menus;
using GitUI.Hotkey;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class WorkingDirectorySelectorTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
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
        selector.Height.Should().Be(23);
    }

    [AvaloniaTest]
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
            Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
            Repository alpha = new(@"C:\repos\alpha");
            Repository beta = new(@"C:\repos\beta");
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

            accessor.Filter.Text = "beta";
            accessor.ApplyFilter();

            repositoryItems.Single(item => ((RecentRepoInfo)item.Tag!).Repo.Path == beta.Path)
                .IsVisible.Should().BeTrue();
            repositoryItems.Where(item => ((RecentRepoInfo)item.Tag!).Repo.Path != beta.Path)
                .Should().OnlyContain(item => !item.IsVisible);
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
            FormRecentReposSettings form = new(
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
    public void Working_directory_selector_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        WorkingDirectoryToolStripSplitButton selector = new();

        ((ITranslate)selector).AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "_repositorySearchPlaceholder", "Text", "Search repositories...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "tsmiFavouriteRepositories", "Text", "&Favorite repositories");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "openToolStripMenuItem", "Text", "Open repository...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "closeToolStripMenuItem", "Text", "&Close repository");
    }

    [AvaloniaTest]
    public void FormRecentReposSettings_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        FormRecentReposSettings form = new([]);

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
            nameof(FormRecentReposSettings), "removeRecentToolStripMenuItem", "Text", "Remove from recent repositories");
    }

    [Test]
    public void HotkeySettingsManager_should_restore_open_and_close_repository_defaults()
    {
        HotkeySettings browse = HotkeySettingsManager.CreateDefaultSettings()
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

    private static void Click(TopLevel topLevel, Control control, MouseButton button)
    {
        Point clickPoint = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            topLevel) ?? throw new InvalidOperationException("The control position was not available.");
        topLevel.MouseDown(clickPoint, button, RawInputModifiers.None);
        topLevel.MouseUp(clickPoint, button, RawInputModifiers.None);
    }
}
