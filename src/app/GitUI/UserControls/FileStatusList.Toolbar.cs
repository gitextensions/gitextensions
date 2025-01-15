#nullable enable

using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI;

partial class FileStatusList
{
    private readonly Image _treeImage = Images.FileTree;
    private readonly Image _flatListImage = Images.DocumentTree;

    private void ApplyGroupBy()
    {
        bool flatList = btnAsTree.Image == _flatListImage;
        DiffListSortService.Instance.DiffListSorting =
            btnByPath.Checked ? flatList ? DiffListSortType.FilePathFlat : DiffListSortType.FilePath
            : btnByExtension.Checked ? flatList ? DiffListSortType.FileExtensionFlat : DiffListSortType.FileExtension
            : btnByStatus.Checked ? flatList ? DiffListSortType.FileStatusFlat : DiffListSortType.FileStatus
            : throw new InvalidOperationException("Exactly one group-by button must be checked");
    }

    private void AsTree_ButtonClick(object sender, EventArgs e)
    {
        btnAsTree.Image = btnAsTree.Image == _treeImage ? _flatListImage : _treeImage;
        ApplyGroupBy();
    }

    private void CollapseGroups_Click(object sender, EventArgs e)
    {
        bool collapsed = false;

        if (_showDiffGroups)
        {
            foreach (TreeNode diffGroup in FileStatusListView.Nodes)
            {
                collapsed |= CollapseGroupByGroups(diffGroup.Nodes);
                if (diffGroup.IsExpanded)
                {
                    diffGroup.Collapse(ignoreChildren: true);
                    collapsed = true;
                }
            }
        }
        else
        {
            collapsed = CollapseGroupByGroups(FileStatusListView.Nodes);
        }

        if (collapsed)
        {
            TreeNode? focusedNode = FileStatusListView.FocusedNode;
            while (focusedNode?.Parent is not null)
            {
                focusedNode = focusedNode.Parent;
            }

            FileStatusListView.SelectedNode = focusedNode ?? FileStatusListView.Nodes[0];
        }
        else
        {
            FileStatusListView.FocusedNode?.Expand();
        }

        return;

        static bool CollapseGroupByGroups(TreeNodeCollection nodes)
        {
            bool collapsed = false;

            foreach (TreeNode group in nodes)
            {
                if (group.IsExpanded && group.Tag is GroupKey)
                {
                    group.Collapse(ignoreChildren: true);
                    collapsed = true;
                }
            }

            return collapsed;
        }
    }

