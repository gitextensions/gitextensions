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
using GitUI.Compat;
using GitUI.Hotkey;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[Category("P4.2")]
public sealed class MenuParityTests
{
    [AvaloniaTest]
    public void Start_menu_should_delegate_repository_history_and_forward_module_changes()
    {
        Repository recent = new(@"C:\repos\recent");
        IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
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

        history.Received(1).PopulateFavouriteRepositoriesMenu(
            Arg.Is<GitUI.Compat.WinFormsControls.ToolStripDropDownItem>(item => ReferenceEquals(item, accessor.FavouriteRepositoriesMenuItem)));
        history.Received(1).PopulateRecentRepositoriesMenu(
            Arg.Is<GitUI.Compat.WinFormsControls.ToolStripDropDownItem>(item => ReferenceEquals(item, accessor.RecentRepositoriesMenuItem)));

        GitModule module = new(executorProvider, recent.Path);
        history.GitModuleChanged += Raise.Event<EventHandler<GitModuleEventArgs>>(history, new GitModuleEventArgs(module));

        transition.Should().NotBeNull();
        transition!.GitModule.WorkingDir.Should().Be(recent.Path);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Start_menu_should_keep_source_shortcut_display_alongside_the_executable_gesture()
    {
        HotkeySettings browse = HotkeySettingsManager.CreateDefaultSettingsCore(scriptsManager: null)
            .Single(settings => settings.Name == FormBrowse.HotkeySettingsName);
        StartToolStripMenuItem menu = new();

        menu.RefreshShortcutKeys(browse.Commands);

        MenuItem open = menu.OpenRepositoryMenuItem;
        open.InputGesture.Should().Be(KeysMapper.ToKeyGesture(
            GitExtensions.Shims.WinForms.Keys.Control | GitExtensions.Shims.WinForms.Keys.O));
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(open).Should().Be("Ctrl+O");
        browse.Commands!.Single(command => command.CommandCode == (int)FormBrowse.Command.OpenRepo).KeyData =
            GitExtensions.Shims.WinForms.Keys.Control | GitExtensions.Shims.WinForms.Keys.Oemcomma;

        menu.RefreshShortcutKeys(browse.Commands);

        open.InputGesture.Should().Be(KeysMapper.ToKeyGesture(
            GitExtensions.Shims.WinForms.Keys.Control | GitExtensions.Shims.WinForms.Keys.Oemcomma));
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(open).Should().Be("Ctrl+,");
        menu.RefreshShortcutKeys(null);
        open.InputGesture.Should().BeNull();
        WinFormsToolStripMenuSizer.GetShortcutDisplayString(open).Should().BeNull();
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
