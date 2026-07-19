using System.ComponentModel;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI;

// Twin of GitUI/UserControls/FileStatusList.cs. Avalonia's ListBox/TreeView templates replace
// MultiSelectTreeView while preserving the original filtering, staging/context-menu, status,
// revision grouping, and repository-hierarchy boundaries used by its consumers.
public partial class FileStatusList : GitModuleControl
{
    private static readonly TimeSpan FilterThrottleDuration = TimeSpan.FromMilliseconds(250);

    private readonly DispatcherTimer _filterTimer = new() { Interval = FilterThrottleDuration };
    private IReadOnlyList<object> _allListItems = [];
    private IReadOnlyList<FileStatusItem> _allTreeItems = [];
    private IReadOnlyList<GitItemStatus> _gitItemFilteredStatuses = [];
    private IReadOnlyList<GitItemStatus> _gitItemStatuses = [];
    private Action? _refreshAction;
    private IGitUICommands? _boundCommands;
    private bool _isFileTreeMode;
    private bool _suppressSelectionChanged;
    private Regex? _filter;

    public FileStatusList()
    {
        InitializeComponent();

        lstFiles.ItemTemplate = new FuncDataTemplate<object>(CreateFileRow, supportsRecycling: false);
        tvFiles.ItemTemplate = new FuncTreeDataTemplate<FileTreeNode>(
            (node, _) => CreateTreeRow(node),
            node => node.Children);
        lstFiles.ContextMenu = ItemContextMenu;
        tvFiles.ContextMenu = ItemContextMenu;
        lstFiles.SelectionChanged += (_, _) => RaiseSelectedIndexChanged();
        tvFiles.SelectionChanged += (_, _) => RaiseSelectedIndexChanged();
        lstFiles.DoubleTapped += (_, _) => DoubleClick?.Invoke(this, EventArgs.Empty);
        tvFiles.DoubleTapped += (_, _) => DoubleClick?.Invoke(this, EventArgs.Empty);
        cboFilterComboBox.TextChanged += cboFilterComboBox_TextChanged;
        DeleteFilterButton.Click += DeleteFilterButton_Click;
        _filterTimer.Tick += FilterTimer_Tick;
        WireContextMenu();
        UICommandsSourceSet += (_, e) => BindCommands(e.GitUICommandsSource.UICommands);
        AttachedToLogicalTree += (_, _) =>
        {
            if (TryGetUICommandsDirect(out IGitUICommands? commands))
            {
                BindCommands(commands);
            }
        };
        DetachedFromLogicalTree += (_, _) =>
        {
            _filterTimer.Stop();
            UnbindCommands();
        };

        InitializeComplete();
    }

    /// <summary>
    ///  Occurs when the selected file changes (named like the WinForms event).
    /// </summary>
    public event EventHandler? SelectedIndexChanged;

    /// <summary>
    ///  Occurs when a file is double-clicked (named like the WinForms event).
    /// </summary>
    public event EventHandler? DoubleClick;

    /// <summary>
    ///  Occurs when the displayed data source is replaced.
    /// </summary>
    public event EventHandler? DataSourceChanged;

    /// <summary>
    ///  Occurs when the filename filter changes.
    /// </summary>
    public event EventHandler? FilterChanged;

    /// <summary>
    ///  Gets the selected file status item, or <see langword="null"/>.
    /// </summary>
    public GitItemStatus? SelectedItem => _isFileTreeMode
        ? (tvFiles.SelectedItem as FileTreeNode)?.Item?.Item
        : GetFileStatusItem(lstFiles.SelectedItem)?.Item ?? lstFiles.SelectedItem as GitItemStatus;

    /// <summary>
    ///  Gets the selected revision-aware item, or <see langword="null"/>.
    /// </summary>
    public FileStatusItem? SelectedFileStatusItem => _isFileTreeMode
        ? (tvFiles.SelectedItem as FileTreeNode)?.Item
        : GetFileStatusItem(lstFiles.SelectedItem);

