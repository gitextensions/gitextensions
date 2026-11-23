using System.Collections;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Properties;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

using ResourceManager;
using PixelPoint = Avalonia.PixelPoint;
using Rectangle = Avalonia.Rect;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

// Avalonia's ListBox/TreeView templates replace MultiSelectTreeView while preserving the
// original filtering, staging/context-menu, status, revision grouping, and repository hierarchy.
public partial class FileStatusList : GitModuleControl
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private static readonly TimeSpan FilterThrottleDuration = TimeSpan.FromMilliseconds(250);
    private readonly DispatcherTimer _filterTimer = new() { Interval = FilterThrottleDuration };
    private readonly DispatcherTimer _selectedIndexChangeTimer = new() { Interval = SelectedIndexChangeThrottleDuration };
    private IReadOnlyList<FileStatusItem> _allTreeItems = [];
    private IReadOnlyList<GitItemStatus> _gitItemFilteredStatuses = [];
    private IReadOnlyList<GitItemStatus> _gitItemStatuses = [];
    private Action? _refreshAction;
    private IGitUICommands? _boundCommands;
    private bool _hasKeyboardFocus;
    private PointerPressedEventArgs? _dragPointerPressedEventArgs;
    public static readonly TimeSpan SelectedIndexChangeThrottleDuration = TimeSpan.FromMilliseconds(50);

    private readonly FileStatusDiffCalculator _diffCalculator;
    private readonly IFullPathResolver _fullPathResolver;
    private readonly IGitRevisionTester _gitRevisionTester;
    private readonly FileAssociatedIconProvider _iconProvider = new();
    private readonly IRevisionDiffController _revisionDiffController;
    private readonly SortDiffListContextMenuItem _sortByContextMenu;
    private static readonly StatusSorter _sorter = new();
    private readonly IReadOnlyList<GitItemStatus> _noItemStatuses;
    private readonly MenuItem _NO_TRANSLATE_openSubmoduleMenuItem;
    private readonly CancellationTokenSequence _reloadSequence = new();
    private readonly Separator _sortBySeparator = new();
    private GitItemStatus? _nextItemToSelect;
    private bool _enableSelectedIndexChangeEvent = true;
    private bool _flatList;
    private GroupBy? _groupBy;
    private bool _isFileTreeMode;
    private bool _mouseEntered;
    private Rectangle _dragBoxFromMouseDown;
    private IDisposable? _diffListSortSubscription;
    private FormFindInCommitFilesGitGrep? _formFindInCommitFilesGitGrep;
    private bool _showDiffGroups;

    // Enable menu item to disable AppSettings.ShowDiffForAllParents in some forms
    private bool _enableDisablingShowDiffForAllParents;

    [GeneratedRegex(@"(^|\s)-e(\s|\s+['""])", RegexOptions.ExplicitCapture)]
    private static partial Regex GrepStringRegex { get; }

    /// <summary>
    ///  Occurs when the selected file changes (named like the WinForms event).
    /// </summary>
    public event EventHandler? SelectedIndexChanged;

    /// <summary>
    ///  Occurs when the displayed data source is replaced.
    /// </summary>
    public event EventHandler? DataSourceChanged;

    /// <summary>
    ///  Occurs when the filename filter changes.
    /// </summary>
    public event EventHandler? FilterChanged;

    /// <summary>
    ///  Occurs when a file is double-clicked (named like the WinForms event).
    /// </summary>
    public event EventHandler? DoubleClick;

    // Avalonia Control has no Enter event, so the WinForms new modifier is neither required nor legal here.
    public event EnterEventHandler? Enter;

    [Description("Disable showing open submodule menu items as bold")]
    [DefaultValue(false)]
    public bool DisableSubmoduleMenuItemBold { get; set; }

    // Framework constraint: joined revision reloads must return through their owner's UI factory.
    internal JoinableTaskFactory? MainThreadFactory { get; set; }

    // Framework constraint: a keyed native-image registry replaces WinForms ImageList storage and retains its source indices.
    private record ImageListData(IReadOnlyDictionary<string, IImage> ImageList, Dictionary<string, int> StateImageIndexMap, IImage DefaultFileImage, int IconSize);

    private static readonly ImageListData _imageListData = CreateImageListData();

    public FileStatusList()
    {
        _diffCalculator = new FileStatusDiffCalculator(() => Module);
        _fullPathResolver = new FullPathResolver(() =>
        {
            if (TryGetUICommandsDirect(out IGitUICommands? commands))
            {
                return commands.Module.WorkingDir;
            }

            // Avalonia also creates detached file lists for the previewer.
            return this.GetLogicalAncestors().Any(ancestor => ancestor is IGitUICommandsSource)
                ? Module.WorkingDir
                : string.Empty;
        });
        _gitRevisionTester = new GitRevisionTester(_fullPathResolver);
        _revisionDiffController = new RevisionDiffController(() => Module, _fullPathResolver);
        InitializeComponent();

        _NO_TRANSLATE_openSubmoduleMenuItem = CreateOpenSubmoduleMenuItem();
        _sortByContextMenu = new SortDiffListContextMenuItem(DiffListSortService.Instance)
        {
            Name = "sortListByContextMenuItem",
        };
        tsmiCopyPaths.Initialize(
            getUICommands: () => UICommands,
            getSelectedFilePaths: () => SelectedFolder is RelativePath relativePath
                ? [relativePath.Value]
                : SelectedItems.Select(fsi => fsi.Item.Name));
        tsmiFilterFileInGrid.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(TranslatedStrings.FilterFileInGrid);

        cboFilterComboBox.Items.Add("^(?!.*NotThisWord)");
        cboFilterComboBox.Items.Add(@"^(?!.*\bg?tests?/)");

        lstFiles.ItemTemplate = new FuncDataTemplate<object>(CreateFileRow, supportsRecycling: false);
        tvDiffFiles.ItemTemplate = new FuncTreeDataTemplate<DiffTreeNode>(
            (node, _) => CreateDiffTreeRow(node),
            node => node.Children);
        tvFiles.ItemTemplate = new FuncTreeDataTemplate<FileTreeNode>(
            (node, _) => CreateTreeRow(node),
            node => node.Children);
        tvDiffFiles.ContextMenu = ItemContextMenu;
        tvFiles.ContextMenu = ItemContextMenu;
        lstFiles.SelectionChanged += (_, _) => RaiseSelectedIndexChanged();
        tvDiffFiles.SelectionChanged += (_, _) => DiffTreeSelectionChanged();
        tvDiffFiles.ContainerPrepared += DiffTreeContainerPrepared;
        tvFiles.SelectionChanged += (_, _) => RaiseSelectedIndexChanged();
        tvFiles.ContainerPrepared += TreeContainerPrepared;
        lstFiles.DoubleTapped += FileStatusListView_DoubleClick;

        // Avalonia tree rows handle the double tap for expansion before it bubbles to the list.
        tvDiffFiles.AddHandler(InputElement.DoubleTappedEvent, FileStatusListView_DoubleClick, RoutingStrategies.Bubble, handledEventsToo: true);
        tvFiles.DoubleTapped += FileStatusListView_DoubleClick;
        cboFilterComboBox.PropertyChanged += FilterComboBox_TextUpdate;
        cboFilterComboBox.PointerEntered += FilterComboBox_MouseEnter;
        cboFilterComboBox.SelectionChanged += FilterComboBox_SelectedIndexChanged;
        cboFilterComboBox.SizeChanged += FilterComboBox_SizeChanged;
        InitializeFilterToolTip();
        cboFindInCommitFilesGitGrep.PropertyChanged += cboFindInCommitFilesGitGrep_TextUpdate;
        cboFindInCommitFilesGitGrep.SelectionChanged += cboFindInCommitFilesGitGrep_SelectedIndexChanged;
        cboFindInCommitFilesGitGrep.SizeChanged += cboFindInCommitFilesGitGrep_SizeChanged;
        DeleteFilterButton.Click += DeleteFilterButton_Click;
        DeleteSearchButton.Click += DeleteSearchButton_Click;

        InitialiseFiltering();
        _selectedIndexChangeTimer.Tick += SelectedIndexChangeTimer_Tick;
        KeyDown += (_, e) =>
        {
            if (ProcessHotkey(KeysMapper.ToKeys(e)))
            {
                e.Handled = true;
            }
        };
        GotFocus += FileStatusList_Enter;
        PropertyChanged += FileStatusList_LostFocus;
        Control[] fileStatusListViews = [lstFiles, tvDiffFiles, tvFiles];
        foreach (Control fileStatusListView in fileStatusListViews)
        {
            // Avalonia tree rows handle NumPad Multiply before the key bubbles to the list.
            fileStatusListView.AddHandler(KeyDownEvent, FileStatusListView_KeyDown, RoutingStrategies.Tunnel);
            fileStatusListView.AddHandler(PointerPressedEvent, FileStatusListView_MouseDown, RoutingStrategies.Bubble, handledEventsToo: true);
            fileStatusListView.AddHandler(PointerMovedEvent, FileStatusListView_MouseMove, RoutingStrategies.Bubble, handledEventsToo: true);
            fileStatusListView.AddHandler(PointerReleasedEvent, FileStatusListView_MouseUp, RoutingStrategies.Bubble, handledEventsToo: true);
        }

        WireToolbar();
        WireContextMenu();
        WinFormsAutoSizeTextBlock.Attach(NoFiles, includePadding: true);
        AttachedToLogicalTree += (_, e) =>
        {
            SetupUnifiedDiffListSorting();

            if (TryGetUICommandsDirect(out IGitUICommands? commands))
            {
                BindCommands(commands);
                OnLoad(e);
                SetFindInCommitFilesGitGrepVisibility(AppSettings.ShowFindInCommitFilesGitGrep);
            }
        };
        DetachedFromLogicalTree += (_, _) => DisposeCustomResources();
        LayoutUpdated += (_, _) => RefreshFileStatusLabels(layoutChanged: true);

        InitializeComplete();

        NoFiles.Text = TranslatedStrings.NoChanges;
        LoadingFiles.Text = TranslatedStrings.LoadingData;
        _noItemStatuses =
        [
            new GitItemStatus(name: $"- {NoFiles.Text} -")
            {
                IsStatusOnly = true,
                ErrorMessage = string.Empty,
            },
        ];

        // Trigger initialisation of Search and Filter boxes
        SetFileStatusListVisibility(showNoFiles: true);

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

    // Framework constraint: logical attachment supplies the original Load boundary.
    protected void OnLoad(EventArgs e)
    {
        OnRuntimeLoad();
        LoadCustomDifftools();
    }

    private static ImageListData CreateImageListData()
    {
        // Note: The order of images also affects sorting by file status as second key after DiffBranchStatus
        (string imageKey, IImage icon)[] images =
        [
            (nameof(Images.FolderClosed), Images.FolderClosed),
            (nameof(Images.FileStatusUnknown), Images.FileStatusUnknown),
            (nameof(Images.Unmerged), Images.Unmerged),
            (nameof(Images.FileStatusModifiedUnequal), Images.FileStatusModifiedUnequal),
            (nameof(Images.FileStatusModifiedOnlyB), Images.FileStatusModifiedOnlyB),
            (nameof(Images.FileStatusModifiedOnlyA), Images.FileStatusModifiedOnlyA),
            (nameof(Images.FileStatusModifiedSame), Images.FileStatusModifiedSame),
            (nameof(Images.FileStatusModified), Images.FileStatusModified),
            (nameof(Images.FileStatusCopiedUnequal), Images.FileStatusCopiedUnequal),
            (nameof(Images.FileStatusCopiedOnlyB), Images.FileStatusCopiedOnlyB),
            (nameof(Images.FileStatusCopiedOnlyA), Images.FileStatusCopiedOnlyA),
            (nameof(Images.FileStatusCopiedSame), Images.FileStatusCopiedSame),
            (nameof(Images.FileStatusCopied), Images.FileStatusCopied),
            (nameof(Images.FileStatusRenamedUnequal), Images.FileStatusRenamedUnequal.AdaptLightness()),
            (nameof(Images.FileStatusRenamedOnlyB), Images.FileStatusRenamedOnlyB.AdaptLightness()),
            (nameof(Images.FileStatusRenamedOnlyA), Images.FileStatusRenamedOnlyA.AdaptLightness()),
            (nameof(Images.FileStatusRenamedSame), Images.FileStatusRenamedSame.AdaptLightness()),
            (nameof(Images.FileStatusRenamed), Images.FileStatusRenamed.AdaptLightness()),
            (nameof(Images.FileStatusAddedUnequal), Images.FileStatusAddedUnequal),
            (nameof(Images.FileStatusAddedOnlyB), Images.FileStatusAddedOnlyB),
            (nameof(Images.FileStatusAddedOnlyA), Images.FileStatusAddedOnlyA),
            (nameof(Images.FileStatusAddedSame), Images.FileStatusAddedSame),
            (nameof(Images.FileStatusAdded), Images.FileStatusAdded),
            (nameof(Images.FileStatusRemovedUnequal), Images.FileStatusRemovedUnequal),
            (nameof(Images.FileStatusRemovedOnlyB), Images.FileStatusRemovedOnlyB),
            (nameof(Images.FileStatusRemovedOnlyA), Images.FileStatusRemovedOnlyA),
            (nameof(Images.FileStatusRemovedSame), Images.FileStatusRemovedSame),
            (nameof(Images.FileStatusRemoved), Images.FileStatusRemoved),
            (nameof(Images.SubmodulesManage), Images.SubmodulesManage),
            (nameof(Images.FolderSubmodule), Images.FolderSubmodule),
            (nameof(Images.SubmoduleDirty), Images.SubmoduleDirty),
            (nameof(Images.SubmoduleRevisionUp), Images.SubmoduleRevisionUp),
            (nameof(Images.SubmoduleRevisionUpDirty), Images.SubmoduleRevisionUpDirty),
            (nameof(Images.SubmoduleRevisionDown), Images.SubmoduleRevisionDown),
            (nameof(Images.SubmoduleRevisionDownDirty), Images.SubmoduleRevisionDownDirty),
            (nameof(Images.SubmoduleRevisionSemiUp), Images.SubmoduleRevisionSemiUp),
            (nameof(Images.SubmoduleRevisionSemiUpDirty), Images.SubmoduleRevisionSemiUpDirty),
            (nameof(Images.SubmoduleRevisionSemiDown), Images.SubmoduleRevisionSemiDown),
            (nameof(Images.SubmoduleRevisionSemiDownDirty), Images.SubmoduleRevisionSemiDownDirty),
            (nameof(FileStatusDiffCalculator.GitGrepIconName), Images.ViewFile),
            (nameof(ImageListData.DefaultFileImage), Images.File),
            (nameof(Images.Diff), Images.Diff),
            (nameof(Images.DiffR), Images.DiffR),
            (nameof(Images.DiffB), Images.DiffB),
            (nameof(Images.DiffA), Images.DiffA),
            (nameof(Images.DiffC), Images.DiffC),
        ];

        Dictionary<string, int> stateImageIndexDict = [];
        Dictionary<string, IImage> list = [];
        for (int i = 0; i < images.Length; i++)
        {
            list.Add(images[i].imageKey, images[i].icon);
            stateImageIndexDict.Add(images[i].imageKey, i);
        }

        return new ImageListData(list, stateImageIndexDict, Images.File, 16);
    }

    // Framework constraint: logical attachment dispatches the retained runtime-load callback.
    protected override void OnRuntimeLoad()
    {
        base.OnRuntimeLoad();
        ReloadHotkeys();
        if (OperatingSystem.IsWindows())
        {
            VisualStudioIntegration.Init();
        }
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
            // Framework constraint: command discovery can run on the diff worker; read visibility on the UI thread.
            this.InvokeAndForget(() =>
            {
                if (IsVisible)
                {
                    InitializeContextMenuAndToolbar();
                    SetFindInCommitFilesGitGrepVisibility(AppSettings.ShowFindInCommitFilesGitGrep);
                }

                RefreshFileStatusLabels();
            });
        }
    }

    public void Bind(
        Action refreshArtificial,
        bool canAutoRefresh = false,
        Func<ObjectId, string>? describeRevision = null,
        Func<GitRevision, GitRevision>? getActualRevision = null,
        bool isFileTreeMode = false)
    {
        _refreshAction = refreshArtificial;
        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            BindCommands(commands);
        }

        btnRefresh.IsVisible = true;
        tsmiRefreshOnFormFocus.IsVisible = canAutoRefresh;
        sepToolbar.IsVisible = canAutoRefresh;
        DescribeRevision = describeRevision;
        _diffCalculator.DescribeRevision = describeRevision;
        _diffCalculator.GetActualRevision = getActualRevision;
        SetFileTreeMode(isFileTreeMode);
        lblSplitter.Height = isFileTreeMode ? 1 : 0;
        if (isFileTreeMode)
        {
            _diffCalculator.SetGrep("", fileTreeMode: true);
        }
        else
        {
            UpdateToolbar();
        }
    }

    /// <summary>
    ///  Selects the tree node matching the passed relative path.
    /// </summary>
    /// <param name="relativePath">The relative POSIX path to the item or folder.</param>
    /// <returns><c>true</c> if a matching tree node was found.</returns>
    public bool SelectFileOrFolder(RelativePath relativePath, bool firstGroupOnly = false, bool notify = true)
    {
        if (ShowDiffTree)
        {
            IEnumerable<DiffTreeNode> roots = firstGroupOnly && _showDiffGroups
                ? tvDiffFiles.Items.Cast<DiffTreeNode>().Take(1)
                : tvDiffFiles.Items.Cast<DiffTreeNode>();
            DiffTreeNode? diffNode = roots
                .SelectMany(DescendantsAndSelf)
                .FirstOrDefault(candidate => candidate.Item?.Item.Name == relativePath.Value
                                             || candidate.FolderPath?.Value == relativePath.Value);
            if (diffNode is null)
            {
                return false;
            }

            SetSelectedItem(diffNode, notify);
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

            SetSelectedItem(item, notify);
            return true;
        }

        FileTreeNode? node = tvFiles.Items.Cast<FileTreeNode>()
            .SelectMany(DescendantsAndSelf)
            .FirstOrDefault(candidate => candidate.FullPath == relativePath.Value);
        if (node is null)
        {
            return false;
        }

        SetSelectedItem(node, notify);
        return true;
    }

    private string? SelectedItemAbsolutePath => _fullPathResolver.Resolve(SelectedItem?.Item.Name)?.NormalizePath();

    private void SetupUnifiedDiffListSorting()
    {
        // cleanup the previous subscription if it exists.
        _diffListSortSubscription?.Dispose();

        _diffListSortSubscription = DiffListSortService.Instance.CurrentAndFutureSorting()
            .Do(_ => ApplyCurrentSort())
            .Catch<DiffListSortType, Exception>(exception =>
            {
                System.Diagnostics.Trace.WriteLine(exception);
                return Observable.Empty<DiffListSortType>();
            })
            .Subscribe();

        return;

        void ApplyCurrentSort()
        {
            if (GitItemStatusesWithDescription.Count == 0 || _isFileTreeMode)
            {
                return;
            }

            if (Dispatcher.UIThread.CheckAccess())
            {
                UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true);
            }
            else
            {
                Dispatcher.UIThread.Post(() => UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true));
            }
        }
    }

    // Properties

    public IEnumerable<FileStatusItem> AllItems
        => _isFileTreeMode
            ? tvFiles.Items.Cast<FileTreeNode>().SelectMany(DescendantsAndSelf).Select(node => node.Item).OfType<FileStatusItem>()
            : ShowDiffTree
                ? tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(DescendantsAndSelf).Select(node => node.Item).OfType<FileStatusItem>()
                : lstFiles.Items.Cast<object>().Select(GetFileStatusItem).OfType<FileStatusItem>();

    public int AllItemsCount => AllItems.Count();

    public Func<ObjectId, string>? DescribeRevision { get; set; }

    /// <summary>
    ///  Gets whether the filename filter owns keyboard focus.
    /// </summary>
    public bool FilterFilesByNameRegexFocused => cboFilterComboBox.IsKeyboardFocusWithin;

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

        if (visible)
        {
            // Focus the box if it is becoming shown, but not on init of parent controls (which broke the form's focus management)
            // Avalonia uses its routed focus-within flag for the active child of this control.
            if (IsKeyboardFocusWithin)
            {
                cboFindInCommitFilesGitGrep.Focus();
            }
        }
        else if (_formFindInCommitFilesGitGrep?.IsVisible is not true && !string.IsNullOrEmpty(cboFindInCommitFilesGitGrep.Text))
        {
            cboFindInCommitFilesGitGrep.Text = string.Empty;
            FindInCommitFilesGitGrep();
        }

        SetFileStatusListVisibility(showNoFiles: NoFiles.IsVisible);

        // Sizes are automatically changed by visibility
    }

    // Framework constraint: collapsing the Avalonia host row reproduces WinForms Control.Visible layout.
    private void SetFileStatusListVisibility(bool showNoFiles)
    {
        // Switching Avalonia list/tree presenters clears focus; WinForms retains one control.
        bool restoreFocus = lstFiles.IsKeyboardFocusWithin || tvDiffFiles.IsKeyboardFocusWithin || tvFiles.IsKeyboardFocusWithin;
        LoadingFiles.IsVisible = false;

        // Adjust visibility
        // Use variable to prevent bad value retrieved from `Visible` property
        bool showFilesFilter = !showNoFiles || FindInCommitFilesGitGrepActive;

        ((Control)(cboFilterComboBox.Parent
            ?? throw new InvalidOperationException("The file filter is not attached to its layout row."))).IsVisible = showFilesFilter;
        cboFilterComboBox.IsVisible = showFilesFilter;
        NoFiles.IsVisible = showNoFiles;

        lstFiles.IsVisible = !_isFileTreeMode && !ShowDiffTree;
        tvDiffFiles.IsVisible = ShowDiffTree;
        tvFiles.IsVisible = _isFileTreeMode;
        if (restoreFocus && !IsKeyboardFocusWithin)
        {
            Focus();
        }
    }

    public bool Focused
        => (_isFileTreeMode ? (Control)tvFiles : ShowDiffTree ? tvDiffFiles : lstFiles).IsKeyboardFocusWithin;

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
    ///  Gets the statuses currently visible after filtering.
    /// </summary>
    public IReadOnlyList<GitItemStatus> GitItemFilteredStatuses => _gitItemFilteredStatuses;

    /// <summary>
    ///  Gets the displayed file statuses (named like the WinForms property).
    /// </summary>
    public IReadOnlyList<GitItemStatus> GitItemStatuses => _gitItemStatuses;

    private IReadOnlyList<FileStatusWithDescription> GitItemStatusesWithDescription { get; set; } = [];

    /// <summary>
    ///  Gets or sets whether revision descriptions are represented as parent nodes.
    /// </summary>
    public bool GroupByRevision { get; set; }

    /// <summary>
    ///  Gets whether the unfiltered list is empty.
    /// </summary>
    public bool IsEmpty => _gitItemStatuses.Count == 0;

    /// <summary>
    ///  Gets whether at least one item is selected.
    /// </summary>
    public bool HasSelection
        => _isFileTreeMode
            ? tvFiles.SelectedItems is { Count: > 0 }
            : ShowDiffTree
                ? tvDiffFiles.SelectedItems is { Count: > 0 }
                : lstFiles.SelectedItems is { Count: > 0 };

    /// <summary>
    ///  Gets the <see cref="RelativePath"/> of a single selected folder node.
    /// </summary>
    public RelativePath? SelectedFolder
        => _isFileTreeMode && tvFiles.SelectedItem is FileTreeNode { IsFolder: true } node
            ? RelativePath.From(node.FullPath)
            : ShowDiffTree
              && tvDiffFiles.SelectedItems is { Count: 1 }
              && tvDiffFiles.SelectedItem is DiffTreeNode { Children.Count: > 0 } diffNode
                ? diffNode.FolderPath ?? RelativePath.From(string.Empty)
                : null;

    /// <summary>
    ///  Gets or sets the selected Git item (named like the WinForms property).
    /// </summary>
    public GitItemStatus? SelectedGitItem
    {
        get => SelectedItem?.Item;
        set
        {
            object? itemToBeSelected = GetItemByStatus(value);
            if (itemToBeSelected is null)
            {
                ClearSelected();
            }
            else
            {
                SetSelectedItem(itemToBeSelected, notify: true);
            }

            return;

            object? GetItemByStatus(GitItemStatus? status)
            {
                if (status is null)
                {
                    return null;
                }

                IEnumerable<object> items = _isFileTreeMode
                    ? tvFiles.Items.Cast<FileTreeNode>().SelectMany(DescendantsAndSelf)
                    : ShowDiffTree
                        ? tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(DescendantsAndSelf)
                        : lstFiles.Items.Cast<object>();
                object? newSelected = null;
                foreach (object node in items)
                {
                    if (GetGitItemStatus(node) is not GitItemStatus item)
                    {
                        continue;
                    }

                    if (item == status)
                    {
                        return node;
                    }

                    if (status.CompareName(item) == 0 && newSelected is null)
                    {
                        newSelected = node;
                    }
                }

                return newSelected;
            }
        }
    }

    /// <summary>
    ///  Gets the selected Git statuses, including worktree lists that do not carry revisions.
    /// </summary>
    public IReadOnlyList<GitItemStatus> SelectedGitItems
    {
        get => [.. SelectedItems.Select(item => item.Item)];
        set
        {
            if (value is null)
            {
                ClearSelected();
                return;
            }

            HashSet<GitItemStatus> selected = [.. value];
            SelectItems(item => GetGitItemStatus(item) is GitItemStatus status && selected.Contains(status));
        }
    }

    /// <summary>
    ///  Gets the selected file status item, or <see langword="null"/>.
    /// </summary>
    public FileStatusItem? SelectedItem
    {
        get
        {
            IList? selectedItems = _isFileTreeMode ? tvFiles.SelectedItems
                : ShowDiffTree ? tvDiffFiles.SelectedItems
                : lstFiles.SelectedItems;
            return selectedItems is { Count: 1 } ? GetFileStatusItem(selectedItems[0]) : null;
        }
    }

    /// <summary>
    ///  Gets all selected revision-aware items.
    /// </summary>
    public IEnumerable<FileStatusItem> SelectedItems
    {
        get
        {
            if (_isFileTreeMode)
            {
                FileTreeNode[] selected = [.. tvFiles.SelectedItems?.OfType<FileTreeNode>() ?? []];
                return selected.Length == 1
                    ? DescendantsAndSelf(selected[0]).Select(node => node.Item).OfType<FileStatusItem>()
                    : selected.Select(node => node.Item).OfType<FileStatusItem>();
            }

            if (ShowDiffTree)
            {
                DiffTreeNode[] selected = [.. tvDiffFiles.SelectedItems?.OfType<DiffTreeNode>() ?? []];
                return selected.Length == 1
                    ? DescendantsAndSelf(selected[0]).Select(node => node.Item).OfType<FileStatusItem>().Where(item => item.Item != _noItemStatuses[0])
                    : selected.Select(node => node.Item).OfType<FileStatusItem>();
            }

            return lstFiles.SelectedItems?.Cast<object>()
                       .Select(GetFileStatusItem)
                       .OfType<FileStatusItem>()
                   ?? [];
        }
        set
        {
            if (value is null)
            {
                ClearSelected();
                return;
            }

            HashSet<FileStatusItem> selected = [.. value];
            SelectItems(item => GetFileStatusItem(item) is FileStatusItem status && selected.Contains(status));
        }
    }

    public IEnumerable<FileStatusItem> FirstGroupItems
        => _showDiffGroups && tvDiffFiles.Items.Cast<DiffTreeNode>().FirstOrDefault() is DiffTreeNode first
            ? first.Children.SelectMany(DescendantsAndSelf).Select(node => node.Item).OfType<FileStatusItem>()
            : AllItems;

    public bool SelectFirstItemOnSetItems { get; set; } = true;

    /// <summary>
    ///  Gets the number of items before filtering.
    /// </summary>
    public int UnfilteredItemsCount => _gitItemStatuses.Count;

    /// <summary>
    ///  Gets whether a filename filter is active.
    /// </summary>
    public bool IsFilterActive => !string.IsNullOrEmpty(cboFilterComboBox.Text);

    // Public methods

    public void ClearSelected()
    {
        lstFiles.Selection.Clear();
        tvDiffFiles.SelectedItems?.Clear();
        tvFiles.SelectedItems?.Clear();
    }

    /// <summary>
    ///  Focuses the active file list or repository tree.
    /// </summary>
    // Avalonia's focus overload has parameters, so the parameterless WinForms-shaped method is a distinct member.
    public void Focus()
    {
        if (!HasSelection)
        {
            SelectFirstVisibleItem();
        }

        if (!GetTarget().Focus())
        {
            // Framework constraint: a newly selected tab lays out its native presenter after the focus request.
            Dispatcher.UIThread.Post(() => GetTarget().Focus(), DispatcherPriority.Loaded);
        }

        return;

        Control GetTarget() => _isFileTreeMode ? tvFiles : ShowDiffTree ? tvDiffFiles : lstFiles;
    }

    // Avalonia's item templates pass the native row label and status directly.
    private static (string? prefix, string text, string? suffix) FormatListViewItem(
        string nodeText, GitItemStatus? gitItemStatus, string? parentPath = null, PathFormatter? formatter = null, int itemWidth = 0)
    {
        if (gitItemStatus is null || gitItemStatus.IsRangeDiff)
        {
            return (prefix: null, text: nodeText, suffix: null);
        }

        string? prefix = null;
        string? text = nodeText;
        string? suffix = null;
        if (AppSettings.TruncatePathMethod == TruncatePathMethod.FileNameOnly)
        {
            (text, suffix) = PathFormatter.FormatTextForFileNameOnly(gitItemStatus.Name, gitItemStatus.OldName);
        }
        else if (formatter is not null)
        {
            string name = gitItemStatus.Name.TrimEnd('/');
            if (!string.IsNullOrEmpty(parentPath) && name.StartsWith(parentPath + "/", StringComparison.Ordinal))
            {
                name = name[(parentPath.Length + 1)..];
            }

            (prefix, text, suffix) = formatter.FormatTextForDrawing(itemWidth, name, gitItemStatus.OldName);
        }
        else
        {
            if (gitItemStatus.OldName is { Length: > 0 } oldName)
            {
                string oldNameSuffix = $" ({oldName})";
                if (nodeText.EndsWith(oldNameSuffix, StringComparison.Ordinal))
                {
                    text = nodeText[..^oldNameSuffix.Length];
                    suffix = oldNameSuffix;
                }
            }

            text = text.TrimEnd('/');
        }

        return (prefix, AppendItemSubmoduleStatus(text ?? string.Empty, gitItemStatus), suffix);
    }

    public FileStatusItem? SelectNextItem(bool backwards, bool loop, bool notify = true)
    {
        FileStatusItem[] items = [.. AllItems.Where(item => !item.Item.IsStatusOnly && !item.Item.IsRangeDiff)];
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

    /// <summary>
    ///  Selects all visible files.
    /// </summary>
    public void SelectAll()
    {
        // Avoid scrolling and repainting between folders while expanding the whole selection.
        TreeView? tree = _isFileTreeMode ? tvFiles : ShowDiffTree ? tvDiffFiles : null;
        bool autoScroll = tree?.AutoScrollToSelectedItem ?? false;
        try
        {
            if (tree is not null)
            {
                tree.AutoScrollToSelectedItem = false;
            }

            if (_isFileTreeMode)
            {
                foreach (FileTreeNode node in tvFiles.Items.Cast<FileTreeNode>())
                {
                    ExpandAll(node);
                }

                tvFiles.SelectedItems?.Clear();
                foreach (FileTreeNode node in tvFiles.Items.Cast<FileTreeNode>().SelectMany(DescendantsAndSelf).Where(node => node.Item is not null))
                {
                    tvFiles.SelectedItems?.Add(node);
                }
            }
            else if (ShowDiffTree)
            {
                foreach (DiffTreeNode node in tvDiffFiles.Items.Cast<DiffTreeNode>())
                {
                    ExpandAll(node);
                }

                tvDiffFiles.SelectedItems?.Clear();
                foreach (DiffTreeNode node in tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(DescendantsAndSelf).Where(node => node.Item is not null))
                {
                    tvDiffFiles.SelectedItems?.Add(node);
                }
            }
            else
            {
                lstFiles.SelectAll();
            }
        }
        finally
        {
            if (tree is not null)
            {
                tree.AutoScrollToSelectedItem = autoScroll;
            }
        }
    }

    /// <summary>
    ///  Selects the first file in the active list/tree.
    /// </summary>
    public void SelectFirstVisibleItem()
    {
        if (ShowDiffTree)
        {
            // Skip collapsed or empty groups
            DiffTreeNode? first = tvDiffFiles.Items.Cast<DiffTreeNode>()
                .SelectMany(FlattenVisible)
                .FirstOrDefault(node => node.Item is not null && node.Item.Item != _noItemStatuses[0]);
            tvDiffFiles.SelectedItem = first ?? tvDiffFiles.Items.Cast<DiffTreeNode>().FirstOrDefault();

            return;
        }

        if (_isFileTreeMode)
        {
            if (_filter is null && !FindInCommitFilesGitGrepActive)
            {
                tvFiles.SelectedItem = tvFiles.Items.Cast<FileTreeNode>().FirstOrDefault();
                return;
            }

            // Descend
            FileTreeNode? first = tvFiles.Items.Cast<FileTreeNode>()
                .SelectMany(DescendantsAndSelf)
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

    public void SelectStoredNextItem(bool orSelectFirst = false)
    {
        SelectedGitItem = _nextItemToSelect;
        _nextItemToSelect = null;
        if (orSelectFirst && SelectedItem is null)
        {
            SelectFirstVisibleItem();
        }
    }

    /// <summary>
    ///  Calculates and shows files changed by the given revisions.
    /// </summary>
    public void SetDiffs(IReadOnlyList<GitRevision> revisions)
    {
        CancellationToken cancellationToken = _reloadSequence.Next();
        FileStatusListLoading();
        UpdateToolbar(revisions);
        _enableDisablingShowDiffForAllParents = true;
        _diffCalculator.SetDiff(revisions, headId: default, allowMultiDiff: false);
        UpdateFileStatusListView(
            _diffCalculator.Calculate(prevList: [], refreshDiff: true, refreshGrep: false, cancellationToken),
            cancellationToken: cancellationToken);
    }

    public async Task SetDiffsAsync(IReadOnlyList<GitRevision> revisions, ObjectId headId, CancellationToken cancellationToken)
    {
        _reloadSequence.CancelCurrent();
        FileStatusListLoading();
        UpdateToolbar(revisions);
        bool isFileTreeMode = _isFileTreeMode;
        _enableDisablingShowDiffForAllParents = !isFileTreeMode;
        bool showSkipWorktreeFiles = tsmiShowSkipWorktreeFiles.IsChecked == true;
        bool showUntrackedFiles = tsmiShowUntrackedFiles.IsChecked == true;
        JoinableTaskFactory mainThreadFactory = MainThreadFactory ?? ThreadHelper.JoinableTaskFactory;
        IReadOnlyList<FileStatusWithDescription> groups = [];
        if (isFileTreeMode)
        {
            GitItemStatusesWithDescription = [];
        }
        else
        {
            groups = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                _diffCalculator.SetDiff(revisions, headId, allowMultiDiff: true, showSkipWorktreeFiles, showUntrackedFiles);
                return _diffCalculator.Calculate(prevList: [], refreshDiff: true, refreshGrep: false, cancellationToken);
            }, cancellationToken);

            await mainThreadFactory.SwitchToMainThreadAsync(cancellationToken);
            bool withGitGrep = FindInCommitFilesGitGrepActive;
            UpdateFileStatusListView(groups, gitGrepState: withGitGrep ? GitGrepState.Preparing : GitGrepState.None, cancellationToken: cancellationToken);

            // git grep, fetched as a separate step
            if (!withGitGrep)
            {
                return;
            }
        }

        string search = cboFindInCommitFilesGitGrep.Text ?? "";
        groups = await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (isFileTreeMode)
            {
                _diffCalculator.SetDiff(revisions, headId, allowMultiDiff: false);
            }

            _diffCalculator.SetGrep(GetGrepArguments(search), fileTreeMode: isFileTreeMode && string.IsNullOrWhiteSpace(search));
            return _diffCalculator.Calculate(prevList: groups, refreshDiff: false, refreshGrep: true, cancellationToken);
        }, cancellationToken);

        await mainThreadFactory.SwitchToMainThreadAsync(cancellationToken);
        UpdateFileStatusListView(groups, gitGrepState: GitGrepState.Provided, cancellationToken: cancellationToken);
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
    public void SetStashDiffs(GitRevision headRev,
        GitRevision indexRev,
        string indexDesc,
        IReadOnlyList<GitItemStatus> indexItems,
        GitRevision workTreeRev,
        string workTreeDesc,
        IReadOnlyList<GitItemStatus> workTreeItems)
    {
        FileStatusListLoading();
        UpdateToolbar([workTreeRev, indexRev]);
        UpdateFileStatusListView(
        [
            new FileStatusWithDescription(indexRev, workTreeRev, workTreeDesc, workTreeItems),
            new FileStatusWithDescription(headRev, indexRev, indexDesc, indexItems),
        ]);
    }

    /// <summary>
    ///  Shows files changed between two revisions.
    /// </summary>
    public void SetDiffs(GitRevision? firstRev, GitRevision secondRev, IReadOnlyList<GitItemStatus> items)
    {
        FileStatusListLoading();
        UpdateToolbar([secondRev]);
        UpdateFileStatusListView(
        [
            new(
                firstRev: firstRev,
                secondRev: secondRev,
                summary: TranslatedStrings.DiffWithParent + GetDescriptionForRevision(firstRev?.ObjectId ?? default(ObjectId)),
                statuses: items),
        ]);
    }

    public void ClearDiffs()
    {
        _reloadSequence.CancelCurrent();
        UpdateToolbar([]);
        UpdateFileStatusListView([]);
    }

    private string? GetDescriptionForRevision(ObjectId objectId)
        => DescribeRevision is not null ? DescribeRevision(objectId)
            : objectId == ObjectId.WorkTreeId ? ResourceManager.TranslatedStrings.Workspace
            : objectId == ObjectId.IndexId ? ResourceManager.TranslatedStrings.Index
            : objectId.ToShortString();

    public void SetNoFilesText(string text)
        => NoFiles.Text = text;

    // Framework constraint: separate native presenters own the selected tree node or flat-list item.
    private void SetSelectedItem(object node, bool notify)
    {
        _enableSelectedIndexChangeEvent = notify;
        try
        {
            switch (node)
            {
                case DiffTreeNode diffNode:
                    ExpandAncestors(diffNode);
                    tvDiffFiles.SelectedItem = diffNode;
                    break;

                case FileTreeNode fileNode:
                    ExpandAncestors(fileNode);
                    tvFiles.SelectedItem = fileNode;
                    break;

                default:
                    lstFiles.SelectedItem = node;
                    break;
            }
        }
        finally
        {
            _enableSelectedIndexChangeEvent = true;
        }

        if (node is DiffTreeNode selectedDiff)
        {
            tvDiffFiles.ScrollIntoView(selectedDiff);
        }
        else if (node is not FileTreeNode)
        {
            lstFiles.ScrollIntoView(node);
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

    public void StoreNextItemToSelect()
    {
        GitItemStatus[] visible = [.. AllItems.Select(item => item.Item)];
        int currentIndex = Array.IndexOf(visible, SelectedItem?.Item);
        _nextItemToSelect = currentIndex >= 0 && currentIndex + 1 < visible.Length
            ? visible[currentIndex + 1]
            : currentIndex > 0 ? visible[currentIndex - 1] : null;
    }

    protected override void DisposeCustomResources()
    {
        _selectedIndexChangeTimer.Stop();
        _diffListSortSubscription?.Dispose();
        _diffListSortSubscription = null;

        // Stop the throttled filtering: a pending callback would otherwise run after the form was
        // closed and access the detached list view.
        _filterTimer.Stop();
        CancelLoadCustomDifftools();
        UnbindCommands();
    }

    private static string AppendItemSubmoduleStatus(string text, GitItemStatus item)
    {
        if (item.IsSubmodule
            && item.GetSubmoduleStatusAsync() is Task<GitSubmoduleStatus?> { IsCompleted: true } task
            && task.CompletedResult() is GitSubmoduleStatus status)
        {
            text += status.AddedAndRemovedString();
        }

        return text;
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

    // Framework constraint: the native tree and flat-list presenters select different item objects.
    private void SelectItems(Func<object, bool> predicate)
    {
        ClearSelected();
        if (ShowDiffTree)
        {
            foreach (DiffTreeNode node in tvDiffFiles.Items.Cast<DiffTreeNode>().SelectMany(DescendantsAndSelf))
            {
                if (node.Item is not null && predicate(node.Item))
                {
                    tvDiffFiles.SelectedItems?.Add(node);
                }
            }
        }
        else if (_isFileTreeMode)
        {
            foreach (FileTreeNode node in tvFiles.Items.Cast<FileTreeNode>().SelectMany(DescendantsAndSelf))
            {
                if (node.Item is not null && predicate(node.Item))
                {
                    tvFiles.SelectedItems?.Add(node);
                }
            }
        }
        else
        {
            foreach (object item in lstFiles.Items.Cast<object>().Where(predicate))
            {
                lstFiles.SelectedItems?.Add(item);
            }
        }
    }

    private void SetDeleteFilterButtonVisibility()
    {
        DeleteFilterButton.IsVisible = IsFilterActive;
    }

    private void SetDeleteSearchButtonVisibility()
        => DeleteSearchButton.IsVisible = FindInCommitFilesGitGrepActive;

    private void FileStatusListLoading()
    {
        // Show "Files loading" below the filterbox

        if (_isFileTreeMode)
        {
            SetFileStatusListVisibility(showNoFiles: false);
        }

        NoFiles.IsVisible = false;
        LoadingFiles.IsVisible = true;
        ClearSelected();
        lstFiles.ItemsSource = null;
        tvDiffFiles.ItemsSource = null;
        tvFiles.ItemsSource = null;
    }

    private void UpdateFileStatusListView(IReadOnlyList<FileStatusWithDescription> items, bool updateCausedByFilter = false, GitGrepState gitGrepState = GitGrepState.Unknown, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        HashSet<GitItemStatus>? previouslySelectedItems = null;
        if (updateCausedByFilter)
        {
            IList? selectedItems = tvFiles.IsVisible ? tvFiles.SelectedItems
                : tvDiffFiles.IsVisible ? tvDiffFiles.SelectedItems
                : lstFiles.SelectedItems;
            previouslySelectedItems = selectedItems is null ? []
                : [.. selectedItems.Cast<object>().Select(GetGitItemStatus).OfType<GitItemStatus>()];
        }

        FileStatusItem? selectedFileStatus = SelectedFileStatusItem;
        RelativePath? selectedPath = SelectedRelativePath;

        DiffListSortType sortType = DiffListSortService.Instance.DiffListSorting;
        _groupBy = sortType switch
        {
            DiffListSortType.FilePath or DiffListSortType.FilePathFlat
                => null,
            DiffListSortType.FileExtension or DiffListSortType.FileExtensionFlat
                => new GroupBy(status => GroupKey.From(Path.GetExtension(status.Name)), GetImageKey: GetExtensionImageKey, GetLabel: group => group.Key.Value),
            DiffListSortType.FileStatus or DiffListSortType.FileStatusFlat
                => new GroupBy(GetStatusKey, GetImageKey: group => GetItemImageKey(group.First()), GetLabel: _ => ""),
            _ => throw new NotSupportedException($"{sortType} is not a supported sorting method.")
        };
        _flatList = sortType is DiffListSortType.FilePathFlat or DiffListSortType.FileExtensionFlat or DiffListSortType.FileStatusFlat;

        if (items.Count == 0)
        {
            // Avalonia can retain the selection after replacing the tree's items.
            ClearSelected();
            _showDiffGroups = false;
        }

        if (items.Count == 0 || !ReferenceEquals(items, GitItemStatusesWithDescription))
        {
            FileStatusItem[] statusItems =
            [
                .. items.SelectMany(group => group.Statuses.Select(
                    item => new FileStatusItem(group.FirstRev, group.SecondRev, item, group.BaseA, group.BaseB))),
            ];
            _gitItemStatuses = [.. statusItems.Select(item => item.Item)];
            _allTreeItems = _isFileTreeMode ? statusItems : [];
            GitItemStatusesWithDescription = items;
        }

        bool filesPresent;
        if (_isFileTreeMode)
        {
            FileStatusItem[] visibleItems = [.. _allTreeItems.Where(item => IsFilterMatch(item.Item))];
            _gitItemFilteredStatuses = [.. visibleItems.Select(item => item.Item)];
            tvFiles.ItemsSource = BuildFileTree(visibleItems);
            filesPresent = items.Any(group => group.Statuses.Count > 0);
        }
        else if (items.Count > 0)
        {
            Dictionary<int, bool> expansion = tvDiffFiles.Items
                .Cast<DiffTreeNode>()
                .Where(node => node.GroupIndex is not null)
                .ToDictionary(node => node.GroupIndex!.Value, node => node.IsExpanded);
            (List<TreeNodeInfo> nodes, _showDiffGroups, filesPresent) = GetNodes(
                items,
                GroupByRevision,
                IsFilterMatch,
                _groupBy,
                _flatList,
                expandIfFewFiles: true,
                gitGrepState,
                _noItemStatuses,
                expansion,
                cancellationToken);

            foreach ((DiffTreeNode node, ExpandCollapseState state) in nodes)
            {
                switch (state)
                {
                    case ExpandCollapseState.Collapsed:
                        foreach (DiffTreeNode child in node.Children)
                        {
                            SetExpansion(child, expanded: true);
                        }

                        break;

                    case ExpandCollapseState.Expanded:
                        SetExpansion(node, expanded: true);
                        break;

                    case ExpandCollapseState.PartiallyExpanded:
                        node.IsExpanded = true;
                        break;
                }
            }

            DiffTreeNode[] roots = [.. nodes.Select(info => info.Node)];
            _gitItemFilteredStatuses =
            [
                .. roots.SelectMany(DescendantsAndSelf)
                    .Select(node => node.Item?.Item)
                    .OfType<GitItemStatus>(),
            ];

            bool showDiffTree = _showDiffGroups || !_flatList || roots.Any(node => node.IsGroupHeader);
            if (showDiffTree)
            {
                tvDiffFiles.ItemsSource = roots;
                lstFiles.ItemsSource = null;
            }
            else
            {
                List<object> visibleItems =
                [
                    .. roots.SelectMany(DescendantsAndSelf)
                        .Where(node => node.Item is not null)
                        .Select(node => (object)node.Item!),
                ];
                _gitItemFilteredStatuses = [.. visibleItems.Select(GetGitItemStatus).OfType<GitItemStatus>()];
                lstFiles.ItemsSource = visibleItems;
                tvDiffFiles.ItemsSource = null;
            }

            ShowDiffTree = showDiffTree;
        }
        else
        {
            filesPresent = false;
            ShowDiffTree = false;
            _gitItemFilteredStatuses = [];
            lstFiles.ItemsSource = null;
            tvDiffFiles.ItemsSource = null;
        }

        SetFileStatusListVisibility(showNoFiles: !filesPresent && items.Count <= 1 && !_isFileTreeMode);
        UpdateToolbar();
        bool selectionRestored = false;
        if (previouslySelectedItems?.Count is > 0)
        {
            SelectItems(item => GetGitItemStatus(item) is GitItemStatus status && previouslySelectedItems.Contains(status));
            selectionRestored = HasSelection;
        }

        selectionRestored |= !selectionRestored
                             && selectedFileStatus is not null
                             && SelectFileStatusItem(selectedFileStatus, notify: false);
        selectionRestored |= !selectionRestored
                             && selectedPath is not null
                             && SelectFileOrFolder(selectedPath, notify: false);
        bool singleRootFile = _isFileTreeMode
            ? tvFiles.Items is [FileTreeNode { IsFolder: false }]
            : ShowDiffTree
                ? tvDiffFiles.Items is [DiffTreeNode { Item: not null, Children.Count: 0 }]
                : lstFiles.ItemCount == 1;
        if (singleRootFile || (!selectionRestored && !updateCausedByFilter && SelectFirstItemOnSetItems))
        {
            SelectFirstVisibleItem();
        }

        DataSourceChanged?.Invoke(this, EventArgs.Empty);
        return;

        static string GetExtensionImageKey(IGrouping<GroupKey, GitItemStatus> group)
        {
            string extension = group.Key.Value;
            return _imageListData.StateImageIndexMap.ContainsKey(extension) ? extension : nameof(ImageListData.DefaultFileImage);
        }

        static GroupKey GetStatusKey(GitItemStatus status)
        {
            char inverseDiffStatus = (char)((int)'Z' - (int)status.DiffStatus);
            int imageIndex = _imageListData.StateImageIndexMap[GetItemImageKey(status)];
            DebugHelpers.Assert(inverseDiffStatus >= 0, $"offset of {nameof(inverseDiffStatus)} needs to be adapted");
            DebugHelpers.Assert(imageIndex < 100, $"width of {nameof(imageIndex)} needs to be adapted");
            return GroupKey.From($"{inverseDiffStatus}{imageIndex:D02}");
        }
    }

    private record struct TreeNodeInfo(DiffTreeNode Node, ExpandCollapseState State);

    private static (List<TreeNodeInfo> Nodes, bool ShowDiffGroups, bool FilesPresent) GetNodes(
        IReadOnlyList<FileStatusWithDescription> items,
        bool groupByRevision,
        Func<GitItemStatus, bool> isFilterMatch,
        GroupBy? groupBy,
        bool flatList,
        bool expandIfFewFiles,
        GitGrepState gitGrepState,
        IReadOnlyList<GitItemStatus> noItemStatuses,
        IReadOnlyDictionary<int, bool> previousExpansion,
        CancellationToken cancellationToken)
    {
        bool showDiffGroups = items.Count > 1
                              || (groupByRevision && !(items.Count == 1 && items[0].Statuses.Count == 0));
        bool hasGrepGroup = gitGrepState != GitGrepState.None
                            && (gitGrepState != GitGrepState.Unknown || items.Any(FileStatusDiffCalculator.IsGrepItemStatuses));
        bool filesPresent = items.Any(group => group.Statuses.Count > 0);
        bool showGroupLabel = (filesPresent && (items.Count > 1 || groupByRevision)) || hasGrepGroup;
        List<TreeNodeInfo> rootNodes = [];
        for (int groupIndex = 0; groupIndex < items.Count; groupIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FileStatusWithDescription group = items[groupIndex];
            bool emptyGroup = showGroupLabel && group.Statuses.Count == 0;
            if (group.Statuses.Count == 1 && group.Statuses[0].IsRangeDiff)
            {
                FileStatusItem rangeDiff = new(group.FirstRev, group.SecondRev, group.Statuses[0], group.BaseA, group.BaseB);
                rootNodes.Add(new TreeNodeInfo(CreateDiffFileNode(rangeDiff, parent: null), ExpandCollapseState.LeafNode));
                continue;
            }

            IEnumerable<GitItemStatus> statuses = emptyGroup
                ? noItemStatuses
                : group.Statuses.Where(isFilterMatch);
            FileStatusItem[] visibleItems =
            [
                .. statuses.Select(item => new FileStatusItem(group.FirstRev, group.SecondRev, item, group.BaseA, group.BaseB)),
            ];

            // Always expand grep results
            // Collapse some groups for diffs with common BASE
            bool isGrepGroup = FileStatusDiffCalculator.IsGrepItemStatuses(group);
            bool defaultExpanded = hasGrepGroup
                ? isGrepGroup
                : ((group.Statuses.Count <= 7 && group.IconName == nameof(Images.Diff))
                   || items.Count < 3
                   || groupIndex == 0)
                  && group.Statuses.Count > 0;
            bool expanded = !emptyGroup
                && (gitGrepState == GitGrepState.Unknown && previousExpansion.TryGetValue(groupIndex, out bool wasExpanded)
                    ? wasExpanded
                    : defaultExpanded);
            ExpandCollapseState state = !expanded
                ? ExpandCollapseState.Collapsed
                : isGrepGroup && (!expandIfFewFiles || visibleItems.Length >= 100)
                    ? ExpandCollapseState.PartiallyExpanded
                    : ExpandCollapseState.Expanded;

            if (showDiffGroups)
            {
                // Show shown and total number of files only if different; avoid showing "1/0" for "- No changes -"
                string shownDisplay = visibleItems.Length >= group.Statuses.Count ? string.Empty : $"{visibleItems.Length}/";
                DiffTreeNode header = new(
                    $"({shownDisplay}{group.Statuses.Count}) {group.Summary}",
                    GetGroupImage(group.IconName),
                    item: null,
                    folderPath: null,
                    parent: null,
                    isGroupHeader: true,
                    groupIndex);
                header.Children.AddRange(CreateGroup(visibleItems, header));
                rootNodes.Add(new TreeNodeInfo(header, state));
            }
            else
            {
                // Add nodes of single group as root nodes
                if (state == ExpandCollapseState.PartiallyExpanded)
                {
                    state = ExpandCollapseState.Collapsed;
                }

                foreach (DiffTreeNode node in CreateGroup(visibleItems, parent: null))
                {
                    rootNodes.Add(new TreeNodeInfo(node, state));
                }
            }
        }

        return (rootNodes, showDiffGroups, filesPresent);

        IReadOnlyList<DiffTreeNode> CreateGroup(IReadOnlyList<FileStatusItem> fileItems, DiffTreeNode? parent)
        {
            if (groupBy is null)
            {
                return BuildPathNodes(fileItems, parent, flatList);
            }

            Dictionary<GitItemStatus, FileStatusItem> itemsByStatus = new(ReferenceEqualityComparer.Instance);
            foreach (FileStatusItem item in fileItems)
            {
                itemsByStatus[item.Item] = item;
            }

            IOrderedEnumerable<IGrouping<GroupKey, GitItemStatus>> grouped = fileItems
                .Select(item => item.Item)
                .GroupBy(groupBy.GetGroupKey)
                .OrderBy(group => group.Key);
            List<DiffTreeNode> groupRoots = [];
            foreach (IGrouping<GroupKey, GitItemStatus> group in grouped)
            {
                FileStatusItem[] groupItems = [.. group.Select(status => itemsByStatus[status])];
                IReadOnlyList<DiffTreeNode> children = BuildPathNodes(groupItems, parent: null, flatList);
                bool showGroupNode = !flatList || AppSettings.FileStatusShowGroupNodesInFlatList;
                if (!showGroupNode || (children.Count == 1 && children[0].Children.Count == 0))
                {
                    groupRoots.AddRange(BuildPathNodes(groupItems, parent, flatList));
                    continue;
                }

                DiffTreeNode groupNode = new(
                    groupBy.GetLabel(group),
                    _imageListData.ImageList[groupBy.GetImageKey(group)],
                    item: null,
                    folderPath: null,
                    parent,
                    isGroupHeader: true,
                    groupIndex: null);
                groupNode.Children.AddRange(BuildPathNodes(groupItems, groupNode, flatList));
                groupRoots.Add(groupNode);
            }

            return groupRoots;
        }

        static IReadOnlyList<DiffTreeNode> BuildPathNodes(
            IReadOnlyList<FileStatusItem> items,
            DiffTreeNode? parent,
            bool flat)
        {
            Dictionary<GitItemStatus, FileStatusItem> itemsByStatus = new(ReferenceEqualityComparer.Instance);
            foreach (FileStatusItem item in items)
            {
                itemsByStatus[item.Item] = item;
            }

            StatusNode statusRoot = _sorter.CreateTreeSortedByPath(
                items.Select(item => item.Item),
                flat,
                AppSettings.FileStatusMergeSingleItemWithFolder,
                status =>
                {
                    FileStatusItem item = itemsByStatus[status];
                    string oldName = status.OldName is null ? string.Empty : $" ({status.OldName})";
                    return new StatusNode($"{status.Name}{oldName}") { Tag = item };
                });
            return [.. statusRoot.Nodes.Select(node => Convert(node, parent))];

            DiffTreeNode Convert(StatusNode statusNode, DiffTreeNode? currentParent)
            {
                FileStatusItem? item = statusNode.Tag as FileStatusItem;
                DiffTreeNode node = new(
                    statusNode.Text,
                    item is null ? Images.FolderClosed : GetItemImage(item.Item),
                    item,
                    statusNode.Tag is RelativePath folderPath ? folderPath : null,
                    currentParent,
                    isGroupHeader: false,
                    groupIndex: null);
                foreach (StatusNode child in statusNode.Nodes)
                {
                    node.Children.Add(Convert(child, node));
                }

                return node;
            }
        }
    }

    private static string GetItemImageKey(GitItemStatus gitItemStatus)
    {
        if (gitItemStatus.IsDeleted)
        {
            return gitItemStatus.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => nameof(Images.FileStatusRemovedOnlyA),
                DiffBranchStatus.OnlyBChange => nameof(Images.FileStatusRemovedOnlyB),
                DiffBranchStatus.SameChange => nameof(Images.FileStatusRemovedSame),
                DiffBranchStatus.UnequalChange => nameof(Images.FileStatusRemovedUnequal),
                _ => nameof(Images.FileStatusRemoved)
            };
        }

        if (gitItemStatus.IsRangeDiff)
        {
            return nameof(Images.DiffR);
        }

        // Image without evaluating added/removed etc
        if (!string.IsNullOrWhiteSpace(gitItemStatus.GrepString))
        {
            return nameof(ImageListData.DefaultFileImage);
        }

        if (gitItemStatus.IsNew || !gitItemStatus.IsTracked)
        {
            return gitItemStatus.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => nameof(Images.FileStatusAddedOnlyA),
                DiffBranchStatus.OnlyBChange => nameof(Images.FileStatusAddedOnlyB),
                DiffBranchStatus.SameChange => nameof(Images.FileStatusAddedSame),
                DiffBranchStatus.UnequalChange => nameof(Images.FileStatusAddedUnequal),
                _ => nameof(Images.FileStatusAdded)
            };
        }

        if (gitItemStatus.IsUnmerged)
        {
            return nameof(Images.Unmerged);
        }

        if (gitItemStatus.IsSubmodule)
        {
            return GetSubmoduleItemImageKey(gitItemStatus);
        }

        if (gitItemStatus.IsChanged || (gitItemStatus.IsRenamed && gitItemStatus.RenameCopyPercentage != "100"))
        {
            return gitItemStatus.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => nameof(Images.FileStatusModifiedOnlyA),
                DiffBranchStatus.OnlyBChange => nameof(Images.FileStatusModifiedOnlyB),
                DiffBranchStatus.SameChange => nameof(Images.FileStatusModifiedSame),
                DiffBranchStatus.UnequalChange => nameof(Images.FileStatusModifiedUnequal),
                _ => nameof(Images.FileStatusModified)
            };
        }

        if (gitItemStatus.IsRenamed)
        {
            return gitItemStatus.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => nameof(Images.FileStatusRenamedOnlyA),
                DiffBranchStatus.OnlyBChange => nameof(Images.FileStatusRenamedOnlyB),
                DiffBranchStatus.SameChange => nameof(Images.FileStatusRenamedSame),
                DiffBranchStatus.UnequalChange => nameof(Images.FileStatusRenamedUnequal),
                _ => nameof(Images.FileStatusRenamed)
            };
        }

        if (gitItemStatus.IsCopied)
        {
            return gitItemStatus.DiffStatus switch
            {
                DiffBranchStatus.OnlyAChange => nameof(Images.FileStatusCopiedOnlyA),
                DiffBranchStatus.OnlyBChange => nameof(Images.FileStatusCopiedOnlyB),
                DiffBranchStatus.SameChange => nameof(Images.FileStatusCopiedSame),
                DiffBranchStatus.UnequalChange => nameof(Images.FileStatusCopiedUnequal),
                _ => nameof(Images.FileStatusCopied)
            };
        }

        // Illegal flag combinations or no flags set?
        return nameof(Images.FileStatusUnknown);
    }

    // Framework constraint: Avalonia images are resolved by the original resource key rather than a WinForms ImageList index.
    private static IImage GetItemImage(GitItemStatus item)
        => _imageListData.ImageList[GetItemImageKey(item)];

    private static string GetSubmoduleItemImageKey(GitItemStatus gitItemStatus)
    {
        GitSubmoduleStatus? status = gitItemStatus.GetSubmoduleStatusAsync() is Task<GitSubmoduleStatus?> { IsCompleted: true } task
            ? task.CompletedResult()
            : null;
        return GetSubmoduleItemImageKey(gitItemStatus, status);
    }

    private static string GetSubmoduleItemImageKey(GitItemStatus item, GitSubmoduleStatus? status)
    {
        if (status is null)
        {
            return item.IsDirty ? nameof(Images.SubmoduleDirty) : nameof(Images.SubmodulesManage);
        }

        return (status.Status, status.IsDirty) switch
        {
            (SubmoduleStatus.FastForward, true) => nameof(Images.SubmoduleRevisionUpDirty),
            (SubmoduleStatus.FastForward, false) => nameof(Images.SubmoduleRevisionUp),
            (SubmoduleStatus.Rewind, true) => nameof(Images.SubmoduleRevisionDownDirty),
            (SubmoduleStatus.Rewind, false) => nameof(Images.SubmoduleRevisionDown),
            (SubmoduleStatus.NewerTime, true) => nameof(Images.SubmoduleRevisionSemiUpDirty),
            (SubmoduleStatus.NewerTime, false) => nameof(Images.SubmoduleRevisionSemiUp),
            (SubmoduleStatus.OlderTime, true) => nameof(Images.SubmoduleRevisionSemiDownDirty),
            (SubmoduleStatus.OlderTime, false) => nameof(Images.SubmoduleRevisionSemiDown),
            (SubmoduleStatus.SameCommit, false) => nameof(Images.FolderSubmodule),
            _ => nameof(Images.SubmoduleDirty),
        };
    }

    /// <summary>
    ///  Selects the preceding visible file, wrapping to the first file at the boundary.
    /// </summary>
    public void SelectPreviousVisibleItem()
    {
        if (ShowDiffTree)
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
        if (ShowDiffTree)
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

    // Event handlers
    private void ItemContextMenu_Opening(object? sender, CancelEventArgs e)
    {
        if (sender is null || (SelectedFileStatusItem?.Item.IsStatusOnly ?? false))
        {
            e.Cancel = true;
            return;
        }

        System.Collections.IList items = ItemContextMenu.Items;
        InsertTreeContextMenuItems(items, index: 0);
        UpdateStatusOfTreeContextMenuItems();

        UpdateStatusOfMenuItems();

        // TODO The handling of _NO_TRANSLATE_openSubmoduleMenuItem need to be revised
        // This code handles the 'bold' in the menu for submodules. Other default actions are not set to bold.
        // The actual implementation of the default handling with doubleclick is in each form,
        // separate from this menu item
        if (!items.Contains(_NO_TRANSLATE_openSubmoduleMenuItem))
        {
            items.Insert(0, _NO_TRANSLATE_openSubmoduleMenuItem);
        }

        bool isSubmoduleSelected = SelectedFileStatusItem?.Item.IsSubmodule ?? false;
        _NO_TRANSLATE_openSubmoduleMenuItem.IsVisible = isSubmoduleSelected;
        _NO_TRANSLATE_openSubmoduleMenuItem.FontWeight = isSubmoduleSelected
                                                          && !DisableSubmoduleMenuItemBold
                                                          && AppSettings.OpenSubmoduleDiffInSeparateWindow
            ? FontWeight.Bold
            : FontWeight.Normal;

        _sortBySeparator.IsVisible = !_isFileTreeMode;
        _sortByContextMenu.IsVisible = !_isFileTreeMode;
        if (!items.Contains(_sortByContextMenu))
        {
            items.Add(_sortBySeparator);
            items.Add(_sortByContextMenu);
        }

        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            sepScripts.IsVisible = ItemContextMenu.AddUserScripts(
                tsmiRunScript,
                ExecuteCommand,
                script => script.OnEvent == ScriptEvent.ShowInFileList,
                commands);
        }
        else
        {
            ItemContextMenu.RemoveUserScripts(tsmiRunScript);
            sepScripts.IsVisible = false;
        }
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

                // offset a few pixels compared to FindAndReplaceForm
                Position = owner.Position + new PixelPoint(90, 110),
            };
            _formFindInCommitFilesGitGrep.Closed += (_, _) => _formFindInCommitFilesGitGrep = null;
        }

        _formFindInCommitFilesGitGrep.GitGrepExpressionText = !string.IsNullOrEmpty(text)
            ? text
            : FindInCommitFilesGitGrepActive ? cboFindInCommitFilesGitGrep.Text : null;
        _formFindInCommitFilesGitGrep.SetSearchItems(cboFindInCommitFilesGitGrep.Items);
        _formFindInCommitFilesGitGrep.SetShowFindInCommitFilesGitGrep(FindInCommitFilesGitGrepVisible);
        if (!_formFindInCommitFilesGitGrep.IsVisible)
        {
            _formFindInCommitFilesGitGrep.Show(owner);
        }

        _formFindInCommitFilesGitGrep.Activate();
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

    // Avalonia TextBlock inlines carry the source drawing colors through the native row styles.
    private static void FileStatusListView_DrawNodeImpl(TextBlock label, (string? prefix, string text, string? suffix) parts)
    {
        (string? prefix, string text, string? suffix) = parts;
        if (string.IsNullOrEmpty(text) || (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix)))
        {
            label.Inlines?.Clear();
            label.Text = text;
            return;
        }

        label.Text = null;
        InlineCollection inlines = label.Inlines ??= new InlineCollection();
        inlines.Clear();
        AddGrayText(prefix);
        inlines.Add(new Run(text));
        AddGrayText(suffix);

        return;

        void AddGrayText(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Run run = new(value);
                run.Classes.Add("filestatus-label-decoration");
                inlines.Add(run);
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

        if (e.Key == Key.Multiply && e.KeyModifiers == KeyModifiers.None && sender is TreeView tree)
        {
            // Set e.Handled = true; Avalonia has no TreeView Ding sound
            Control? focusedElement = e.Source as Control;
            TreeViewItem? focusedRow = focusedElement?.FindAncestorOfType<TreeViewItem>(includeSelf: true);
            ExpandAll(focusedRow?.DataContext ?? tree.SelectedItem, focusedRow);
            e.Handled = true;
        }
    }

    private void FileStatusListView_MouseDown(object? sender, PointerPressedEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(this);

        // SELECT
        if (!e.Handled && point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            Avalonia.Visual? source = e.Source as Avalonia.Visual;
            if (source?.FindAncestorOfType<TreeViewItem>(includeSelf: true) is { DataContext: object treeItem })
            {
                if (_isFileTreeMode)
                {
                    tvFiles.SelectedItem = treeItem;
                }
                else if (ShowDiffTree)
                {
                    tvDiffFiles.SelectedItem = treeItem;
                }
            }
            else if (source?.FindAncestorOfType<ListBoxItem>(includeSelf: true) is { DataContext: object listItem })
            {
                lstFiles.SelectedItem = listItem;
            }
        }

        // DRAG
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
        {
            if (SelectedItems.Any())
            {
                // Remember the point where the mouse down occurred.
                // The DragSize indicates the size that the mouse can move
                // before a drag event should be started.
                // Avalonia exposes pointer drag initiation but not the desktop drag-size setting.
                Avalonia.Size dragSize = new(4, 4);
                Avalonia.Point position = e.GetPosition(this);

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                _dragBoxFromMouseDown = new Rectangle(
                    position.X - (dragSize.Width / 2),
                    position.Y - (dragSize.Height / 2),
                    dragSize.Width,
                    dragSize.Height);
                _dragPointerPressedEventArgs = e;
            }
            else
            {
                // Reset the rectangle if the mouse is not over an item in the ListView.
                ClearDragState();
            }
        }
    }

    private void FileStatusListView_MouseUp(object? sender, PointerReleasedEventArgs e)
    {
        // Release the drag capture
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            ClearDragState();
        }
    }

    private void FileStatusListView_MouseMove(object? sender, PointerEventArgs e)
    {
        // DRAG
        // If the mouse moves outside the rectangle, start the drag.
        if (_dragBoxFromMouseDown != default
            && !_dragBoxFromMouseDown.Contains(e.GetPosition(this))
            && _dragPointerPressedEventArgs is PointerPressedEventArgs pressedEventArgs
            && SelectedItems.Any())
        {
            ClearDragState();
            this.InvokeAndForget(() => StartFileDragAsync(pressedEventArgs));
        }
    }

    private void FileStatusListView_SelectedIndexChanged()
    {
        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
    }

    // Avalonia reports the activation's input origin on the routed focus event.
    private void FileStatusList_Enter(object? sender, FocusChangedEventArgs e)
    {
        if (_hasKeyboardFocus)
        {
            return;
        }

        _hasKeyboardFocus = true;
        _mouseEntered = e.NavigationMethod == NavigationMethod.Pointer;
        Enter?.Invoke(this, new EnterEventArgs(_mouseEntered));
        _mouseEntered = false;
    }

    private string _toolTipText = "";
    private Regex? _filter;

    /// <summary>
    ///  Applies a case-insensitive regular-expression filter to file and old names.
    /// </summary>
    public void SetFilter(string value)
    {
        SetFilterText(value);
        FilterFiles(value);
    }

    // Avalonia reports programmatic text assignments through the same property notification.
    private void SetFilterText(string value)
    {
        cboFilterComboBox.PropertyChanged -= FilterComboBox_TextUpdate;
        try
        {
            cboFilterComboBox.Text = value;
        }
        finally
        {
            cboFilterComboBox.PropertyChanged += FilterComboBox_TextUpdate;
        }
    }

    private void DeleteFilterButton_Click(object? sender, EventArgs e)
    {
        SetFilter(string.Empty);
    }

    private int FilterFiles(string value)
    {
        if (!string.Equals(cboFilterComboBox.Text, value, StringComparison.Ordinal))
        {
            SetFilterText(value);
        }

        _filterTimer.Stop();
        StoreFilter(value);

        // Feed back the current list of files
        UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true);
        FilterChanged?.Invoke(this, EventArgs.Empty);
        return _gitItemFilteredStatuses.Count;
    }

    private bool IsFilterMatch(GitItemStatus item)
    {
        if (item.IsRangeDiff)
        {
            return true;
        }

        if (!IsDiffStatusMatch(item.DiffStatus))
        {
            return false;
        }

        if (_filter is null)
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
            _toolTipText = exception.Message;
            SetFilterState(FilterState.Invalid, _toolTipText);
            return false;
        }
    }

    private void InitialiseFiltering()
    {
        // TODO this code is very similar to code in FormCommit
        _filterTimer.Tick += FilterTimer_Tick;
        return;

        void FilterTimer_Tick(object? sender, EventArgs e)
        {
            _filterTimer.Stop();
            TaskManager.HandleExceptions(() =>
            {
                _toolTipText = "";
                string filterText = cboFilterComboBox.Text ?? string.Empty;
                int fileCount = 0;
                try
                {
                    fileCount = FilterFiles(filterText);
                }
                catch (ArgumentException exception)
                {
                    _toolTipText = exception.Message;
                }

                if (fileCount > 0 && !cboFilterComboBox.Classes.Contains("file-filter-invalid"))
                {
                    AddToSelectionFilter(filterText);
                }
            }, WinFormsShims.Application.OnThreadException);
        }

        void AddToSelectionFilter(string filter)
        {
            if (cboFilterComboBox.Items.Cast<string>().Any(candidate => candidate == filter))
            {
                return;
            }

            const int SelectionFilterMaxLength = 10;
            if (cboFilterComboBox.Items.Count == SelectionFilterMaxLength)
            {
                cboFilterComboBox.Items.RemoveAt(SelectionFilterMaxLength - 1);
            }

            cboFilterComboBox.Items.Insert(0, filter);
        }
    }

    private void FilterComboBox_TextUpdate(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ComboBox.TextProperty)
        {
            return;
        }

        // show DeleteFilterButton at once
        SetDeleteFilterButtonVisibility();

        string filterText = cboFilterComboBox.Text ?? string.Empty;
        bool strippedWorkingDirectory = false;
        string workingDir = TryGetUICommandsDirect(out IGitUICommands? commands) ? commands.Module.WorkingDir : string.Empty;
        if (workingDir.Length > 0 && filterText.Length > workingDir.Length)
        {
            string posixWorkingDir = PathUtil.ToPosixPath(workingDir);
            string posixFilterText = PathUtil.ToPosixPath(filterText);
            if (posixFilterText.StartsWith(posixWorkingDir, StringComparison.InvariantCultureIgnoreCase))
            {
                filterText = posixFilterText.SubstringAfter(posixWorkingDir, StringComparison.InvariantCultureIgnoreCase);
                SetFilterText(filterText);
                strippedWorkingDirectory = true;
            }
        }

        if (cboFilterComboBox.IsKeyboardFocusWithin)
        {
            // The native TextBox completes its input selection after the Text notification.
            Dispatcher.UIThread.Post(() =>
            {
                TextBox? editor = cboFilterComboBox.GetVisualDescendants()
                    .OfType<TextBox>().FirstOrDefault(control => control.Name == "PART_EditableTextBox");
                if (editor is not { IsFocused: true } || cboFilterComboBox.Text != filterText)
                {
                    return;
                }

                if (strippedWorkingDirectory && editor.Text != filterText)
                {
                    editor.SetCurrentValue(TextBox.TextProperty, filterText);
                }

                // workaround for text getting selected if it matches the start of the combobox items
                int selectionStart = Math.Min(editor.SelectionStart, editor.SelectionEnd);
                int selectionEnd = Math.Max(editor.SelectionStart, editor.SelectionEnd);
                if (strippedWorkingDirectory || (selectionStart == 0 && selectionEnd == filterText.Length))
                {
                    editor.CaretIndex = filterText.Length;
                    editor.SelectionStart = filterText.Length;
                    editor.SelectionEnd = filterText.Length;
                }
            }, DispatcherPriority.Input);
        }

        _filterTimer.Stop();
        _filterTimer.Start();
    }

    private void FilterComboBox_MouseEnter(object? sender, EventArgs e)
    {
        SetFilterToolTip(_toolTipText);
    }

    private void InitializeFilterToolTip()
    {
        // Avalonia's attached value is detached until the original error route supplies a message.
        ToolTip.SetTip(cboFilterComboBox, null);

        // The WinForms component's AutomaticDelay owns this single target and its display duration.
        DispatcherTimer autoPopTimer = new() { Interval = TimeSpan.FromMilliseconds(10 * ToolTip.GetShowDelay(cboFilterComboBox)) };
        autoPopTimer.Tick += (_, _) =>
        {
            autoPopTimer.Stop();
            ToolTip.SetIsOpen(cboFilterComboBox, false);
        };
        cboFilterComboBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == ToolTip.IsOpenProperty)
            {
                autoPopTimer.IsEnabled = ToolTip.GetIsOpen(cboFilterComboBox);
            }
        };
        cboFilterComboBox.DetachedFromVisualTree += (_, _) => autoPopTimer.Stop();
    }

    private void SetFilterToolTip(string? message)
    {
        lblFilterToolTipMessage.Text = message;

        // Avalonia's attached tip is disabled by a null value.
        ToolTip.SetTip(cboFilterComboBox, string.IsNullOrEmpty(message) ? null : FilterToolTip);
    }

    private void FilterComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        FilterFiles(cboFilterComboBox.Text ?? string.Empty);
    }

    private void FilterComboBox_SizeChanged(object? sender, EventArgs e)
    {
        // invalidate it on resize so its look does not become distorted
        cboFilterComboBox.InvalidateVisual();
    }

    private void cboFindInCommitFilesGitGrep_TextUpdate(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ComboBox.TextProperty)
        {
            FindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Text ?? string.Empty, delay: 200);
        }
    }

    private void FindInCommitFilesGitGrep()
    {
        FindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Text ?? string.Empty);
    }

    private void FindInCommitFilesGitGrep(string search, int delay = 0)
    {
        SetDeleteSearchButtonVisibility();

        CancellationToken cancellationToken = _reloadSequence.Next();
        this.InvokeAndForget(async () =>
        {
            // delay to handle keypresses
            await Task.Delay(delay, cancellationToken);
            bool isFileTreeMode = _isFileTreeMode;
            IReadOnlyList<FileStatusWithDescription> previous = GitItemStatusesWithDescription;
            IReadOnlyList<FileStatusWithDescription> groups = await Task.Run(() =>
            {
                _diffCalculator.SetGrep(GetGrepArguments(search), fileTreeMode: isFileTreeMode && string.IsNullOrWhiteSpace(search));
                return _diffCalculator.Calculate(
                    prevList: previous,
                    refreshDiff: false,
                    refreshGrep: true,
                    cancellationToken);
            }, cancellationToken);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                cboFindInCommitFilesGitGrep.Classes.Set("file-filter-active", !string.IsNullOrEmpty(search));
                UpdateFileStatusListView(groups, gitGrepState: string.IsNullOrEmpty(search) ? GitGrepState.None : GitGrepState.Provided, cancellationToken: cancellationToken);
                if (!string.IsNullOrEmpty(search))
                {
                    AddToSearchFilter(search);
                }
            });

            void AddToSearchFilter(string search)
            {
                int index = cboFindInCommitFilesGitGrep.Items.IndexOf(search);
                if (index == 0)
                {
                    return;
                }

                // Avalonia also reports programmatic changes to the editor text.
                cboFindInCommitFilesGitGrep.PropertyChanged -= cboFindInCommitFilesGitGrep_TextUpdate;
                cboFindInCommitFilesGitGrep.SelectionChanged -= cboFindInCommitFilesGitGrep_SelectedIndexChanged;
                try
                {
                    if (index > 0)
                    {
                        cboFindInCommitFilesGitGrep.Items.RemoveAt(index);
                        cboFindInCommitFilesGitGrep.Items.Insert(0, search);
                        cboFindInCommitFilesGitGrep.Text = search;
                        TextBox? editor = cboFindInCommitFilesGitGrep.GetVisualDescendants().OfType<TextBox>()
                            .FirstOrDefault(control => control.Name == "PART_EditableTextBox");
                        if (editor is not null)
                        {
                            editor.CaretIndex = search.Length;
                            editor.SelectionStart = search.Length;
                            editor.SelectionEnd = search.Length;
                        }
                    }
                    else
                    {
                        const int SearchFilterMaxLength = 30;
                        if (cboFindInCommitFilesGitGrep.Items.Count >= SearchFilterMaxLength)
                        {
                            cboFindInCommitFilesGitGrep.Items.RemoveAt(SearchFilterMaxLength - 1);
                        }

                        cboFindInCommitFilesGitGrep.Items.Insert(0, search);
                    }

                    if (_formFindInCommitFilesGitGrep?.IsVisible is true)
                    {
                        _formFindInCommitFilesGitGrep.SetSearchItems(cboFindInCommitFilesGitGrep.Items);
                        if (FindInCommitFilesGitGrepVisible)
                        {
                            _formFindInCommitFilesGitGrep.GitGrepExpressionText = search;
                        }
                    }
                }
                finally
                {
                    cboFindInCommitFilesGitGrep.PropertyChanged += cboFindInCommitFilesGitGrep_TextUpdate;
                    cboFindInCommitFilesGitGrep.SelectionChanged += cboFindInCommitFilesGitGrep_SelectedIndexChanged;
                }
            }
        });
    }

    // Framework constraint: both native search updates and revision refreshes use the current editor text.
    private static string GetGrepArguments(string search)
    {
        if (string.IsNullOrWhiteSpace(search) || GrepStringRegex.IsMatch(search))
        {
            return search;
        }

        return $@"-e ""{search.Replace(@"\\", @"\\\\")}""";
    }

    private void cboFindInCommitFilesGitGrep_SelectedIndexChanged(object? sender, EventArgs e)
    {
        FindInCommitFilesGitGrep();
    }

    private void cboFindInCommitFilesGitGrep_SizeChanged(object? sender, EventArgs e)
    {
        cboFindInCommitFilesGitGrep.InvalidateVisual();
    }

    private void DeleteSearchButton_Click(object? sender, EventArgs e)
    {
        cboFindInCommitFilesGitGrep.Text = string.Empty;
        FindInCommitFilesGitGrep();
    }

    // Avalonia uses separate list and tree renderers, so renderer selection stays distinct from the source's revision-group flag.
    private bool ShowDiffTree { get; set; }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FileStatusList), "columnHeader", "Text", "Files");
        tsmiCopyPaths.AddControlTranslationItems(translation);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        tsmiCopyPaths.TranslateControlItems(translation);
        ToolTip.SetTip(btnSettings, btnSettings.Content);
    }

    public delegate void EnterEventHandler(object? sender, EnterEventArgs e);

    /// <summary>
    ///  Gets the selected revision-aware item, or <see langword="null"/>.
    /// </summary>
    public FileStatusItem? SelectedFileStatusItem => _isFileTreeMode
        ? (tvFiles.SelectedItem as FileTreeNode)?.Item
        : ShowDiffTree
            ? (tvDiffFiles.SelectedItem as DiffTreeNode)?.Item
            : GetFileStatusItem(lstFiles.SelectedItem);

    /// <summary>
    ///  Gets the selected file or folder path.
    /// </summary>
    public RelativePath? SelectedRelativePath
        => _isFileTreeMode && tvFiles.SelectedItem is FileTreeNode node
            ? RelativePath.From(node.FullPath)
            : ShowDiffTree && tvDiffFiles.SelectedItem is DiffTreeNode diffNode
                ? diffNode.Item is not null
                    ? RelativePath.From(diffNode.Item.Item.Name)
                    : diffNode.FolderPath
            : SelectedItem is FileStatusItem item
                ? RelativePath.From(item.Item.Name)
                : null;

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

    private void StoreFilter(string value)
    {
        SetDeleteFilterButtonVisibility();
        if (value.Length == 0)
        {
            SetFilterState(FilterState.Empty, toolTip: null);
            _filter = null;
            return;
        }

        try
        {
            _filter = new Regex(value, RegexOptions.IgnoreCase, RegexTimeout);
            SetFilterState(FilterState.Valid, toolTip: null);
        }
        catch
        {
            SetFilterState(FilterState.Invalid, toolTip: null);
            throw;
        }
    }

    /// <summary>
    ///  Shows calculated revision groups as either the ordinary diff list or a repository tree.
    /// </summary>
    public void SetDiffs(IReadOnlyList<FileStatusWithDescription> groups, bool isFileTreeMode)
    {
        if (_isFileTreeMode != isFileTreeMode)
        {
            GitItemStatusesWithDescription = [];
        }

        SetFileTreeMode(isFileTreeMode);
        UpdateFileStatusListView(groups);
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    private void SetFileTreeMode(bool isFileTreeMode)
    {
        bool modeChanged = _isFileTreeMode != isFileTreeMode;
        _isFileTreeMode = isFileTreeMode;
        Toolbar.IsVisible = !isFileTreeMode;
        if (modeChanged)
        {
            ShowDiffTree = false;
            lblSplitter.Height = isFileTreeMode ? 1 : 0;
        }

        lstFiles.IsVisible = !isFileTreeMode;
        tvDiffFiles.IsVisible = false;
        tvFiles.IsVisible = isFileTreeMode;
        if (isFileTreeMode)
        {
            SetFindInCommitFilesGitGrepVisibilityImpl(AppSettings.ShowFindInCommitFilesGitGrep);
            GroupByRevision = false;
        }
    }

    private void RaiseSelectedIndexChanged()
    {
        if (_enableSelectedIndexChangeEvent)
        {
            _selectedIndexChangeTimer.Stop();
            _selectedIndexChangeTimer.Start();
        }
    }

    private void SelectedIndexChangeTimer_Tick(object? sender, EventArgs e)
    {
        _selectedIndexChangeTimer.Stop();
        TaskManager.HandleExceptions(FileStatusListView_SelectedIndexChanged, WinFormsShims.Application.OnThreadException);
    }

    private async Task StartFileDragAsync(PointerPressedEventArgs pressedEventArgs)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            ClearDragState();
            return;
        }

        DataTransfer data = new();
        foreach (FileStatusItem item in SelectedItems)
        {
            string? fileName = _fullPathResolver.Resolve(item.Item.Name);
            if (!string.IsNullOrWhiteSpace(fileName)
                && await topLevel.StorageProvider.TryGetFileFromPathAsync(new Uri(fileName)) is { } file)
            {
                data.Add(DataTransferItem.CreateFile(file));
            }
        }

        if (data.Items.Count > 0)
        {
            // Proceed with the drag and drop, passing in the list item.
            await DragDrop.DoDragDropAsync(pressedEventArgs, data, DragDropEffects.Copy);
        }

        ClearDragState();
    }

    private void ClearDragState()
    {
        _dragBoxFromMouseDown = default;
        _dragPointerPressedEventArgs = null;
    }

    // The native focus-within property changes as soon as focus leaves the whole list.
    private void FileStatusList_LostFocus(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsKeyboardFocusWithinProperty && !IsKeyboardFocusWithin)
        {
            _hasKeyboardFocus = false;
        }
    }

    private bool SelectFileStatusItem(FileStatusItem item, bool notify)
    {
        if (ShowDiffTree)
        {
            DiffTreeNode? node = tvDiffFiles.Items.Cast<DiffTreeNode>()
                .SelectMany(DescendantsAndSelf)
                .FirstOrDefault(candidate => ReferenceEquals(candidate.Item?.Item, item.Item));
            if (node is null)
            {
                return false;
            }

            SetSelectedItem(node, notify);
            return true;
        }

        if (_isFileTreeMode)
        {
            FileTreeNode? node = tvFiles.Items.Cast<FileTreeNode>()
                .SelectMany(DescendantsAndSelf)
                .FirstOrDefault(candidate => ReferenceEquals(candidate.Item?.Item, item.Item));
            if (node is null)
            {
                return false;
            }

            SetSelectedItem(node, notify);
            return true;
        }

        object? listItem = lstFiles.Items.Cast<object>()
            .FirstOrDefault(candidate => ReferenceEquals(GetFileStatusItem(candidate)?.Item, item.Item));
        if (listItem is null)
        {
            return false;
        }

        SetSelectedItem(listItem, notify);
        return true;
    }

    private void SetFilterState(FilterState state, string? toolTip)
    {
        cboFilterComboBox.Classes.Set("file-filter-active", state == FilterState.Valid);
        cboFilterComboBox.Classes.Set("file-filter-invalid", state == FilterState.Invalid);
        SetFilterToolTip(toolTip);
        DeleteFilterButton.IsVisible = IsFilterActive;
    }

    private void DiffTreeSelectionChanged()
    {
        RaiseSelectedIndexChanged();
    }

    private void DiffTreeContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container.DataContext is not DiffTreeNode node)
        {
            return;
        }

        e.Container.Classes.Set("diff-group-header", node.IsGroupHeader);
        e.Container.Classes.Set("leaf-node", node.Children.Count == 0);
        e.Container.Focusable = true;
    }

    private void TreeContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container.DataContext is FileTreeNode node)
        {
            e.Container.Classes.Set("leaf-node", node.Children.Count == 0);
        }
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
            .. tvFiles.Items.Cast<FileTreeNode>().SelectMany(DescendantsAndSelf).Where(node => !node.IsFolder),
        ];
        if (files.Length == 0)
        {
            return;
        }

        int selectedIndex = Array.IndexOf(files, tvFiles.SelectedItem as FileTreeNode);
        int nextIndex = selectedIndex < 0 ? 0 : Math.Clamp(selectedIndex + offset, 0, files.Length - 1);
        tvFiles.SelectedItem = files[nextIndex];
    }

    internal readonly struct TestAccessor
    {
        private readonly FileStatusList _fileStatusList;

        internal TestAccessor(FileStatusList fileStatusList)
        {
            _fileStatusList = fileStatusList;
        }

        internal ComboBox FilterComboBox => _fileStatusList.cboFilterComboBox;
        internal bool UpdateContextMenu()
        {
            CancelEventArgs eventArgs = new();
            _fileStatusList.ItemContextMenu_Opening(_fileStatusList.ItemContextMenu, eventArgs);
            return eventArgs.Cancel;
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
        Dictionary<GitItemStatus, FileStatusItem> itemsByStatus = new(ReferenceEqualityComparer.Instance);
        foreach (FileStatusItem item in items)
        {
            itemsByStatus[item.Item] = item;
        }

        StatusNode statusRoot = _sorter.CreateTreeSortedByPath(
            items.Select(item => item.Item),
            flat: false,
            AppSettings.FileStatusMergeSingleItemWithFolder,
            status =>
            {
                string oldName = status.OldName is null ? string.Empty : $" ({status.OldName})";
                return new StatusNode($"{status.Name}{oldName}") { Tag = itemsByStatus[status] };
            });
        return [.. statusRoot.Nodes.Select(node => Convert(node, parent: null))];

        static FileTreeNode Convert(StatusNode statusNode, FileTreeNode? parent)
        {
            FileStatusItem? item = statusNode.Tag as FileStatusItem;
            string fullPath = statusNode.Tag switch
            {
                RelativePath folderPath => folderPath.Value,
                FileStatusItem file => file.Item.Name,
                _ => statusNode.Text,
            };
            FileTreeNode node = new(statusNode.Text, fullPath, item, parent);
            foreach (StatusNode child in statusNode.Nodes)
            {
                node.Children.Add(Convert(child, node));
            }

            return node;
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
        TextBlock text = new()
        {
            Text = node.Text,
            FontWeight = FontWeight.Normal,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        // Scale image size and images scale automatically
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
            FileStatusLabel label = new(node.Text, node.Item.Item, node.Parent?.FolderPath?.Value);
            text.Tag = label;
            text.Classes.Add("filestatus-item-label");
            RefreshFileStatusLabel(text, label, layoutChanged: false);

            // Also set the automation name in order to provide accessibility information
            AutomationProperties.SetName(text, node.Text);
            UpdateSubmoduleVisualWhenReady(image, text, node.Item.Item);
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
                text,
            },
        };
    }

    private Control CreateTreeRow(FileTreeNode node)
    {
        TextBlock text = new()
        {
            Text = node.Name,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        Image image = new()
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            Source = node.Item is null
                ? Images.FolderClosed
                : _isFileTreeMode && !node.Item.Item.IsSubmodule
                    ? Images.File
                    : GetItemImage(node.Item.Item),
            Margin = new Avalonia.Thickness(1, 0, 3, 0),
        };
        if (node.Item is not null)
        {
            FileStatusLabel label = new(node.Name, node.Item.Item, node.Parent?.FullPath);
            text.Tag = label;
            text.Classes.Add("filestatus-item-label");
            RefreshFileStatusLabel(text, label, layoutChanged: false);
            AutomationProperties.SetName(text, node.Name);
            UpdateSubmoduleVisualWhenReady(image, text, node.Item.Item);
            if (!node.Item.Item.IsSubmodule
                && (_isFileTreeMode || !string.IsNullOrWhiteSpace(node.Item.Item.GrepString)))
            {
                // The source file-tree group uses associated file icons even when its
                // GetTreeFiles entries have no grep expression or diff-status flags.
                LoadFileIcons([(image, node.Item.Item.Name)], CancellationToken.None);
            }
        }

        return new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children =
            {
                image,
                text,
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
            FileStatusItem fileStatusItem => fileStatusItem,
            DiffTreeNode node => node.Item,
            FileTreeNode node => node.Item,
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

        GitItemStatus? gitItemStatus = GetGitItemStatus(item);
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
        string itemText = gitItemStatus.OldName is null
            ? gitItemStatus.Name
            : $"{gitItemStatus.Name} ({gitItemStatus.OldName})";
        TextBlock text = new()
        {
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        FileStatusLabel label = new(itemText, gitItemStatus, parentPath: null);
        text.Tag = label;
        text.Classes.Add("filestatus-item-label");
        RefreshFileStatusLabel(text, label, layoutChanged: false);
        AutomationProperties.SetName(text, itemText);
        UpdateSubmoduleVisualWhenReady(image, text, gitItemStatus);
        if (!gitItemStatus.IsSubmodule && !string.IsNullOrWhiteSpace(gitItemStatus.GrepString))
        {
            LoadFileIcons([(image, gitItemStatus.Name)], CancellationToken.None);
        }

        row.Children.Add(image);
        row.Children.Add(text);

        return row;
    }

    private static void UpdateSubmoduleVisualWhenReady(Image image, TextBlock text, GitItemStatus item)
    {
        if (!item.IsSubmodule)
        {
            return;
        }

        Task<GitSubmoduleStatus?> task = item.GetSubmoduleStatusAsync();
        ThreadHelper.FileAndForget(async () =>
        {
#pragma warning disable VSTHRD003 // GitItemStatus owns and starts the cached status task.
            // Avoid awaiting foreign Tasks
            GitSubmoduleStatus? status = await task.ConfigureAwait(false);
#pragma warning restore VSTHRD003
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                image.Source = _imageListData.ImageList[GetSubmoduleItemImageKey(item, status)];
                if (text.Tag is FileStatusLabel label)
                {
                    RefreshFileStatusLabel(text, label, layoutChanged: false);
                }
            });
        });
    }

    // Avalonia computes row and viewport bounds during layout instead of owner drawing.
    private void RefreshFileStatusLabels(bool layoutChanged = false)
    {
        if (layoutChanged && (!IsEffectivelyVisible || AppSettings.TruncatePathMethod is not (TruncatePathMethod.Compact or TruncatePathMethod.TrimStart)))
        {
            return;
        }

        foreach (TextBlock text in this.GetVisualDescendants().OfType<TextBlock>())
        {
            if (text.Tag is FileStatusLabel label)
            {
                RefreshFileStatusLabel(text, label, layoutChanged);
            }
        }
    }

    private static void RefreshFileStatusLabel(TextBlock text, FileStatusLabel label, bool layoutChanged)
    {
        TruncatePathMethod mode = AppSettings.TruncatePathMethod;
        if (mode is (TruncatePathMethod.Compact or TruncatePathMethod.TrimStart)
            && text.FindAncestorOfType<ScrollContentPresenter>() is { Bounds.Width: > 0 } viewport
            && text.TranslatePoint(default, viewport) is Avalonia.Point origin)
        {
            int width = (int)Math.Max(0, Math.Floor(viewport.Bounds.Width - origin.X));
            Typeface typeface = new(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch);
            var format = (mode, width, typeface, text.FontSize);
            if (!layoutChanged || label.LastFormat != format)
            {
                label.LastFormat = format;
                PathFormatter formatter = new(typeface, text.FontSize);
                FileStatusListView_DrawNodeImpl(text, FormatListViewItem(label.NodeText, label.Item, label.ParentPath, formatter, width));
            }

            return;
        }

        if (!layoutChanged)
        {
            label.LastFormat = null;
            PathFormatter? formatter = mode == TruncatePathMethod.None
                ? new PathFormatter(new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize)
                : null;
            FileStatusListView_DrawNodeImpl(text, FormatListViewItem(label.NodeText, label.Item, label.ParentPath, formatter));
        }
    }

    private sealed class FileStatusLabel(string nodeText, GitItemStatus item, string? parentPath)
    {
        public string NodeText { get; } = nodeText;
        public GitItemStatus Item { get; } = item;
        public string? ParentPath { get; } = parentPath;
        public (TruncatePathMethod Mode, int Width, Typeface Typeface, double FontSize)? LastFormat { get; set; }
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
