using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI;

partial class FileStatusList
{
    private readonly Avalonia.Media.IImage _treeImage = Images.FileTree;
    private readonly Avalonia.Media.IImage _flatListImage = Images.DocumentTree.AdaptLightness();

    private MenuItem[] FindUsingMenuItems => field ??= [tsmiFindUsingDialog, tsmiFindUsingInputBox, tsmiFindUsingBoth];

    private void WireToolbar()
    {
        btnCollapseGroups.Click += CollapseGroups_Click;
        btnAsTree.Click += AsTree_ButtonClick;
        btnByPath.Click += GroupBy_Click;
        btnByExtension.Click += GroupBy_Click;
        btnByStatus.Click += GroupBy_Click;
        btnUnequalChange.Click += FilterByDiffStatus_Click;
        btnOnlyB.Click += FilterByDiffStatus_Click;
        btnOnlyA.Click += FilterByDiffStatus_Click;
        btnSameChange.Click += FilterByDiffStatus_Click;
        tsmiGroupByFilePathTree.Click += GroupByToolStripMenuItem_Click;
        tsmiGroupByFilePathFlat.Click += GroupByToolStripMenuItem_Click;
        tsmiGroupByFileExtensionTree.Click += GroupByToolStripMenuItem_Click;
        tsmiGroupByFileExtensionFlat.Click += GroupByToolStripMenuItem_Click;
        tsmiGroupByFileStatusTree.Click += GroupByToolStripMenuItem_Click;
        tsmiGroupByFileStatusFlat.Click += GroupByToolStripMenuItem_Click;
        tsmiDenseTree.Click += DenseTree_Click;
        tsmiShowGroupNodesInFlatList.Click += ShowGroupNodesInFlatList_Click;
        tsmiShowIgnoredFiles.Click += ShowIgnoredFiles_Click;
        tsmiShowSkipWorktreeFiles.Click += ShowSkipWorktreeFiles_Click;
        tsmiShowAssumeUnchangedFiles.Click += ShowAssumeUnchangedFiles_Click;
        tsmiShowUntrackedFiles.Click += ShowUntrackedFiles_Click;
        tsmiEditGitIgnore.Click += EditGitIgnore_Click;
        tmsiEditLocallyIgnoredFiles.Click += EditLocallyIgnoredFiles_Click;
        tsmiRefreshOnFormFocus.Click += RefreshOnFormFocus_Click;
        tsmiShowDiffForAllParents.Click += ShowDiffForAllParents_Click;

        SetSortTag(tsmiGroupByFilePathTree, DiffListSortType.FilePath);
        SetSortTag(tsmiGroupByFilePathFlat, DiffListSortType.FilePathFlat);
        SetSortTag(tsmiGroupByFileExtensionTree, DiffListSortType.FileExtension);
        SetSortTag(tsmiGroupByFileExtensionFlat, DiffListSortType.FileExtensionFlat);
        SetSortTag(tsmiGroupByFileStatusTree, DiffListSortType.FileStatus);
        SetSortTag(tsmiGroupByFileStatusFlat, DiffListSortType.FileStatusFlat);

        ((MenuFlyout)btnAsTree.Flyout!).Opening += (_, _) => UpdateToolbar();
        ((MenuFlyout)btnSettings.Flyout!).Opening += (_, _) => Settings_DropDownOpening();
        UpdateToolbar();

        static void SetSortTag(MenuItem item, DiffListSortType sortType)
            => item.Tag = sortType;
    }

    private void ApplyGroupBy()
    {
        bool flatList = ReferenceEquals(btnAsTree.Icon, _flatListImage);
        DiffListSortService.Instance.DiffListSorting =
            btnByPath.IsChecked == true ? flatList ? DiffListSortType.FilePathFlat : DiffListSortType.FilePath
            : btnByExtension.IsChecked == true ? flatList ? DiffListSortType.FileExtensionFlat : DiffListSortType.FileExtension
            : btnByStatus.IsChecked == true ? flatList ? DiffListSortType.FileStatusFlat : DiffListSortType.FileStatus
            : throw new InvalidOperationException("Exactly one group-by button must be checked");
    }

    private void AsTree_ButtonClick(object? sender, EventArgs e)
    {
        btnAsTree.Icon = ReferenceEquals(btnAsTree.Icon, _treeImage) ? _flatListImage : _treeImage;
        ApplyGroupBy();
    }

    private void CollapseGroups_Click(object? sender, EventArgs e)
    {
        if (!_showDiffGroups)
        {
            return;
        }

        DiffTreeNode[] roots = [.. tvDiffFiles.Items.Cast<DiffTreeNode>()];
        bool collapse = roots.Any(node => node.IsExpanded);
        foreach (DiffTreeNode root in roots)
        {
            root.IsExpanded = !collapse;
        }

        SelectFirstVisibleItem();
    }

    private void DenseTree_Click(object? sender, EventArgs e)
    {
        AppSettings.FileStatusMergeSingleItemWithFolder.Value = tsmiDenseTree.IsChecked;
        ApplyFilter(selectFirstItem: false);
    }

