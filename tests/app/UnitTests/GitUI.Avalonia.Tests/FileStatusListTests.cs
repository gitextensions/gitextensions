using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.LeftPanel;
using GitUI.Properties;
using GitUIPluginInterfaces;
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
        control.SetDiffs([renamed, documentation, rangeDiff]);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();

        control.SetFilter("old-name").Should().Be(2, "the renamed file matches its old name and range-diff markers stay visible");
        control.GitItemFilteredStatuses.Should().Equal(renamed, rangeDiff);
        accessor.CountLabel.Text.Should().Be("2 / 3 files");
        accessor.FilterComboBox.Classes.Should().Contain("file-filter-active");

        control.SetFilter("[").Should().Be(2, "an invalid expression keeps the last valid result");
        control.GitItemFilteredStatuses.Should().Equal(renamed, rangeDiff);
        accessor.FilterComboBox.Classes.Should().Contain("file-filter-invalid");
        ToolTip.GetTip(accessor.FilterComboBox).Should().BeOfType<string>().Which.Should().NotBeEmpty();

        control.SetFilter(string.Empty).Should().Be(3);
        control.GitItemFilteredStatuses.Should().Equal(renamed, documentation, rangeDiff);
        accessor.CountLabel.Text.Should().Be("3 files");
        accessor.FilterComboBox.Classes.Should().NotContain("file-filter-active");
        accessor.FilterComboBox.Classes.Should().NotContain("file-filter-invalid");
    }

    [AvaloniaTest]
    public void FileStatusList_should_expose_multi_selection_and_invoke_only_bound_context_actions()
    {
        FileStatusList control = new() { SelectionMode = SelectionMode.Multiple };
        GitItemStatus first = new("first.txt") { IsChanged = true, IsTracked = true };
        GitItemStatus second = new("second.txt") { IsChanged = true, IsTracked = true };
        control.SetDiffs([first, second]);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
        Window window = new() { Width = 320, Height = 180, Content = control };
        int stageCalls = 0;
        control.BindContextMenu(() => { }, canAutoRefresh: true, () => stageCalls++, unstage: null);

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            accessor.List.SelectedItems!.Clear();
            accessor.List.SelectedItems.Add(first);
            accessor.List.SelectedItems.Add(second);

            control.SelectedGitItems.Should().Equal(first, second);
            accessor.UpdateContextMenu();
            accessor.StageMenuItem.IsVisible.Should().BeTrue();
            accessor.StageMenuItem.IsEnabled.Should().BeTrue();
            accessor.UnstageMenuItem.IsVisible.Should().BeFalse();
            accessor.CherryPickMenuItem.IsVisible.Should().BeFalse();

            accessor.StageMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            stageCalls.Should().Be(1);
        }
        finally
        {
            accessor.ContextMenu.Close();
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

        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
        FileStatusList.DiffTreeNode header = accessor.DiffTree.Items.Cast<FileStatusList.DiffTreeNode>().Single();
        header.Text.Should().Be("(2) Diff with parent");
        header.IsGroupHeader.Should().BeTrue();
        header.Item.Should().BeNull();
        header.IsExpanded.Should().BeTrue();
        header.Children.SelectMany(Flatten).Count(node => node.Item is not null).Should().Be(2);
        accessor.DiffTree.IsVisible.Should().BeTrue();
        accessor.List.IsVisible.Should().BeFalse();
        control.SelectedGitItem.Should().Be(first);

        accessor.DiffTree.SelectedItem = header;

        control.SelectedGitItem.Should().BeNull("a revision summary is not a file selection");
    }

    [AvaloniaTest]
    public void FileStatusList_should_promote_a_single_ordinary_group_and_keep_its_summary_out_of_file_rows()
    {
        FileStatusList control = new();
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus item = new("src/only.cs") { IsChanged = true, IsTracked = true };

        control.SetDiffs(
            [new FileStatusWithDescription(null, revision, "Diff with parent", [item])],
            isFileTreeMode: false);

        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
        accessor.DiffTree.IsVisible.Should().BeFalse();
        accessor.List.IsVisible.Should().BeTrue();
        accessor.List.ItemCount.Should().Be(1);
        accessor.List.Items.Cast<object>().Single().Should().NotBeOfType<FileStatusList.DiffTreeNode>();
        control.SelectedGitItem.Should().Be(item);
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
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
        FileStatusList.DiffTreeNode[] roots = [.. accessor.DiffTree.Items.Cast<FileStatusList.DiffTreeNode>()];
        roots.Should().HaveCount(3);
        roots[0].Text.Should().Be("(2) Changes");
        roots[1].Text.Should().Be("(0) No changes");
        roots[1].Children.Should().BeEmpty();
        roots[2].Item?.Item.Should().Be(range, "a range-diff marker remains a selectable root like WinForms");
        roots[2].IsGroupHeader.Should().BeFalse();
        accessor.NoFilesLabel.IsVisible.Should().BeFalse();

        control.SetFilter("source").Should().Be(2, "range-diff markers remain visible through file filters");

        roots = [.. accessor.DiffTree.Items.Cast<FileStatusList.DiffTreeNode>()];
        roots[0].Text.Should().Be("(1/2) Changes");
        roots[1].Text.Should().Be("(0) No changes");
        roots[2].Item?.Item.Should().Be(range);
        control.GitItemFilteredStatuses.Should().Equal(source, range);
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
            .. control.GetTestAccessor().DiffTree.Items.Cast<FileStatusList.DiffTreeNode>(),
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

        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
        FileStatusList.DiffTreeNode secondNode = accessor.DiffTree.Items
            .Cast<FileStatusList.DiffTreeNode>()
            .ElementAt(1)
            .Children.SelectMany(Flatten)
            .Single(node => node.Item is not null);
        accessor.DiffTree.SelectedItem = secondNode;

        control.SetFilter("shared").Should().Be(2);

        control.SelectedGitItem.Should().BeSameAs(secondParent);
    }

    [AvaloniaTest]
    public void FileStatusList_should_use_the_complete_primary_status_icon_matrix()
    {
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("added") { IsNew = true })
            .Should().BeSameAs(Images.FileStatusAdded);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("removed") { IsDeleted = true })
            .Should().BeSameAs(Images.FileStatusRemoved);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("modified") { IsChanged = true, IsTracked = true })
            .Should().BeSameAs(Images.FileStatusModified);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("renamed") { IsRenamed = true, IsTracked = true, RenameCopyPercentage = "100" })
            .Should().BeSameAs(Images.FileStatusRenamed);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("copied") { IsCopied = true, IsTracked = true })
            .Should().BeSameAs(Images.FileStatusCopied);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("unmerged") { IsUnmerged = true, IsTracked = true })
            .Should().BeSameAs(Images.Unmerged);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("range") { IsRangeDiff = true })
            .Should().BeSameAs(Images.DiffR);

        GitItemStatus branchDiff = new("branch-diff")
        {
            IsChanged = true,
            IsTracked = true,
            DiffStatus = DiffBranchStatus.UnequalChange,
        };
        FileStatusList.TestAccessor.GetItemImage(branchDiff).Should().BeSameAs(Images.FileStatusModifiedUnequal);
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
            FileStatusList.TestAccessor.GetSubmoduleImage(item, resolved).Should().BeSameAs(expected);
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
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "tsmiStageFile", "Text", "&Stage selected");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "_collapseAll", "Text", "C&ollapse all");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void FileStatusList_tree_modes_should_share_the_native_hierarchy_connectors()
    {
        FileStatusList control = new();
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus first = new("src/folder/first.cs") { IsChanged = true, IsTracked = true };
        GitItemStatus second = new("src/folder/second.cs") { IsChanged = true, IsTracked = true };
        control.SetDiffs(
            [new FileStatusWithDescription(null, revision, "Files", [first, second])],
            isFileTreeMode: true);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
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
            accessor.Tree.Classes.Should().Contain("gitextensions-native-tree");
            TreeViewItem root = accessor.Tree.GetVisualDescendants().OfType<TreeViewItem>().First();
            root.IsExpanded = true;
            Dispatcher.UIThread.RunJobs();

            accessor.Tree.GetVisualDescendants()
                .OfType<TreeConnectorControl>()
                .Should().NotBeEmpty();
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
