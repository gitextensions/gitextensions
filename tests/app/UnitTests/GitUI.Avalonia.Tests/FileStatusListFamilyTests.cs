using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Avalonia.Tests;

public sealed class FileStatusListFamilyTests
{
    [Test]
    public void StatusSorter_should_compare_path_segments_before_extensions()
    {
        FileStatusList.StatusSorter.TestAccessor.Compare("dir/sub/file", "dir.ext/file").Should().Be(-1);
        FileStatusList.StatusSorter.TestAccessor.Compare("dir.ext/file", "dir/sub/file").Should().Be(1);
    }

    [TestCase("", "", "")]
    [TestCase("a", "a", "a")]
    [TestCase("a", "b", "")]
    [TestCase("a", "a/b/c", "a")]
    [TestCase("a/b", "a/bc", "a")]
    [TestCase("a/b/cc", "a/b/cc/de", "a/b/cc")]
    public void StatusSorter_should_find_only_complete_common_path_segments(string left, string right, string expected)
    {
        FileStatusList.StatusSorter.TestAccessor.GetCommonPath(left, right).Should().Be(expected);
        FileStatusList.StatusSorter.TestAccessor.GetCommonPath(right, left).Should().Be(expected);
    }

    [Test]
    public void StatusSorter_should_build_folder_nodes_and_optionally_merge_single_files()
    {
        GitItemStatus[] statuses = [new("one/a.txt"), new("one/two/b.txt"), new("root.txt")];
        FileStatusList.StatusSorter sorter = new();

        FileStatusList.StatusNode root = sorter.CreateTreeSortedByPath(
            statuses,
            flat: false,
            mergeSingleItemsWithFolder: false,
            status => new FileStatusList.StatusNode(status.Name) { Tag = status });

        root.Nodes.Should().HaveCount(2);
        root.Nodes[0].Tag.Should().BeOfType<RelativePath>();
        root.Nodes.SelectMany(Flatten).Select(node => node.Tag).OfType<GitItemStatus>()
            .Should().BeEquivalentTo(statuses);

        static IEnumerable<FileStatusList.StatusNode> Flatten(FileStatusList.StatusNode node)
        {
            yield return node;
            foreach (FileStatusList.StatusNode child in node.Nodes.SelectMany(Flatten))
            {
                yield return child;
            }
        }
    }

    [Test]
    public void PathFormatter_should_preserve_filename_priority_while_truncating_old_name()
    {
        TruncatePathMethod original = AppSettings.TruncatePathMethod;
        try
        {
            AppSettings.TruncatePathMethod = TruncatePathMethod.TrimStart;
            const string name = "file.ext";
            const string oldName = "very-long-old-name.ext";
            for (int step = 0; step <= oldName.Length; step++)
            {
                PathFormatter.TestAccessor.FormatString(name, oldName, step).text.Should().Be(name);
            }

            PathFormatter.TestAccessor.FormatString(name, oldName, oldName.Length + 1).text.Should().NotBe(name);
        }
        finally
        {
            AppSettings.TruncatePathMethod = original;
        }
    }

    [TestCase("path/name.ext", null, "name.ext", null)]
    [TestCase("path/name.ext", "old/old.ext", "name.ext", " (old.ext)")]
    public void PathFormatter_should_support_filename_only_mode(string name, string? oldName, string expectedText, string? expectedSuffix)
    {
        PathFormatter.FormatTextForFileNameOnly(name, oldName).Should().Be((expectedText, expectedSuffix));
    }

    [AvaloniaTest]
    public void SortDiffListContextMenuItem_should_requery_and_write_all_six_sort_modes()
    {
        IDiffListSortService service = Substitute.For<IDiffListSortService>();
        service.DiffListSorting.Returns(DiffListSortType.FilePath);
        SortDiffListContextMenuItem menu = new(service);
        SortDiffListContextMenuItem.TestAccessor accessor = menu.GetTestAccessor();

        accessor.Items.Should().HaveCount(6);
        accessor.Items.Single(item => Equals(item.Tag, DiffListSortType.FilePath)).IsChecked.Should().BeTrue();

        service.DiffListSorting.Returns(DiffListSortType.FileStatusFlat);
        accessor.RaiseDropDownOpening();
        accessor.Items.Single(item => Equals(item.Tag, DiffListSortType.FileStatusFlat)).IsChecked.Should().BeTrue();
    }

