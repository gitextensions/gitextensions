using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
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
        MenuItem[] items = [.. menu.Items.OfType<MenuItem>()];

        items.Should().HaveCount(6);
        items.Single(item => Equals(item.Tag, DiffListSortType.FilePath)).IsChecked.Should().BeTrue();

        service.DiffListSorting.Returns(DiffListSortType.FileStatusFlat);
        accessor.RaiseDropDownOpening();
        items.Single(item => Equals(item.Tag, DiffListSortType.FileStatusFlat)).IsChecked.Should().BeTrue();
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
        control.TranslateItems(Substitute.For<ITranslation>());

        control.FindControl<StackPanel>("Toolbar")!.Children.Should().NotBeEmpty();
        control.FindControl<MenuItem>("tsmiUpdateSubmodule").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiResetFileTo").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiOpenWithDifftool").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiSaveAs").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiSkipWorktree").Should().NotBeNull();
        control.FindControl<MenuItem>("tsmiStopTracking").Should().NotBeNull();
        ToolTip.GetTip(control.FindControl<Control>("btnSettings")!).Should().Be("Settings");
        AssertFilterButton("btnUnequalChange", "#FFFF0000");
        AssertFilterButton("btnOnlyB", "#FFBD7CFF");
        AssertFilterButton("btnOnlyA", "#FFA8A800");
        AssertFilterButton("btnSameChange", "#FF00A800");

        return;

        void AssertFilterButton(string name, string color)
        {
            ToggleButton button = control.FindControl<ToggleButton>(name)!;
            button.Width.Should().Be(23);
            button.Foreground.Should().BeAssignableTo<ISolidColorBrush>().Which.Color.Should().Be(Color.Parse(color));
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_collapse_the_filter_row_only_for_the_empty_state()
    {
        FileStatusList control = new();
        TextBlock noFiles = control.FindControl<TextBlock>("NoFiles")!;

        control.ClearDiffs();

        noFiles.IsVisible.Should().BeTrue();
        Control filterRow = (Control)control.FindControl<ComboBox>("cboFilterComboBox")!.Parent!;
        filterRow.IsVisible.Should().BeFalse();
        control.FindControl<ListBox>("lstFiles")!.IsVisible.Should().BeTrue("the WinForms list remains beneath the empty-state label");
        control.FindControl<Button>("btnRefresh")!.IsEnabled.Should().BeFalse();
        control.FindControl<MenuItem>("tsmiShowSkipWorktreeFiles")!.IsEnabled.Should().BeFalse();
        control.FindControl<MenuItem>("tsmiShowUntrackedFiles")!.IsEnabled.Should().BeFalse();

        GitItemStatus worktree = new("tracked.txt")
        {
            IsChanged = true,
            IsTracked = true,
            Staged = StagedStatus.WorkTree,
        };
        control.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), [worktree]);

        noFiles.IsVisible.Should().BeFalse();
        filterRow.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FileStatusList_context_menu_should_restore_dynamic_entries_state_and_action_routes()
    {
        FileStatusList control = new();
        Window window = new() { Width = 660, Height = 380, Content = control };
        ContextMenu menu = control.FindControl<ContextMenu>("ItemContextMenu")!;
        MenuItem resetToParent = control.FindControl<MenuItem>("tsmiResetFileToParent")!;
        ListBox fileList = control.FindControl<ListBox>("lstFiles")!;
        GitItemStatus worktree = new("tracked.txt")
        {
            IsChanged = true,
            IsTracked = true,
            Staged = StagedStatus.WorkTree,
        };

        int stageCalls = 0;
        int filterCalls = 0;
        int showInTreeCalls = 0;
        control.BindContextMenu(
            blame: null,
            cherryPickChanges: () => Assert.Fail("A working-tree item cannot be cherry-picked."),
            filterFileInGrid: () => filterCalls++,
            refreshParent: () => { },
            openInFileTreeTab_AsBlame: _ => showInTreeCalls++,
            getCurrentRevision: null,
            getLineNumber: () => 0,
            getSelectedText: null,
            getSupportLinePatching: () => true);
        control.BindContextMenu(() => { }, canAutoRefresh: true, () => stageCalls++, unstage: null);

        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType previousSort = DiffListSortService.Instance.DiffListSorting;
        try
        {
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
            window.Show();
            control.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), [worktree]);
            fileList.SelectedIndex = 0;

            OpenAlreadySelectedRow();
            control.FindControl<MenuItem>("tsmiStageFile")!.IsVisible.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiUnstageFile")!.IsVisible.Should().BeFalse();
            control.FindControl<MenuItem>("tsmiResetChunkOfFile")!.IsVisible.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiInteractiveAdd")!.IsVisible.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiResetFileTo")!.IsVisible.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiResetFileToSelected")!.IsVisible.Should().BeFalse();
            resetToParent.IsVisible.Should().BeTrue();
            resetToParent.Header.Should().Be("First: A Commit index");
            control.FindControl<MenuItem>("tsmiAddFileToGitIgnore")!.IsVisible.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiAddFileToGitInfoExclude")!.IsVisible.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiSkipWorktree")!.IsVisible.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiStopTracking")!.IsVisible.Should().BeTrue();
            MenuItem submoduleMenuItem = menu.Items.OfType<MenuItem>().Single(item => item.Name == "openSubmoduleMenuItem");
            submoduleMenuItem.IsVisible.Should().BeFalse();
            menu.Items.OfType<SortDiffListContextMenuItem>().Single().IsVisible.Should().BeTrue();
            menu.Items.OfType<SortDiffListContextMenuItem>().Should().ContainSingle();

            OpenAlreadySelectedRow();
            menu.Items.OfType<SortDiffListContextMenuItem>().Should().ContainSingle();

            menu.Close();
            control.ExecuteCommand(RevisionDiffControl.Command.StageSelectedFile).Should().BeTrue();
            control.ExecuteCommand(RevisionDiffControl.Command.FilterFileInGrid).Should().BeTrue();
            control.ExecuteCommand(RevisionDiffControl.Command.ShowFileTree).Should().BeTrue();
            stageCalls.Should().Be(1);
            filterCalls.Should().Be(1);
            showInTreeCalls.Should().Be(1);
            control.FindControl<ToggleButton>("btnByPath")!.Focus().Should().BeTrue();
            control.ExecuteCommand(RevisionDiffControl.Command.AddFileToGitIgnore).Should().BeFalse("the original route requires list focus");
            control.ExecuteCommand(RevisionDiffControl.Command.ResetSelectedFiles).Should().BeFalse("the original route requires list focus");
            control.ExecuteCommand(RevisionDiffControl.Command.SelectFirstGroupChanges).Should().BeFalse("the original route requires list focus");

            GitItemStatus submodule = new("submodule") { IsSubmodule = true, IsTracked = true };
            control.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), [submodule]);
            fileList.SelectedIndex = 0;
            OpenAlreadySelectedRow();
            submoduleMenuItem.IsVisible.Should().BeTrue();

            menu.Close();

            GitItemStatus statusOnly = new("summary") { IsStatusOnly = true };
            control.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), [statusOnly]);
            fileList.SelectedIndex = 0;
            OpenAlreadySelectedRow(expectedOpen: false);
        }
        finally
        {
            menu.Close();
            window.Close();
            DiffListSortService.Instance.DiffListSorting = previousSort;
        }

        void OpenAlreadySelectedRow(bool expectedOpen = true)
        {
            menu.Close();
            window.UpdateLayout();
            ListBoxItem row = fileList.ContainerFromIndex(fileList.SelectedIndex).Should().BeOfType<ListBoxItem>().Which;
            RightClick(window, row);
            menu.IsOpen.Should().Be(expectedOpen);
        }
    }

    [AvaloniaTest]
    public void FileStatusList_file_context_menu_should_open_from_the_file_presenter_only()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType previousSort = DiffListSortService.Instance.DiffListSorting;
        DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
        FileStatusList control = new();
        Window window = new() { Width = 660, Height = 380, Content = control };
        ListBox files = control.FindControl<ListBox>("lstFiles")!;
        ContextMenu menu = files.ContextMenu!;
        try
        {
            window.Show();
            GitItemStatus item = new("tracked.cs") { IsTracked = true, IsChanged = true };
            control.SetDiffs(null, new GitRevision(ObjectId.Random()), [item]);
            control.SelectedGitItem.Should().BeSameAs(item);
            window.UpdateLayout();

            ToggleButton groupByPath = control.FindControl<ToggleButton>("btnByPath")!;
            RightClick(window, groupByPath);
            menu.IsOpen.Should().BeFalse("the source file context menu is attached to the file presenter");

            ListBoxItem row = files.ContainerFromIndex(0).Should().BeOfType<ListBoxItem>().Which;
            RightClick(window, row);
            menu.IsOpen.Should().BeTrue();
            control.SelectedGitItem.Should().BeSameAs(item);
        }
        finally
        {
            menu.Close();
            window.Close();
            DiffListSortService.Instance.DiffListSorting = previousSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_context_menu_should_render_derived_menu_items_with_the_standard_menu_theme()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType previousSort = DiffListSortService.Instance.DiffListSorting;
        FileStatusList control = new();
        ListBox fileList = control.FindControl<ListBox>("lstFiles")!;
        ContextMenu menu = fileList.ContextMenu!;
        Window window = new() { Width = 660, Height = 380, Content = control };

        try
        {
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
            window.Show();
            GitItemStatus worktree = new("tracked.txt") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree };
            control.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), [worktree]);
            window.UpdateLayout();

            ListBoxItem row = fileList.ContainerFromIndex(0).Should().BeOfType<ListBoxItem>().Which;
            RightClick(window, row);
            menu.IsOpen.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiCopyPaths")!.Bounds.Height.Should().BeGreaterThan(0);
            menu.Items.OfType<SortDiffListContextMenuItem>().Single().Bounds.Height.Should().BeGreaterThan(0);
        }
        finally
        {
            menu.Close();
            window.Close();
            DiffListSortService.Instance.DiffListSorting = previousSort;
        }
    }

    private static void RightClick(Window window, Control target)
    {
        target.IsEffectivelyVisible.Should().BeTrue();
        Point point = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        window.MouseMove(point, RawInputModifiers.None);
        window.MouseDown(point, MouseButton.Right, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Right, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void FileStatusList_tree_context_menu_should_scope_expand_collapse_and_select_all_to_the_selected_subtree()
    {
        FileStatusList control = new() { SelectionMode = SelectionMode.Multiple };
        GitRevision index = new(ObjectId.IndexId);
        GitRevision workTree = new(ObjectId.WorkTreeId);
        control.SetDiffs(
        [
            new FileStatusWithDescription(
                index,
                workTree,
                "working tree",
                [
                    new GitItemStatus("folder/first.txt") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree },
                    new GitItemStatus("folder/nested/second.txt") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree },
                ]),
        ],
        isFileTreeMode: true);
        TreeView tree = control.FindControl<TreeView>("tvFiles")!;
        ContextMenu menu = tree.ContextMenu!;
        Window window = new() { Width = 680, Height = 460, Content = control };
        try
        {
            window.Show();
            control.SelectFileOrFolder(RelativePath.From("folder")).Should().BeTrue();
            window.UpdateLayout();
            TreeViewItem root = tree.ContainerFromIndex(0).Should().BeOfType<TreeViewItem>().Which;
            OpenMenu();
            MenuItem selectAll = FindMenuItem("S_elect all");
            MenuItem collapseAll = FindMenuItem("C_ollapse all");
            MenuItem expandAll = FindMenuItem("E_xpand all");
            MenuItem collapseRoots = FindMenuItem("Collap_se root folders");
            expandAll.IsVisible.Should().BeTrue();
            collapseAll.IsVisible.Should().BeTrue();
            selectAll.IsVisible.Should().BeTrue();
            int selectAllIndex = menu.Items.IndexOf(selectAll);
            menu.Items.IndexOf(collapseAll).Should().Be(selectAllIndex + 1);
            menu.Items.IndexOf(expandAll).Should().Be(selectAllIndex + 2);
            selectAll.Icon.Should().BeOfType<Image>();
            collapseAll.Icon.Should().BeOfType<Image>();
            expandAll.Icon.Should().BeOfType<Image>();
            collapseRoots.Icon.Should().BeOfType<Image>();

            ClickMenuItem(expandAll);
            root.IsExpanded.Should().BeTrue();
            ClickMenuItem(collapseAll);
            root.IsExpanded.Should().BeFalse();

            window.UpdateLayout();
            TextBox filter = control.FindControl<ComboBox>("cboFilterComboBox")!.GetVisualDescendants()
                .OfType<TextBox>().Single(item => item.Name == "PART_EditableTextBox");
            filter.Focus().Should().BeTrue();
            PressMultiply();
            root.IsExpanded.Should().BeFalse("the original shortcut belongs to the file list");

            root.Focus().Should().BeTrue();
            PressMultiply();
            root.IsExpanded.Should().BeTrue();
            root.IsKeyboardFocusWithin.Should().BeTrue();
            root.GetVisualDescendants().OfType<TreeViewItem>()
                .Where(item => item.ItemCount > 0).Should().NotBeEmpty().And.OnlyContain(item => item.IsExpanded);

            ClickMenuItem(expandAll);
            ClickMenuItem(selectAll);
            control.SelectedGitItems.Should().HaveCount(2);

            control.SelectFileOrFolder(RelativePath.From("folder/first.txt")).Should().BeTrue();
            ClickMenuItem(collapseRoots);
            root.IsExpanded.Should().BeFalse();
        }
        finally
        {
            menu.Close();
            window.Close();
        }

        MenuItem FindMenuItem(string header)
            => menu.Items.OfType<MenuItem>().Single(item => item.Header as string == header);

        void PressMultiply()
        {
            window.KeyPress(Key.Multiply, RawInputModifiers.None, PhysicalKey.NumPadMultiply, "*");
            window.KeyRelease(Key.Multiply, RawInputModifiers.None, PhysicalKey.NumPadMultiply, "*");
            Dispatcher.UIThread.RunJobs();
        }

        void OpenMenu()
        {
            if (!menu.IsOpen)
            {
                TreeViewItem selected = tree.GetVisualDescendants().OfType<TreeViewItem>()
                    .First(item => item.IsSelected && item.IsEffectivelyVisible);
                Control header = selected.GetVisualDescendants().OfType<Control>()
                    .First(control => control.Name == "PART_HeaderPresenter");
                Point point = header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), window)!.Value;
                window.MouseMove(point, RawInputModifiers.None);
                window.MouseDown(point, MouseButton.Right, RawInputModifiers.None);
                window.MouseUp(point, MouseButton.Right, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
            }

            menu.IsOpen.Should().BeTrue();
        }

        void ClickMenuItem(MenuItem item)
        {
            OpenMenu();
            item.IsEffectivelyVisible.Should().BeTrue();
            TopLevel popup = TopLevel.GetTopLevel(item)
                ?? throw new InvalidOperationException("The context menu item must belong to the open popup.");
            Point point = item.TranslatePoint(new Point(item.Bounds.Width / 2, item.Bounds.Height / 2), popup)!.Value;
            popup.MouseMove(point, RawInputModifiers.None);
            popup.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            popup.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            menu.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_group_extensions_by_the_original_case_sensitive_key()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        FileStatusList control = new() { GroupByRevision = false };
        Window window = new() { Width = 670, Height = 390, Content = control };
        window.Show();
        try
        {
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FileExtension;
            control.SetDiffs(null, new GitRevision(ObjectId.Random()),
                [
                    new GitItemStatus("lower-a.cs") { IsChanged = true, IsTracked = true },
                    new GitItemStatus("lower-b.cs") { IsChanged = true, IsTracked = true },
                    new GitItemStatus("upper-a.CS") { IsChanged = true, IsTracked = true },
                    new GitItemStatus("upper-b.CS") { IsChanged = true, IsTracked = true },
                ]);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            TreeView tree = control.FindControl<TreeView>("tvDiffFiles")!;
            tree.IsVisible.Should().BeTrue();
            string?[] headings = [.. tree.GetVisualDescendants().OfType<TextBlock>()
                .Select(text => text.Text).Where(text => text is ".cs" or ".CS")];
            string[] expected = [.. new[] { ".cs", ".CS" }.OrderBy(FileStatusList.GroupKey.From)];
            headings.Should().Equal(expected, "the original groups and orders by GroupKey");
            control.GitItemFilteredStatuses.Should().HaveCount(4);
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    [TestCase(DiffListSortType.FilePathFlat, false, "lstFiles")]
    [TestCase(DiffListSortType.FilePath, false, "tvDiffFiles")]
    [TestCase(DiffListSortType.FilePath, true, "tvFiles")]
    public void FileStatusList_should_display_file_names_only_in_each_file_presentation(
        DiffListSortType sortType, bool isFileTreeMode, string presentationName)
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        AppSettings.TruncatePathMethod = TruncatePathMethod.None;
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        FileStatusList control = new() { GroupByRevision = false };
        Window window = new() { Width = 600, Height = 360, Content = control };
        try
        {
            DiffListSortService.Instance.DiffListSorting = sortType;
            window.Show();
            GitItemStatus rename = new("new-folder/new.txt") { OldName = "old-folder/old.txt", IsTracked = true, IsRenamed = true };
            GitItemStatus move = new("new-folder/same.txt") { OldName = "old-folder/same.txt", IsTracked = true, IsRenamed = true };
            FileStatusWithDescription[] groups = [new(null, new GitRevision(ObjectId.Random()), "files", [rename, move])];
            control.SetDiffs(groups, isFileTreeMode);
            Dispatcher.UIThread.RunJobs();
            control.SelectFileOrFolder(RelativePath.From(rename.Name)).Should().BeTrue();
            window.UpdateLayout();

            ItemsControl presentation = control.FindControl<ItemsControl>(presentationName)!;
            presentation.IsVisible.Should().BeTrue();
            presentation.GetVisualDescendants().OfType<TextBlock>().Select(ReadLabelText)
                .Should().Contain(label => label != null && label.Contains("old-folder/old.txt"));

            AppSettings.TruncatePathMethod = TruncatePathMethod.FileNameOnly;
            control.SetDiffs(groups, isFileTreeMode);
            Dispatcher.UIThread.RunJobs();
            control.SelectFileOrFolder(RelativePath.From(rename.Name)).Should().BeTrue();
            window.UpdateLayout();

            TextBlock[] rows = [.. presentation.GetVisualDescendants().OfType<TextBlock>()];
            TextBlock renamedRow = rows.Should().ContainSingle(row => ReadLabelText(row) == "new.txt (old.txt)").Which;
            AutomationProperties.GetName(renamedRow).Should().Contain("old-folder/old.txt");
            string?[] labels = [.. rows.Select(ReadLabelText)];
            labels.Should().Contain("same.txt");
            labels.Should().NotContain(label => label != null && label.Contains("old-folder/"));
            control.GitItemFilteredStatuses.Select(item => item.Name).Should().BeEquivalentTo([rename.Name, move.Name]);
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    [TestCase(DiffListSortType.FilePathFlat, false, "lstFiles", false)]
    [TestCase(DiffListSortType.FilePath, false, "tvDiffFiles", false)]
    [TestCase(DiffListSortType.FilePath, true, "tvFiles", false)]
    [TestCase(DiffListSortType.FilePathFlat, false, "lstFiles", true)]
    [TestCase(DiffListSortType.FilePath, false, "tvDiffFiles", true)]
    [TestCase(DiffListSortType.FilePath, true, "tvFiles", true)]
    public void FileStatusList_should_refresh_realized_file_names_on_the_settings_event(
        DiffListSortType sortType, bool isFileTreeMode, string presentationName, bool hideWhileUpdating)
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        AppSettings.TruncatePathMethod = TruncatePathMethod.None;
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);
        FileStatusList control = new() { GroupByRevision = false, UICommandsSource = source };
        Window window = new() { Width = 600, Height = 360, Content = control };
        try
        {
            DiffListSortService.Instance.DiffListSorting = sortType;
            window.Show();
            GitItemStatus rename = new("new-folder/new.txt") { OldName = "old-folder/old.txt", IsTracked = true, IsRenamed = true };
            control.SetDiffs(
                [new FileStatusWithDescription(null, new GitRevision(ObjectId.Random()), "files", [rename])],
                isFileTreeMode);
            Dispatcher.UIThread.RunJobs();
            control.SelectFileOrFolder(RelativePath.From(rename.Name)).Should().BeTrue();
            window.UpdateLayout();
            ItemsControl presentation = control.FindControl<ItemsControl>(presentationName)!;
            TextBlock row = presentation.GetVisualDescendants().OfType<TextBlock>()
                .Single(label => ReadLabelText(label)?.Contains("old-folder/old.txt") == true);
            string? originalText = ReadLabelText(row);

            control.IsVisible = !hideWhileUpdating;
            PublishSetting(TruncatePathMethod.FileNameOnly);
            control.IsVisible = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            ReadLabelText(row).Should().Be("new.txt (old.txt)");
            presentation.GetVisualDescendants().Should().Contain(row);
            control.SelectedGitItem.Should().BeSameAs(rename);

            PublishSetting(TruncatePathMethod.None);
            ReadLabelText(row).Should().Be(originalText);
            presentation.GetVisualDescendants().Should().Contain(row);
            control.SelectedGitItem.Should().BeSameAs(rename);
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }

        void PublishSetting(TruncatePathMethod method)
        {
            AppSettings.TruncatePathMethod = method;
            commands.PostSettings += Raise.Event<EventHandler<GitUIPostActionEventArgs>>(
                commands, new GitUIPostActionEventArgs(null, commands, actionDone: true));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    [AvaloniaTest]
    [TestCase(DiffListSortType.FilePathFlat, false, "lstFiles", TruncatePathMethod.TrimStart)]
    [TestCase(DiffListSortType.FilePath, false, "tvDiffFiles", TruncatePathMethod.TrimStart)]
    [TestCase(DiffListSortType.FilePath, true, "tvFiles", TruncatePathMethod.TrimStart)]
    [TestCase(DiffListSortType.FilePathFlat, false, "lstFiles", TruncatePathMethod.Compact)]
    public void FileStatusList_should_trim_long_paths_to_the_native_viewport(
        DiffListSortType sortType, bool isFileTreeMode, string presentationName, TruncatePathMethod pathMethod)
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        AppSettings.TruncatePathMethod = pathMethod;
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        FileStatusList control = new() { GroupByRevision = false };
        Window window = new() { Width = 1300, Height = 360, Content = control };
        try
        {
            DiffListSortService.Instance.DiffListSorting = sortType;
            window.Show();
            string originalDirectory = new('q', 70);
            string newDirectory = new('x', 70);
            GitItemStatus rename = new($"{newDirectory}/new.txt")
            {
                OldName = $"{originalDirectory}/old.txt",
                IsTracked = true,
                IsRenamed = true,
            };
            control.SetDiffs(
                [new FileStatusWithDescription(null, new GitRevision(ObjectId.Random()), "files", [rename])],
                isFileTreeMode);
            Dispatcher.UIThread.RunJobs();
            control.SelectFileOrFolder(RelativePath.From(rename.Name)).Should().BeTrue();
            window.UpdateLayout();

            ItemsControl presentation = control.FindControl<ItemsControl>(presentationName)!;
            TextBlock row = presentation.GetVisualDescendants().OfType<TextBlock>()
                .Single(label => ReadLabelText(label)?.Contains("old.txt") == true);
            string? wideLabel = ReadLabelText(row);
            wideLabel.Should().Contain(originalDirectory);

            Resize(250);
            if (pathMethod == TruncatePathMethod.Compact && !OperatingSystem.IsWindows())
            {
                ReadLabelText(row).Should().Be(wideLabel);
            }
            else
            {
                ReadLabelText(row).Should().Contain("new.txt").And.Contain("...").And.NotContain(originalDirectory);
            }

            control.SelectedGitItem.Should().BeSameAs(rename);

            Resize(1300);
            ReadLabelText(row).Should().Be(wideLabel);
            control.SelectedGitItem.Should().BeSameAs(rename);
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }

        void Resize(double width)
        {
            window.Width = width;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    [TestCase(DiffListSortType.FilePathFlat, false, "lstFiles")]
    [TestCase(DiffListSortType.FilePath, false, "tvDiffFiles")]
    [TestCase(DiffListSortType.FilePath, true, "tvFiles")]
    public void FileStatusList_should_color_rename_labels_for_the_current_selection_and_theme(
        DiffListSortType sortType, bool isFileTreeMode, string presentationName)
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        ThemeId originalTheme = AppSettings.ThemeId;
        string[] originalVariations = AppSettings.ThemeVariations;
        bool originalUseSystemVisualStyle = AppSettings.UseSystemVisualStyle;
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        using FileStatusList control = new() { GroupByRevision = false };
        Button outside = new() { Content = "Outside" };
        DockPanel root = new() { Children = { outside, control } };
        DockPanel.SetDock(outside, Dock.Bottom);
        Window window = new() { Width = 640, Height = 340, Content = root };
        try
        {
            AppSettings.ThemeVariations = [];
            AppSettings.UseSystemVisualStyle = false;
            AppSettings.TruncatePathMethod = TruncatePathMethod.None;
            DiffListSortService.Instance.DiffListSorting = sortType;
            ApplyTheme(ThemeId.DefaultLight);
            window.Show();
            GitItemStatus rename = new("src/new.txt") { OldName = "old/old.txt", IsTracked = true, IsRenamed = true };
            GitItemStatus other = new("src/other.txt") { IsTracked = true, IsChanged = true };
            control.SetDiffs([new FileStatusWithDescription(null, new GitRevision(ObjectId.Random()), "Files", [rename, other])], isFileTreeMode);
            Dispatcher.UIThread.RunJobs();
            control.SelectFileOrFolder(RelativePath.From(rename.Name)).Should().BeTrue();
            window.UpdateLayout();

            ItemsControl presentation = control.FindControl<ItemsControl>(presentationName)!;
            TextBlock label = presentation.GetVisualDescendants().OfType<TextBlock>()
                .Single(block => AutomationProperties.GetName(block)?.Contains("new.txt", StringComparison.Ordinal) == true);
            PixelSize size = new((int)Math.Ceiling(label.Bounds.Width), (int)Math.Ceiling(label.Bounds.Height));
            new GitExtensionsTests.AvaloniaControlTreeReader(label, renderScale: 1).ReadPrimary(label, size).Root.Text
                .Should().EndWith("new.txt (old/old.txt)");
            outside.Focus().Should().BeTrue();
            presentation.IsKeyboardFocusWithin.Should().BeFalse();
            ReadColor("new.txt").Should().Be(Color.Parse("#000000"));
            ReadColor("old.txt").Should().Be(Color.Parse("#6D6D6D"));
            if (sortType == DiffListSortType.FilePathFlat)
            {
                ReadColor("src/").Should().Be(Color.Parse("#6D6D6D"));
            }

            presentation.Focus().Should().BeTrue();
            presentation.IsKeyboardFocusWithin.Should().BeTrue();
            ReadColor("new.txt").Should().Be(Color.Parse("#FFFFFF"));
            ReadColor("old.txt").Should().Be(Color.Parse("#C0C0C0"));
            control.SelectFileOrFolder(RelativePath.From(other.Name)).Should().BeTrue();
            ReadColor("new.txt").Should().Be(Color.Parse("#000000"));
            ReadColor("old.txt").Should().Be(Color.Parse("#6D6D6D"));

            if (presentation is TreeView)
            {
                TreeViewItem fileRow = label.FindAncestorOfType<TreeViewItem>()!;
                TreeViewItem folderRow = fileRow.FindAncestorOfType<TreeViewItem>()!;
                control.SelectFileOrFolder(RelativePath.From("src")).Should().BeTrue();
                Dispatcher.UIThread.RunJobs();
                folderRow.IsSelected.Should().BeTrue();
                folderRow.IsExpanded.Should().BeTrue();
                fileRow.IsSelected.Should().BeFalse();
                ReadColor("new.txt").Should().Be(Color.Parse("#000000"));
                ReadColor("old.txt").Should().Be(Color.Parse("#6D6D6D"));
            }

            control.SelectFileOrFolder(RelativePath.From(rename.Name)).Should().BeTrue();

            ApplyTheme(ThemeId.DefaultDark);
            ReadColor("new.txt").Should().Be(Color.Parse("#FFFFFF"));
            ReadColor("old.txt").Should().Be(Color.Parse("#969696"));

            outside.Focus().Should().BeTrue();
            ReadColor("new.txt").Should().Be(Color.Parse("#F0F0F0"));
            ReadColor("old.txt").Should().Be(Color.Parse("#969696"));
            control.SelectedGitItem.Should().BeSameAs(rename);

            Color ReadColor(string segment)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                return label.TextLayout.TextLines.SelectMany(line => line.TextRuns)
                    .OfType<global::Avalonia.Media.TextFormatting.ShapedTextRun>()
                    .Single(run => run.Text.ToString().Contains(segment, StringComparison.Ordinal))
                    .Properties.ForegroundBrush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;
            }
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
            AppSettings.ThemeVariations = originalVariations;
            AppSettings.UseSystemVisualStyle = originalUseSystemVisualStyle;
            ApplyTheme(originalTheme);
        }

        void ApplyTheme(ThemeId theme)
        {
            AppSettings.ThemeId = theme;
            AvaloniaThemeSettings.ApplyAppSettings();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_apply_the_current_sort_when_the_view_reattaches()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
        FileStatusList control = new() { GroupByRevision = false };
        Window window = new() { Width = 480, Height = 280, Content = control };
        window.Show();
        try
        {
            control.SetDiffs(null, new GitRevision(ObjectId.Random()),
                [
                    new GitItemStatus("a/first.cs") { IsChanged = true, IsTracked = true },
                    new GitItemStatus("b/second.md") { IsChanged = true, IsTracked = true },
                ]);
            Dispatcher.UIThread.RunJobs();
            TreeView tree = control.FindControl<TreeView>("tvDiffFiles")!;
            ListBox list = control.FindControl<ListBox>("lstFiles")!;
            tree.IsVisible.Should().BeFalse();
            list.IsVisible.Should().BeTrue();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
            window.Content = control;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            tree.IsVisible.Should().BeTrue("the original sorting subscription starts with the current sort");
            list.IsVisible.Should().BeFalse();
            control.GitItemFilteredStatuses.Should().HaveCount(2);
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_keep_all_selected_files_when_the_diff_list_is_resorted_or_filtered()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
        RevisionDiffControl diff = new();
        FileStatusList files = diff.FindControl<FileStatusList>("DiffFiles")!;
        Window window = new() { Width = 790, Height = 410, Content = diff };
        window.Show();
        try
        {
            files.SetDiffs(null, new GitRevision(ObjectId.Random()),
                [
                    new GitItemStatus("first.cs") { IsChanged = true, IsTracked = true },
                    new GitItemStatus("other.md") { IsChanged = true, IsTracked = true },
                    new GitItemStatus("second.cs") { IsChanged = true, IsTracked = true },
                ]);
            string[] selected = ["first.cs", "second.cs"];
            files.SetSelectionFilter(@"\.cs$").Should().Be(2);
            files.SelectedGitItems.Select(item => item.Name).Should().BeEquivalentTo(selected);

            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FileExtension;
            files.SelectedGitItems.Select(item => item.Name).Should().BeEquivalentTo(selected, "the original keeps every selected file when rebuilding the sorted tree");

            files.SetFilter(@"\.cs$");
            files.GitItemFilteredStatuses.Select(item => item.Name).Should().BeEquivalentTo(selected);
            files.SelectedGitItems.Select(item => item.Name).Should().BeEquivalentTo(selected, "the original keeps every selected file which still matches the filter");
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_select_the_exact_status_when_two_revisions_touch_the_same_path()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        FileStatusList control = new() { GroupByRevision = true };
        Window window = new() { Width = 620, Height = 350, Content = control };
        window.Show();
        try
        {
            GitRevision first = new(ObjectId.Random());
            GitRevision second = new(ObjectId.Random());
            GitItemStatus modified = new("same.cs") { IsChanged = true, IsTracked = true };
            GitItemStatus deleted = new("same.cs") { IsDeleted = true, IsTracked = true };
            control.SetDiffs(
                [
                    new FileStatusWithDescription(null, first, "first", [modified]),
                    new FileStatusWithDescription(first, second, "second", [deleted]),
                ],
                isFileTreeMode: false);

            control.SelectedGitItem = deleted;
            control.SelectedGitItem.Should().BeSameAs(deleted);
            control.SelectedItem!.SecondRevision.Should().BeSameAs(second);

            control.SelectedGitItem = new GitItemStatus("same.cs");
            control.SelectedGitItem.Should().BeSameAs(modified);

            control.SelectedGitItem = new GitItemStatus("missing.cs");
            control.HasSelection.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_publish_a_single_selected_item_only_when_exactly_one_file_is_selected()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        RevisionDiffControl diff = new();
        FileStatusList files = diff.FindControl<FileStatusList>("DiffFiles")!;
        Window window = new() { Width = 790, Height = 410, Content = diff };
        window.Show();
        try
        {
            GitItemStatus first = new("first.cs") { IsChanged = true, IsTracked = true };
            GitItemStatus second = new("second.cs") { IsChanged = true, IsTracked = true };
            files.SetDiffs(null, new GitRevision(ObjectId.Random()), [first, second]);

            files.SetSelectionFilter(@"\.cs$").Should().Be(2);
            files.SelectedGitItems.Should().BeEquivalentTo(new[] { first, second });
            files.SelectedGitItem.Should().BeNull();
            files.SelectedItem.Should().BeNull();

            files.SetSelectionFilter(@"^first\.cs$").Should().Be(1);
            files.SelectedGitItem.Should().BeSameAs(first);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_sort_submenu_should_render_the_original_captions_and_mnemonics()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
        FileStatusList control = new() { GroupByRevision = false };
        control.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId),
            [new GitItemStatus("tracked.cs") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree }]);
        ListBox list = control.FindControl<ListBox>("lstFiles")!;
        ContextMenu menu = list.ContextMenu!;
        Window window = new() { Width = 640, Height = 400, Content = control };
        window.Show();
        try
        {
            window.UpdateLayout();
            ListBoxItem row = (ListBoxItem)list.ContainerFromIndex(0)!;
            Point point = row.TranslatePoint(
                new Point(row.Bounds.Width / 2, row.Bounds.Height / 2), window)
                ?? throw new InvalidOperationException("The actual file row must belong to the test window.");
            window.MouseMove(point, RawInputModifiers.None);
            window.MouseDown(point, MouseButton.Right, RawInputModifiers.None);
            window.MouseUp(point, MouseButton.Right, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            menu.IsOpen.Should().BeTrue();
            SortDiffListContextMenuItem sort = menu.Items.OfType<SortDiffListContextMenuItem>().Single();
            sort.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();

            (string Caption, string Key)[] expected =
            [
                ("File path - tree", "P"),
                ("File path - flat", "F"),
                ("File extension - tree", "E"),
                ("File extension - flat", "X"),
                ("File status - tree", "S"),
                ("File status - flat", "T"),
            ];
            MenuItem[] items = [.. sort.Items.OfType<MenuItem>()];
            items.Should().HaveCount(expected.Length);
            for (int i = 0; i < items.Length; i++)
            {
                TopLevel.GetTopLevel(items[i]).Should().NotBeNull("the sort item belongs to the open submenu");
                AccessText text = items[i].GetVisualDescendants().OfType<AccessText>().Single();
                text.Measure(Size.Infinity);
                string renderedCaption = string.Concat(text.TextLayout.TextLines.SelectMany(line => line.TextRuns)
                    .OfType<global::Avalonia.Media.TextFormatting.ShapedTextRun>().Select(run => run.Text.ToString()));
                renderedCaption.Should().Be(expected[i].Caption);
                (text.AccessKey ?? string.Empty).ToUpperInvariant().Should().Be(expected[i].Key);
            }
        }
        finally
        {
            menu.Close();
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_dispose_should_release_the_live_sort_subscription()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
        FileStatusList control = new() { GroupByRevision = false };
        Window window = new() { Width = 620, Height = 350, Content = control };
        window.Show();
        try
        {
            control.SetDiffs(null, new GitRevision(ObjectId.Random()),
                [
                    new GitItemStatus("a/first.cs") { IsChanged = true, IsTracked = true },
                    new GitItemStatus("b/second.md") { IsChanged = true, IsTracked = true },
                ]);
            int rebuilt = 0;
            control.DataSourceChanged += (_, _) => rebuilt++;

            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
            rebuilt.Should().Be(1, "the live attached control currently follows the sort service");

            ((IDisposable)control).Dispose();
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
            Dispatcher.UIThread.RunJobs();

            rebuilt.Should().Be(1, "the original DisposeCustomResources releases the sort subscription");
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_apply_all_sort_and_branch_diff_filters()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        using FileStatusList control = new() { GroupByRevision = true };
        Window window = new() { Width = 680, Height = 420, Content = control };
        try
        {
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
            window.Show();

            foreach (DiffListSortType sortType in Enum.GetValues<DiffListSortType>())
            {
                DiffListSortService.Instance.DiffListSorting = sortType;
                Dispatcher.UIThread.RunJobs();
                control.GitItemFilteredStatuses.Should().HaveCount(2);
            }

            window.UpdateLayout();
            ToggleButton onlyA = control.FindControl<ToggleButton>("btnOnlyA")!;
            onlyA.IsEffectivelyVisible.Should().BeTrue();
            Point point = onlyA.TranslatePoint(new Point(onlyA.Bounds.Width / 2, onlyA.Bounds.Height / 2), window)!.Value;
            window.MouseMove(point, RawInputModifiers.None);
            window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            control.GitItemFilteredStatuses.Should().ContainSingle().Which.Name.Should().Be("only-b.cs");
        }
        finally
        {
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_switch_renderer_for_all_ungrouped_tree_and_flat_sort_modes()
    {
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        try
        {
            FileStatusList control = new();
            GitRevision revision = new(ObjectId.Random());
            GitItemStatus first = new("src/first.cs") { IsChanged = true, IsTracked = true };
            GitItemStatus second = new("docs/readme.md") { IsChanged = true, IsTracked = true };
            control.SetDiffs(
                [new FileStatusWithDescription(null, revision, "Diff with parent", [first, second])],
                isFileTreeMode: false);
            Window window = new() { Width = 360, Height = 240, Content = control };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            try
            {
                foreach (DiffListSortType sortType in Enum.GetValues<DiffListSortType>())
                {
                    DiffListSortService.Instance.DiffListSorting = sortType;
                    Dispatcher.UIThread.RunJobs();
                    bool flat = sortType is DiffListSortType.FilePathFlat
                        or DiffListSortType.FileExtensionFlat
                        or DiffListSortType.FileStatusFlat;
                    control.FindControl<ListBox>("lstFiles")!.IsVisible.Should().Be(flat, $"{sortType} is a flat renderer");
                    control.FindControl<TreeView>("tvDiffFiles")!.IsVisible.Should().Be(!flat, $"{sortType} preserves hierarchy");
                    control.AllItems.Should().HaveCount(2);
                    control.SelectFileOrFolder(RelativePath.From(first.Name)).Should().BeTrue();
                    control.SelectedGitItem.Should().BeSameAs(first);
                }
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_persist_each_toolbar_item_visibility()
    {
        using GitExtensionsTests.AppSettingsTestScope settings = new();
        const string settingsKey = "FileStatusList.Toolbar.Visibility.btnByPath";
        AppSettings.SetBool(settingsKey, false);
        using FileStatusList control = new();
        control.Bind(() => { });
        ToggleButton byPath = control.FindControl<ToggleButton>("btnByPath")!;
        int buttonIndex = control.FindControl<StackPanel>("Toolbar")!.Children.IndexOf(byPath);
        MenuItem toolbar = control.FindControl<MenuItem>("tsmiToolbar")!;
        MenuItem visibilityItem = (MenuItem)toolbar.Items[buttonIndex]!;
        IconDropDownButton settingsButton = control.FindControl<IconDropDownButton>("btnSettings")!;
        MenuFlyout menu = (MenuFlyout)settingsButton.Flyout!;

        Window window = new() { Width = 680, Height = 420, Content = control };
        try
        {
            window.Show();
            visibilityItem.IsChecked.Should().BeFalse();
            byPath.IsVisible.Should().BeFalse();

            Click(settingsButton);
            menu.IsOpen.Should().BeTrue();
            Click(toolbar);
            toolbar.IsSubMenuOpen.Should().BeTrue();
            Click(visibilityItem);

            AppSettings.GetBool(settingsKey, defaultValue: true).Should().BeTrue();
            byPath.IsVisible.Should().BeTrue();
        }
        finally
        {
            menu.Hide();
            window.Close();
        }

        static void Click(Control target)
        {
            TopLevel owner = TopLevel.GetTopLevel(target)
                ?? throw new InvalidOperationException("The toolbar item must be attached to its actual window or popup.");
            Point point = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), owner)!.Value;
            owner.MouseMove(point, RawInputModifiers.None);
            owner.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            owner.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_restore_refresh_settings_and_file_tree_toolbar_boundaries()
    {
        using FileStatusList control = new();
        control.Bind(() => { }, canAutoRefresh: false);

        control.FindControl<Button>("btnRefresh")!.IsVisible.Should().BeTrue();
        control.FindControl<MenuItem>("tsmiRefreshOnFormFocus")!.IsVisible.Should().BeFalse();
        control.FindControl<Separator>("sepToolbar")!.IsVisible.Should().BeFalse();

        control.Bind(() => { }, isFileTreeMode: true);

        control.FindControl<StackPanel>("Toolbar")!.IsVisible.Should().BeFalse();
        control.FindControl<TextBlock>("lblSplitter")!.Height.Should().Be(1);

        using FileStatusList autoRefreshControl = new();
        autoRefreshControl.Bind(() => { }, canAutoRefresh: true);
        autoRefreshControl.FindControl<MenuItem>("tsmiRefreshOnFormFocus")!.IsVisible.Should().BeTrue();
        autoRefreshControl.FindControl<Separator>("sepToolbar")!.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FileStatusList_should_show_the_collapse_button_when_revision_groups_are_rendered()
    {
        FileStatusList control = new() { GroupByRevision = true };
        GitRevision revision = new(ObjectId.Random());

        control.SetDiffs(
        [
            new FileStatusWithDescription(null, revision, "First", [new GitItemStatus("first.cs") { IsChanged = true }]),
            new FileStatusWithDescription(null, revision, "Second", [new GitItemStatus("second.cs") { IsChanged = true }]),
        ],
        isFileTreeMode: false);

        control.FindControl<Button>("btnCollapseGroups")!.IsVisible.Should().BeTrue();
    }

    private static string? ReadLabelText(TextBlock label) => label.Text ?? label.Inlines?.Text;
}