    /// <summary>
    ///  Gets all selected revision-aware items.
    /// </summary>
    public IEnumerable<FileStatusItem> SelectedItems => _isFileTreeMode
        ? tvFiles.SelectedItems?.OfType<FileTreeNode>().Select(node => node.Item).OfType<FileStatusItem>() ?? []
        : lstFiles.SelectedItems?.Cast<object>().Select(item => GetFileStatusItem(item)).OfType<FileStatusItem>() ?? [];

    /// <summary>
    ///  Gets the selected Git statuses, including worktree lists that do not carry revisions.
    /// </summary>
    public IReadOnlyList<GitItemStatus> SelectedGitItems => _isFileTreeMode
        ? [.. tvFiles.SelectedItems?.OfType<FileTreeNode>().Select(node => node.Item?.Item).OfType<GitItemStatus>() ?? []]
        : [.. lstFiles.SelectedItems?.Cast<object>().Select(GetGitItemStatus).OfType<GitItemStatus>() ?? []];

    /// <summary>
    ///  Gets the selected folder path in file-tree mode, or <see langword="null"/>.
    /// </summary>
    public RelativePath? SelectedFolder
        => tvFiles.SelectedItem is FileTreeNode { IsFolder: true } node ? RelativePath.From(node.FullPath) : null;

    /// <summary>
    ///  Gets the selected file or folder path.
    /// </summary>
    public RelativePath? SelectedRelativePath
        => _isFileTreeMode && tvFiles.SelectedItem is FileTreeNode node
            ? RelativePath.From(node.FullPath)
            : SelectedItem is GitItemStatus item
                ? RelativePath.From(item.Name)
                : null;

    /// <summary>
    ///  Gets the displayed file statuses (named like the WinForms property).
    /// </summary>
    public IReadOnlyList<GitItemStatus> GitItemStatuses => _gitItemStatuses;

    /// <summary>
    ///  Gets the statuses currently visible after filtering.
    /// </summary>
    public IReadOnlyList<GitItemStatus> GitItemFilteredStatuses => _gitItemFilteredStatuses;

    /// <summary>
    ///  Gets whether at least one item is selected.
    /// </summary>
    public bool HasSelection => SelectedGitItems.Count > 0 || SelectedFolder is not null;

    /// <summary>
    ///  Gets whether the unfiltered list is empty.
    /// </summary>
    public bool IsEmpty => _gitItemStatuses.Count == 0;

    /// <summary>
    ///  Gets the number of items before filtering.
    /// </summary>
    public int UnfilteredItemsCount => _gitItemStatuses.Count;

    /// <summary>
    ///  Gets whether a filename filter is active.
    /// </summary>
    public bool IsFilterActive => !string.IsNullOrEmpty(cboFilterComboBox.Text);

    /// <summary>
    ///  Gets whether the filename filter owns keyboard focus.
    /// </summary>
    public bool FilterFilesByNameRegexFocused => cboFilterComboBox.IsKeyboardFocusWithin;

