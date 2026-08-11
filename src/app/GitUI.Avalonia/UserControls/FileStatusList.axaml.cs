using System.ComponentModel;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Compat;
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
    public static readonly TimeSpan SelectedIndexChangeThrottleDuration = TimeSpan.FromMilliseconds(50);

    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private static readonly TimeSpan FilterThrottleDuration = TimeSpan.FromMilliseconds(250);

    private readonly FileStatusDiffCalculator _diffCalculator;
    private readonly FileAssociatedIconProvider _iconProvider = new();
    private readonly CancellationTokenSequence _customDiffToolsSequence = new();
    private readonly GitRevisionTester _gitRevisionTester;
    private readonly MenuItem _NO_TRANSLATE_openSubmoduleMenuItem;
    private readonly CancellationTokenSequence _reloadSequence = new();
    private readonly SortDiffListContextMenuItem _sortByContextMenu;
    private readonly Separator _sortBySeparator = new();
    private readonly DispatcherTimer _filterTimer = new() { Interval = FilterThrottleDuration };
    private readonly DispatcherTimer _selectedIndexChangeTimer = new() { Interval = SelectedIndexChangeThrottleDuration };
    private readonly List<string> _filterHistory = [];
    private readonly List<object?> _gitGrepSearchItems = [];
    private IReadOnlyList<object> _allListItems = [];
    private IReadOnlyList<FileStatusItem> _allTreeItems = [];
    private IReadOnlyList<GitItemStatus> _gitItemFilteredStatuses = [];
    private IReadOnlyList<GitItemStatus> _gitItemStatuses = [];
    private Action? _refreshAction;
    private IGitUICommands? _boundCommands;
    private bool _isFileTreeMode;
    private bool _isSortSubscriptionActive;

    // Enable menu item to disable AppSettings.ShowDiffForAllParents in some forms
    private bool _enableDisablingShowDiffForAllParents;
    private bool _showDiffGroups;
    private bool _suppressSelectionChanged;
    private bool _hasKeyboardFocus;
    private bool _mouseEntered;
    private Regex? _filter;
    private GitItemStatus? _nextItemToSelect;
    private FormFindInCommitFilesGitGrep? _formFindInCommitFilesGitGrep;

    [GeneratedRegex(@"(^|\s)-e(\s|\s+['""])", RegexOptions.ExplicitCapture)]
    private static partial Regex GrepStringRegex { get; }

    public FileStatusList()
    {
        _diffCalculator = new FileStatusDiffCalculator(() => Module);
        _fullPathResolver = new FullPathResolver(() => TryGetUICommandsDirect(out IGitUICommands? commands) ? commands.Module.WorkingDir : string.Empty);
        _gitRevisionTester = new GitRevisionTester(_fullPathResolver);
        _revisionDiffController = new RevisionDiffController(() => Module, _fullPathResolver);
        InitializeComponent();

        // Trigger initialisation of Search and Filter boxes
        _NO_TRANSLATE_openSubmoduleMenuItem = CreateOpenSubmoduleMenuItem();
        _sortByContextMenu = new SortDiffListContextMenuItem(DiffListSortService.Instance)
        {
            Name = "sortListByContextMenuItem",
        };
        tsmiCopyPaths.Initialize(
            () => TryGetUICommandsDirect(out IGitUICommands? commands)
                ? commands
                : throw new InvalidOperationException("The menu is not attached to UI commands."),
            GetSelectedFilePaths);

        lstFiles.ItemTemplate = new FuncDataTemplate<object>(CreateFileRow, supportsRecycling: false);
        tvDiffFiles.ItemTemplate = new FuncTreeDataTemplate<DiffTreeNode>(
            (node, _) => CreateDiffTreeRow(node),
            node => node.Children);
        tvFiles.ItemTemplate = new FuncTreeDataTemplate<FileTreeNode>(
            (node, _) => CreateTreeRow(node),
            node => node.Children);
        lstFiles.ContextMenu = ItemContextMenu;
        tvDiffFiles.ContextMenu = ItemContextMenu;
        tvFiles.ContextMenu = ItemContextMenu;
        lstFiles.SelectionChanged += (_, _) => RaiseSelectedIndexChanged();
        tvDiffFiles.SelectionChanged += (_, _) => DiffTreeSelectionChanged();
        tvDiffFiles.ContainerPrepared += DiffTreeContainerPrepared;
        tvFiles.SelectionChanged += (_, _) => RaiseSelectedIndexChanged();
        lstFiles.DoubleTapped += FileStatusListView_DoubleClick;
        tvDiffFiles.DoubleTapped += (_, _) =>
        {
            if (SelectedFileStatusItem is not null)
            {
                FileStatusListView_DoubleClick(this, EventArgs.Empty);
            }
        };
        tvFiles.DoubleTapped += FileStatusListView_DoubleClick;
        cboFilterComboBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == ComboBox.TextProperty)
            {
                FilterComboBox_TextUpdate(this, EventArgs.Empty);
            }
        };
        cboFilterComboBox.PointerEntered += FilterComboBox_MouseEnter;
        cboFilterComboBox.SelectionChanged += FilterComboBox_SelectedIndexChanged;
        cboFilterComboBox.SizeChanged += FilterComboBox_SizeChanged;
        cboFindInCommitFilesGitGrep.PropertyChanged += (_, e) =>
        {
            if (e.Property == ComboBox.TextProperty)
            {
                cboFindInCommitFilesGitGrep_TextUpdate(this, EventArgs.Empty);
            }
        };
        cboFindInCommitFilesGitGrep.SelectionChanged += cboFindInCommitFilesGitGrep_SelectedIndexChanged;
        cboFindInCommitFilesGitGrep.SizeChanged += cboFindInCommitFilesGitGrep_SizeChanged;
        DeleteFilterButton.Click += DeleteFilterButton_Click;
        DeleteSearchButton.Click += DeleteSearchButton_Click;
        _filterTimer.Tick += FilterTimer_Tick;
        _selectedIndexChangeTimer.Tick += SelectedIndexChangeTimer_Tick;
        KeyDown += FileStatusListView_KeyDown;
        GotFocus += FileStatusList_Enter;
        LostFocus += FileStatusList_LostFocus;
        PointerPressed += FileStatusList_PointerPressed;
        WireToolbar();
        WireContextMenu();
        AttachedToLogicalTree += (_, _) =>
        {
            if (!_isSortSubscriptionActive)
            {
                DiffListSortService.Instance.DiffListSortingChanged += DiffListSortingChanged;
                _isSortSubscriptionActive = true;
            }

            if (TryGetUICommandsDirect(out IGitUICommands? commands))
            {
                BindCommands(commands);
                ReloadHotkeys();
                LoadCustomDifftools();
                SetFindInCommitFilesGitGrepVisibility(AppSettings.ShowFindInCommitFilesGitGrep.Value);
            }
        };
        DetachedFromLogicalTree += (_, _) =>
        {
            _filterTimer.Stop();
            _selectedIndexChangeTimer.Stop();
            if (_isSortSubscriptionActive)
            {
                DiffListSortService.Instance.DiffListSortingChanged -= DiffListSortingChanged;
                _isSortSubscriptionActive = false;
            }

            CancelLoadCustomDifftools();
            UnbindCommands();
        };

        InitializeComplete();

        MenuItem CreateOpenSubmoduleMenuItem()
        {
            MenuItem item = new()
            {
                Name = "openSubmoduleMenuItem",
                Tag = "1",
                Header = TranslatedStrings.OpenWithGitExtensions,
                Icon = new Image { Source = Images.GitExtensionsLogo16 }
            };
            item.Click += (_, _) => this.InvokeAndForget(OpenSubmoduleAsync);
            return item;
        }
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        tsmiCopyPaths.AddControlTranslationItems(translation);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        tsmiCopyPaths.TranslateControlItems(translation);
    }

    // Wire up events to respond to Settings changes
    protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
    {
        base.OnUICommandsSourceSet(source);
        source.UICommandsChanged += OnUICommandsChanged;
        OnUICommandsChanged(source, null);
        return;

        void OnUICommandsChanged(object? sender, GitUICommandsChangedEventArgs? e)
        {
            if (e?.OldCommands is not null)
            {
                e.OldCommands.PostSettings -= UICommands_PostSettings;
            }

            IGitUICommandsSource? commandSource = sender as IGitUICommandsSource;
            if (commandSource?.UICommands is not null)
            {
                BindCommands(commandSource.UICommands);
                commandSource.UICommands.PostSettings += UICommands_PostSettings;
                UICommands_PostSettings(commandSource.UICommands, null);
            }
        }

        // Show/hide the search box if settings are changed
        void UICommands_PostSettings(object? sender, GitUIPostActionEventArgs? e)
        {
            if (CanUseFindInCommitFilesGitGrep && IsVisible)
            {
                Dispatcher.UIThread.Post(() => SetFindInCommitFilesGitGrepVisibility(AppSettings.ShowFindInCommitFilesGitGrep.Value));
            }
        }
    }

    private void FileStatusListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.A)
        {
            SelectAll();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Multiply)
        {
            // Avalonia does not produce WinForms' TreeView ding, so this route can be handled after expanding the focused node.
            object? focusedNode = (_isFileTreeMode ? tvFiles : tvDiffFiles).SelectedItem;
            if (focusedNode is not null)
            {
                SetExpansion(focusedNode, expanded: true);
            }

            e.Handled = true;
            return;
        }

        if (ProcessHotkey(KeysMapper.ToKeys(e)))
        {
            e.Handled = true;
        }
    }

    private void FileStatusListView_DoubleClick(object? sender, EventArgs e)
    {
        if (DoubleClick is null)
        {
            if (SelectedItem?.Item is null)
            {
                return;
            }

            if (AppSettings.OpenSubmoduleDiffInSeparateWindow && SelectedItem.Item.IsSubmodule)
            {
                this.InvokeAndForget(OpenSubmoduleAsync);
            }
            else
            {
                UICommands.StartFileHistoryDialog(GetOwner(), SelectedItem.Item.Name, SelectedItem.SecondRevision);
            }
        }
        else
        {
            DoubleClick?.Invoke(sender, e);
        }
    }

    private IEnumerable<string?> GetSelectedFilePaths()
        => SelectedFolder is RelativePath folder
            ? [folder.Value]
            : SelectedGitItems.Select(item => item.Name);

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

    public delegate void EnterEventHandler(object? sender, EnterEventArgs e);

    // Avalonia Control has no Enter event, so the WinForms new modifier is neither required nor legal here.
    public event EnterEventHandler? Enter;

    /// <summary>
    ///  Gets the selected file status item, or <see langword="null"/>.
    /// </summary>
    public FileStatusItem? SelectedItem => _isFileTreeMode
        ? (tvFiles.SelectedItem as FileTreeNode)?.Item
        : _showDiffGroups
            ? (tvDiffFiles.SelectedItem as DiffTreeNode)?.Item
            : GetFileStatusItem(lstFiles.SelectedItem);

    /// <summary>
    ///  Gets the selected revision-aware item, or <see langword="null"/>.
    /// </summary>
    public FileStatusItem? SelectedFileStatusItem => _isFileTreeMode
        ? (tvFiles.SelectedItem as FileTreeNode)?.Item
        : _showDiffGroups
            ? (tvDiffFiles.SelectedItem as DiffTreeNode)?.Item
            : GetFileStatusItem(lstFiles.SelectedItem);

    /// <summary>
    ///  Gets all selected revision-aware items.
    /// </summary>
    public IEnumerable<FileStatusItem> SelectedItems
    {
        get => _isFileTreeMode
            ? tvFiles.SelectedItems?.OfType<FileTreeNode>().Select(node => node.Item).OfType<FileStatusItem>() ?? []
            : _showDiffGroups
                ? tvDiffFiles.SelectedItems?.OfType<DiffTreeNode>().Select(node => node.Item).OfType<FileStatusItem>() ?? []
                : lstFiles.SelectedItems?.Cast<object>().Select(item => GetFileStatusItem(item)).OfType<FileStatusItem>() ?? [];
        set
        {
            if (value is null)
            {
                ClearSelected();
                return;
            }

            HashSet<FileStatusItem> selected = [.. value];
            ClearSelected();
            if (_showDiffGroups)
            {
                foreach (DiffTreeNode node in tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(Flatten).Where(node => node.Item is not null && selected.Contains(node.Item)))
                {
                    tvDiffFiles.SelectedItems?.Add(node);
                }
            }
            else if (_isFileTreeMode)
            {
                foreach (FileTreeNode node in tvFiles.Items.Cast<FileTreeNode>().SelectMany(Flatten).Where(node => node.Item is not null && selected.Contains(node.Item)))
                {
                    tvFiles.SelectedItems?.Add(node);
                }
            }
            else
            {
                foreach (object item in lstFiles.Items.Cast<object>().Where(item => GetFileStatusItem(item) is FileStatusItem status && selected.Contains(status)))
                {
                    lstFiles.SelectedItems?.Add(item);
                }
            }
        }
    }

    /// <summary>
    ///  Gets the selected Git statuses, including worktree lists that do not carry revisions.
    /// </summary>
    public IReadOnlyList<GitItemStatus> SelectedGitItems
    {
        get => _isFileTreeMode
            ? [.. tvFiles.SelectedItems?.OfType<FileTreeNode>().Select(node => node.Item?.Item).OfType<GitItemStatus>() ?? []]
            : _showDiffGroups
                ? [.. tvDiffFiles.SelectedItems?.OfType<DiffTreeNode>().Select(node => node.Item?.Item).OfType<GitItemStatus>() ?? []]
                : [.. lstFiles.SelectedItems?.Cast<object>().Select(GetGitItemStatus).OfType<GitItemStatus>() ?? []];
        set
        {
            if (value is null)
            {
                ClearSelected();
                return;
            }

            HashSet<GitItemStatus> selected = [.. value];
            ClearSelected();
            if (_showDiffGroups)
            {
                foreach (DiffTreeNode node in tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(Flatten).Where(node => node.Item is not null && selected.Contains(node.Item.Item)))
                {
                    tvDiffFiles.SelectedItems?.Add(node);
                }
            }
            else if (_isFileTreeMode)
            {
                foreach (FileTreeNode node in tvFiles.Items.Cast<FileTreeNode>().SelectMany(Flatten).Where(node => node.Item is not null && selected.Contains(node.Item.Item)))
                {
                    tvFiles.SelectedItems?.Add(node);
                }
            }
            else
            {
                foreach (object item in lstFiles.Items.Cast<object>().Where(item => GetGitItemStatus(item) is GitItemStatus status && selected.Contains(status)))
                {
                    lstFiles.SelectedItems?.Add(item);
                }
            }
        }
    }

    /// <summary>
    ///  Gets the <see cref="RelativePath"/> of a single selected folder node.
    /// </summary>
    public RelativePath? SelectedFolder
        => _isFileTreeMode && tvFiles.SelectedItem is FileTreeNode { IsFolder: true } node
            ? RelativePath.From(node.FullPath)
            : _showDiffGroups && tvDiffFiles.SelectedItem is DiffTreeNode { FolderPath: not null } diffNode
                ? diffNode.FolderPath
                : null;

    /// <summary>
    ///  Gets the selected file or folder path.
    /// </summary>
    public RelativePath? SelectedRelativePath
        => _isFileTreeMode && tvFiles.SelectedItem is FileTreeNode node
            ? RelativePath.From(node.FullPath)
            : _showDiffGroups && tvDiffFiles.SelectedItem is DiffTreeNode diffNode
                ? diffNode.Item is not null
                    ? RelativePath.From(diffNode.Item.Item.Name)
                    : diffNode.FolderPath
            : SelectedItem is FileStatusItem item
                ? RelativePath.From(item.Item.Name)
                : null;

    private string? SelectedItemAbsolutePath => _fullPathResolver.Resolve(SelectedItem?.Item.Name)?.NormalizePath();

    /// <summary>
    ///  Gets the displayed file statuses (named like the WinForms property).
    /// </summary>
    public IReadOnlyList<GitItemStatus> GitItemStatuses => _gitItemStatuses;

    private IReadOnlyList<FileStatusWithDescription> GitItemStatusesWithDescription { get; set; } = [];

    public Func<ObjectId, string>? DescribeRevision { get; set; }

    // Properties

    public IEnumerable<FileStatusItem> AllItems
        => _isFileTreeMode
            ? _allTreeItems
            : _allListItems.Select(GetFileStatusItem).OfType<FileStatusItem>();

    public int AllItemsCount => AllItems.Count();

    public IEnumerable<FileStatusItem> FirstGroupItems
        => _showDiffGroups && tvDiffFiles.Items.Cast<DiffTreeNode>().FirstOrDefault() is DiffTreeNode first
            ? first.Children.SelectMany(Flatten).Select(node => node.Item).OfType<FileStatusItem>()
            : AllItems;

    public bool FindInCommitFilesGitGrepActive => !string.IsNullOrEmpty(cboFindInCommitFilesGitGrep.Text);

    public bool FindInCommitFilesGitGrepFocused => cboFindInCommitFilesGitGrep.IsKeyboardFocusWithin;

    public bool FindInCommitFilesGitGrepVisible => FindInCommitFilesGitGrepPanel.IsVisible;

    /// <summary>
    ///  Indicates whether the git-grep search functionality is enabled for this control.
    /// </summary>
    [DefaultValue(false)]
    public bool CanUseFindInCommitFilesGitGrep { get; set; }

    private void SetFindInCommitFilesGitGrepVisibility(bool visible)
    {
        if (!CanUseFindInCommitFilesGitGrep || FindInCommitFilesGitGrepPanel.IsVisible == visible)
        {
            return;
        }

        SetFindInCommitFilesGitGrepVisibilityImpl(visible);
    }

    private void SetFindInCommitFilesGitGrepVisibilityImpl(bool visible)
    {
        _formFindInCommitFilesGitGrep?.SetShowFindInCommitFilesGitGrep(visible);
        FindInCommitFilesGitGrepPanel.IsVisible = visible;

        // Focus the box if it is becoming shown, but not on init of parent controls (which broke the form's focus management)
        if (visible)
        {
            cboFindInCommitFilesGitGrep.Focus();
        }
        else if (_formFindInCommitFilesGitGrep?.IsVisible is not true && !string.IsNullOrEmpty(cboFindInCommitFilesGitGrep.Text))
        {
            cboFindInCommitFilesGitGrep.Text = string.Empty;
            FindInCommitFilesGitGrep();
        }

        // Avalonia adjusts sizes automatically when visibility changes.
    }

    public bool SelectFirstItemOnSetItems { get; set; } = true;

    [Description("Disable showing open submodule menu items as bold")]
    [DefaultValue(false)]
    public bool DisableSubmoduleMenuItemBold { get; set; }

    /// <summary>
    ///  Gets or sets the selected Git item (named like the WinForms property).
    /// </summary>
    public GitItemStatus? SelectedGitItem
    {
        get => SelectedItem?.Item;
        set
        {
            if (value is null)
            {
                ClearSelected();
                return;
            }

            SelectFileOrFolder(RelativePath.From(value.Name));
        }
    }

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

    public bool Focused
        => (_isFileTreeMode ? (Control)tvFiles : _showDiffGroups ? tvDiffFiles : lstFiles).IsKeyboardFocusWithin;

    public FileStatusItem? FocusedItem
    {
        get
        {
            Control? focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
            object? dataContext = focused?.DataContext
                                  ?? focused?.GetVisualAncestors().OfType<Control>().Select(control => control.DataContext).FirstOrDefault(data => data is not null);
            return dataContext switch
            {
                FileStatusItem item => item,
                DiffTreeNode node => node.Item,
                FileTreeNode node => node.Item,
                _ => SelectedItem
            };
        }
    }

    /// <summary>
    ///  Gets or sets the selection mode of the underlying list.
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => lstFiles.SelectionMode;
        set
        {
            lstFiles.SelectionMode = value;
            tvDiffFiles.SelectionMode = value;
            tvFiles.SelectionMode = value;
        }
    }

    /// <summary>
    ///  Gets or sets whether revision descriptions are represented as parent nodes.
    /// </summary>
    public bool GroupByRevision { get; set; }

    /// <summary>
    ///  Shows the given diff items.
    /// </summary>
    public void SetDiffs(IReadOnlyList<GitItemStatus> items)
    {
        SetFileTreeMode(false);
        GitItemStatusesWithDescription = [];
        _showDiffGroups = false;
        _gitItemStatuses = items;
        _allListItems = [.. items.Cast<object>()];
        _allTreeItems = [];
        ApplyFilter(selectFirstItem: SelectFirstItemOnSetItems);
    }

    /// <summary>
    ///  Calculates and shows files changed by the given revisions.
    /// </summary>
    public void SetDiffs(IReadOnlyList<GitRevision> revisions)
    {
        FileStatusListLoading();
        _enableDisablingShowDiffForAllParents = true;
        _diffCalculator.SetDiff(revisions, headId: default, allowMultiDiff: false);
        IReadOnlyList<FileStatusWithDescription> groups = _diffCalculator.Calculate(
            prevList: [],
            refreshDiff: true,
            refreshGrep: false,
            CancellationToken.None);
        SetDiffs(groups, isFileTreeMode: false);
    }

    public async Task SetDiffsAsync(IReadOnlyList<GitRevision> revisions, ObjectId headId, CancellationToken cancellationToken)
    {
        FileStatusListLoading();
        UpdateToolbar(revisions);
        bool isFileTreeMode = _isFileTreeMode;
        _enableDisablingShowDiffForAllParents = !isFileTreeMode;
        bool showSkipWorktreeFiles = tsmiShowSkipWorktreeFiles.IsChecked == true;
        bool showUntrackedFiles = tsmiShowUntrackedFiles.IsChecked == true;
        IReadOnlyList<FileStatusWithDescription> groups = await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            _diffCalculator.SetDiff(
                revisions,
                headId,
                allowMultiDiff: !isFileTreeMode,
                showSkipWorktreeFiles,
                showUntrackedFiles);
            return _diffCalculator.Calculate(
                prevList: [],
                refreshDiff: true,
                refreshGrep: false,
                cancellationToken);
        }, cancellationToken);
        bool withGitGrep = FindInCommitFilesGitGrepActive;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SetDiffs(groups, isFileTreeMode);
        });

        // git grep, fetched as a separate step
        if (withGitGrep)
        {
            groups = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return _diffCalculator.Calculate(
                    prevList: groups,
                    refreshDiff: false,
                    refreshGrep: true,
                    cancellationToken);
            }, cancellationToken);
            await Dispatcher.UIThread.InvokeAsync(() => SetDiffs(groups, isFileTreeMode));
        }

        await Dispatcher.UIThread.InvokeAsync(() => LoadingFiles.IsVisible = false);
    }

    /// <summary>
    ///  Shows files changed between two revisions.
    /// </summary>
    public void SetDiffs(GitRevision? firstRev, GitRevision secondRev, IReadOnlyList<GitItemStatus> items)
    {
        FileStatusListLoading();
        List<FileStatusEntry> entries = items
            .Select(item => new FileStatusEntry(new FileStatusItem(firstRev, secondRev, item)))
            .ToList();
        SetRevisionEntries(entries);
    }

    private string? GetDescriptionForRevision(ObjectId objectId)
        => DescribeRevision is not null ? DescribeRevision(objectId)
            : objectId == ObjectId.WorkTreeId ? ResourceManager.TranslatedStrings.Workspace
            : objectId == ObjectId.IndexId ? ResourceManager.TranslatedStrings.Index
            : objectId.ToShortString();

    /// <summary>
    ///  Shows calculated revision groups as either the ordinary diff list or a repository tree.
    /// </summary>
    public void SetDiffs(IReadOnlyList<FileStatusWithDescription> groups, bool isFileTreeMode)
    {
        SetFileTreeMode(isFileTreeMode);
        GitItemStatusesWithDescription = isFileTreeMode ? [] : groups;
        _showDiffGroups = !isFileTreeMode
                          && (groups.Count > 1
                              || (GroupByRevision && !(groups.Count == 1 && groups[0].Statuses.Count == 0)));
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
            foreach (GitItemStatus status in group.Statuses)
            {
                entries.Add(new FileStatusEntry(
                    new FileStatusItem(group.FirstRev, group.SecondRev, status, group.BaseA, group.BaseB)));
            }
        }

        _gitItemStatuses = entries.Select(entry => entry.Item.Item).ToList();
        _allListItems = [.. entries.Cast<object>()];
        _allTreeItems = [];
        ApplyFilter(selectFirstItem: SelectFirstItemOnSetItems);
    }

    /// <summary>
    /// FormStash init for WorkTree and Index.
    /// </summary>
    /// <param name="headRev">The GitRevision for HEAD.</param>
    /// <param name="indexRev">The GitRevision for Index.</param>
    /// <param name="indexDesc">The description for Index.</param>
    /// <param name="indexItems">The GitItems for Index.</param>
    /// <param name="workTreeRev">The GitRevision for WorkTree.</param>
    /// <param name="workTreeDesc">The description for WorkTree.</param>
    /// <param name="workTreeItems">The GitItems for WorkTree.</param>
    public void SetStashDiffs(
        GitRevision headRev,
        GitRevision indexRev,
        string indexDesc,
        IReadOnlyList<GitItemStatus> indexItems,
        GitRevision workTreeRev,
        string workTreeDesc,
        IReadOnlyList<GitItemStatus> workTreeItems)
    {
        FileStatusListLoading();
        SetDiffs(
        [
            new FileStatusWithDescription(indexRev, workTreeRev, workTreeDesc, workTreeItems),
            new FileStatusWithDescription(headRev, indexRev, indexDesc, indexItems),
        ],
        isFileTreeMode: false);
    }

    /// <summary>
    ///  Clears all diff entries.
    /// </summary>
    public void ClearDiffs() => Clear();

    public void SetNoFilesText(string text)
        => NoFiles.Text = text;

    public void Clear()
    {
        _allListItems = [];
        GitItemStatusesWithDescription = [];
        _allTreeItems = [];
        _gitItemFilteredStatuses = [];
        _gitItemStatuses = [];
        _showDiffGroups = false;
        lstFiles.ItemsSource = null;
        tvDiffFiles.ItemsSource = null;
        tvFiles.ItemsSource = null;
        UpdateEmptyState();
    }

    /// <summary>
    ///  Focuses the active file list or repository tree.
    /// </summary>
    // Avalonia's focus overload has parameters, so the parameterless WinForms-shaped method is a distinct member.
    public void Focus()
    {
        if (FocusedItem is null)
        {
            SelectFirstVisibleItem();
        }

        Control target = _isFileTreeMode ? tvFiles : _showDiffGroups ? tvDiffFiles : lstFiles;
        if (!target.Focus())
        {
            Dispatcher.UIThread.Post(() => target.Focus());
        }
    }

    // Public methods

    public void ClearSelected()
    {
        lstFiles.Selection.Clear();
        tvDiffFiles.SelectedItems?.Clear();
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
        else if (_showDiffGroups)
        {
            tvDiffFiles.SelectedItems?.Clear();
            foreach (DiffTreeNode node in tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(Flatten).Where(node => node.Item is not null))
            {
                tvDiffFiles.SelectedItems?.Add(node);
            }
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
    ///  Selects the tree node matching the passed relative path.
    /// </summary>
    /// <param name="relativePath">The relative POSIX path to the item or folder.</param>
    /// <returns><c>true</c> if a matching tree node was found.</returns>
    public bool SelectFileOrFolder(RelativePath relativePath, bool firstGroupOnly = false, bool notify = true)
    {
        if (_showDiffGroups)
        {
            IEnumerable<DiffTreeNode> roots = firstGroupOnly
                ? tvDiffFiles.Items.Cast<DiffTreeNode>().Take(1)
                : tvDiffFiles.Items.Cast<DiffTreeNode>();
            DiffTreeNode? diffNode = roots
                .SelectMany(Flatten)
                .FirstOrDefault(candidate => candidate.Item?.Item.Name == relativePath.Value
                                             || candidate.FolderPath?.Value == relativePath.Value);
            if (diffNode is null)
            {
                return false;
            }

            _suppressSelectionChanged = !notify;
            ExpandAncestors(diffNode);
            tvDiffFiles.SelectedItem = diffNode;
            _suppressSelectionChanged = false;
            tvDiffFiles.ScrollIntoView(diffNode);
            return true;
        }

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
        if (_showDiffGroups)
        {
            // Skip collapsed or empty groups
            DiffTreeNode? first = tvDiffFiles.Items.Cast<DiffTreeNode>()
                .SelectMany(FlattenVisible)
                .FirstOrDefault(node => node.Item is not null);
            if (first is not null)
            {
                tvDiffFiles.SelectedItem = first;
            }

            return;
        }

        if (_isFileTreeMode)
        {
            // Descend
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
        if (_showDiffGroups)
        {
            SelectAdjacentDiffFile(-1);
            return;
        }

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
        if (_showDiffGroups)
        {
            SelectAdjacentDiffFile(1);
            return;
        }

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

    public void Bind(
        Action refreshArtificial,
        bool canAutoRefresh = false,
        Func<ObjectId, string>? describeRevision = null,
        Func<GitRevision, GitRevision>? getActualRevision = null,
        bool isFileTreeMode = false)
    {
        Bind(refreshArtificial);
        btnRefresh.IsVisible = true;
        tsmiRefreshOnFormFocus.IsVisible = canAutoRefresh;
        sepToolbar.IsVisible = canAutoRefresh;
        DescribeRevision = describeRevision;
        _diffCalculator.DescribeRevision = describeRevision;
        _diffCalculator.GetActualRevision = getActualRevision;
        SetFileTreeMode(isFileTreeMode);
    }

    public FileStatusItem? SelectNextItem(bool backwards, bool loop, bool notify = true)
    {
        FileStatusItem[] items = [.. GetVisibleFileStatusItems().Where(item => !item.Item.IsStatusOnly && !item.Item.IsRangeDiff)];
        if (items.Length == 0)
        {
            return null;
        }

        int currentIndex = Array.FindIndex(items, item => ReferenceEquals(item.Item, SelectedFileStatusItem?.Item));
        int nextIndex = currentIndex + (backwards ? -1 : 1);
        if (loop)
        {
            nextIndex = (nextIndex + items.Length) % items.Length;
        }
        else
        {
            nextIndex = Math.Clamp(nextIndex, 0, items.Length - 1);
        }

        SelectFileStatusItem(items[nextIndex], notify);
        return SelectedFileStatusItem;
    }

    public void StoreNextItemToSelect()
    {
        GitItemStatus[] visible = [.. GetVisibleFileStatusItems().Select(item => item.Item)];
        int currentIndex = Array.IndexOf(visible, SelectedItem?.Item);
        _nextItemToSelect = currentIndex >= 0 && currentIndex + 1 < visible.Length
            ? visible[currentIndex + 1]
            : currentIndex > 0 ? visible[currentIndex - 1] : null;
    }

    public void SelectStoredNextItem(bool orSelectFirst = false)
    {
        SelectedGitItem = _nextItemToSelect;
        _nextItemToSelect = null;
        if (orSelectFirst && SelectedItem is null)
        {
            SelectFirstVisibleItem();
        }
    }

    public int SetSelectionFilter(string selectionFilter)
    {
        GitItemStatus[] selectedItems =
        [
            .. _gitItemFilteredStatuses.Where(item => string.IsNullOrEmpty(selectionFilter)
                || Regex.IsMatch(item.Name, selectionFilter, RegexOptions.IgnoreCase)),
        ];
        SelectedGitItems = selectedItems;
        return selectedItems.Length;
    }

    /// <summary>
    /// Open the currently selected submodule (no checks done) in a new Browse instance
    /// If the submodule is a diff, both first and currently selected commits are initially selected.
    /// </summary>
    /// <returns>async Task.</returns>
    public async Task OpenSubmoduleAsync()
    {
        FileStatusItem selected = SelectedFileStatusItem
            ?? throw new InvalidOperationException("A submodule must be selected.");
        string submoduleName = selected.Item.Name;
        GitSubmoduleStatus? status = await selected.Item.GetSubmoduleStatusAsync().ConfigureAwait(false);
        ObjectId selectedId = selected.SecondRevision.ObjectId == ObjectId.WorkTreeId
            ? ObjectId.WorkTreeId
            : status?.Commit ?? default;
        ObjectId firstId = status?.OldCommit ?? default;
        string path = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator()) ?? string.Empty;
        if (!Directory.Exists(path))
        {
            MessageBoxes.SubmoduleDirectoryDoesNotExist(GetOwner(), path, submoduleName);
            return;
        }

        GitUICommands.LaunchBrowse(path, selectedId, firstId);
    }

    private IEnumerable<FileStatusItem> GetVisibleFileStatusItems()
        => _isFileTreeMode
            ? tvFiles.Items.Cast<FileTreeNode>().SelectMany(Flatten).Select(node => node.Item).OfType<FileStatusItem>()
            : _showDiffGroups
                ? tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(Flatten).Select(node => node.Item).OfType<FileStatusItem>()
                : lstFiles.Items.Cast<object>().Select(GetFileStatusItem).OfType<FileStatusItem>();

    private void SetRevisionEntries(IReadOnlyList<FileStatusEntry> entries)
    {
        SetFileTreeMode(false);
        GitItemStatusesWithDescription = [];
        _showDiffGroups = false;
        _gitItemStatuses = entries.Select(entry => entry.Item.Item).ToList();
        _allListItems = [.. entries.Cast<object>()];
        _allTreeItems = [];
        ApplyFilter(selectFirstItem: SelectFirstItemOnSetItems);
    }

    private void SetTreeEntries(IReadOnlyList<FileStatusItem> items)
    {
        GitItemStatusesWithDescription = [];
        _showDiffGroups = false;
        _gitItemStatuses = items.Select(item => item.Item).ToList();
        _allListItems = [];
        _allTreeItems = items;
        ApplyFilter(selectFirstItem: false);
    }

    private void SetFileTreeMode(bool isFileTreeMode)
    {
        _isFileTreeMode = isFileTreeMode;
        Toolbar.IsVisible = !isFileTreeMode;
        lblSplitter.Height = isFileTreeMode ? 1 : 0;
        lstFiles.IsVisible = !isFileTreeMode;
        tvDiffFiles.IsVisible = false;
        tvFiles.IsVisible = isFileTreeMode;
        if (isFileTreeMode)
        {
            SetFindInCommitFilesGitGrepVisibilityImpl(AppSettings.ShowFindInCommitFilesGitGrep.Value);
            GroupByRevision = false;
        }
    }

    private void RaiseSelectedIndexChanged()
    {
        if (!_suppressSelectionChanged)
        {
            _selectedIndexChangeTimer.Stop();
            _selectedIndexChangeTimer.Start();
        }
    }

    private void SelectedIndexChangeTimer_Tick(object? sender, EventArgs e)
    {
        _selectedIndexChangeTimer.Stop();
        FileStatusListView_SelectedIndexChanged();
    }

    private void FileStatusListView_SelectedIndexChanged()
    {
        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FileStatusList_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _mouseEntered = !IsKeyboardFocusWithin;
    }

    private void FileStatusList_Enter(object? sender, EventArgs e)
    {
        if (_hasKeyboardFocus)
        {
            return;
        }

        _hasKeyboardFocus = true;
        Enter?.Invoke(this, new EnterEventArgs(_mouseEntered));
        _mouseEntered = false;
    }

    private void FileStatusList_LostFocus(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() => _hasKeyboardFocus = IsKeyboardFocusWithin);
    }

    private void FilterComboBox_TextUpdate(object? sender, EventArgs e)
    {
        // show DeleteFilterButton at once
        SetDeleteFilterButtonVisibility();

        string filterText = cboFilterComboBox.Text ?? string.Empty;
        string workingDir = TryGetUICommandsDirect(out IGitUICommands? commands) ? commands.Module.WorkingDir : string.Empty;
        if (workingDir.Length > 0 && filterText.Length > workingDir.Length)
        {
            string posixWorkingDir = PathUtil.ToPosixPath(workingDir);
            string posixFilterText = PathUtil.ToPosixPath(filterText);
            if (posixFilterText.StartsWith(posixWorkingDir, StringComparison.InvariantCultureIgnoreCase))
            {
                filterText = posixFilterText.SubstringAfter(posixWorkingDir, StringComparison.InvariantCultureIgnoreCase);
                cboFilterComboBox.Text = filterText;
            }
        }

        // Avalonia's editable ComboBox does not auto-select a matching history prefix, so no selection workaround is required.
        _filterTimer.Stop();
        _filterTimer.Start();
    }

    private void FilterComboBox_MouseEnter(object? sender, EventArgs e)
    {
        // Avalonia's ToolTip service retains the current validation text without assigning it again on pointer entry.
    }

    private void FilterComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        FilterFiles(cboFilterComboBox.Text ?? string.Empty);
    }

    private void FilterComboBox_SizeChanged(object? sender, EventArgs e)
    {
        // Avalonia redraws resized controls automatically; invalidation preserves the original explicit refresh boundary.
        cboFilterComboBox.InvalidateVisual();
    }

    private void SetDeleteFilterButtonVisibility()
    {
        DeleteFilterButton.IsVisible = IsFilterActive;
    }

    private void DeleteFilterButton_Click(object? sender, EventArgs e)
    {
        SetFilter(string.Empty);
        cboFilterComboBox.Focus();
    }

    private void cboFindInCommitFilesGitGrep_TextUpdate(object? sender, EventArgs e)
        => FindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Text ?? string.Empty, delay: 200);

    private void cboFindInCommitFilesGitGrep_SelectedIndexChanged(object? sender, EventArgs e)
    {
        FindInCommitFilesGitGrep();
    }

    private void cboFindInCommitFilesGitGrep_SizeChanged(object? sender, EventArgs e)
    {
        cboFindInCommitFilesGitGrep.InvalidateVisual();
    }

    private void FindInCommitFilesGitGrep()
    {
        FindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Text ?? string.Empty);
    }

    private void FindInCommitFilesGitGrep(string search, int delay = 0)
    {
        DeleteSearchButton.IsVisible = !string.IsNullOrEmpty(search);

        CancellationToken cancellationToken = _reloadSequence.Next();
        this.InvokeAndForget(async () =>
        {
            // delay to handle keypresses
            await Task.Delay(delay, cancellationToken);
            string searchArg = search;
            if (!string.IsNullOrWhiteSpace(searchArg) && !GrepStringRegex.IsMatch(searchArg))
            {
                searchArg = searchArg.Replace(@"\\", @"\\\\");
                searchArg = $@"-e ""{searchArg}""";
            }

            bool isFileTreeMode = _isFileTreeMode;
            IReadOnlyList<FileStatusWithDescription> previous = GitItemStatusesWithDescription;
            IReadOnlyList<FileStatusWithDescription> groups = await Task.Run(() =>
            {
                _diffCalculator.SetGrep(searchArg, fileTreeMode: isFileTreeMode && string.IsNullOrWhiteSpace(searchArg));
                return _diffCalculator.Calculate(
                    prevList: previous,
                    refreshDiff: false,
                    refreshGrep: true,
                    cancellationToken);
            }, cancellationToken);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SetDiffs(groups, isFileTreeMode);
                if (!string.IsNullOrEmpty(search))
                {
                    AddToSearchFilter(search);
                }
            });
        });
    }

    private void AddToSearchFilter(string search)
    {
        int index = _gitGrepSearchItems.FindIndex(item => string.Equals(item?.ToString(), search, StringComparison.Ordinal));
        if (index == 0)
        {
            return;
        }

        if (index > 0)
        {
            _gitGrepSearchItems.RemoveAt(index);
        }

        _gitGrepSearchItems.Insert(0, search);
        const int SearchFilterMaxLength = 30;
        if (_gitGrepSearchItems.Count > SearchFilterMaxLength)
        {
            _gitGrepSearchItems.RemoveRange(SearchFilterMaxLength, _gitGrepSearchItems.Count - SearchFilterMaxLength);
        }

        if (_formFindInCommitFilesGitGrep?.IsVisible is true)
        {
            _formFindInCommitFilesGitGrep.SetSearchItems(_gitGrepSearchItems);
            if (FindInCommitFilesGitGrepVisible)
            {
                _formFindInCommitFilesGitGrep.GitGrepExpressionText = search;
            }
        }
    }

    private void DeleteSearchButton_Click(object? sender, EventArgs e)
    {
        cboFindInCommitFilesGitGrep.Text = string.Empty;
        FindInCommitFilesGitGrep();
    }

    public void ShowFindInCommitFileGitGrepDialog(string text)
    {
        if (!CanUseFindInCommitFilesGitGrep
            || !TryGetUICommandsDirect(out IGitUICommands? commands)
            || TopLevel.GetTopLevel(this) is not Window owner)
        {
            return;
        }

        if (_formFindInCommitFilesGitGrep is null)
        {
            _formFindInCommitFilesGitGrep = new FormFindInCommitFilesGitGrep(commands)
            {
                FilesGitGrepLocator = value =>
                {
                    FindInCommitFilesGitGrep(value);
                    cboFindInCommitFilesGitGrep.Text = value;
                },
                FindInCommitFilesGitGrepToggle = SetFindInCommitFilesGitGrepVisibility,
            };
            _formFindInCommitFilesGitGrep.Closed += (_, _) => _formFindInCommitFilesGitGrep = null;
        }

        _formFindInCommitFilesGitGrep.GitGrepExpressionText = !string.IsNullOrEmpty(text)
            ? text
            : FindInCommitFilesGitGrepActive ? cboFindInCommitFilesGitGrep.Text : null;
        _formFindInCommitFilesGitGrep.SetSearchItems(_gitGrepSearchItems);
        _formFindInCommitFilesGitGrep.SetShowFindInCommitFilesGitGrep(FindInCommitFilesGitGrepVisible);
        if (!_formFindInCommitFilesGitGrep.IsVisible)
        {
            _formFindInCommitFilesGitGrep.Show(owner);
        }

        _formFindInCommitFilesGitGrep.Activate();
    }

    private void FilterTimer_Tick(object? sender, EventArgs e)
    {
        _filterTimer.Stop();
        int fileCount = FilterFiles(cboFilterComboBox.Text ?? string.Empty);
        string filterText = cboFilterComboBox.Text ?? string.Empty;
        if (fileCount > 0 && filterText.Length > 0 && !cboFilterComboBox.Classes.Contains("file-filter-invalid"))
        {
            AddToSelectionFilter(filterText);
        }
    }

    private int FilterFiles(string value)
    {
        if (!string.Equals(cboFilterComboBox.Text, value, StringComparison.Ordinal))
        {
            cboFilterComboBox.Text = value;
        }

        // Feed back the current list of files
        return ApplyFilter(selectFirstItem: false);
    }

    private void AddToSelectionFilter(string filter)
    {
        if (_filterHistory.Contains(filter, StringComparer.Ordinal))
        {
            return;
        }

        const int SelectionFilterMaxLength = 10;
        if (_filterHistory.Count == SelectionFilterMaxLength)
        {
            _filterHistory.RemoveAt(SelectionFilterMaxLength - 1);
        }

        _filterHistory.Insert(0, filter);
        cboFilterComboBox.ItemsSource = _filterHistory;
    }

    private int ApplyFilter(bool selectFirstItem)
    {
        _filterTimer.Stop();
        FileStatusItem? selectedFileStatus = SelectedFileStatusItem;
        RelativePath? selectedPath = SelectedRelativePath;
        string filterText = cboFilterComboBox.Text ?? string.Empty;
        try
        {
            StoreFilter(filterText);
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
        else if (GitItemStatusesWithDescription.Count > 0)
        {
            Dictionary<int, bool> expansion = tvDiffFiles.Items
                .Cast<DiffTreeNode>()
                .Where(node => node.GroupIndex is not null)
                .ToDictionary(node => node.GroupIndex!.Value, node => node.IsExpanded);
            IReadOnlyList<DiffTreeNode> roots = BuildDiffTree(GitItemStatusesWithDescription, expansion);
            _showDiffGroups = GitItemStatusesWithDescription.Count > 1
                              || (GroupByRevision
                              && !(GitItemStatusesWithDescription.Count == 1 && GitItemStatusesWithDescription[0].Statuses.Count == 0));
            _gitItemFilteredStatuses =
            [
                .. roots.SelectMany(Flatten)
                    .Select(node => node.Item?.Item)
                    .OfType<GitItemStatus>(),
            ];

            if (_showDiffGroups)
            {
                tvDiffFiles.ItemsSource = roots;
                lstFiles.ItemsSource = null;
            }
            else
            {
                List<object> visibleItems =
                [
                    .. roots.SelectMany(Flatten)
                        .Where(node => node.Item is not null)
                        .Select(node => (object)new FileStatusEntry(node.Item!)),
                ];
                _gitItemFilteredStatuses = [.. visibleItems.Select(GetGitItemStatus).OfType<GitItemStatus>()];
                lstFiles.ItemsSource = visibleItems;
                tvDiffFiles.ItemsSource = null;
            }
        }
        else
        {
            List<object> visibleItems =
            [
                .. _allListItems.Where(item => GetGitItemStatus(item) is GitItemStatus status && IsFilterMatch(status)),
            ];

            _gitItemFilteredStatuses = [.. visibleItems.Select(GetGitItemStatus).OfType<GitItemStatus>()];
            lstFiles.ItemsSource = visibleItems;
        }

        UpdateEmptyState();
        bool selectionRestored = selectedFileStatus is not null && SelectFileStatusItem(selectedFileStatus, notify: false);
        selectionRestored |= !selectionRestored
                             && selectedPath is not null
                             && SelectFileOrFolder(selectedPath, notify: false);
        if (!selectionRestored && selectFirstItem)
        {
            SelectFirstVisibleItem();
        }

        DataSourceChanged?.Invoke(this, EventArgs.Empty);
        FilterChanged?.Invoke(this, EventArgs.Empty);
        return _gitItemFilteredStatuses.Count;
    }

    private void StoreFilter(string value)
    {
        SetDeleteFilterButtonVisibility();
        _filter = value.Length == 0
            ? null
            : new Regex(
                value,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                RegexTimeout);
        SetFilterState(value.Length == 0 ? FilterState.Empty : FilterState.Valid, toolTip: null);
    }

    private bool SelectFileStatusItem(FileStatusItem item, bool notify)
    {
        if (_showDiffGroups)
        {
            DiffTreeNode? node = tvDiffFiles.Items.Cast<DiffTreeNode>()
                .SelectMany(Flatten)
                .FirstOrDefault(candidate => ReferenceEquals(candidate.Item?.Item, item.Item));
            if (node is null)
            {
                return false;
            }

            _suppressSelectionChanged = !notify;
            ExpandAncestors(node);
            tvDiffFiles.SelectedItem = node;
            _suppressSelectionChanged = false;
            tvDiffFiles.ScrollIntoView(node);
            return true;
        }

        if (_isFileTreeMode)
        {
            FileTreeNode? node = tvFiles.Items.Cast<FileTreeNode>()
                .SelectMany(Flatten)
                .FirstOrDefault(candidate => ReferenceEquals(candidate.Item?.Item, item.Item));
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

        object? listItem = lstFiles.Items.Cast<object>()
            .FirstOrDefault(candidate => ReferenceEquals(GetFileStatusItem(candidate)?.Item, item.Item));
        if (listItem is null)
        {
            return false;
        }

        _suppressSelectionChanged = !notify;
        lstFiles.SelectedItem = listItem;
        _suppressSelectionChanged = false;
        lstFiles.ScrollIntoView(listItem);
        return true;
    }

    private bool IsFilterMatch(GitItemStatus item)
    {
        if (item.IsRangeDiff || _filter is null)
        {
            return true;
        }

        string name = item.Name.TrimEnd(PathUtil.PosixDirectorySeparatorChar);
        string? oldName = item.OldName;
        if (AppSettings.TruncatePathMethod == TruncatePathMethod.FileNameOnly)
        {
            name = Path.GetFileName(name);
            oldName = Path.GetFileName(oldName);
        }

        try
        {
            return _filter.IsMatch(name)
                   || (oldName is not null && _filter.IsMatch(oldName));
        }
        catch (RegexMatchTimeoutException exception)
        {
            SetFilterState(FilterState.Invalid, exception.Message);
            return false;
        }
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
        LoadingFiles.IsVisible = false;
        bool hasItems = _gitItemFilteredStatuses.Count > 0;
        bool hasGroupRows = _showDiffGroups && tvDiffFiles.ItemCount > 0;
        NoFiles.IsVisible = !hasItems && !hasGroupRows;
        lstFiles.IsVisible = hasItems && !_isFileTreeMode && !_showDiffGroups;
        tvDiffFiles.IsVisible = (hasItems || hasGroupRows) && !_isFileTreeMode && _showDiffGroups;
        tvFiles.IsVisible = hasItems && _isFileTreeMode;
    }

    private void FileStatusListLoading()
    {
        // Show "Files loading" below the filterbox

        NoFiles.IsVisible = false;
        LoadingFiles.IsVisible = true;
        ClearSelected();
        lstFiles.ItemsSource = null;
        tvDiffFiles.ItemsSource = null;
        tvFiles.ItemsSource = null;
    }

    private void DiffListSortingChanged(object? sender, EventArgs e)
    {
        if (GitItemStatusesWithDescription.Count == 0 || _isFileTreeMode)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyFilter(selectFirstItem: false);
        }
        else
        {
            Dispatcher.UIThread.Post(() => ApplyFilter(selectFirstItem: false));
        }
    }

    private void DiffTreeSelectionChanged()
    {
        if (tvDiffFiles.SelectedItems is { } selectedItems)
        {
            foreach (DiffTreeNode header in selectedItems.OfType<DiffTreeNode>().Where(node => node.IsGroupHeader).ToArray())
            {
                selectedItems.Remove(header);
            }
        }

        RaiseSelectedIndexChanged();
    }

    private void DiffTreeContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container.DataContext is not DiffTreeNode node)
        {
            return;
        }

        e.Container.Classes.Set("diff-group-header", node.IsGroupHeader);
        e.Container.Focusable = !node.IsGroupHeader;
    }

    private void SelectAdjacentDiffFile(int offset)
    {
        DiffTreeNode[] files =
        [
            .. tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(FlattenVisible).Where(node => node.Item is not null),
        ];
        if (files.Length == 0)
        {
            return;
        }

        int selectedIndex = Array.IndexOf(files, tvDiffFiles.SelectedItem as DiffTreeNode);
        int nextIndex = selectedIndex < 0 ? 0 : Math.Clamp(selectedIndex + offset, 0, files.Length - 1);
        tvDiffFiles.SelectedItem = files[nextIndex];
        tvDiffFiles.ScrollIntoView(files[nextIndex]);
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

    private IReadOnlyList<DiffTreeNode> BuildDiffTree(
        IReadOnlyList<FileStatusWithDescription> groups,
        IReadOnlyDictionary<int, bool> previousExpansion)
    {
        bool showDiffGroups = groups.Count > 1
                              || (GroupByRevision && !(groups.Count == 1 && groups[0].Statuses.Count == 0));
        DiffListSortType sortType = DiffListSortService.Instance.DiffListSorting;
        bool hasGrepGroup = groups.Any(FileStatusDiffCalculator.IsGrepItemStatuses);
        List<DiffTreeNode> roots = [];
        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            FileStatusWithDescription group = groups[groupIndex];
            FileStatusItem[] visibleItems =
            [
                .. group.Statuses
                    .Where(item => IsFilterMatch(item) && IsDiffStatusMatch(item.DiffStatus))
                    .Select(item => new FileStatusItem(group.FirstRev, group.SecondRev, item, group.BaseA, group.BaseB)),
            ];

            if (group.Statuses.Count == 1 && group.Statuses[0].IsRangeDiff)
            {
                if (visibleItems.Length == 1)
                {
                    roots.Add(CreateDiffFileNode(visibleItems[0], parent: null));
                }

                continue;
            }

            bool defaultExpanded = hasGrepGroup
                ? FileStatusDiffCalculator.IsGrepItemStatuses(group) && visibleItems.Length < 100
                : ((group.Statuses.Count <= 7 && group.IconName == nameof(Images.Diff))
                   || groups.Count < 3
                   || groupIndex == 0)
                  && group.Statuses.Count > 0;
            bool expanded = previousExpansion.TryGetValue(groupIndex, out bool wasExpanded)
                ? wasExpanded
                : defaultExpanded;

            if (showDiffGroups)
            {
                string shownDisplay = visibleItems.Length >= group.Statuses.Count ? string.Empty : $"{visibleItems.Length}/";
                DiffTreeNode header = new(
                    $"({shownDisplay}{group.Statuses.Count}) {group.Summary}",
                    GetGroupImage(group.IconName),
                    item: null,
                    folderPath: null,
                    parent: null,
                    isGroupHeader: true,
                    groupIndex)
                {
                    IsExpanded = expanded,
                };
                header.Children.AddRange(BuildDiffPathTree(visibleItems, header, expanded, sortType));
                roots.Add(header);
            }
            else
            {
                roots.AddRange(BuildDiffPathTree(visibleItems, parent: null, expanded, sortType));
            }
        }

        return roots;
    }

    private static IReadOnlyList<DiffTreeNode> BuildDiffPathTree(
        IReadOnlyList<FileStatusItem> items,
        DiffTreeNode? parent,
        bool expanded,
        DiffListSortType sortType)
    {
        bool flat = sortType is DiffListSortType.FilePathFlat
            or DiffListSortType.FileExtensionFlat
            or DiffListSortType.FileStatusFlat;
        if (sortType is DiffListSortType.FilePath or DiffListSortType.FilePathFlat)
        {
            return BuildPathNodes(items, parent, expanded, flat);
        }

        IEnumerable<IGrouping<string, FileStatusItem>> groups = sortType switch
        {
            DiffListSortType.FileExtension or DiffListSortType.FileExtensionFlat
                => items.GroupBy(item => Path.GetExtension(item.Item.Name), StringComparer.OrdinalIgnoreCase)
                    .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase),
            DiffListSortType.FileStatus or DiffListSortType.FileStatusFlat
                => items.GroupBy(item => GetStatusGroupKey(item.Item))
                    .OrderBy(group => group.Key, StringComparer.Ordinal),
            _ => throw new NotSupportedException($"{sortType} is not a supported diff-list sorting mode."),
        };

        List<DiffTreeNode> roots = [];
        foreach (IGrouping<string, FileStatusItem> group in groups)
        {
            FileStatusItem[] groupItems = [.. group];
            IReadOnlyList<DiffTreeNode> children = BuildPathNodes(groupItems, parent: null, expanded, flat);
            bool showGroupNode = !flat || AppSettings.FileStatusShowGroupNodesInFlatList.Value;
            if (!showGroupNode || (children.Count == 1 && children[0].Children.Count == 0))
            {
                roots.AddRange(BuildPathNodes(groupItems, parent, expanded, flat));
                continue;
            }

            string text = sortType is DiffListSortType.FileExtension or DiffListSortType.FileExtensionFlat
                ? group.Key
                : string.Empty;
            DiffTreeNode groupNode = new(
                text,
                sortType is DiffListSortType.FileExtension or DiffListSortType.FileExtensionFlat
                    ? Images.File
                    : GetItemImage(groupItems[0].Item),
                item: null,
                folderPath: null,
                parent,
                isGroupHeader: true,
                groupIndex: null)
            {
                IsExpanded = expanded,
            };
            groupNode.Children.AddRange(BuildPathNodes(groupItems, groupNode, expanded, flat));
            roots.Add(groupNode);
        }

        return roots;

        static string GetStatusGroupKey(GitItemStatus status)
        {
            string primaryStatus = status switch
            {
                { IsDeleted: true } => "01-removed",
                { IsRangeDiff: true } => "02-range",
                { IsNew: true } or { IsTracked: false } => "03-added",
                { IsUnmerged: true } => "04-unmerged",
                { IsSubmodule: true, IsDirty: true } => "05-submodule-dirty",
                { IsSubmodule: true } => "06-submodule",
                { IsChanged: true } => "07-modified",
                { IsRenamed: true } => "08-renamed",
                { IsCopied: true } => "09-copied",
                _ => "10-unknown",
            };
            return $"{(char)((int)'Z' - (int)status.DiffStatus)}:{primaryStatus}";
        }
    }

    private static IReadOnlyList<DiffTreeNode> BuildPathNodes(
        IReadOnlyList<FileStatusItem> items,
        DiffTreeNode? parent,
        bool expanded,
        bool flat)
    {
        if (flat)
        {
            return
            [
                .. items.OrderBy(item => item.Item.Name, StringComparer.Ordinal)
                    .Select(item => CreateDiffFileNode(item, parent, showFullPath: true)),
            ];
        }

        List<DiffTreeNode> roots = [];
        foreach (FileStatusItem item in items)
        {
            string[] segments = item.Item.Name.Split(PathUtil.PosixDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            List<DiffTreeNode> siblings = roots;
            DiffTreeNode? currentParent = parent;
            string path = string.Empty;
            for (int index = 0; index < segments.Length; index++)
            {
                path = path.Length == 0 ? segments[index] : $"{path}/{segments[index]}";
                bool isFile = index == segments.Length - 1;
                DiffTreeNode? node = siblings.FirstOrDefault(
                    candidate => candidate.Text == segments[index] && candidate.Item is null);
                if (isFile || node is null)
                {
                    node = isFile
                        ? CreateDiffFileNode(item, currentParent, showFullPath: false)
                        : new DiffTreeNode(
                            segments[index],
                            Images.FolderClosed,
                            item: null,
                            RelativePath.From(path),
                            currentParent,
                            isGroupHeader: false,
                            groupIndex: null)
                        {
                            IsExpanded = expanded,
                        };
                    siblings.Add(node);
                }

                currentParent = node;
                siblings = node.Children;
            }
        }

        Sort(roots);
        return roots;

        static void Sort(List<DiffTreeNode> nodes)
        {
            nodes.Sort((left, right) => (left.IsFolder, right.IsFolder) switch
            {
                (true, false) => -1,
                (false, true) => 1,
                _ => string.Compare(left.Text, right.Text, StringComparison.Ordinal),
            });
            foreach (DiffTreeNode node in nodes)
            {
                Sort(node.Children);
            }
        }
    }

    private static DiffTreeNode CreateDiffFileNode(
        FileStatusItem item,
        DiffTreeNode? parent,
        bool showFullPath = true)
        => new(
            item.Item.OldName is null
                ? showFullPath ? item.Item.Name : item.Item.Name.Split('/')[^1]
                : $"{(showFullPath ? item.Item.Name : item.Item.Name.Split('/')[^1])} ({item.Item.OldName})",
            GetItemImage(item.Item),
            item,
            folderPath: null,
            parent,
            isGroupHeader: false,
            groupIndex: null);

    private static IImage GetGroupImage(string iconName)
        => iconName switch
        {
            nameof(Images.DiffA) => Images.DiffA,
            nameof(Images.DiffB) => Images.DiffB,
            nameof(Images.DiffC) => Images.DiffC,
            nameof(Images.DiffR) => Images.DiffR,
            nameof(FileStatusDiffCalculator.GitGrepIconName) => Images.ViewFile,
            _ => Images.Diff,
        };

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

    private static IEnumerable<DiffTreeNode> Flatten(DiffTreeNode node)
    {
        yield return node;
        foreach (DiffTreeNode child in node.Children.SelectMany(Flatten))
        {
            yield return child;
        }
    }

    private static IEnumerable<DiffTreeNode> FlattenVisible(DiffTreeNode node)
    {
        yield return node;
        if (!node.IsExpanded)
        {
            yield break;
        }

        foreach (DiffTreeNode child in node.Children.SelectMany(FlattenVisible))
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

    private static void ExpandAncestors(DiffTreeNode node)
    {
        for (DiffTreeNode? parent = node.Parent; parent is not null; parent = parent.Parent)
        {
            parent.IsExpanded = true;
        }
    }

    private Control CreateDiffTreeRow(DiffTreeNode node)
    {
        Image image = new()
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            Source = node.Image,
            Margin = new Avalonia.Thickness(1, 0, 3, 0),
        };
        if (node.Item is not null)
        {
            UpdateSubmoduleImageWhenReady(image, node.Item.Item);
            if (!node.Item.Item.IsSubmodule && !string.IsNullOrWhiteSpace(node.Item.Item.GrepString))
            {
                LoadFileIcons([(image, node.Item.Item.Name)], CancellationToken.None);
            }
        }

        return new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children =
            {
                image,
                new TextBlock
                {
                    Text = node.Text,
                    FontWeight = node.IsGroupHeader ? FontWeight.Bold : FontWeight.Normal,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                },
            },
        };
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
            if (!node.Item.Item.IsSubmodule && !string.IsNullOrWhiteSpace(node.Item.Item.GrepString))
            {
                LoadFileIcons([(image, node.Item.Item.Name)], CancellationToken.None);
            }
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
            Orientation = Avalonia.Layout.Orientation.Horizontal,
        };

        Image image = new()
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            Source = GetItemImage(gitItemStatus),
            Margin = new Avalonia.Thickness(3, 0, 3, 0),
        };
        UpdateSubmoduleImageWhenReady(image, gitItemStatus);
        if (!gitItemStatus.IsSubmodule && !string.IsNullOrWhiteSpace(gitItemStatus.GrepString))
        {
            LoadFileIcons([(image, gitItemStatus.Name)], CancellationToken.None);
        }

        row.Children.Add(image);
        row.Children.Add(
            new TextBlock
            {
                Text = gitItemStatus.OldName is null
                    ? gitItemStatus.Name
                    : $"{gitItemStatus.Name} ({gitItemStatus.OldName})",
                TextTrimming = TextTrimming.CharacterEllipsis,
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
                DiffBranchStatus.OnlyAChange => Images.FileStatusRenamedOnlyA.AdaptLightness(),
                DiffBranchStatus.OnlyBChange => Images.FileStatusRenamedOnlyB.AdaptLightness(),
                DiffBranchStatus.SameChange => Images.FileStatusRenamedSame.AdaptLightness(),
                DiffBranchStatus.UnequalChange => Images.FileStatusRenamedUnequal.AdaptLightness(),
                _ => Images.FileStatusRenamed.AdaptLightness(),
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

    private sealed record FileStatusEntry(FileStatusItem Item);

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(FileStatusList control)
    {
        internal WatermarkComboBox FilterComboBox => control.cboFilterComboBox;
        internal TextBlock NoFilesLabel => control.NoFiles;
        internal ListBox List => control.lstFiles;
        internal TreeView DiffTree => control.tvDiffFiles;
        internal TreeView Tree => control.tvFiles;
        internal ContextMenu ContextMenu => control.ItemContextMenu;
        internal StackPanel Toolbar => control.Toolbar;
        internal MenuItem StageMenuItem => control.tsmiStageFile;
        internal MenuItem UnstageMenuItem => control.tsmiUnstageFile;
        internal MenuItem CherryPickMenuItem => control.tsmiCherryPickChanges;
        internal MenuItem OpenWithDifftoolMenuItem => control.tsmiOpenWithDifftool;
        internal MenuItem OpenSubmoduleMenuItem => control._NO_TRANSLATE_openSubmoduleMenuItem;
        internal MenuItem CopyPathsMenuItem => control.tsmiCopyPaths;
        internal SortDiffListContextMenuItem SortByContextMenuItem => control._sortByContextMenu;
        internal Separator SortBySeparator => control._sortBySeparator;
        internal MenuItem ResetChunkMenuItem => control.tsmiResetChunkOfFile;
        internal MenuItem InteractiveAddMenuItem => control.tsmiInteractiveAdd;
        internal MenuItem ResetToMenuItem => control.tsmiResetFileTo;
        internal MenuItem ResetToSelectedMenuItem => control.tsmiResetFileToSelected;
        internal MenuItem ResetToParentMenuItem => control.tsmiResetFileToParent;
        internal MenuItem AddToGitIgnoreMenuItem => control.tsmiAddFileToGitIgnore;
        internal MenuItem AddToGitInfoExcludeMenuItem => control.tsmiAddFileToGitInfoExclude;
        internal MenuItem OpenInVisualStudioMenuItem => control.tsmiOpenInVisualStudio;
        internal MenuItem SelectAllMenuItem => control._selectAll;
        internal MenuItem CollapseAllMenuItem => control._collapseAll;
        internal MenuItem ExpandAllMenuItem => control._expandAll;
        internal MenuItem CollapseRootFoldersMenuItem => control._collapseRootFolders;
        internal MenuItem SkipWorktreeMenuItem => control.tsmiSkipWorktree;
        internal MenuItem AssumeUnchangedMenuItem => control.tsmiAssumeUnchanged;
        internal MenuItem StopTrackingMenuItem => control.tsmiStopTracking;
        internal ToggleButton ByPathButton => control.btnByPath;
        internal ToggleButton ByExtensionButton => control.btnByExtension;
        internal ToggleButton ByStatusButton => control.btnByStatus;
        internal Button RefreshButton => control.btnRefresh;
        internal MenuItem RefreshOnFormFocusMenuItem => control.tsmiRefreshOnFormFocus;
        internal Separator ToolbarSeparator => control.sepToolbar;
        internal IconSplitButton FindInFilesButton => control.btnFindInFilesGitGrep;
        internal MenuItem FindUsingMatchCaseMenuItem => control.tsmiFindUsingMatchCase;
        internal MenuItem FindUsingWholeWordMenuItem => control.tsmiFindUsingWholeWord;
        internal MenuItem FindUsingOptionsMenuItem => control.tsmiFindUsingOptions;
        internal MenuItem FindUsingBasicMenuItem => control._NO_TRANSLATE_tsmiFindUsingBasic;
        internal MenuItem FindUsingInputBoxMenuItem => control.tsmiFindUsingInputBox;
        internal MenuItem ShowDiffForAllParentsMenuItem => control.tsmiShowDiffForAllParents;
        internal MenuItem ToolbarMenuItem => control.tsmiToolbar;
        internal Grid FindInFilesPanel => control.FindInCommitFilesGitGrepPanel;
        internal Separator Splitter => control.lblSplitter;

        internal bool UpdateContextMenu()
        {
            CancelEventArgs eventArgs = new();
            control.ItemContextMenu_Opening(control.ItemContextMenu, eventArgs);
            return eventArgs.Cancel;
        }

        internal bool FirstTreeRootExpanded => ((FileTreeNode)control.tvFiles.Items[0]!).IsExpanded;

        internal void SelectFirstTreeRoot()
            => control.tvFiles.SelectedItem = control.tvFiles.Items[0];

        internal void SelectFirstTreeLeaf()
            => control.tvFiles.SelectedItem = control.tvFiles.Items.Cast<FileTreeNode>().SelectMany(Flatten).First(node => node.Item is not null);

        internal void SetSort(DiffListSortType sortType)
        {
            DiffListSortService.Instance.DiffListSorting = sortType;
            control.UpdateToolbar();
        }

        internal void UpdateToolbar()
            => control.UpdateToolbar();

        internal void OpenFindInFilesMenu()
            => control.FindInFilesGitGrep_DropDownOpening(control.btnFindInFilesGitGrep, EventArgs.Empty);

        internal void ToggleFindInFiles()
            => control.FindInFilesGitGrep_ButtonClick(control.btnFindInFilesGitGrep, EventArgs.Empty);

        internal void OpenSettingsMenu()
            => control.Settings_DropDownOpening(control.btnSettings, EventArgs.Empty);

        internal void EnableShowDiffForAllParents()
            => control._enableDisablingShowDiffForAllParents = true;

        internal void SetDiffStatusVisible(DiffBranchStatus status, bool visible)
        {
            ToggleButton button = status switch
            {
                DiffBranchStatus.OnlyAChange => control.btnOnlyA,
                DiffBranchStatus.OnlyBChange => control.btnOnlyB,
                DiffBranchStatus.SameChange => control.btnSameChange,
                DiffBranchStatus.UnequalChange => control.btnUnequalChange,
                _ => throw new ArgumentOutOfRangeException(nameof(status)),
            };
            button.IsChecked = visible;
            control.ApplyFilter(selectFirstItem: false);
        }

        internal static IImage GetItemImageForTesting(GitItemStatus item)
            => FileStatusList.GetItemImage(item);

        // parity-scaffolding: verifies the WinForms delayed-expansion contract against Avalonia's lazy containers.
        internal void RestoreDiffTreeChildrenForTesting(bool delayExpansion, Action? afterAction = null)
            => RestoreChildrenOfFolderNodes(control.tvDiffFiles.Items.Cast<DiffTreeNode>(), afterAction, delayExpansion);

        internal (int descendants, bool allExpanded) GetDiffTreeExpansionForTesting()
        {
            DiffTreeNode[] nodes = [.. control.tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(Flatten)];
            return (nodes.Length, nodes.All(node => node.IsExpanded));
        }

        internal static IImage GetSubmoduleImageForTesting(GitItemStatus item, GitSubmoduleStatus? status)
            => FileStatusList.GetSubmoduleImage(item, status);
    }

    private enum FilterState
    {
        Empty,
        Valid,
        Invalid
    }

    internal sealed class DiffTreeNode(
        string text,
        IImage image,
        FileStatusItem? item,
        RelativePath? folderPath,
        DiffTreeNode? parent,
        bool isGroupHeader,
        int? groupIndex) : INotifyPropertyChanged
    {
        private bool _isExpanded;

        public string Text { get; } = text;
        public IImage Image { get; } = image;
        public FileStatusItem? Item { get; } = item;
        public RelativePath? FolderPath { get; } = folderPath;
        public DiffTreeNode? Parent { get; } = parent;
        public bool IsGroupHeader { get; } = isGroupHeader;
        public bool IsFolder => FolderPath is not null;
        public int? GroupIndex { get; } = groupIndex;
        public List<DiffTreeNode> Children { get; } = [];

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
