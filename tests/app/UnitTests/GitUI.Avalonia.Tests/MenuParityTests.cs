using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.Menus;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[Category("P4.2")]
public sealed class MenuParityTests
{
    [AvaloniaTest]
    public void Start_menu_should_populate_shared_recent_and_favourite_history_and_route_selection()
    {
        Repository recent = new(@"C:\repos\recent");
        Repository regular = new(@"C:\repos\regular");
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
        history.LoadSnapshot().Returns(new RepositoryHistorySnapshot(
            [
                new RepositoryHistoryEntry(recent, "Recent caption", "main", IsFavourite: false, IsAnchored: true),
                new RepositoryHistoryEntry(regular, "Regular caption", null, IsFavourite: false, IsAnchored: false),
            ],
            [new RepositoryHistoryEntry(favourite, "Favourite caption", "feature", IsFavourite: true, IsAnchored: false)]));
        history.CanOpenRepository(recent.Path).Returns(true);
        IGitExecutorProvider executorProvider = Substitute.For<IGitExecutorProvider>();
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.WorkingDir.Returns(recent.Path);
        executor.GetGitDirectory().Returns(Path.Join(recent.Path, ".git"));
        executorProvider.GetExecutor(recent.Path).Returns(executor);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(history);
        commands.GetService(typeof(IGitExecutorProvider)).Returns(executorProvider);
        StartToolStripMenuItem menu = new();
        menu.Initialize(() => commands);
        StartToolStripMenuItem.TestAccessor accessor = menu.GetTestAccessor();
        GitModuleEventArgs? transition = null;
        menu.GitModuleChanged += (_, e) => transition = e;

        accessor.FavouriteRepositoriesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
        accessor.RecentRepositoriesMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

        MenuItem category = accessor.FavouriteRepositoriesMenuItem.Items.OfType<MenuItem>().Single();
        category.Header.Should().Be("Team");
        GetHeaderText(category.Items.OfType<MenuItem>().Single()).Should().Equal("_1: Favourite caption", "feature");
        MenuItem recentItem = accessor.RecentRepositoriesMenuItem.Items.OfType<MenuItem>().First();
        GetHeaderText(recentItem).Should().Equal("_1: Recent caption", "main");
        recentItem.Icon.Should().BeOfType<Image>();
        accessor.RecentRepositoriesMenuItem.Items.Should().HaveCount(5);
        accessor.RecentRepositoriesMenuItem.Items.ElementAt(1).Should().BeOfType<Separator>();

        recentItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        transition.Should().NotBeNull();
        transition!.GitModule.WorkingDir.Should().Be(recent.Path);

        return;

        static string?[] GetHeaderText(MenuItem item)
            => ((Grid)item.Header!).Children.OfType<TextBlock>().Select(text => text.Text).ToArray();
    }

    [AvaloniaTest]
    public void Copy_paths_menu_should_preserve_inventory_platform_visibility_and_path_formatting()
    {
        CopyPathsToolStripMenuItem menu = new();
        CopyPathsToolStripMenuItem.TestAccessor accessor = menu.GetTestAccessor();

        menu.Items.OfType<MenuItem>().Select(item => item.Name).Should().Equal(
            "copyRelativePathsPosixToolStripMenuItem",
            "copyRelativePathsNativeToolStripMenuItem",
            "copyFullPathsNativeToolStripMenuItem",
            "copyFullPathsWslToolStripMenuItem",
            "copyFullPathsCygwinToolStripMenuItem");
        accessor.FullNativeMenuItem.FontWeight.Should().Be(Avalonia.Media.FontWeight.Bold);
        accessor.FullWslMenuItem.IsVisible.Should().Be(OperatingSystem.IsWindows());
        accessor.FullCygwinMenuItem.IsVisible.Should().Be(OperatingSystem.IsWindows());
        CopyPathsToolStripMenuItem.TestAccessor.GetFilePaths(
                [@"folder\file.txt", null, @"folder\file.txt", string.Empty],
                string.Empty,
                path => path.Replace('\\', '/'))
            .Should().Be($"folder/file.txt{Environment.NewLine}.");
    }

    [AvaloniaTest]
    public void Copy_paths_menu_should_preserve_FileStatusList_translation_identities()
    {
        FileStatusList list = new();
        ITranslation translation = Substitute.For<ITranslation>();

        list.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FileStatusList), "tsmiCopyPaths", "Text", "Copy &path(s)");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "copyRelativePathsPosixToolStripMenuItem", "Text", "Copy relative path(s) - &POSIX");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "copyFullPathsNativeToolStripMenuItem", "Text", "Copy &full path(s) - native");
    }

    [Test]
    public void Browse_helpers_should_preserve_file_directory_and_tree_sorting_behavior()
    {
        string root = Path.Join(Path.GetTempPath(), $"GitExtensions.P4.2-{Guid.NewGuid():N}");
        string file = Path.Join(root, "file.txt");
        Directory.CreateDirectory(root);
        File.WriteAllText(file, "test");
        try
        {
            FormBrowseUtil.IsFileOrDirectory(file).Should().BeTrue();
            FormBrowseUtil.IsFileOrDirectory(root).Should().BeTrue();
            FormBrowseUtil.FileOrParentDirectoryExists(root).Should().BeTrue();
            FormBrowseUtil.IsFileOrDirectory(Path.Join(root, "missing")).Should().BeFalse();

            GitFileTreeComparer comparer = new();
            GitItem tree = new(0, GitObjectType.Tree, ObjectId.Random(), "z-tree");
            GitItem commit = new(0, GitObjectType.Commit, ObjectId.Random(), "z-commit");
            GitItem blobA = new(0, GitObjectType.Blob, ObjectId.Random(), "a-blob");
            GitItem blobB = new(0, GitObjectType.Blob, ObjectId.Random(), "b-blob");
            comparer.Compare(tree, blobA).Should().BeNegative();
            comparer.Compare(commit, blobA).Should().BeNegative();
            comparer.Compare(blobA, tree).Should().BePositive();
            comparer.Compare(blobA, blobB).Should().BeNegative();
            comparer.Compare(null, tree).Should().BePositive();
            comparer.Compare(null, null).Should().Be(0);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public void Settings_changed_event_args_should_preserve_previous_values()
    {
        SettingsChangedEventArgs args = new("de", CommitInfoPosition.LeftwardFromList);

        args.OldTranslation.Should().Be("de");
        args.OldCommitInfoPosition.Should().Be(CommitInfoPosition.LeftwardFromList);
    }
}