    [Test]
    public void ContextMenuController_should_gate_local_diff_variants()
    {
        FileStatusListContextMenuController controller = new();
        GitRevision revision = new(ObjectId.Random());

        controller.ShouldShowMenuFirstToSelected(new ContextMenuDiffToolInfo()).Should().BeFalse();
        controller.ShouldShowMenuFirstToLocal(new ContextMenuDiffToolInfo(selectedRevision: revision, localExists: false)).Should().BeFalse();
        controller.ShouldShowMenuSelectedToLocal(new ContextMenuDiffToolInfo(selectedRevision: revision, allAreDeleted: true)).Should().BeFalse();
        controller.ShouldShowMenuSelectedToLocal(new ContextMenuDiffToolInfo(selectedRevision: revision)).Should().BeTrue();
    }

    [Test]
    public void RememberController_should_emit_commitish_for_revision_and_worktree()
    {
        RememberFileContextMenuController controller = new();
        ObjectId id = ObjectId.Random();
        FileStatusItem revisionItem = new(new GitRevision(id), new GitRevision(id), new GitItemStatus("new.txt") { OldName = "old.txt" });
        FileStatusItem worktreeItem = new(new GitRevision(id), new GitRevision(ObjectId.WorkTreeId), new GitItemStatus("work.txt"));

        controller.GetGitCommit(null, revisionItem, isSecondRevision: false).Should().Be($"{id}:old.txt");
        controller.GetGitCommit(null, worktreeItem, isSecondRevision: true).Should().Be("work.txt");
    }

    [Test]
    public void RevisionDiff_hotkeys_should_preserve_all_upstream_commands_and_defaults()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsLoader loader = new HotkeySettingsManager();

            IReadOnlyList<HotkeyCommand> hotkeys = loader.LoadHotkeys(RevisionDiffControl.HotkeySettingsName);