    /// <summary>
    ///  Gets or sets the selection mode of the underlying list.
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => lstFiles.SelectionMode;
        set
        {
            lstFiles.SelectionMode = value;
            tvFiles.SelectionMode = value;
        }
    }

    /// <summary>
    ///  Shows the given diff items.
    /// </summary>
    public void SetDiffs(IReadOnlyList<GitItemStatus> items)
    {
        SetFileTreeMode(false);
        _gitItemStatuses = items;
        _allListItems = [.. items.Cast<object>()];
        _allTreeItems = [];
        ApplyFilter(selectFirstItem: true);
    }

    /// <summary>
    ///  Shows files changed between two revisions.
    /// </summary>
    public void SetDiffs(GitRevision? firstRev, GitRevision secondRev, IReadOnlyList<GitItemStatus> items)
    {
        List<FileStatusEntry> entries = items
            .Select(item => new FileStatusEntry(new FileStatusItem(firstRev, secondRev, item), null))
            .ToList();
        SetRevisionEntries(entries);
    }

    /// <summary>
    ///  Shows calculated revision groups as either the ordinary diff list or a repository tree.
    /// </summary>
    public void SetDiffs(IReadOnlyList<FileStatusWithDescription> groups, bool isFileTreeMode)
    {
        SetFileTreeMode(isFileTreeMode);
        if (isFileTreeMode)
        {
            FileStatusItem[] items =
            [
                .. groups.SelectMany(group => group.Statuses.Select(
                    item => new FileStatusItem(group.FirstRev, group.SecondRev, item, group.BaseA, group.BaseB))),
            ];
            SetTreeEntries(items);
            return;
        }

        List<FileStatusEntry> entries = [];
        foreach (FileStatusWithDescription group in groups)
        {
            for (int index = 0; index < group.Statuses.Count; index++)
            {
                entries.Add(new FileStatusEntry(
                    new FileStatusItem(group.FirstRev, group.SecondRev, group.Statuses[index], group.BaseA, group.BaseB),
                    index == 0 ? group.Summary : null));
            }
        }

        SetRevisionEntries(entries);
    }

    /// <summary>
    ///  Shows the worktree and index portions of the current working directory.
    /// </summary>
    public void SetStashDiffs(
        GitRevision headRev,
        GitRevision indexRev,
        string indexDesc,
        IReadOnlyList<GitItemStatus> indexItems,
        GitRevision workTreeRev,
        string workTreeDesc,
        IReadOnlyList<GitItemStatus> workTreeItems)
    {
        List<FileStatusEntry> entries = [];
        entries.AddRange(CreateGroup(indexRev, workTreeRev, workTreeDesc, workTreeItems));
        entries.AddRange(CreateGroup(headRev, indexRev, indexDesc, indexItems));
        SetRevisionEntries(entries);
    }

    /// <summary>
    ///  Clears all diff entries.
    /// </summary>
    public void ClearDiffs() => Clear();

    public void Clear()
    {
        _allListItems = [];
        _allTreeItems = [];
        _gitItemFilteredStatuses = [];
        _gitItemStatuses = [];
        lstFiles.ItemsSource = null;
        tvFiles.ItemsSource = null;
        UpdateCount(0);
        UpdateEmptyState();
    }

    /// <summary>
    ///  Focuses the active file list or repository tree.
    /// </summary>
    public void FocusFiles()
    {
        Control target = _isFileTreeMode ? tvFiles : lstFiles;
        if (!target.Focus())
        {
            Dispatcher.UIThread.Post(() => target.Focus());
        }
    }

    /// <summary>
    ///  Clears the current selection (named like the WinForms method).
    /// </summary>
    public void ClearSelected()
    {
        lstFiles.Selection.Clear();
        tvFiles.SelectedItems?.Clear();
    }

    /// <summary>
    ///  Selects all visible files.
    /// </summary>
    public void SelectAll()
    {
        if (_isFileTreeMode)
        {
            tvFiles.SelectAll();
        }
        else
        {
            lstFiles.SelectAll();
        }
    }

    /// <summary>
    ///  Applies a case-insensitive regular-expression filter to file and old names.
    /// </summary>
    /// <returns>The number of visible files.</returns>
    public int SetFilter(string value)
    {
        cboFilterComboBox.Text = value;
        return ApplyFilter(selectFirstItem: true);
    }

    /// <summary>
    ///  Selects a file or folder by its repository-relative POSIX path.
    /// </summary>
    public bool SelectFileOrFolder(RelativePath relativePath, bool notify = true)
    {
        if (!_isFileTreeMode)
        {
            object? item = lstFiles.Items.Cast<object>().FirstOrDefault(
                candidate => GetFileStatusItem(candidate)?.Item.Name == relativePath.Value);
            if (item is null)
            {
                return false;
            }

            _suppressSelectionChanged = !notify;
            lstFiles.SelectedItem = item;
            _suppressSelectionChanged = false;
            lstFiles.ScrollIntoView(item);
            return true;
        }

        FileTreeNode? node = tvFiles.Items.Cast<FileTreeNode>()
            .SelectMany(Flatten)
            .FirstOrDefault(candidate => candidate.FullPath == relativePath.Value);
        if (node is null)
        {
            return false;
        }

        _suppressSelectionChanged = !notify;
        ExpandAncestors(node);
        tvFiles.SelectedItem = node;
        _suppressSelectionChanged = false;
        return true;
    }

    /// <summary>
    ///  Selects the first file in the active list/tree.
    /// </summary>
    public void SelectFirstVisibleItem()
    {
        if (_isFileTreeMode)
        {
            FileTreeNode? first = tvFiles.Items.Cast<FileTreeNode>()
                .SelectMany(Flatten)
                .FirstOrDefault(node => !node.IsFolder);
            if (first is not null)
            {
                ExpandAncestors(first);
                tvFiles.SelectedItem = first;
            }

            return;
        }

        if (lstFiles.ItemCount > 0)
        {
            lstFiles.SelectedIndex = 0;
        }
    }

    /// <summary>
    ///  Selects the preceding visible file, wrapping to the first file at the boundary.
    /// </summary>
    public void SelectPreviousVisibleItem()
    {
        if (_isFileTreeMode)
        {
            SelectAdjacentTreeFile(-1);
            return;
        }

        if (lstFiles.ItemCount == 0)
        {
            return;
        }

        lstFiles.SelectedIndex = lstFiles.SelectedIndex > 0
            ? lstFiles.SelectedIndex - 1
            : 0;
        if (lstFiles.SelectedItem is object selectedItem)
        {
            lstFiles.ScrollIntoView(selectedItem);
        }
    }

    /// <summary>
    ///  Selects the following visible file, wrapping to the first file at the boundary.
    /// </summary>
    public void SelectNextVisibleItem()
    {
        if (_isFileTreeMode)
        {
            SelectAdjacentTreeFile(1);
            return;
        }

        if (lstFiles.ItemCount == 0)
        {
            return;
        }

        lstFiles.SelectedIndex = lstFiles.SelectedIndex + 1 < lstFiles.ItemCount
            ? lstFiles.SelectedIndex + 1
            : 0;
        if (lstFiles.SelectedItem is object selectedItem)
        {
            lstFiles.ScrollIntoView(selectedItem);
        }
    }

    /// <summary>
    ///  Refreshes this list when repository-change notifications are raised.
    /// </summary>
    public void Bind(Action refreshAction)
    {
        _refreshAction = refreshAction;
        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            BindCommands(commands);
        }
    }

    private static IEnumerable<FileStatusEntry> CreateGroup(
        GitRevision firstRev,
        GitRevision secondRev,
        string description,
        IReadOnlyList<GitItemStatus> items)
    {
        for (int index = 0; index < items.Count; index++)
        {
            yield return new FileStatusEntry(
                new FileStatusItem(firstRev, secondRev, items[index]),
                index == 0 ? description : null);
        }
    }

    private void SetRevisionEntries(IReadOnlyList<FileStatusEntry> entries)
    {
        SetFileTreeMode(false);
        _gitItemStatuses = entries.Select(entry => entry.Item.Item).ToList();
        _allListItems = [.. entries.Cast<object>()];
        _allTreeItems = [];
        ApplyFilter(selectFirstItem: true);
    }

    private void SetTreeEntries(IReadOnlyList<FileStatusItem> items)
    {
        _gitItemStatuses = items.Select(item => item.Item).ToList();
        _allListItems = [];
        _allTreeItems = items;
        ApplyFilter(selectFirstItem: false);
    }

    private void SetFileTreeMode(bool isFileTreeMode)
    {
        _isFileTreeMode = isFileTreeMode;
        lstFiles.IsVisible = !isFileTreeMode;
        tvFiles.IsVisible = isFileTreeMode;
    }

    private void RaiseSelectedIndexChanged()
    {
        if (!_suppressSelectionChanged)
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void cboFilterComboBox_TextChanged(object? sender, EventArgs e)
    {
        DeleteFilterButton.IsVisible = IsFilterActive;
        _filterTimer.Stop();
        _filterTimer.Start();
    }

    private void DeleteFilterButton_Click(object? sender, EventArgs e)
    {
        SetFilter(string.Empty);
        cboFilterComboBox.Focus();
    }

    private void FilterTimer_Tick(object? sender, EventArgs e)
    {
        _filterTimer.Stop();
        ApplyFilter(selectFirstItem: false);
    }

    private int ApplyFilter(bool selectFirstItem)
    {
        _filterTimer.Stop();
        RelativePath? selectedPath = SelectedRelativePath;
        string filterText = cboFilterComboBox.Text ?? string.Empty;
        try
        {
            _filter = filterText.Length == 0
                ? null
                : new Regex(filterText, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            SetFilterState(filterText.Length == 0 ? FilterState.Empty : FilterState.Valid, toolTip: null);
        }
        catch (ArgumentException exception)
        {
            SetFilterState(FilterState.Invalid, exception.Message);
            return _gitItemFilteredStatuses.Count;
        }

        if (_isFileTreeMode)
        {
            FileStatusItem[] visibleItems = [.. _allTreeItems.Where(item => IsFilterMatch(item.Item))];
            _gitItemFilteredStatuses = [.. visibleItems.Select(item => item.Item)];
            tvFiles.ItemsSource = BuildFileTree(visibleItems);
        }
        else
        {
            List<object> visibleItems = [];
            string? currentGroupDescription = null;
            bool groupDescriptionUsed = false;
            foreach (object item in _allListItems)
            {
                if (item is FileStatusEntry { GroupDescription: not null } groupEntry)
                {
                    currentGroupDescription = groupEntry.GroupDescription;
                    groupDescriptionUsed = false;
                }

                if (GetGitItemStatus(item) is not GitItemStatus status || !IsFilterMatch(status))
                {
                    continue;
                }

                if (item is FileStatusEntry entry)
                {
                    visibleItems.Add(entry with
                    {
                        GroupDescription = groupDescriptionUsed ? null : currentGroupDescription,
                    });
                    groupDescriptionUsed = true;
                }
                else
                {
                    visibleItems.Add(item);
                }
            }

            _gitItemFilteredStatuses = [.. visibleItems.Select(GetGitItemStatus).OfType<GitItemStatus>()];
            lstFiles.ItemsSource = visibleItems;
        }

        UpdateCount(_gitItemFilteredStatuses.Count);
        UpdateEmptyState();
        bool selectionRestored = selectedPath is not null && SelectFileOrFolder(selectedPath, notify: false);
        if (!selectionRestored && selectFirstItem)
        {
            SelectFirstVisibleItem();
        }

        DataSourceChanged?.Invoke(this, EventArgs.Empty);
        FilterChanged?.Invoke(this, EventArgs.Empty);
        return _gitItemFilteredStatuses.Count;
    }

    private bool IsFilterMatch(GitItemStatus item)
    {
        if (item.IsRangeDiff || _filter is null)
        {
            return true;
        }

        string name = item.Name.TrimEnd(PathUtil.PosixDirectorySeparatorChar);
        return _filter.IsMatch(name)
               || (item.OldName is string oldName && _filter.IsMatch(oldName));
    }

    private void SetFilterState(FilterState state, string? toolTip)
    {
        cboFilterComboBox.Classes.Set("file-filter-active", state == FilterState.Valid);
        cboFilterComboBox.Classes.Set("file-filter-invalid", state == FilterState.Invalid);
        ToolTip.SetTip(cboFilterComboBox, toolTip);
        DeleteFilterButton.IsVisible = IsFilterActive;
    }

    private void UpdateEmptyState()
    {
        bool hasItems = _gitItemFilteredStatuses.Count > 0;
        NoFiles.IsVisible = !hasItems;
        lstFiles.IsVisible = hasItems && !_isFileTreeMode;
        tvFiles.IsVisible = hasItems && _isFileTreeMode;
    }

    private void SelectAdjacentTreeFile(int offset)
    {
        FileTreeNode[] files =
        [
            .. tvFiles.Items.Cast<FileTreeNode>().SelectMany(Flatten).Where(node => !node.IsFolder),
        ];
        if (files.Length == 0)
        {
            return;
        }

        int selectedIndex = Array.IndexOf(files, tvFiles.SelectedItem as FileTreeNode);
        int nextIndex = selectedIndex < 0 ? 0 : Math.Clamp(selectedIndex + offset, 0, files.Length - 1);
        tvFiles.SelectedItem = files[nextIndex];
    }

    private static IReadOnlyList<FileTreeNode> BuildFileTree(IReadOnlyList<FileStatusItem> items)
    {
        List<FileTreeNode> roots = [];
        foreach (FileStatusItem item in items)
        {
            string[] segments = item.Item.Name.Split(PathUtil.PosixDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            List<FileTreeNode> siblings = roots;
            FileTreeNode? parent = null;
            string path = string.Empty;
            for (int index = 0; index < segments.Length; index++)
            {
                path = path.Length == 0 ? segments[index] : $"{path}/{segments[index]}";
                bool isFile = index == segments.Length - 1;
                FileTreeNode? node = siblings.FirstOrDefault(candidate => candidate.Name == segments[index]);
                if (node is null)
                {
                    node = new FileTreeNode(segments[index], path, isFile ? item : null, parent);
                    siblings.Add(node);
                }

                parent = node;
                siblings = node.Children;
            }
        }

        Sort(roots);
        return roots;

        static void Sort(List<FileTreeNode> nodes)
        {
            nodes.Sort((left, right) => (left.IsFolder, right.IsFolder) switch
            {
                (true, false) => -1,
                (false, true) => 1,
                _ => string.Compare(left.Name, right.Name, StringComparison.Ordinal),
            });
            foreach (FileTreeNode node in nodes)
            {
                Sort(node.Children);
            }
        }
    }

    private static IEnumerable<FileTreeNode> Flatten(FileTreeNode node)
    {
        yield return node;
        foreach (FileTreeNode child in node.Children.SelectMany(Flatten))
        {
            yield return child;
        }
    }

    private static void ExpandAncestors(FileTreeNode node)
    {
        for (FileTreeNode? parent = node.Parent; parent is not null; parent = parent.Parent)
        {
            parent.IsExpanded = true;
        }
    }

    private Control CreateTreeRow(FileTreeNode node)
    {
        Image image = new()
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            Source = node.Item is null ? Images.FolderClosed : GetItemImage(node.Item.Item),
            Margin = new Avalonia.Thickness(1, 0, 3, 0),
        };
        if (node.Item is not null)
        {
            UpdateSubmoduleImageWhenReady(image, node.Item.Item);
        }

        return new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children =
            {
                image,
                new TextBlock
                {
                    Text = node.Name,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                },
            },
        };
    }

    private void UpdateCount(int count)
    {
        string suffix = _gitItemStatuses.Count == 1 ? "file" : "files";
        lblCount.Text = count == _gitItemStatuses.Count
            ? $"{count} {suffix}"
            : $"{count} / {_gitItemStatuses.Count} {suffix}";
    }

    private void BindCommands(IGitUICommands commands)
    {
        if (_refreshAction is null || ReferenceEquals(_boundCommands, commands))
        {
            return;
        }

        UnbindCommands();
        _boundCommands = commands;
        _boundCommands.PostRepositoryChanged += BoundCommands_PostRepositoryChanged;
    }

    private void UnbindCommands()
    {
        if (_boundCommands is not null)
        {
            _boundCommands.PostRepositoryChanged -= BoundCommands_PostRepositoryChanged;
            _boundCommands = null;
        }
    }

    private void BoundCommands_PostRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        _refreshAction?.Invoke();
    }

    private static FileStatusItem? GetFileStatusItem(object? item)
        => item switch
        {
            FileStatusEntry entry => entry.Item,
            FileStatusItem fileStatusItem => fileStatusItem,
            _ => null,
        };

    private static GitItemStatus? GetGitItemStatus(object? item)
        => GetFileStatusItem(item)?.Item ?? item as GitItemStatus;

    private Control CreateFileRow(object item, INameScope nameScope)
    {
        // Avalonia can briefly rebuild a recycled presenter with null while ItemsSource is
        // replaced after staging. The typed template annotation does not expose that state.
        if (item is null)
        {
            return new Panel();
        }

        FileStatusEntry? entry = item as FileStatusEntry;
        GitItemStatus? gitItemStatus = entry?.Item.Item ?? item as GitItemStatus;
        if (gitItemStatus is null)
        {
            return new Panel();
        }

        StackPanel row = new()
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };
        if (entry?.GroupDescription is not null)
        {
            row.Children.Add(new TextBlock
            {
                Text = entry.GroupDescription,
                FontWeight = FontWeight.Bold,
                Margin = new Avalonia.Thickness(4, 5, 4, 2),
            });
        }

        Image image = new()
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            Source = GetItemImage(gitItemStatus),
            Margin = new Avalonia.Thickness(3, 0, 3, 0),
        };
        UpdateSubmoduleImageWhenReady(image, gitItemStatus);
        row.Children.Add(
            new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Children =
                {
                    image,
                    new TextBlock
                    {
                        Text = gitItemStatus.OldName is null
                            ? gitItemStatus.Name
                            : $"{gitItemStatus.Name} ({gitItemStatus.OldName})",
                        TextTrimming = TextTrimming.CharacterEllipsis,
                    },
                },
            });

        return row;
    }

    private static IImage GetItemImage(GitItemStatus item)
    {
        if (item.IsDeleted)
        {
            return item.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => Images.FileStatusRemovedOnlyA,
                DiffBranchStatus.OnlyBChange => Images.FileStatusRemovedOnlyB,
                DiffBranchStatus.SameChange => Images.FileStatusRemovedSame,
                DiffBranchStatus.UnequalChange => Images.FileStatusRemovedUnequal,
                _ => Images.FileStatusRemoved,
            };
        }

        if (item.IsRangeDiff)
        {
            return Images.DiffR;
        }

        if (!string.IsNullOrWhiteSpace(item.GrepString))
        {
            return Images.File;
        }

        if (item.IsNew || !item.IsTracked)
        {
            return item.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => Images.FileStatusAddedOnlyA,
                DiffBranchStatus.OnlyBChange => Images.FileStatusAddedOnlyB,
                DiffBranchStatus.SameChange => Images.FileStatusAddedSame,
                DiffBranchStatus.UnequalChange => Images.FileStatusAddedUnequal,
                _ => Images.FileStatusAdded,
            };
        }

        if (item.IsUnmerged)
        {
            return Images.Unmerged;
        }

        if (item.IsSubmodule)
        {
            return item.IsDirty ? Images.SubmoduleDirty : Images.SubmodulesManage;
        }

        if (item.IsChanged || (item.IsRenamed && item.RenameCopyPercentage != "100"))
        {
            return item.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => Images.FileStatusModifiedOnlyA,
                DiffBranchStatus.OnlyBChange => Images.FileStatusModifiedOnlyB,
                DiffBranchStatus.SameChange => Images.FileStatusModifiedSame,
                DiffBranchStatus.UnequalChange => Images.FileStatusModifiedUnequal,
                _ => Images.FileStatusModified,
            };
        }

        if (item.IsRenamed)
        {
            return item.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => Images.FileStatusRenamedOnlyA,
                DiffBranchStatus.OnlyBChange => Images.FileStatusRenamedOnlyB,
                DiffBranchStatus.SameChange => Images.FileStatusRenamedSame,
                DiffBranchStatus.UnequalChange => Images.FileStatusRenamedUnequal,
                _ => Images.FileStatusRenamed,
            };
        }

        if (item.IsCopied)
        {
            return item.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => Images.FileStatusCopiedOnlyA,
                DiffBranchStatus.OnlyBChange => Images.FileStatusCopiedOnlyB,
                DiffBranchStatus.SameChange => Images.FileStatusCopiedSame,
                DiffBranchStatus.UnequalChange => Images.FileStatusCopiedUnequal,
                _ => Images.FileStatusCopied,
            };
        }

        return Images.FileStatusUnknown;
    }

    private static IImage GetSubmoduleImage(GitItemStatus item, GitSubmoduleStatus? status)
    {
        if (status is null)
        {
            return item.IsDirty ? Images.SubmoduleDirty : Images.SubmodulesManage;
        }

        return (status.Status, status.IsDirty) switch
        {
            (SubmoduleStatus.FastForward, true) => Images.SubmoduleRevisionUpDirty,
            (SubmoduleStatus.FastForward, false) => Images.SubmoduleRevisionUp,
            (SubmoduleStatus.Rewind, true) => Images.SubmoduleRevisionDownDirty,
            (SubmoduleStatus.Rewind, false) => Images.SubmoduleRevisionDown,
            (SubmoduleStatus.NewerTime, true) => Images.SubmoduleRevisionSemiUpDirty,
            (SubmoduleStatus.NewerTime, false) => Images.SubmoduleRevisionSemiUp,
            (SubmoduleStatus.OlderTime, true) => Images.SubmoduleRevisionSemiDownDirty,
            (SubmoduleStatus.OlderTime, false) => Images.SubmoduleRevisionSemiDown,
            (SubmoduleStatus.SameCommit, false) => Images.FolderSubmodule,
            _ => Images.SubmoduleDirty,
        };
    }

    private static void UpdateSubmoduleImageWhenReady(Image image, GitItemStatus item)
    {
        if (!item.IsSubmodule)
        {
            return;
        }

        Task<GitSubmoduleStatus?> task = item.GetSubmoduleStatusAsync();
        ThreadHelper.FileAndForget(async () =>
        {
#pragma warning disable VSTHRD003 // GitItemStatus owns and starts the cached status task.
            GitSubmoduleStatus? status = await task.ConfigureAwait(false);
#pragma warning restore VSTHRD003
            await Dispatcher.UIThread.InvokeAsync(() => image.Source = GetSubmoduleImage(item, status));
        });
    }

    private sealed record FileStatusEntry(FileStatusItem Item, string? GroupDescription);

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(FileStatusList control)
    {
        internal TextBox FilterComboBox => control.cboFilterComboBox;
        internal TextBlock CountLabel => control.lblCount;
        internal TextBlock NoFilesLabel => control.NoFiles;
        internal ListBox List => control.lstFiles;
        internal TreeView Tree => control.tvFiles;
        internal ContextMenu ContextMenu => control.ItemContextMenu;
        internal MenuItem StageMenuItem => control.tsmiStageFile;
        internal MenuItem UnstageMenuItem => control.tsmiUnstageFile;
        internal MenuItem CherryPickMenuItem => control.tsmiCherryPickChanges;

        internal void UpdateContextMenu()
            => control.ItemContextMenu_Opening(control.ItemContextMenu, EventArgs.Empty);

        internal static IImage GetItemImage(GitItemStatus item)
            => FileStatusList.GetItemImage(item);

        internal static IImage GetSubmoduleImage(GitItemStatus item, GitSubmoduleStatus? status)
            => FileStatusList.GetSubmoduleImage(item, status);
    }

    private enum FilterState
    {
        Empty,
        Valid,
        Invalid
    }

    private sealed class FileTreeNode(string name, string fullPath, FileStatusItem? item, FileTreeNode? parent) : INotifyPropertyChanged
    {
        private bool _isExpanded;

        public string Name { get; } = name;
        public string FullPath { get; } = fullPath;
        public FileStatusItem? Item { get; } = item;
        public FileTreeNode? Parent { get; } = parent;
        public bool IsFolder => Item is null;
        public List<FileTreeNode> Children { get; } = [];

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                {
                    return;
                }

                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