    private void DenseTree_Click(object sender, EventArgs e)
    {
        AppSettings.FileStatusMergeSingleItemWithFolder.Value = tsmiDenseTree.Checked;
        UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true);
    }

    private void Filter_ButtonClick(object sender, EventArgs e)
    {
        FilterFiles(_NO_TRANSLATE_FilterComboBox.Text);
    }

    private void FindInFilesGitGrep_ButtonClick(object sender, EventArgs e)
    {
        bool usingInputBox = !tsmiFindUsingDialog.Checked;
        bool usingDialog = !tsmiFindUsingInputBox.Checked;
        bool isVisible = (!usingInputBox || cboFindInCommitFilesGitGrep.Visible) && (!usingDialog || _formFindInCommitFilesGitGrep?.Visible is true);
        bool setVisible = sender == btnFindInFilesGitGrep ? !isVisible : true;

        bool inputBoxVisible = setVisible && usingInputBox;
        AppSettings.ShowFindInCommitFilesGitGrep.Value = inputBoxVisible;
        SetFindInCommitFilesGitGrepVisibility(inputBoxVisible);
        if (!inputBoxVisible)
        {
            ActiveControl = FileStatusListView;
        }

        if (setVisible && usingDialog)
        {
            ShowFindInCommitFileGitGrepDialog(text: "");
        }
        else
        {
            _formFindInCommitFilesGitGrep?.Close();
        }
    }

    private void FindUsing_Click(object sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item)
        {
            AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value = btnFindInFilesGitGrep.DropDown.Items.IndexOf(item);
        }

        tsmiFindUsingDialog.Checked = sender == tsmiFindUsingDialog;
        tsmiFindUsingInputBox.Checked = sender == tsmiFindUsingInputBox;
        tsmiFindUsingBoth.Checked = sender == tsmiFindUsingBoth;
        FindInFilesGitGrep_ButtonClick(sender, e);
    }

    private void GroupBy_Click(object sender, EventArgs e)
    {
        btnByPath.Checked = sender == btnByPath;
        btnByExtension.Checked = sender == btnByExtension;
        btnByStatus.Checked = sender == btnByStatus;
        ApplyGroupBy();
    }

    private void GroupByToolStripMenuItem_Click(object sender, EventArgs e)
    {
        DiffListSortService.Instance.DiffListSorting = (DiffListSortType)((ToolStripMenuItem)sender).Tag!;
    }

    private bool IsDiffStatusMatch(DiffBranchStatus diffStatus)
    {
        return diffStatus switch
        {
            DiffBranchStatus.UnequalChange => btnUnequalChange.Checked,
            DiffBranchStatus.OnlyBChange => btnOnlyB.Checked,
            DiffBranchStatus.OnlyAChange => btnOnlyA.Checked,
            DiffBranchStatus.SameChange => btnSameChange.Checked,
            _ => true
        };
    }

    private void RefreshOnFormFocus_Click(object sender, EventArgs e)
    {
        AppSettings.RefreshArtificialCommitOnApplicationActivated = tsmiRefreshOnFormFocus.Checked;
    }

    private void ShowGroupNodesInFlatList_Click(object sender, EventArgs e)
    {
        AppSettings.FileStatusShowGroupNodesInFlatList.Value = tsmiShowGroupNodesInFlatList.Checked;
        UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true);
    }

    private void UpdateToolbar()
    {
        bool hasGroups = CanUseFindInCommitFilesGitGrep || (FileStatusListView.Nodes.Count > 0 && FileStatusListView.Nodes[0].Tag is GitRevision);
        btnCollapseGroups.Visible = hasGroups;
        sepRefresh.Visible = hasGroups && btnRefresh.Visible;
        sepAsTree.Visible = hasGroups || btnRefresh.Visible;

        tsmiRefreshOnFormFocus.Checked = tsmiRefreshOnFormFocus.Enabled && AppSettings.RefreshArtificialCommitOnApplicationActivated;

        DiffListSortType sortType = DiffListSortService.Instance.DiffListSorting;
        btnByPath.Checked = sortType is DiffListSortType.FilePath or DiffListSortType.FilePathFlat;
        btnByExtension.Checked = sortType is DiffListSortType.FileExtension or DiffListSortType.FileExtensionFlat;
        btnByStatus.Checked = sortType is DiffListSortType.FileStatus or DiffListSortType.FileStatusFlat;
        bool flatList = sortType.ToString().EndsWith("Flat");
        btnAsTree.Image = flatList ? _flatListImage : _treeImage;
        tsmiGroupByFilePathTree.Checked = sortType == DiffListSortType.FilePath;
        tsmiGroupByFilePathFlat.Checked = sortType == DiffListSortType.FilePathFlat;
        tsmiGroupByFileExtensionTree.Checked = sortType == DiffListSortType.FileExtension;
        tsmiGroupByFileExtensionFlat.Checked = sortType == DiffListSortType.FileExtensionFlat;
        tsmiGroupByFileStatusTree.Checked = sortType == DiffListSortType.FileStatus;
        tsmiGroupByFileStatusFlat.Checked = sortType == DiffListSortType.FileStatusFlat;

        tsmiDenseTree.Checked = AppSettings.FileStatusMergeSingleItemWithFolder.Value;
        tsmiDenseTree.Enabled = !flatList;
        tsmiShowGroupNodesInFlatList.Checked = AppSettings.FileStatusShowGroupNodesInFlatList.Value;
        tsmiShowGroupNodesInFlatList.Enabled = _groupBy is not null && flatList;

        bool filterByDiffStatus = HasDiffABGroups();
        btnUnequalChange.Visible = filterByDiffStatus;
        btnOnlyB.Visible = filterByDiffStatus;
        btnOnlyA.Visible = filterByDiffStatus;
        btnSameChange.Visible = filterByDiffStatus;
        sepFilter.Visible = filterByDiffStatus;

        bool findInFilesGitGrepVisible = CanUseFindInCommitFilesGitGrep;
        btnFindInFilesGitGrep.Visible = findInFilesGitGrepVisible;
        sepOptions.Visible = findInFilesGitGrepVisible;
        for (int itemIndex = 0; itemIndex < btnFindInFilesGitGrep.DropDown.Items.Count; ++itemIndex)
        {
            ((ToolStripMenuItem)btnFindInFilesGitGrep.DropDown.Items[itemIndex]).Checked = AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value == itemIndex;
        }

        return;

        bool HasDiffABGroups()
        {
            foreach (FileStatusWithDescription diffGroup in GitItemStatusesWithDescription)
            {
                if (diffGroup.IconName is nameof(Images.DiffB) or nameof(Images.DiffA))
                {
                    return true;
                }
            }

            return false;
        }
    }

    private void UpdateToolbar(IReadOnlyList<GitRevision> revisions)
    {
        btnRefresh.Enabled = revisions.Any(revision => revision.IsArtificial);
    }
}