            hotkeys.Should().HaveCount(Enum.GetValues<RevisionDiffControl.Command>().Length);
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)RevisionDiffControl.Command.DeleteSelectedFiles
                && command.KeyData == WinFormsShims.Keys.Delete);
            hotkeys.Should().ContainSingle(command =>
                command.CommandCode == (int)RevisionDiffControl.Command.OpenWorkingDirectoryFileWith
                && command.KeyData == (WinFormsShims.Keys.Shift | WinFormsShims.Keys.Control | WinFormsShims.Keys.F4));
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_expose_original_toolbar_and_full_context_menu_shape()
    {
        FileStatusList control = new();
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();

        accessor.Toolbar.Children.Should().NotBeEmpty();
        control.FindControl<MenuItem>("tsmiUpdateSubmodule").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiResetFileTo").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiOpenWithDifftool").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiSaveAs").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiSkipWorktree").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiStopTracking").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FileStatusList_should_apply_all_sort_and_branch_diff_filters()
    {
        FileStatusList control = new() { GroupByRevision = true };
        GitRevision first = new(ObjectId.Random());
        GitRevision second = new(ObjectId.Random());
        control.SetDiffs(
        [
            new FileStatusWithDescription(
                first,
                second,
                "branch diff",
                [
                    new GitItemStatus("only-a.txt") { IsChanged = true, IsTracked = true, DiffStatus = DiffBranchStatus.OnlyAChange },
                    new GitItemStatus("only-b.cs") { IsChanged = true, IsTracked = true, DiffStatus = DiffBranchStatus.OnlyBChange },
                ],
                iconName: nameof(Images.DiffA)),
        ],
        isFileTreeMode: false);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();

        foreach (DiffListSortType sortType in Enum.GetValues<DiffListSortType>())
        {
            accessor.SetSort(sortType);
            control.GitItemFilteredStatuses.Should().HaveCount(2);
        }

        accessor.SetDiffStatusVisible(DiffBranchStatus.OnlyAChange, visible: false);
        control.GitItemFilteredStatuses.Should().ContainSingle().Which.Name.Should().Be("only-b.cs");
    }

    [AvaloniaTest]
    public void FileStatusList_should_persist_git_grep_mode_and_options_from_the_original_toolbar_routes()
    {
        bool originalIgnoreCase = AppSettings.GitGrepIgnoreCase.Value;
        bool originalWholeWord = AppSettings.GitGrepMatchWholeWord.Value;
        string originalArguments = AppSettings.GitGrepUserArguments.Value;
        int originalTypeIndex = AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value;
        bool originalVisible = AppSettings.ShowFindInCommitFilesGitGrep.Value;
        try
        {
            AppSettings.GitGrepIgnoreCase.Value = false;
            AppSettings.GitGrepMatchWholeWord.Value = true;
            AppSettings.GitGrepUserArguments.Value = "--extended-regexp";
            AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value = 1;
            AppSettings.ShowFindInCommitFilesGitGrep.Value = false;
            FileStatusList control = new();
            FileStatusList.TestAccessor accessor = control.GetTestAccessor();

            accessor.OpenFindInFilesMenu();

            accessor.FindUsingMatchCaseMenuItem.IsChecked.Should().BeTrue();
            accessor.FindUsingWholeWordMenuItem.IsChecked.Should().BeTrue();
            accessor.FindUsingOptionsMenuItem.Header.Should().Be("_Options: --extended-regexp");

            accessor.FindUsingMatchCaseMenuItem.IsChecked = false;
            accessor.FindUsingMatchCaseMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            AppSettings.GitGrepIgnoreCase.Value.Should().BeTrue();

            accessor.FindUsingWholeWordMenuItem.IsChecked = false;
            accessor.FindUsingWholeWordMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            AppSettings.GitGrepMatchWholeWord.Value.Should().BeFalse();

            accessor.FindUsingBasicMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            AppSettings.GitGrepUserArguments.Value.Should().Be("--basic-regexp");

            control.CanUseFindInCommitFilesGitGrep = true;
            accessor.UpdateToolbar();
            accessor.FindUsingInputBoxMenuItem.IsChecked = true;
            accessor.FindUsingInputBoxMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value.Should().Be(1);
            AppSettings.ShowFindInCommitFilesGitGrep.Value.Should().BeTrue();
            accessor.FindInFilesPanel.IsVisible.Should().BeTrue();

            accessor.ToggleFindInFiles();
            AppSettings.ShowFindInCommitFilesGitGrep.Value.Should().BeFalse();
            accessor.FindInFilesPanel.IsVisible.Should().BeFalse();
        }
        finally
        {
            AppSettings.GitGrepIgnoreCase.Value = originalIgnoreCase;
            AppSettings.GitGrepMatchWholeWord.Value = originalWholeWord;
            AppSettings.GitGrepUserArguments.Value = originalArguments;
            AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value = originalTypeIndex;
            AppSettings.ShowFindInCommitFilesGitGrep.Value = originalVisible;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_persist_each_toolbar_item_visibility()
    {
        const string settingsKey = "FileStatusList.Toolbar.Visibility.btnByPath";
        bool originalValue = AppSettings.GetBool(settingsKey, defaultValue: true);
        try
        {
            AppSettings.SetBool(settingsKey, false);
            FileStatusList control = new();
            FileStatusList.TestAccessor accessor = control.GetTestAccessor();
            int buttonIndex = accessor.Toolbar.Children.IndexOf(accessor.ByPathButton);
            MenuItem visibilityItem = (MenuItem)accessor.ToolbarMenuItem.Items[buttonIndex]!;

            visibilityItem.IsChecked.Should().BeFalse();
            accessor.ByPathButton.IsVisible.Should().BeFalse();

            visibilityItem.IsChecked = true;
            visibilityItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            AppSettings.GetBool(settingsKey, defaultValue: true).Should().BeTrue();
            accessor.ByPathButton.IsVisible.Should().BeTrue();
        }
        finally
        {
            AppSettings.SetBool(settingsKey, originalValue ? null : false);
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_restore_refresh_settings_and_file_tree_toolbar_boundaries()
    {
        bool originalShowAllParents = AppSettings.ShowDiffForAllParents;
        try
        {
            AppSettings.ShowDiffForAllParents = true;
            FileStatusList control = new();
            FileStatusList.TestAccessor accessor = control.GetTestAccessor();

            control.Bind(() => { }, canAutoRefresh: false);

            accessor.RefreshButton.IsVisible.Should().BeTrue();
            accessor.RefreshOnFormFocusMenuItem.IsVisible.Should().BeFalse();
            accessor.ToolbarSeparator.IsVisible.Should().BeFalse();

            accessor.EnableShowDiffForAllParents();
            accessor.OpenSettingsMenu();
            accessor.ShowDiffForAllParentsMenuItem.IsVisible.Should().BeTrue();
            accessor.ShowDiffForAllParentsMenuItem.IsChecked.Should().BeTrue();

            control.SetDiffs([], isFileTreeMode: true);

            accessor.Toolbar.IsVisible.Should().BeFalse();
            accessor.Splitter.Height.Should().Be(1);

            FileStatusList autoRefreshControl = new();
            FileStatusList.TestAccessor autoRefreshAccessor = autoRefreshControl.GetTestAccessor();
            autoRefreshControl.Bind(() => { }, canAutoRefresh: true);
            autoRefreshAccessor.RefreshOnFormFocusMenuItem.IsVisible.Should().BeTrue();
            autoRefreshAccessor.ToolbarSeparator.IsVisible.Should().BeTrue();
        }
        finally
        {
            AppSettings.ShowDiffForAllParents = originalShowAllParents;
        }
    }
}
