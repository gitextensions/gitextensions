#nullable enable

using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI;

partial class FileStatusList
{
    private readonly Image _treeImage = Images.FileTree;
    private readonly Image _flatListImage = Images.DocumentTree.AdaptLightness();

    // order in AppSettings.FileStatusFindInFilesGitGrepTypeIndex
    private ToolStripMenuItem[] FindUsingMenuItems => field ??= [tsmiFindUsingDialog, tsmiFindUsingInputBox, tsmiFindUsingBoth];

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

    private void EditGitIgnore_Click(object sender, EventArgs e)
    {
        UICommands.StartEditGitIgnoreDialog(this, localExcludes: false);
        RequestRefresh();
    }

    private void EditLocallyIgnoredFiles_Click(object sender, EventArgs e)
    {
        UICommands.StartEditGitIgnoreDialog(this, localExcludes: true);
        RequestRefresh();
    }

    private void Filter_ButtonClick(object sender, EventArgs e)
    {
        FilterFiles(cboFilterComboBox.Text);
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

    private void FindUsingMatchCase_Click(object sender, EventArgs e)
    {
        AppSettings.GitGrepIgnoreCase.Value = !tsmiFindUsingMatchCase.Checked;
        FindInCommitFilesGitGrep();
    }

    private void FindUsingWholeWord_Click(object sender, EventArgs e)
    {
        AppSettings.GitGrepMatchWholeWord.Value = tsmiFindUsingWholeWord.Checked;
        FindInCommitFilesGitGrep();
    }

    private void FindUsing_Click(object sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item)
        {
            AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value = Array.IndexOf(FindUsingMenuItems, item);
        }

        foreach (ToolStripMenuItem menuItem in FindUsingMenuItems)
        {
            menuItem.Checked = sender == menuItem;
        }

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

    private void Settings_DropDownOpening(object sender, EventArgs e)
    {
        tsmiRefreshOnFormFocus.Checked = AppSettings.RefreshArtificialCommitOnApplicationActivated;

        tsmiShowDiffForAllParents.Visible = _enableDisablingShowDiffForAllParents;
        tsmiShowDiffForAllParents.Checked = AppSettings.ShowDiffForAllParents;
        tsmiShowDiffForAllParents.ToolTipText = TranslatedStrings.ShowDiffForAllParentsTooltip;
    }

    private void FindInFilesGitGrep_DropDownOpening(object sender, EventArgs e)
    {
        tsmiFindUsingMatchCase.Checked = !AppSettings.GitGrepIgnoreCase.Value;
        tsmiFindUsingWholeWord.Checked = AppSettings.GitGrepMatchWholeWord.Value;
    }

    private void ShowAssumeUnchangedFiles_Click(object sender, EventArgs e)
    {
        RequestRefresh();
    }

    private void ShowDiffForAllParents_Click(object sender, EventArgs e)
    {
        AppSettings.ShowDiffForAllParents = tsmiShowDiffForAllParents.Checked;
        CancellationToken cancellationToken = _reloadSequence.Next();
        FileStatusListLoading();
        ThreadHelper.FileAndForget(async () =>
        {
            IReadOnlyList<FileStatusWithDescription> gitItemStatusesWithDescription = _diffCalculator.Calculate(prevList: GitItemStatusesWithDescription, refreshDiff: true, refreshGrep: false, cancellationToken);

            await this.SwitchToMainThreadAsync(cancellationToken);
            UpdateFileStatusListView(gitItemStatusesWithDescription, cancellationToken: cancellationToken);
        });
    }

    private void ShowGroupNodesInFlatList_Click(object sender, EventArgs e)
    {
        AppSettings.FileStatusShowGroupNodesInFlatList.Value = tsmiShowGroupNodesInFlatList.Checked;
        UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true);
    }

    private void ShowIgnoredFiles_Click(object sender, EventArgs e)
    {
        RequestRefresh();
    }

    private void ShowSkipWorktreeFiles_Click(object sender, EventArgs e)
    {
        RequestRefresh();
    }

    private void ShowUntrackedFiles_Click(object sender, EventArgs e)
    {
        RequestRefresh();
    }

    private void UpdateToolbar()
    {
        bool hasGroups = CanUseFindInCommitFilesGitGrep || (FileStatusListView.Nodes.Count > 0 && FileStatusListView.Nodes[0].Tag is GitRevision);
        btnCollapseGroups.Visible = hasGroups;
        sepRefresh.Visible = hasGroups && btnRefresh.Visible;
        sepAsTree.Visible = hasGroups || btnRefresh.Visible;

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

        for (int itemIndex = 0; itemIndex < FindUsingMenuItems.Length; ++itemIndex)
        {
            FindUsingMenuItems[itemIndex].Checked = AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value == itemIndex;
        }

        if (tsmiToolbar.DropDown.Items.Count == 0)
        {
            for (int itemIndex = 0; itemIndex < Toolbar.Items.Count; ++itemIndex)
            {
                ToolStripItem toolbarItem = Toolbar.Items[itemIndex];
                string settingsKey = $"{nameof(FileStatusList)}.{nameof(Toolbar)}.Visibility.{toolbarItem.Name}";
                ToolStripMenuItem menuItem = new()
                {
                    CheckOnClick = true,
                    Checked = AppSettings.GetBool(settingsKey, defaultValue: true),
                    Enabled = toolbarItem != btnSettings,
                    Image = toolbarItem.Image,
                    Text = toolbarItem is ToolStripSeparator ? $"Separator '{Toolbar.Items[itemIndex + 1].ToolTipText}'" : toolbarItem.ToolTipText,
                };
                menuItem.Click += (s, e) =>
                {
                    AppSettings.SetBool(settingsKey, menuItem.Checked ? null : false);
                    toolbarItem.Visible = menuItem.Checked;
                    UpdateToolbar();
                };
                tsmiToolbar.DropDown.Items.Add(menuItem);
            }
        }

        for (int itemIndex = 0; itemIndex < Toolbar.Items.Count; ++itemIndex)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)tsmiToolbar.DropDown.Items[itemIndex];
            if (!menuItem.Checked)
            {
                Toolbar.Items[itemIndex].Visible = false;
            }
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
