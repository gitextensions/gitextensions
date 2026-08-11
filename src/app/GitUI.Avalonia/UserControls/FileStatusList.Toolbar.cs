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

    private string? _findUsingOptionsPrefix;

    // order in AppSettings.FileStatusFindInFilesGitGrepTypeIndex
    private MenuItem[] FindUsingMenuItems => field ??= [tsmiFindUsingDialog, tsmiFindUsingInputBox, tsmiFindUsingBoth];

    private void WireToolbar()
    {
        btnCollapseGroups.Click += CollapseGroups_Click;
        btnAsTree.Click += AsTree_ButtonClick;
        btnByPath.Click += GroupBy_Click;
        btnByExtension.Click += GroupBy_Click;
        btnByStatus.Click += GroupBy_Click;
        btnUnequalChange.Click += Filter_ButtonClick;
        btnOnlyB.Click += Filter_ButtonClick;
        btnOnlyA.Click += Filter_ButtonClick;
        btnSameChange.Click += Filter_ButtonClick;
        btnFindInFilesGitGrep.Click += FindInFilesGitGrep_ButtonClick;
        tsmiFindUsingMatchCase.Click += FindUsingMatchCase_Click;
        tsmiFindUsingWholeWord.Click += FindUsingWholeWord_Click;
        _NO_TRANSLATE_tsmiFindUsingBasic.Click += FindUsingOption_Click;
        _NO_TRANSLATE_tsmiFindUsingExtended.Click += FindUsingOption_Click;
        _NO_TRANSLATE_tsmiFindUsingFixed.Click += FindUsingOption_Click;
        _NO_TRANSLATE_tsmiFindUsingPerl.Click += FindUsingOption_Click;
        tsmiFindUsingDialog.Click += FindUsing_Click;
        tsmiFindUsingInputBox.Click += FindUsing_Click;
        tsmiFindUsingBoth.Click += FindUsing_Click;
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
        ((MenuFlyout)btnFindInFilesGitGrep.Flyout!).Opening += FindInFilesGitGrep_DropDownOpening;
        ((MenuFlyout)btnSettings.Flyout!).Opening += Settings_DropDownOpening;
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

    private void Filter_ButtonClick(object? sender, EventArgs e)
        => ApplyFilter(selectFirstItem: false);

    private void FindInFilesGitGrep_ButtonClick(object? sender, EventArgs e)
    {
        bool usingInputBox = tsmiFindUsingDialog.IsChecked != true;
        bool usingDialog = tsmiFindUsingInputBox.IsChecked != true;
        bool isVisible = (!usingInputBox || FindInCommitFilesGitGrepPanel.IsVisible)
                         && (!usingDialog || _formFindInCommitFilesGitGrep?.IsVisible is true);
        bool setVisible = !ReferenceEquals(sender, btnFindInFilesGitGrep) || !isVisible;

        bool inputBoxVisible = setVisible && usingInputBox;
        AppSettings.ShowFindInCommitFilesGitGrep.Value = inputBoxVisible;
        SetFindInCommitFilesGitGrepVisibility(inputBoxVisible);
        if (!inputBoxVisible)
        {
            Focus();
        }

        if (setVisible && usingDialog)
        {
            ShowFindInCommitFileGitGrepDialog(text: string.Empty);
        }
        else
        {
            _formFindInCommitFilesGitGrep?.Close();
        }
    }

    private void FindUsingMatchCase_Click(object? sender, EventArgs e)
    {
        AppSettings.GitGrepIgnoreCase.Value = tsmiFindUsingMatchCase.IsChecked != true;
        FindInCommitFilesGitGrep();
    }

    private void FindUsingWholeWord_Click(object? sender, EventArgs e)
    {
        AppSettings.GitGrepMatchWholeWord.Value = tsmiFindUsingWholeWord.IsChecked == true;
        FindInCommitFilesGitGrep();
    }

    private void FindUsingOption_Click(object? sender, EventArgs e)
    {
        AppSettings.GitGrepUserArguments.Value = sender is MenuItem item ? item.Header?.ToString() ?? string.Empty : string.Empty;
        FindInCommitFilesGitGrep();
    }

    private void FindUsing_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem item)
        {
            AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value = Array.IndexOf(FindUsingMenuItems, item);
        }

        foreach (MenuItem menuItem in FindUsingMenuItems)
        {
            menuItem.IsChecked = ReferenceEquals(sender, menuItem);
        }

        FindInFilesGitGrep_ButtonClick(sender, e);
    }

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

    private void Settings_DropDownOpening(object? sender, EventArgs e)
    {
        tsmiRefreshOnFormFocus.IsChecked = AppSettings.RefreshArtificialCommitOnApplicationActivated;
        tsmiShowDiffForAllParents.IsVisible = _enableDisablingShowDiffForAllParents;
        tsmiShowDiffForAllParents.IsChecked = AppSettings.ShowDiffForAllParents;
        ToolTip.SetTip(tsmiShowDiffForAllParents, TranslatedStrings.ShowDiffForAllParentsTooltip);
    }

    private void FindInFilesGitGrep_DropDownOpening(object? sender, EventArgs e)
    {
        tsmiFindUsingMatchCase.IsChecked = !AppSettings.GitGrepIgnoreCase.Value;
        tsmiFindUsingWholeWord.IsChecked = AppSettings.GitGrepMatchWholeWord.Value;
        _findUsingOptionsPrefix ??= tsmiFindUsingOptions.Header + ": ";
        tsmiFindUsingOptions.Header = _findUsingOptionsPrefix + AppSettings.GitGrepUserArguments.Value;
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
        tsmiDenseTree.IsChecked = AppSettings.FileStatusMergeSingleItemWithFolder.Value;
        tsmiShowGroupNodesInFlatList.IsEnabled = flatList;
        tsmiShowGroupNodesInFlatList.IsChecked = AppSettings.FileStatusShowGroupNodesInFlatList.Value;
        bool filterByDiffStatus = GitItemStatusesWithDescription.Any(group => group.IconName is nameof(Images.DiffA) or nameof(Images.DiffB));
        sepFilter.IsVisible = filterByDiffStatus;
        btnUnequalChange.IsVisible = filterByDiffStatus;
        btnOnlyB.IsVisible = filterByDiffStatus;
        btnOnlyA.IsVisible = filterByDiffStatus;
        btnSameChange.IsVisible = filterByDiffStatus;
        btnCollapseGroups.IsVisible = _showDiffGroups;
        sepRefresh.IsVisible = btnCollapseGroups.IsVisible && btnRefresh.IsVisible;

        bool findInFilesGitGrepVisible = CanUseFindInCommitFilesGitGrep;
        btnFindInFilesGitGrep.IsVisible = findInFilesGitGrepVisible;
        sepOptions.IsVisible = findInFilesGitGrepVisible;

        for (int itemIndex = 0; itemIndex < FindUsingMenuItems.Length; ++itemIndex)
        {
            FindUsingMenuItems[itemIndex].IsChecked = AppSettings.FileStatusFindInFilesGitGrepTypeIndex.Value == itemIndex;
        }

        if (tsmiToolbar.Items.Count == 0)
        {
            for (int itemIndex = 0; itemIndex < Toolbar.Children.Count; ++itemIndex)
            {
                Control toolbarItem = Toolbar.Children[itemIndex];
                string settingsKey = $"{nameof(FileStatusList)}.{nameof(Toolbar)}.Visibility.{toolbarItem.Name}";
                MenuItem menuItem = new()
                {
                    ToggleType = MenuItemToggleType.CheckBox,
                    IsChecked = AppSettings.GetBool(settingsKey, defaultValue: true),
                    IsEnabled = !ReferenceEquals(toolbarItem, btnSettings),
                    Header = GetToolbarItemText(itemIndex, toolbarItem),
                    Icon = GetToolbarItemIcon(toolbarItem),
                };
                menuItem.Click += (_, _) =>
                {
                    AppSettings.SetBool(settingsKey, menuItem.IsChecked == true ? null : false);
                    toolbarItem.IsVisible = menuItem.IsChecked == true;
                    UpdateToolbar();
                };
                tsmiToolbar.Items.Add(menuItem);
            }
        }

        for (int itemIndex = 0; itemIndex < Toolbar.Children.Count; ++itemIndex)
        {
            if (tsmiToolbar.Items[itemIndex] is MenuItem { IsChecked: false })
            {
                Toolbar.Children[itemIndex].IsVisible = false;
            }
        }

        return;

        object? GetToolbarItemIcon(Control toolbarItem)
        {
            Avalonia.Media.IImage? image = toolbarItem switch
            {
                IconSplitButton button => button.Icon,
                IconDropDownButton button => button.Icon,
                Button { Content: Image content } => content.Source,
                _ => null,
            };
            return image is null ? null : new Image { Source = image };
        }

        string GetToolbarItemText(int itemIndex, Control toolbarItem)
        {
            Control textSource = toolbarItem is Separator && itemIndex + 1 < Toolbar.Children.Count
                ? Toolbar.Children[itemIndex + 1]
                : toolbarItem;
            string text = ToolTip.GetTip(textSource)?.ToString() ?? textSource.Name ?? textSource.GetType().Name;
            return toolbarItem is Separator ? $"Separator '{text}'" : text;
        }
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