    private void EditGitIgnore_Click(object? sender, EventArgs e)
    {
        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            commands.StartEditGitIgnoreDialog(GetOwner(), localExcludes: false);
            RequestRefresh();
        }
    }

    private void EditLocallyIgnoredFiles_Click(object? sender, EventArgs e)
    {
        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            commands.StartEditGitIgnoreDialog(GetOwner(), localExcludes: true);
            RequestRefresh();
        }
    }

    private void FilterByDiffStatus_Click(object? sender, EventArgs e)
        => ApplyFilter(selectFirstItem: false);

    private void GroupBy_Click(object? sender, EventArgs e)
    {
        btnByPath.IsChecked = ReferenceEquals(sender, btnByPath);
        btnByExtension.IsChecked = ReferenceEquals(sender, btnByExtension);
        btnByStatus.IsChecked = ReferenceEquals(sender, btnByStatus);
        ApplyGroupBy();
    }

    private void GroupByToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem { Tag: DiffListSortType sortType })
        {
            DiffListSortService.Instance.DiffListSorting = sortType;
        }
    }

    private bool IsDiffStatusMatch(DiffBranchStatus diffStatus)
    {
        return diffStatus switch
        {
            DiffBranchStatus.UnequalChange => btnUnequalChange.IsChecked == true,
            DiffBranchStatus.OnlyBChange => btnOnlyB.IsChecked == true,
            DiffBranchStatus.OnlyAChange => btnOnlyA.IsChecked == true,
            DiffBranchStatus.SameChange => btnSameChange.IsChecked == true,
            _ => true
        };
    }

    private void RefreshOnFormFocus_Click(object? sender, EventArgs e)
        => AppSettings.RefreshArtificialCommitOnApplicationActivated = tsmiRefreshOnFormFocus.IsChecked;

    private void Settings_DropDownOpening()
    {
        tsmiRefreshOnFormFocus.IsChecked = AppSettings.RefreshArtificialCommitOnApplicationActivated;
        tsmiShowDiffForAllParents.IsChecked = AppSettings.ShowDiffForAllParents;
        ToolTip.SetTip(tsmiShowDiffForAllParents, TranslatedStrings.ShowDiffForAllParentsTooltip);
        tsmiDenseTree.IsChecked = AppSettings.FileStatusMergeSingleItemWithFolder.Value;
        tsmiShowGroupNodesInFlatList.IsChecked = AppSettings.FileStatusShowGroupNodesInFlatList.Value;
        tsmiEditGitIgnore.IsVisible = true;
        tmsiEditLocallyIgnoredFiles.IsVisible = true;
        sepEdit.IsVisible = true;
    }

    private void ShowAssumeUnchangedFiles_Click(object? sender, EventArgs e)
        => RequestRefresh();

    private void ShowDiffForAllParents_Click(object? sender, EventArgs e)
    {
        AppSettings.ShowDiffForAllParents = tsmiShowDiffForAllParents.IsChecked;
        RequestRefresh();
    }

    private void ShowGroupNodesInFlatList_Click(object? sender, EventArgs e)
    {
        AppSettings.FileStatusShowGroupNodesInFlatList.Value = tsmiShowGroupNodesInFlatList.IsChecked;
        ApplyFilter(selectFirstItem: false);
    }

    private void ShowIgnoredFiles_Click(object? sender, EventArgs e)
        => RequestRefresh();

    private void ShowSkipWorktreeFiles_Click(object? sender, EventArgs e)
        => RequestRefresh();

    private void ShowUntrackedFiles_Click(object? sender, EventArgs e)
        => RequestRefresh();

    private void UpdateToolbar()
    {
        DiffListSortType sortType = DiffListSortService.Instance.DiffListSorting;
        btnByPath.IsChecked = sortType is DiffListSortType.FilePath or DiffListSortType.FilePathFlat;
        btnByExtension.IsChecked = sortType is DiffListSortType.FileExtension or DiffListSortType.FileExtensionFlat;
        btnByStatus.IsChecked = sortType is DiffListSortType.FileStatus or DiffListSortType.FileStatusFlat;
        bool flatList = sortType is DiffListSortType.FilePathFlat
            or DiffListSortType.FileExtensionFlat
            or DiffListSortType.FileStatusFlat;
        btnAsTree.Icon = flatList ? _flatListImage : _treeImage;

        foreach (MenuItem item in new[]
                 {
                     tsmiGroupByFilePathTree,
                     tsmiGroupByFilePathFlat,
                     tsmiGroupByFileExtensionTree,
                     tsmiGroupByFileExtensionFlat,
                     tsmiGroupByFileStatusTree,
                     tsmiGroupByFileStatusFlat,
                 })
        {
            item.IsChecked = Equals(item.Tag, sortType);
            item.ToggleType = MenuItemToggleType.Radio;
        }

        tsmiDenseTree.IsEnabled = !flatList;
        tsmiShowGroupNodesInFlatList.IsEnabled = flatList;
        bool filterByDiffStatus = _revisionGroups.Any(group => group.IconName is nameof(Images.DiffA) or nameof(Images.DiffB));
        sepFilter.IsVisible = filterByDiffStatus;
        btnUnequalChange.IsVisible = filterByDiffStatus;
        btnOnlyB.IsVisible = filterByDiffStatus;
        btnOnlyA.IsVisible = filterByDiffStatus;
        btnSameChange.IsVisible = filterByDiffStatus;
        btnCollapseGroups.IsVisible = _showDiffGroups;
        sepRefresh.IsVisible = btnCollapseGroups.IsVisible && btnRefresh.IsVisible;
    }

    private void UpdateToolbar(IReadOnlyList<GitRevision> revisions)
    {
        bool withArtificial = revisions.Any(revision => revision.IsArtificial);
        btnRefresh.IsEnabled = withArtificial;

        bool isWorktree = withArtificial && revisions.Any(revision => revision.ObjectId == ObjectId.WorkTreeId);
        tsmiShowSkipWorktreeFiles.IsEnabled = isWorktree;
        tsmiShowUntrackedFiles.IsEnabled = isWorktree;
    }
}
