using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Selection;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.LeftPanel;
using GitUI.Properties;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FileStatusListTests
{
    [AvaloniaTest]
    public void FileStatusList_should_filter_names_and_old_names_and_retain_results_for_invalid_regex()
    {
        FileStatusList control = new();
        GitItemStatus renamed = new("src/new-name.cs")
        {
            IsRenamed = true,
            IsTracked = true,
            OldName = "legacy/old-name.cs",
            RenameCopyPercentage = "100",
        };
        GitItemStatus documentation = new("docs/readme.md") { IsChanged = true, IsTracked = true };
        GitItemStatus rangeDiff = new("range-diff") { IsRangeDiff = true };
        control.SetDiffs(null, new GitRevision(ObjectId.IndexId), [renamed, documentation, rangeDiff]);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();

        control.SetFilter("old-name");
        control.GitItemFilteredStatuses.Count.Should().Be(2, "the renamed file matches its old name and range-diff markers stay visible");
        control.GitItemFilteredStatuses.Should().Equal(renamed, rangeDiff);
        accessor.FilterComboBox.Classes.Should().Contain("file-filter-active");

        control.SetFilter("[");
        control.GitItemFilteredStatuses.Count.Should().Be(2, "an invalid expression keeps the last valid result");
        control.GitItemFilteredStatuses.Should().Equal(renamed, rangeDiff);
        accessor.FilterComboBox.Classes.Should().Contain("file-filter-invalid");
        ToolTip.GetTip(accessor.FilterComboBox).Should().BeOfType<string>().Which.Should().NotBeEmpty();

        control.SetFilter(string.Empty);
        control.GitItemFilteredStatuses.Count.Should().Be(3);
        control.GitItemFilteredStatuses.Should().Equal(documentation, renamed, rangeDiff);
        accessor.FilterComboBox.Classes.Should().NotContain("file-filter-active");
        accessor.FilterComboBox.Classes.Should().NotContain("file-filter-invalid");
    }

    [AvaloniaTest]
    public void FileStatusList_filter_should_restore_only_previous_matches_or_select_the_single_remaining_file()
    {
        using AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
        try
        {
            FileStatusList control = new();
            GitItemStatus previous = new("old.cs") { IsTracked = true };
            GitItemStatus first = new("match-first.cs") { IsTracked = true };
            GitItemStatus second = new("match-second.cs") { IsTracked = true };
            control.SetDiffs(null, new GitRevision(ObjectId.Random()), [previous, first, second]);
            control.SelectFileOrFolder(RelativePath.From(previous.Name)).Should().BeTrue();

            control.SetFilter("^match");
            control.GitItemFilteredStatuses.Should().Equal(first, second);
            control.HasSelection.Should().BeFalse();

            control.SetFilter("^match-first");
            control.GitItemFilteredStatuses.Should().ContainSingle().Which.Should().BeSameAs(first);
            control.SelectedGitItem.Should().BeSameAs(first);

            control.SetFilter("^match");
            control.SelectedGitItem.Should().BeSameAs(first);
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_apply_filename_only_filtering_like_the_original()
    {
        TruncatePathMethod original = AppSettings.TruncatePathMethod;
        try
        {
            FileStatusList control = new();
            GitItemStatus item = new("directory/match.cs") { IsChanged = true, IsTracked = true };
            control.SetDiffs(null, new GitRevision(ObjectId.IndexId), [item]);

            AppSettings.TruncatePathMethod = TruncatePathMethod.FileNameOnly;
            control.SetFilter("directory");
            control.GitItemFilteredStatuses.Count.Should().Be(0);
            control.SetFilter("match");
            control.GitItemFilteredStatuses.Count.Should().Be(1);

            AppSettings.TruncatePathMethod = TruncatePathMethod.TrimStart;
            control.SetFilter("directory");
            control.GitItemFilteredStatuses.Count.Should().Be(1);
        }
        finally
        {
            AppSettings.TruncatePathMethod = original;
        }
    }

    [AvaloniaTest]
    public async Task FileStatusList_should_throttle_selection_changes_and_restore_focus_enter_contract()
    {
        FileStatusList control = new() { SelectionMode = SelectionMode.Multiple };
        GitItemStatus first = new("first.txt") { IsChanged = true, IsTracked = true };
        GitItemStatus second = new("second.txt") { IsChanged = true, IsTracked = true };
        control.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), [first, second]);
        ListBox fileList = control.FindControl<ListBox>("lstFiles")!;
        TextBox before = new();
        Window window = new()
        {
            Width = 320,
            Height = 200,
            Content = new StackPanel { Children = { before, control } },
        };
        int selectionChanges = 0;
        List<bool> enterRoutes = [];
        control.SelectedIndexChanged += (_, _) => selectionChanges++;
        control.Enter += (_, e) => enterRoutes.Add(e.ByMouse);

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(TimeSpan.FromMilliseconds(FileStatusList.SelectedIndexChangeThrottleDuration.TotalMilliseconds * 2));
            Dispatcher.UIThread.RunJobs();
            selectionChanges = 0;

            fileList.SelectedIndex = 1;
            fileList.SelectedIndex = 0;
            selectionChanges.Should().Be(0);

            await Task.Delay(TimeSpan.FromMilliseconds(FileStatusList.SelectedIndexChangeThrottleDuration.TotalMilliseconds * 2));
            Dispatcher.UIThread.RunJobs();
            selectionChanges.Should().Be(1);

            before.Focus();
            Dispatcher.UIThread.RunJobs();
            control.Focus();
            Dispatcher.UIThread.RunJobs();

            enterRoutes.Should().Equal(false);
            control.Focused.Should().BeTrue();
            control.FocusedItem.Should().Be(control.SelectedItem);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_restore_original_selection_contract_and_first_group_scope()
    {
        FileStatusList control = new() { GroupByRevision = true, SelectionMode = SelectionMode.Multiple };
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus first = new("first.txt") { IsChanged = true, IsTracked = true };
        GitItemStatus second = new("second.txt") { IsChanged = true, IsTracked = true };
        control.SetDiffs(
        [
            new FileStatusWithDescription(null, revision, "First", [first]),
            new FileStatusWithDescription(null, revision, "Second", [second]),
        ],
        isFileTreeMode: false);

        control.SelectFileOrFolder(RelativePath.From(second.Name), firstGroupOnly: true).Should().BeFalse();
        control.SelectFileOrFolder(RelativePath.From(second.Name)).Should().BeTrue();
        control.SelectedItem.Should().NotBeNull();
        control.SelectedItem!.Item.Should().BeSameAs(second);

        control.SelectedGitItems = [first];
        control.SelectedGitItems.Should().Equal(first);
        control.SelectedItem!.Item.Should().BeSameAs(first);

        control.SetSelectionFilter("second").Should().Be(1);
        control.SelectedGitItems.Should().Equal(second);
        control.SetSelectionFilter("missing").Should().Be(0);
        control.SelectedGitItems.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void FileStatusList_should_preserve_children_while_delaying_visual_expansion()
    {
        FileStatusList control = new() { GroupByRevision = true };
        GitRevision revision = new(ObjectId.Random());
        control.SetDiffs(
        [
            new FileStatusWithDescription(
                null,
                revision,
                "Files",
                [new GitItemStatus("one/two/three.txt") { IsChanged = true, IsTracked = true }]),
        ],
        isFileTreeMode: false);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
        int actions = 0;

        accessor.RestoreDiffTreeChildrenForTesting(delayExpansion: false, () => actions++);
        (int expandedDescendants, bool allExpanded) = accessor.GetDiffTreeExpansionForTesting();
        expandedDescendants.Should().BeGreaterThan(2);
        allExpanded.Should().BeTrue();

        accessor.RestoreDiffTreeChildrenForTesting(delayExpansion: true, () => actions++);
        (int delayedDescendants, bool delayedAllExpanded) = accessor.GetDiffTreeExpansionForTesting();
        delayedDescendants.Should().Be(expandedDescendants);
        delayedAllExpanded.Should().BeFalse();
        actions.Should().Be(2);
    }

    [AvaloniaTest]
    public void FileStatusList_should_expose_multi_selection_and_invoke_only_bound_context_actions()
    {
        FileStatusList control = new() { SelectionMode = SelectionMode.Multiple };
        GitItemStatus first = new("first.txt") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree };
        GitItemStatus second = new("second.txt") { IsChanged = true, IsTracked = true, Staged = StagedStatus.WorkTree };
        control.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), [first, second]);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
        MenuItem stage = control.FindControl<MenuItem>("tsmiStageFile")!;
        Window window = new() { Width = 320, Height = 180, Content = control };
        int stageCalls = 0;
        control.BindContextMenu(() => { }, canAutoRefresh: true, () => stageCalls++, unstage: null);

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            TreeView diffTree = control.FindControl<TreeView>("tvDiffFiles")!;
            diffTree.SelectedItems!.Clear();
            diffTree.SelectedItems.Add(diffTree.Items[0]!);
            diffTree.SelectedItems.Add(diffTree.Items[1]!);

            control.SelectedGitItems.Should().Equal(first, second);
            accessor.UpdateContextMenu();
            stage.IsVisible.Should().BeTrue();
            stage.IsEnabled.Should().BeTrue();
            control.FindControl<MenuItem>("tsmiUnstageFile")!.IsVisible.Should().BeFalse();
            control.FindControl<MenuItem>("tsmiCherryPickChanges")!.IsVisible.Should().BeFalse();

            stage.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            stageCalls.Should().Be(1);
        }
        finally
        {
            control.FindControl<ContextMenu>("ItemContextMenu")!.Close();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_render_revision_summary_as_a_separate_non_file_parent()
    {
        FileStatusList control = new() { GroupByRevision = true };
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus first = new("src/first.cs") { IsChanged = true, IsTracked = true };
        GitItemStatus second = new("readme.md") { IsChanged = true, IsTracked = true };

        control.SetDiffs(
            [new FileStatusWithDescription(null, revision, "Diff with parent", [first, second])],
            isFileTreeMode: false);

        TreeView diffTree = control.FindControl<TreeView>("tvDiffFiles")!;
        FileStatusList.DiffTreeNode header = diffTree.Items.Cast<FileStatusList.DiffTreeNode>().Single();
        header.Text.Should().Be("(2) Diff with parent");
        header.IsGroupHeader.Should().BeTrue();
        header.Item.Should().BeNull();
        header.IsExpanded.Should().BeTrue();
        header.Children.SelectMany(Flatten).Count(node => node.Item is not null).Should().Be(2);
        diffTree.IsVisible.Should().BeTrue();
        control.FindControl<ListBox>("lstFiles")!.IsVisible.Should().BeFalse();
        control.SelectedGitItem.Should().Be(first);

        diffTree.SelectedItem = header;

        control.SelectedGitItem.Should().BeNull("a revision summary is not a file selection");
    }

    [AvaloniaTest]
    public void FileStatusList_should_render_a_single_ordinary_group_as_the_configured_path_tree_without_a_summary_row()
    {
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        try
        {
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
            FileStatusList control = new();
            GitRevision revision = new(ObjectId.Random());
            GitItemStatus item = new("src/only.cs") { IsChanged = true, IsTracked = true };

            control.SetDiffs(
                [new FileStatusWithDescription(null, revision, "Diff with parent", [item])],
                isFileTreeMode: false);

            TreeView diffTree = control.FindControl<TreeView>("tvDiffFiles")!;
            diffTree.IsVisible.Should().BeTrue();
            control.FindControl<ListBox>("lstFiles")!.IsVisible.Should().BeFalse();
            FileStatusList.DiffTreeNode root = diffTree.Items.Cast<FileStatusList.DiffTreeNode>().Single();
            root.Text.Should().Be("src");
            root.IsGroupHeader.Should().BeFalse();
            root.Children.Should().ContainSingle().Which.Text.Should().Be("only.cs");
            root.Children.Single().Item?.Item.Should().BeSameAs(item);
            control.SelectedGitItem.Should().Be(item);
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void FileStatusList_diff_tree_should_compact_common_paths_like_the_source_sorter()
    {
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        bool originalMerge = AppSettings.FileStatusMergeSingleItemWithFolder.Value;
        try
        {
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
            AppSettings.FileStatusMergeSingleItemWithFolder.Value = false;
            FileStatusList control = new();
            GitRevision revision = new(ObjectId.Random());
            GitItemStatus first = new("src/app/GitUI.Avalonia/LeftPanel/first.cs") { IsChanged = true, IsTracked = true };
            GitItemStatus second = new("src/app/GitUI.Avalonia/LeftPanel/second.cs") { IsChanged = true, IsTracked = true };
            GitItemStatus separate = new("eng/avalonia/framework-adaptations.json") { IsChanged = true, IsTracked = true };

            control.SetDiffs(
                [new FileStatusWithDescription(null, revision, "Diff with parent", [first, second, separate])],
                isFileTreeMode: false);

            FileStatusList.DiffTreeNode[] roots =
            [
                .. control.FindControl<TreeView>("tvDiffFiles")!.Items.Cast<FileStatusList.DiffTreeNode>(),
            ];
            roots.Select(node => node.Text).Should().Equal("eng/avalonia", "src/app/GitUI.Avalonia/LeftPanel");
            roots[0].Children.Should().ContainSingle().Which.Text.Should().Be("framework-adaptations.json");
            roots[1].Children.Select(node => node.Text).Should().Equal("first.cs", "second.cs");
            roots.SelectMany(Flatten).Count(node => node.Item is not null).Should().Be(3);
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = originalSort;
            AppSettings.FileStatusMergeSingleItemWithFolder.Value = originalMerge;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void FileStatusList_diff_tree_should_honor_the_source_single_file_folder_merge_setting()
    {
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        bool originalMerge = AppSettings.FileStatusMergeSingleItemWithFolder.Value;
        try
        {
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
            AppSettings.FileStatusMergeSingleItemWithFolder.Value = true;
            FileStatusList control = new();
            GitRevision revision = new(ObjectId.Random());
            GitItemStatus item = new("src/only.cs") { IsChanged = true, IsTracked = true };

            control.SetDiffs(
                [new FileStatusWithDescription(null, revision, "Diff with parent", [item])],
                isFileTreeMode: false);

            FileStatusList.DiffTreeNode root = control.FindControl<TreeView>("tvDiffFiles")!.Items
                .Cast<FileStatusList.DiffTreeNode>()
                .Single();
            root.Text.Should().Be("src/only.cs");
            root.Item?.Item.Should().BeSameAs(item);
            root.Children.Should().BeEmpty();
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = originalSort;
            AppSettings.FileStatusMergeSingleItemWithFolder.Value = originalMerge;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void FileStatusList_selected_folder_should_return_descendant_items_in_source_tree_order()
    {
        bool originalMerge = AppSettings.FileStatusMergeSingleItemWithFolder.Value;
        try
        {
            AppSettings.FileStatusMergeSingleItemWithFolder.Value = false;
            FileStatusList control = new();
            GitItemStatus rootFile = new(".github/FUNDING.yml") { IsChanged = true, IsTracked = true };
            GitItemStatus agentFile = new(".github/agents/README.md") { IsChanged = true, IsTracked = true };
            GitItemStatus workflowFile = new(".github/workflows/build.yml") { IsChanged = true, IsTracked = true };

            GitRevision revision = new(ObjectId.Random());
            control.SetDiffs(
                [new FileStatusWithDescription(null, revision, "Files", [rootFile, agentFile, workflowFile])],
                isFileTreeMode: true);
            control.SelectFirstVisibleItem();

            control.SelectedFolder.Should().Be(RelativePath.From(".github"));
            control.SelectedItems.Select(item => item.Item).Should().Equal([agentFile, workflowFile, rootFile],
                "the source tree enumerates descendant folders before files directly in the selected folder");
            control.SelectedGitItems.Should().Equal(agentFile, workflowFile, rootFile);
        }
        finally
        {
            AppSettings.FileStatusMergeSingleItemWithFolder.Value = originalMerge;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_revision_overload_should_preserve_source_summary_and_path_grouping_pipeline()
    {
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        try
        {
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
            FileStatusList control = new();
            GitRevision first = new(ObjectId.Random());
            GitRevision second = new(ObjectId.Random());
            GitItemStatus item = new("src/App.cs") { IsChanged = true, IsTracked = true };

            control.SetDiffs(first, second, [item]);

            TreeView diffTree = control.FindControl<TreeView>("tvDiffFiles")!;
            diffTree.IsVisible.Should().BeTrue();
            control.FindControl<ListBox>("lstFiles")!.IsVisible.Should().BeFalse();
            FileStatusList.DiffTreeNode root = diffTree.Items.Cast<FileStatusList.DiffTreeNode>().Single();
            root.Text.Should().Be("src");
            root.Children.Should().ContainSingle().Which.Text.Should().Be("App.cs");
            root.Children.Single().Item?.FirstRevision.Should().BeSameAs(first);
            root.Children.Single().Item?.SecondRevision.Should().BeSameAs(second);
            root.Children.Single().IsGroupHeader.Should().BeFalse();
            control.SelectFileOrFolder(RelativePath.From("src")).Should().BeTrue();
            control.SelectedFolder.Should().Be(RelativePath.From("src"));
            control.SelectFileOrFolder(RelativePath.From("src/App.cs")).Should().BeTrue();
            control.SelectedGitItem.Should().BeSameAs(item);
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_file_tree_should_initially_select_the_first_root_folder()
    {
        FileStatusList control = new();
        GitRevision revision = new(ObjectId.Random());
        control.SetDiffs(
            [
                new FileStatusWithDescription(
                    null,
                    revision,
                    "File tree",
                    [
                        new GitItemStatus(".github/workflows/build.yml") { IsTracked = true },
                        new GitItemStatus("src/App.cs") { IsTracked = true },
                    ]),
            ],
            isFileTreeMode: true);

        control.SelectFirstVisibleItem();
        control.SelectedFolder.Should().Be(RelativePath.From(".github/workflows"));
        control.SelectedGitItem.Should().BeNull();

        control.ClearSelected();
        control.SelectStoredNextItem(orSelectFirst: true);

        control.SelectedFolder.Should().Be(RelativePath.From(".github/workflows"));
        control.SelectedGitItem.Should().BeNull();
    }

    [AvaloniaTest]
    public void FileStatusList_should_collapse_all_revision_groups_then_expand_only_the_selected_group()
    {
        using AppSettingsTestScope settings = new();
        FileStatusList control = new();
        Window window = new() { Width = 660, Height = 380, Content = control };
        try
        {
            window.Show();
            GitRevision revision = new(ObjectId.Random());
            control.SetDiffs(
                [
                    new FileStatusWithDescription(null, revision, "First", [new GitItemStatus("first.cs") { IsTracked = true }]),
                    new FileStatusWithDescription(null, revision, "Second", [new GitItemStatus("second.cs") { IsTracked = true }]),
                ],
                isFileTreeMode: false);
            control.SelectFileOrFolder(RelativePath.From("second.cs")).Should().BeTrue();

            Button collapse = control.FindControl<Button>("btnCollapseGroups")!;
            TreeView tree = control.FindControl<TreeView>("tvDiffFiles")!;
            collapse.IsEffectivelyVisible.Should().BeTrue();
            window.UpdateLayout();
            TreeViewItem first = tree.ContainerFromIndex(0).Should().BeOfType<TreeViewItem>().Which;
            TreeViewItem second = tree.ContainerFromIndex(1).Should().BeOfType<TreeViewItem>().Which;
            first.IsExpanded.Should().BeTrue();
            second.IsExpanded.Should().BeTrue();

            collapse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            first.IsExpanded.Should().BeFalse();
            second.IsExpanded.Should().BeFalse();
            tree.SelectedItem.Should().BeSameAs(second.DataContext);
            control.SelectedItems.Select(item => item.Item.Name).Should().Equal("second.cs");

            collapse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            first.IsExpanded.Should().BeFalse();
            second.IsExpanded.Should().BeTrue();
            tree.SelectedItem.Should().BeSameAs(second.DataContext);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_select_the_first_revision_summary_when_all_groups_are_collapsed()
    {
        using AppSettingsTestScope settings = new();
        FileStatusList control = new();
        Window window = new() { Width = 660, Height = 380, Content = control };
        try
        {
            window.Show();
            GitRevision revision = new(ObjectId.Random());
            control.SetDiffs(
                [
                    new FileStatusWithDescription(null, revision, "First", [new GitItemStatus("first.cs") { IsTracked = true }]),
                    new FileStatusWithDescription(null, revision, "Second", [new GitItemStatus("second.cs") { IsTracked = true }]),
                ],
                isFileTreeMode: false);
            Button collapse = control.FindControl<Button>("btnCollapseGroups")!;
            collapse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            control.ClearSelected();
            control.HasSelection.Should().BeFalse();

            control.Focus();

            control.SelectedFolder.Should().Be(RelativePath.From(string.Empty));
            control.SelectedItem.Should().BeNull();
            control.SelectedItems.Select(item => item.Item.Name).Should().Equal("first.cs");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_preserve_empty_and_range_groups_and_filtered_counts()
    {
        FileStatusList control = new() { GroupByRevision = true };
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus source = new("src/source.cs") { IsChanged = true, IsTracked = true };
        GitItemStatus documentation = new("docs/readme.md") { IsChanged = true, IsTracked = true };
        GitItemStatus range = new("range-diff") { IsRangeDiff = true };
        FileStatusWithDescription[] groups =
        [
            new(null, revision, "Changes", [source, documentation]),
            new(null, revision, "No changes", []),
            new(null, revision, "Range", [range], iconName: nameof(Images.DiffR)),
        ];

        control.SetDiffs(groups, isFileTreeMode: false);
        TreeView diffTree = control.FindControl<TreeView>("tvDiffFiles")!;
        FileStatusList.DiffTreeNode[] roots = [.. diffTree.Items.Cast<FileStatusList.DiffTreeNode>()];
        roots.Should().HaveCount(3);
        roots[0].Text.Should().Be("(2) Changes");
        roots[1].Text.Should().Be("(0) No changes");
        roots[1].Children.Should().BeEmpty();
        roots[2].Item?.Item.Should().Be(range, "a range-diff marker remains a selectable root like WinForms");
        roots[2].IsGroupHeader.Should().BeFalse();
        control.FindControl<TextBlock>("NoFiles")!.IsVisible.Should().BeFalse();

        control.SetFilter("source");
        control.GitItemFilteredStatuses.Count.Should().Be(2, "range-diff markers remain visible through file filters");

        roots = [.. diffTree.Items.Cast<FileStatusList.DiffTreeNode>()];
        roots[0].Text.Should().Be("(1/2) Changes");
        roots[1].Text.Should().Be("(0) No changes");
        roots[2].Item?.Item.Should().Be(range);
        control.GitItemFilteredStatuses.Should().Equal(source, range);
    }

    [AvaloniaTest]
    [TestCase(99, false)]
    [TestCase(100, false)]
    [TestCase(101, false)]
    [TestCase(99, true)]
    [TestCase(100, true)]
    [TestCase(101, true)]
    public void Grep_groups_should_remain_open_at_the_partial_expansion_boundary(int count, bool flat)
    {
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        bool originalMerge = AppSettings.FileStatusMergeSingleItemWithFolder.Value;
        try
        {
            DiffListSortService.Instance.DiffListSorting = flat ? DiffListSortType.FilePathFlat : DiffListSortType.FilePath;
            AppSettings.FileStatusMergeSingleItemWithFolder.Value = false;
            FileStatusList control = new() { GroupByRevision = true };
            GitRevision revision = new(ObjectId.Random());
            GitItemStatus[] matches = [.. Enumerable.Range(0, count).Select(index => new GitItemStatus($"src/folder-{index % 2}/match-{index}.cs")
            {
                IsTracked = true,
                IsChanged = true,
                GrepString = "needle",
            })];
            control.SetDiffs(
            [
                new FileStatusWithDescription(null, revision, "Ordinary", [new GitItemStatus("ordinary.cs") { IsTracked = true }]),
                new FileStatusWithDescription(null, revision, "grep: needle", matches, iconName: nameof(FileStatusDiffCalculator.GitGrepIconName)),
                new FileStatusWithDescription(null, revision, "Empty grep", [], iconName: nameof(FileStatusDiffCalculator.GitGrepIconName)),
            ],
            isFileTreeMode: false);

            FileStatusList.DiffTreeNode[] roots = [.. control.FindControl<TreeView>("tvDiffFiles")!.Items.Cast<FileStatusList.DiffTreeNode>()];
            roots.Should().HaveCount(3);
            roots[0].IsExpanded.Should().BeFalse();
            roots[1].IsExpanded.Should().BeTrue();
            roots[2].IsExpanded.Should().BeFalse();
            roots[1].Children.SelectMany(Flatten).Count(node => node.Item is not null).Should().Be(count);
            if (!flat)
            {
                FileStatusList.DiffTreeNode[] folders = [.. roots[1].Children.SelectMany(Flatten).Where(node => node.Item is null)];
                folders.Should().NotBeEmpty();
                folders.Select(node => node.IsExpanded).Should().AllBeEquivalentTo(count < 100);
            }
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = originalSort;
            AppSettings.FileStatusMergeSingleItemWithFolder.Value = originalMerge;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_expand_grep_results_and_collapse_the_other_revision_groups()
    {
        FileStatusList control = new() { GroupByRevision = true };
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus ordinary = new("src/source.cs") { IsChanged = true, IsTracked = true };
        GitItemStatus match = new("src/match.cs") { IsChanged = true, IsTracked = true, GrepString = "needle" };

        control.SetDiffs(
        [
            new FileStatusWithDescription(null, revision, "Diff with parent", [ordinary]),
            new FileStatusWithDescription(null, revision, "grep: needle", [match], iconName: nameof(FileStatusDiffCalculator.GitGrepIconName)),
        ],
        isFileTreeMode: false);

        FileStatusList.DiffTreeNode[] roots =
        [
            .. control.FindControl<TreeView>("tvDiffFiles")!.Items.Cast<FileStatusList.DiffTreeNode>(),
        ];
        roots[0].IsExpanded.Should().BeFalse();
        roots[1].IsExpanded.Should().BeTrue();
        roots[1].Image.Should().BeSameAs(Images.ViewFile);
    }

    [AvaloniaTest]
    public void FileStatusList_should_restore_the_same_multi_parent_file_when_paths_are_equal()
    {
        FileStatusList control = new() { GroupByRevision = true };
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus firstParent = new("src/shared.cs") { IsChanged = true, IsTracked = true };
        GitItemStatus secondParent = new("src/shared.cs") { IsChanged = true, IsTracked = true };
        control.SetDiffs(
        [
            new FileStatusWithDescription(null, revision, "Parent 1", [firstParent]),
            new FileStatusWithDescription(null, revision, "Parent 2", [secondParent]),
        ],
        isFileTreeMode: false);

        TreeView diffTree = control.FindControl<TreeView>("tvDiffFiles")!;
        FileStatusList.DiffTreeNode secondNode = diffTree.Items
            .Cast<FileStatusList.DiffTreeNode>()
            .ElementAt(1)
            .Children.SelectMany(Flatten)
            .Single(node => node.Item is not null);
        diffTree.SelectedItem = secondNode;

        control.SetFilter("shared");
        control.GitItemFilteredStatuses.Count.Should().Be(2);

        control.SelectedGitItem.Should().BeSameAs(secondParent);
    }

    [AvaloniaTest]
    public void FileStatusList_should_show_flat_group_nodes_only_for_an_original_grouped_sort()
    {
        using AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePathFlat;
        AppSettings.FileStatusShowGroupNodesInFlatList.Value = true;
        FileStatusList control = new() { GroupByRevision = false };
        Window window = new() { Width = 660, Height = 380, Content = control };
        MenuFlyout? flyout = null;
        try
        {
            window.Show();
            control.SetDiffs(null, new GitRevision(ObjectId.Random()),
                [
                    new GitItemStatus("first.cs") { IsTracked = true },
                    new GitItemStatus("second.cs") { IsTracked = true },
                    new GitItemStatus("readme.md") { IsTracked = true },
                ]);

            SplitButton sort = control.FindControl<SplitButton>("btnAsTree")!;
            window.UpdateLayout();
            flyout = sort.Flyout.Should().BeAssignableTo<MenuFlyout>().Which;
            flyout.ShowAt(sort);
            Dispatcher.UIThread.RunJobs();
            flyout.IsOpen.Should().BeTrue();
            MenuItem group = flyout.Items.OfType<MenuItem>().Single(item => item.Name == "tsmiShowGroupNodesInFlatList");
            group.IsEnabled.Should().BeFalse();

            MenuItem extension = flyout.Items.OfType<MenuItem>().Single(item => item.Name == "tsmiGroupByFileExtensionFlat");
            extension.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            flyout.Hide();
            window.UpdateLayout();
            DiffListSortService.Instance.DiffListSorting.Should().Be(DiffListSortType.FileExtensionFlat);
            group.IsEnabled.Should().BeTrue();
            TreeView tree = control.FindControl<TreeView>("tvDiffFiles")!;
            tree.IsEffectivelyVisible.Should().BeTrue();
            tree.GetVisualDescendants().OfType<TextBlock>().Should().Contain(block => block.Text == ".cs");
            control.GitItemFilteredStatuses.Select(item => item.Name).Should().Equal("first.cs", "second.cs", "readme.md");
        }
        finally
        {
            flyout?.Hide();
            window.Close();
            DiffListSortService.Instance.DiffListSorting = originalSort;
        }
    }

    [AvaloniaTest]
    public void Status_sort_should_use_the_original_icon_order_after_the_branch_status([Values] bool flat)
    {
        using AppSettingsTestScope settings = new();
        DiffListSortType originalSort = DiffListSortService.Instance.DiffListSorting;
        bool showGroups = AppSettings.FileStatusShowGroupNodesInFlatList;
        try
        {
            AppSettings.FileStatusShowGroupNodesInFlatList.Value = false;
            List<GitItemStatus> expected = [];
            foreach (DiffBranchStatus branchStatus in Enum.GetValues<DiffBranchStatus>().OrderByDescending(status => status))
            {
                GitItemStatus[] statuses =
                [
                    new($"{branchStatus}-00-unknown") { IsTracked = true },
                    new($"{branchStatus}-01-unmerged") { IsTracked = true, IsUnmerged = true },
                    new($"{branchStatus}-02-modified") { IsTracked = true, IsChanged = true },
                    new($"{branchStatus}-03-copied") { IsTracked = true, IsCopied = true },
                    new($"{branchStatus}-04-renamed") { IsTracked = true, IsRenamed = true, RenameCopyPercentage = "100" },
                    new($"{branchStatus}-05-added") { IsNew = true },
                    new($"{branchStatus}-06-removed") { IsDeleted = true },
                    new($"{branchStatus}-07-submodule") { IsTracked = true, IsSubmodule = true },
                    new($"{branchStatus}-08-dirty-submodule") { IsTracked = true, IsSubmodule = true, IsDirty = true },
                    new($"{branchStatus}-09-range") { IsRangeDiff = true },
                ];
                foreach (GitItemStatus status in statuses)
                {
                    status.DiffStatus = branchStatus;
                    expected.Add(status);
                }
            }

            DiffListSortService.Instance.DiffListSorting = flat ? DiffListSortType.FileStatusFlat : DiffListSortType.FileStatus;
            FileStatusList control = new();
            control.SetDiffs(
                [new FileStatusWithDescription(null, new GitRevision(ObjectId.Random()), "Diff with parent", [.. expected.AsEnumerable().Reverse()])],
                isFileTreeMode: false);

            control.GitItemFilteredStatuses.Should().Equal(expected);
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = originalSort;
            AppSettings.FileStatusShowGroupNodesInFlatList.Value = showGroups;
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_use_the_complete_primary_status_icon_matrix()
    {
        FileStatusList.TestAccessor.GetItemImageForTesting(new GitItemStatus("added") { IsNew = true })
            .Should().BeSameAs(Images.FileStatusAdded);
        FileStatusList.TestAccessor.GetItemImageForTesting(new GitItemStatus("removed") { IsDeleted = true })
            .Should().BeSameAs(Images.FileStatusRemoved);
        FileStatusList.TestAccessor.GetItemImageForTesting(new GitItemStatus("modified") { IsChanged = true, IsTracked = true })
            .Should().BeSameAs(Images.FileStatusModified);
        FileStatusList.TestAccessor.GetItemImageForTesting(new GitItemStatus("renamed") { IsRenamed = true, IsTracked = true, RenameCopyPercentage = "100" })
            .Should().BeSameAs(Images.FileStatusRenamed);
        FileStatusList.TestAccessor.GetItemImageForTesting(new GitItemStatus("copied") { IsCopied = true, IsTracked = true })
            .Should().BeSameAs(Images.FileStatusCopied);
        FileStatusList.TestAccessor.GetItemImageForTesting(new GitItemStatus("unmerged") { IsUnmerged = true, IsTracked = true })
            .Should().BeSameAs(Images.Unmerged);
        FileStatusList.TestAccessor.GetItemImageForTesting(new GitItemStatus("range") { IsRangeDiff = true })
            .Should().BeSameAs(Images.DiffR);

        GitItemStatus branchDiff = new("branch-diff")
        {
            IsChanged = true,
            IsTracked = true,
            DiffStatus = DiffBranchStatus.UnequalChange,
        };
        FileStatusList.TestAccessor.GetItemImageForTesting(branchDiff).Should().BeSameAs(Images.FileStatusModifiedUnequal);
    }

    [AvaloniaTest]
    public void FileStatusList_should_use_the_complete_resolved_submodule_icon_matrix()
    {
        GitItemStatus item = new("submodule") { IsSubmodule = true, IsTracked = true };
        (SubmoduleStatus Status, bool Dirty, object Expected)[] cases =
        [
            (SubmoduleStatus.FastForward, false, Images.SubmoduleRevisionUp),
            (SubmoduleStatus.FastForward, true, Images.SubmoduleRevisionUpDirty),
            (SubmoduleStatus.Rewind, false, Images.SubmoduleRevisionDown),
            (SubmoduleStatus.Rewind, true, Images.SubmoduleRevisionDownDirty),
            (SubmoduleStatus.NewerTime, false, Images.SubmoduleRevisionSemiUp),
            (SubmoduleStatus.NewerTime, true, Images.SubmoduleRevisionSemiUpDirty),
            (SubmoduleStatus.OlderTime, false, Images.SubmoduleRevisionSemiDown),
            (SubmoduleStatus.OlderTime, true, Images.SubmoduleRevisionSemiDownDirty),
            (SubmoduleStatus.SameCommit, false, Images.FolderSubmodule),
            (SubmoduleStatus.SameCommit, true, Images.SubmoduleDirty),
        ];

        foreach ((SubmoduleStatus status, bool dirty, object expected) in cases)
        {
            GitSubmoduleStatus resolved = new(
                item.Name,
                oldName: null,
                dirty,
                commit: default,
                oldCommit: default,
                addedCommits: null,
                removedCommits: null,
                getCommitData: null,
                _ => status);
            FileStatusList.TestAccessor.GetSubmoduleImageForTesting(item, resolved).Should().BeSameAs(expected);
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_emit_existing_translation_keys_once()
    {
        FileStatusList control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "cboFilterComboBox", "Watermark", "Filter files using a regular expression...");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "NoFiles", "Text", "No changes");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "columnHeader", "Text", "Files");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "tsmiStageFile", "Text", "&Stage selected");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "_collapseAll", "Text", "C&ollapse all");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "_saveFileFilterAllFiles", "Text", "All files");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "_saveFileFilterCurrentFormat", "Text", "Current format");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void FileStatusList_tree_modes_should_share_the_native_hierarchy_connectors()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        FileStatusList control = new();
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus first = new("src/folder/first.cs") { IsChanged = true, IsTracked = true };
        GitItemStatus second = new("src/folder/second.cs") { IsChanged = true, IsTracked = true };
        control.SetDiffs(
            [new FileStatusWithDescription(null, revision, "Files", [first, second])],
            isFileTreeMode: true);
        TreeView fileTree = control.FindControl<TreeView>("tvFiles")!;
        Window window = new()
        {
            Width = 360,
            Height = 240,
            Content = control,
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            fileTree.Classes.Should().Contain("gitextensions-native-tree");
            TreeViewItem root = fileTree.GetVisualDescendants().OfType<TreeViewItem>().First();
            root.IsExpanded = true;
            Dispatcher.UIThread.RunJobs();

            fileTree.GetVisualDescendants()
                .OfType<TreeConnectorControl>()
                .Should().NotBeEmpty();

            for (int depth = 0; depth < 3; depth++)
            {
                foreach (TreeViewItem item in fileTree.GetVisualDescendants().OfType<TreeViewItem>().ToArray())
                {
                    item.IsExpanded = true;
                }

                Dispatcher.UIThread.RunJobs();
            }

            TreeViewItem file = FindItem("first.cs");
            file.GetVisualDescendants().OfType<Image>().First().Source
                .Should().NotBeSameAs(Images.FileStatusUnknown,
                    "the source file-tree group uses a file-type icon even without grep/status flags");

            TreeViewItem sourceFolder = FindItem("src/folder");
            TreeViewItem secondFile = FindItem("second.cs");
            double sourceHeaderX = HeaderX(sourceFolder);
            HeaderX(file).Should().Be(sourceHeaderX + 14,
                "the source sorter retains a single compact common-path folder and the 14-DIP child indent");
            HeaderX(secondFile).Should().Be(sourceHeaderX + 14);

            Panel leafChevronHost = file.GetVisualDescendants()
                .OfType<Panel>()
                .Single(panel => panel.Name == "PART_ExpandCollapseChevronContainer");
            leafChevronHost.Bounds.Width.Should().Be(12,
                "native leaf rows retain the empty expander slot before their icon");

            TreeConnectorControl firstConnector = file.GetVisualDescendants()
                .OfType<TreeConnectorControl>()
                .Single();
            TreeConnectorControl secondConnector = secondFile.GetVisualDescendants()
                .OfType<TreeConnectorControl>()
                .Single();
            firstConnector.IsLastSibling.Should().BeFalse();
            secondConnector.IsLastSibling.Should().BeTrue(
                "data-bound containers must use their model's sibling index");

            TreeViewItem FindItem(string text)
                => fileTree.GetVisualDescendants().OfType<TreeViewItem>()
                    .Single(item => item.GetVisualDescendants().OfType<ContentPresenter>()
                        .Where(presenter => presenter.Name == "PART_HeaderPresenter"
                            && presenter.FindAncestorOfType<TreeViewItem>() == item)
                        .SelectMany(presenter => presenter.GetVisualDescendants().OfType<TextBlock>())
                        .Any(block => block.Text == text));

            double HeaderX(TreeViewItem item)
                => item.GetVisualDescendants().OfType<ContentPresenter>()
                    .Single(presenter => presenter.Name == "PART_HeaderPresenter"
                        && presenter.FindAncestorOfType<TreeViewItem>() == item)
                    .TranslatePoint(default, fileTree)!.Value.X;
        }
        finally
        {
            window.Close();
        }
    }

    private static IEnumerable<FileStatusList.DiffTreeNode> Flatten(FileStatusList.DiffTreeNode node)
    {
        yield return node;
        foreach (FileStatusList.DiffTreeNode child in node.Children.SelectMany(Flatten))
        {
            yield return child;
        }
    }
}
