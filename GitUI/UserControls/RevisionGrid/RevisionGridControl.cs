using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Avatars;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUI.Script;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using TaskDialog = System.Windows.Forms.TaskDialog;
using TaskDialogButton = System.Windows.Forms.TaskDialogButton;

namespace GitUI
{
    public enum RevisionGraphDrawStyleEnum
    {
        Normal,
        DrawNonRelativesGray,
        HighlightSelected
    }

    public enum SortDirection
    {
        /// <summary>
        /// Sort from smallest to largest. For example, A to Z.
        /// </summary>
        Ascending,

        /// <summary>
        /// Sort from largest to smallest. For example, Z to A.
        /// </summary>
        Descending
    }

    [DefaultEvent("DoubleClick")]
    public sealed partial class RevisionGridControl : GitModuleControl, IScriptHostControl, ICheckRefs, IRunScript, IRevisionGridFilter
    {
        public event EventHandler<DoubleClickRevisionEventArgs>? DoubleClickRevision;
        public event EventHandler<FilterChangedEventArgs>? FilterChanged;
        public event EventHandler? SelectionChanged;

        /// <summary>
        ///  Occurs whenever the revision graph has started loading the data.
        /// </summary>
        public event EventHandler<RevisionLoadEventArgs>? RevisionsLoading;

        /// <summary>
        ///  Occurs whenever the revision graph has been populated with the data.
        /// </summary>
        public event EventHandler<RevisionLoadEventArgs>? RevisionsLoaded;

        /// <summary>
        ///  Occurs whenever a user toggles between the artificial and the HEAD commits
        ///  via the navigation menu item or the shortcut command.
        /// </summary>
        public event EventHandler? ToggledBetweenArtificialAndHeadCommits;

        public static readonly string HotkeySettingsName = "RevisionGrid";

        private readonly TranslationString _droppingFilesBlocked = new("For you own protection dropping more than 10 patch files at once is blocked!");
        private readonly TranslationString _noRevisionFoundError = new("No revision found.");
        private readonly TranslationString _baseForCompareNotSelectedError = new("Base commit for compare is not selected.");
        private readonly TranslationString _strLoading = new("Loading");
        private readonly TranslationString _rebaseConfirmTitle = new("Rebase Confirmation");
        private readonly TranslationString _rebaseBranch = new("Rebase branch.");
        private readonly TranslationString _rebaseBranchInteractive = new("Rebase branch interactively.");
        private readonly TranslationString _areYouSureRebase = new("Are you sure you want to rebase? This action will rewrite commit history.");
        private readonly TranslationString _dontShowAgain = new("Don't show me this message again.");
        private readonly TranslationString _noMergeBaseCommit = new("There is no common ancestor for the selected commits.");
        private readonly TranslationString _invalidDiffContainsFilter = new("Filter text '{0}' not valid for \"Diff contains\" filter.");

        private readonly FilterInfo _filterInfo = new();
        private readonly NavigationHistory _navigationHistory = new();
        private readonly Control _loadingControlText;
        private readonly Control _loadingControlSpinner;
        private readonly RevisionGridToolTipProvider _toolTipProvider;
        private readonly QuickSearchProvider _quickSearchProvider;
        private readonly ParentChildNavigationHistory _parentChildNavigationHistory;
        private readonly AuthorRevisionHighlighting _authorHighlighting;
        private readonly Lazy<IndexWatcher> _indexWatcher;
        private readonly BuildServerWatcher _buildServerWatcher;
        private readonly System.Windows.Forms.Timer _selectionTimer;
        private readonly RevisionGraphColumnProvider _revisionGraphColumnProvider;
        private readonly DataGridViewColumn _maximizedColumn;
        private DataGridViewColumn? _lastVisibleResizableColumn;
        private readonly ArtificialCommitChangeCount _workTreeChangeCount = new();
        private readonly ArtificialCommitChangeCount _indexChangeCount = new();
        private readonly CancellationTokenSequence _customDiffToolsSequence = new();
        private readonly CancellationTokenSequence _refreshRevisionsSequence = new();

        /// <summary>
        /// The set of ref names that are ambiguous.
        /// Any refs present in this collection should be displayed using their full name.
        /// </summary>
        private Lazy<IReadOnlyCollection<string>>? _ambiguousRefs;

        private int _updatingFilters;

        private IDisposable? _revisionSubscription;
        private GitRevision? _baseCommitToCompare;
        private string? _rebaseOnTopOf;
        private bool _isRefreshingRevisions;
        private SuperProjectInfo? _superprojectCurrentCheckout;
        private int _latestSelectedRowIndex;

        // NOTE internal properties aren't serialised by the WinForms designer

        internal ObjectId? CurrentCheckout { get; private set; }
        internal Lazy<string> CurrentBranch { get; private set; } = new(() => "");
        internal FilterInfo CurrentFilter => _filterInfo;
        internal bool ShowUncommittedChangesIfPossible { get; set; } = true;
        internal bool ShowBuildServerInfo { get; set; }
        internal bool DoubleClickDoesNotOpenCommitInfo { get; set; }

        /// <summary>
        /// The last selected commit in the grid (with related CommitInfo in Browse).
        /// </summary>
        internal ObjectId? SelectedId { private get; set; }

        /// <summary>
        /// The first selected, the first commit in a diff.
        /// </summary>
        internal ObjectId? FirstId { private get; set; }

        internal RevisionGridMenuCommands MenuCommands { get; }

        /// <summary>
        /// The (first) seen name for commits, for FileHistory with path filters.
        /// See BuildFilter() for limitations of commits included.
        /// The property is explicitly initialized by FileHistory.
        /// </summary>
        internal Dictionary<ObjectId, string>? FilePathByObjectId { get; set; } = null;

        public RevisionGridControl()
        {
            InitializeComponent();
            openPullRequestPageStripMenuItem.AdaptImageLightness();
            renameBranchToolStripMenuItem.AdaptImageLightness();
            InitializeComplete();

            _loadingControlText = new Label
            {
                Padding = DpiUtil.Scale(new Padding(7, 5, 7, 5)),
                BorderStyle = BorderStyle.None,
                ForeColor = SystemColors.InfoText,
                BackColor = SystemColors.Info,
                Font = AppSettings.Font,
                Visible = false,
                UseMnemonic = false,
                AutoSize = true,
                Text = _strLoading.Text
            };
            Controls.Add(_loadingControlText);

            _loadingControlSpinner = new WaitSpinner
            {
                BackColor = SystemColors.Window,
                Visible = false,
                Size = DpiUtil.Scale(new Size(50, 50))
            };
            Controls.Add(_loadingControlSpinner);

            // Delay raising the SelectionChanged event for a barely noticeable period to throttle
            // rapid changes, for example by holding the down arrow key in the revision grid.
            // 75ms is longer than the default keyboard repeat rate of 15 keypresses per second.
            _selectionTimer = new System.Windows.Forms.Timer(components) { Interval = 75 };
            _selectionTimer.Tick += (_, e) =>
            {
                _selectionTimer.Stop();
                SelectionChanged?.Invoke(this, e);
            };

            _toolTipProvider = new RevisionGridToolTipProvider(_gridView);

            _quickSearchProvider = new QuickSearchProvider(_gridView, () => Module.WorkingDir);

            // Parent-child navigation can expect that SetSelectedRevision is always successful since it always uses first-parents
            _parentChildNavigationHistory = new ParentChildNavigationHistory(objectId =>
            {
                if (!SetSelectedRevision(objectId))
                {
                    MessageBoxes.RevisionFilteredInGrid(this, objectId);
                }
            });
            _authorHighlighting = new AuthorRevisionHighlighting();
            _indexWatcher = new Lazy<IndexWatcher>(() => new IndexWatcher(UICommandsSource));

            copyToClipboardToolStripMenuItem.SetRevisionFunc(() => GetSelectedRevisions());

            MenuCommands = new RevisionGridMenuCommands(this);
            ReloadHotkeys();
            HotkeysEnabled = true;

            // fill View context menu from MenuCommands
            FillMenuFromMenuCommands(MenuCommands.ViewMenuCommands, viewToolStripMenuItem);

            // fill Navigate context menu from MenuCommands
            FillMenuFromMenuCommands(MenuCommands.NavigateMenuCommands, navigateToolStripMenuItem);

            // Apply checkboxes changes also to FormBrowse main menu
            MenuCommands.TriggerMenuChanged();

            _gridView.ShowCellToolTips = false;
            _gridView.AuthorHighlighting = _authorHighlighting;

            _gridView.KeyPress += (_, e) => _quickSearchProvider.OnKeyPress(e);
            _gridView.MouseDown += OnGridViewMouseDown;
            _gridView.CellMouseDown += OnGridViewCellMouseDown;
            _gridView.MouseDoubleClick += OnGridViewDoubleClick;
            _gridView.MouseClick += OnGridViewMouseClick;
            _gridView.MouseEnter += (_, e) => _toolTipProvider.OnCellMouseEnter();
            _gridView.CellMouseMove += (_, e) => _toolTipProvider.OnCellMouseMove(e);

            // Allow to drop patch file on revision grid
            _gridView.AllowDrop = true;
            _gridView.DragEnter += OnGridViewDragEnter;
            _gridView.DragDrop += OnGridViewDragDrop;

            _buildServerWatcher = new BuildServerWatcher(this, _gridView, () => Module);

            GitRevisionSummaryBuilder gitRevisionSummaryBuilder = new();
            _revisionGraphColumnProvider = new RevisionGraphColumnProvider(this, _gridView._revisionGraph, gitRevisionSummaryBuilder);
            _gridView.AddColumn(_revisionGraphColumnProvider);
            _gridView.AddColumn(new MessageColumnProvider(this, gitRevisionSummaryBuilder));
            _gridView.AddColumn(new AvatarColumnProvider(_gridView, AvatarService.DefaultProvider, AvatarService.CacheCleaner));
            _gridView.AddColumn(new AuthorNameColumnProvider(this, _authorHighlighting));
            _gridView.AddColumn(new DateColumnProvider(this));
            _gridView.AddColumn(new CommitIdColumnProvider(this));
            _gridView.AddColumn(_buildServerWatcher.ColumnProvider);
            _maximizedColumn = _gridView.Columns.Cast<DataGridViewColumn>()
                .FirstOrDefault(column => column.Resizable == DataGridViewTriState.True && column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //// MenuCommands not disposable
                //// _superprojectCurrentCheckout not disposable
                //// _selectedObjectIds not disposable
                //// _baseCommitToCompare not disposable
                _revisionSubscription?.Dispose();
                //// _artificialStatus not disposable
                //// _ambiguousRefs not disposable
                //// _refFilterOptions not disposable
                //// _lastVisibleResizableColumn not owned
                //// _maximizedColumn not owned
                //// _revisionGraphColumnProvider not disposable
                //// _selectionTimer handled by this.components
                _buildServerWatcher?.Dispose();
                _customDiffToolsSequence.Dispose();
                _refreshRevisionsSequence.Dispose();

                if (_indexWatcher.IsValueCreated)
                {
                    _indexWatcher.Value.Dispose();
                }

                //// _authorHighlighting not disposable
                //// _parentChildNavigationHistory not disposable
                //// _quickSearchProvider not disposable
                //// _toolTipProvider  not disposable
                //// _loadingControlSync handled by this.Controls
                //// _loadingControlAsync handled by this.Controls
                //// _navigationHistory not disposable

                components?.Dispose();

                if (!Controls.Contains(_gridView))
                {
                    // Dispose _gridView explicitly since it won't be disposed automatically
                    _gridView.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Reset the controls to the supplied content.
        /// This is used to remove spinners added when loading and to replace the gridview at errors.
        /// </summary>
        /// <param name="content">The content to show.</param>
        private void SetPage(Control content)
        {
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                Control oldControl = Controls[i];
                if (!oldControl.Equals(content))
                {
                    Controls.RemoveAt(i);
                }
            }

            if (Controls.Count == 0)
            {
                Controls.Add(content);
            }
        }

        internal int DrawColumnText(DataGridViewCellPaintingEventArgs e, string text, Font font, Color color, Rectangle bounds, bool useEllipsis = true)
        {
            var flags =
                TextFormatFlags.NoPrefix |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine;

            if (useEllipsis)
            {
                flags |= TextFormatFlags.EndEllipsis;
            }

            var size = TextRenderer.MeasureText(e.Graphics, text, font, bounds.Size, flags);
            TextRenderer.DrawText(e.Graphics, text, font, bounds, color, flags);

            _toolTipProvider.SetTruncation(e.ColumnIndex, e.RowIndex, truncated: size.Width > bounds.Width);

            return size.Width;
        }

        internal IndexWatcher IndexWatcher => _indexWatcher.Value;

        internal GitRevision? LatestSelectedRevision => IsValidRevisionIndex(_latestSelectedRowIndex) ? GetRevision(_latestSelectedRowIndex) : null;

        internal bool MultiSelect
        {
            get => _gridView.MultiSelect;
            set => _gridView.MultiSelect = value;
        }

        private static void FillMenuFromMenuCommands(IEnumerable<MenuCommand> menuCommands, ToolStripDropDownItem targetItem)
        {
            foreach (var menuCommand in menuCommands)
            {
                var item = MenuCommand.CreateToolStripItem(menuCommand);

                if (item is ToolStripMenuItem menuItem)
                {
                    menuCommand.RegisterMenuItem(menuItem);
                }

                targetItem.DropDownItems.Add(item);
            }
        }

        // returns " --find-renames=..." according to app settings
        private static ArgumentString FindRenamesOpt()
        {
            return AppSettings.FollowRenamesInFileHistoryExactOnly
                ? " --find-renames=\"100%\""
                : " --find-renames";
        }

        // returns " --find-renames=... --find-copies=..." according to app settings
        private static ArgumentString FindRenamesAndCopiesOpts()
        {
            var findCopies = AppSettings.FollowRenamesInFileHistoryExactOnly
                ? " --find-copies=\"100%\""
                : " --find-copies";
            return FindRenamesOpt() + findCopies;
        }

        public void ResetAllFilters()
        {
            _filterInfo.ResetAllFilters();
        }

        public void ResetAllFiltersAndRefresh()
        {
            _filterInfo.ResetAllFilters();
            PerformRefreshRevisions();
        }

        public void SetAndApplyPathFilter(string filter)
        {
            _filterInfo.ByPathFilter = !string.IsNullOrWhiteSpace(filter);
            if (_filterInfo.ByPathFilter)
            {
                _filterInfo.PathFilter = filter;
            }

            PerformRefreshRevisions();
        }

        private void InitiateRefAction(IReadOnlyList<IGitRef>? gitRefs, Action<IGitRef> action, FormQuickGitRefSelector.Action actionLabel)
        {
            if (gitRefs is null || gitRefs.Count < 1)
            {
                return;
            }

            if (gitRefs.Count == 1)
            {
                action(gitRefs[0]);
                return;
            }

            using FormQuickGitRefSelector dlg = new();
            dlg.Init(actionLabel, gitRefs);
            dlg.Location = GetQuickItemSelectorLocation();
            if (dlg.ShowDialog(ParentForm) != DialogResult.OK || dlg.SelectedRef is null)
            {
                return;
            }

            action(dlg.SelectedRef);
        }

        public Point GetQuickItemSelectorLocation()
        {
            var rect = _gridView.GetCellDisplayRectangle(0, _latestSelectedRowIndex, true);
            return PointToScreen(new Point(rect.Right, rect.Bottom));
        }

        #region Navigation

        private void ResetNavigationHistory()
        {
            _navigationHistory.Clear();
        }

        public void NavigateBackward()
        {
            if (_navigationHistory.CanNavigateBackward)
            {
                ObjectId objectId = _navigationHistory.NavigateBackward();
                if (!SetSelectedRevision(objectId))
                {
                    MessageBoxes.RevisionFilteredInGrid(this, objectId);
                }
            }
        }

        public void NavigateForward()
        {
            if (_navigationHistory.CanNavigateForward)
            {
                ObjectId objectId = _navigationHistory.NavigateForward();
                if (!SetSelectedRevision(objectId))
                {
                    MessageBoxes.RevisionFilteredInGrid(this, objectId);
                }
            }
        }

        #endregion

        public void DisableContextMenu()
        {
            _gridView.ContextMenuStrip = null;
        }

        /// <summary>
        ///  Prevents revisions refreshes and stops <see cref="PerformRefreshRevisions"/> from executing
        ///  until <see cref="ResumeRefreshRevisions"/> is called.
        /// </summary>
        internal void SuspendRefreshRevisions() => _updatingFilters++;

        /// <summary>
        ///  Resume revisions refreshes.
        /// </summary>
        internal void ResumeRefreshRevisions()
        {
            --_updatingFilters;
            Debug.Assert(_updatingFilters >= 0, $"{nameof(ResumeRefreshRevisions)} was called without matching {nameof(SuspendRefreshRevisions)}!");
        }

        public void SetAndApplyBranchFilter(string filter)
        {
            // TODO: clean up and move all internals to FilterInfo

            _filterInfo.ShowCurrentBranchOnly = false;

            // Set filtered branches if there is a filter, handled as all branches otherwise
            string newFilter = filter?.Trim() ?? string.Empty;
            _filterInfo.ByBranchFilter = !string.IsNullOrWhiteSpace(newFilter);
            _filterInfo.BranchFilter = newFilter;
            if (_filterInfo.ByBranchFilter)
            {
                _filterInfo.ShowReflogReferences = false;
            }

            PerformRefreshRevisions();
        }

        /// <summary>
        ///  Applies a revision filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <exception cref="InvalidOperationException">Invalid 'diff contains' filter.</exception>
        public void SetAndApplyRevisionFilter(RevisionFilter filter)
        {
            bool changed = _filterInfo.Apply(filter);
            if (!changed)
            {
                return;
            }

            PerformRefreshRevisions();
        }

        public override void Refresh()
        {
            if (IsDisposed)
            {
                return;
            }

            if (_lastVisibleResizableColumn is not null)
            {
                // restore its resizable state
                _lastVisibleResizableColumn.Resizable = DataGridViewTriState.True;
            }

            _gridView.Refresh(); // columns could change their Resizable state, e.g. the BuildStatusColumnProvider

            base.Refresh();

            _toolTipProvider.Clear();

            // suppress the manual resizing of the last visible column because it will be resized when the maximized column is resized
            //// LINQ because the following did not work reliable:
            //// _lastVisibleResizableColumn = _gridView.Columns.GetLastColumn(DataGridViewElementStates.Visible | DataGridViewElementStates.Resizable, DataGridViewElementStates.None);
            _lastVisibleResizableColumn = _gridView.Columns.Cast<DataGridViewColumn>()
                .OrderBy(column => column.Index).Last(column => column.Visible && column.Resizable == DataGridViewTriState.True);
            if (_lastVisibleResizableColumn is not null)
            {
                _lastVisibleResizableColumn.Resizable = DataGridViewTriState.False;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ShowLoading();
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();
            LoadCustomDifftools();
        }

        public new void Load()
        {
            if (!DesignMode)
            {
                ReloadHotkeys();
            }

            PerformRefreshRevisions();
        }

        public void LoadCustomDifftools()
        {
            List<CustomDiffMergeTool> menus = new()
            {
                new(openCommitsWithDiffToolMenuItem, diffSelectedCommitsMenuItem_Click)
            };

            new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(Module, menus, components, isDiff: true, cancellationToken: _customDiffToolsSequence.Next());
        }

        public void CancelLoadCustomDifftools()
        {
            _customDiffToolsSequence.CancelCurrent();
        }

        /// <summary>
        /// Selects row containing revision matching <paramref name="objectId"/>.
        /// Returns whether the required revision was found and selected.
        /// </summary>
        /// <param name="objectId">Id of the revision to select.</param>
        /// <param name="toggleSelection">Toggle if the selected state for the revision.</param>
        /// <returns><c>true</c> if the required revision was found and selected, otherwise <c>false</c>.</returns>
        public bool SetSelectedRevision(ObjectId? objectId, bool toggleSelection = false, bool updateNavigationHistory = true)
        {
            _gridView.ClearToBeSelected();
            if (_gridView.TryGetRevisionIndex(objectId) is not int index || index < 0 || index >= _gridView.RowCount)
            {
                return false;
            }

            Validates.NotNull(objectId);
            SetSelectedIndex(_gridView, index, toggleSelection);
            if (updateNavigationHistory)
            {
                _navigationHistory.Push(objectId);
            }

            return true;

            static void SetSelectedIndex(RevisionDataGridView gridView, int index, bool toggleSelection = false)
            {
                try
                {
                    gridView.Select();

                    // Prevent exception when changing of repository because the grid still contains no rows.
                    if (index >= gridView.Rows.Count)
                    {
                        return;
                    }

                    bool shallSelect;
                    bool wasSelected = gridView.Rows[index].Selected;
                    if (toggleSelection)
                    {
                        // Toggle the selection, but do not deselect if it is the last one.
                        shallSelect = !wasSelected || gridView.SelectedRows.Count == 1;
                    }
                    else
                    {
                        // Single select this line.
                        shallSelect = true;
                        if (!wasSelected || gridView.SelectedRows.Count > 1)
                        {
                            gridView.ClearSelection();
                            wasSelected = false;
                        }
                    }

                    if (wasSelected && shallSelect)
                    {
                        gridView.EnsureRowVisible(index);
                        return;
                    }

                    gridView.Rows[index].Selected = shallSelect;

                    // Set the first selected row as current.
                    // Assigning _gridView.CurrentCell results in a single selection of that row.
                    // So do not set row as current but make it visible at least.
                    var selectedRows = gridView.SelectedRows;
                    var firstSelectedRow = selectedRows[0];
                    if (selectedRows.Count == 1)
                    {
                        gridView.CurrentCell = firstSelectedRow.Cells[1];
                    }

                    gridView.EnsureRowVisible(firstSelectedRow.Index);
                }
                catch (ArgumentException)
                {
                    // Ignore if selection failed. Datagridview is not threadsafe
                }

                return;
            }
        }

        public GitRevision? GetRevision(ObjectId objectId)
        {
            return _gridView.GetRevision(objectId);
        }

        private void HighlightBranch(ObjectId id)
        {
            _revisionGraphColumnProvider.RevisionGraphDrawStyle = RevisionGraphDrawStyleEnum.HighlightSelected;
            _revisionGraphColumnProvider.HighlightBranch(id);
            _gridView.Update();
        }

        public string DescribeRevision(GitRevision revision, int maxLength = 0)
        {
            var description = revision.IsArtificial
                ? string.Empty
                : revision.ObjectId.ToShortString() + ": ";

            GitRefListsForRevision gitRefListsForRevision = new(revision);

            var descriptiveRef = gitRefListsForRevision.AllBranches
                .Concat(gitRefListsForRevision.AllTags)
                .FirstOrDefault();

            description += descriptiveRef is not null
                ? GetRefUnambiguousName(descriptiveRef)
                : revision.Subject;

            if (maxLength > 0)
            {
                description = description.ShortenTo(maxLength);
            }

            return description;
        }

        /// <summary>
        /// Get the selected revisions in the grid.
        /// Note that the parents may be rewritten if a filter is applied.
        /// </summary>
        /// <param name="direction">Sort direction if set.</param>
        /// <returns>The selected revisions.</returns>
        public IReadOnlyList<GitRevision> GetSelectedRevisions(SortDirection? direction = null)
        {
            var rows = _gridView
                .SelectedRows
                .Cast<DataGridViewRow>()
                .Where(row => _gridView.RowCount > row.Index);

            if (direction.HasValue)
            {
                var d = direction.Value == SortDirection.Ascending ? 1 : -1;
                rows = rows.OrderBy(row => row.Index, (r1, r2) => d * (r1 - r2));
            }

            return rows
                .Select(row => GetRevision(row.Index))
                .WhereNotNull()
                .ToList();
        }

        private (ObjectId? firstId, GitRevision? selectedRev) getFirstAndSelected()
        {
            var revisions = GetSelectedRevisions();
            var selectedRev = revisions?.FirstOrDefault();
            var firstId = revisions is not null && revisions.Count > 1 ? revisions.LastOrDefault().ObjectId : selectedRev?.FirstParentId;

            return (firstId, selectedRev);
        }

        public IReadOnlyList<ObjectId> GetRevisionChildren(ObjectId objectId)
        {
            return _gridView.GetRevisionChildren(objectId);
        }

        private bool IsValidRevisionIndex(int index)
        {
            return index >= 0 && index < _gridView.RowCount;
        }

        private GitRevision? GetRevision(int row)
        {
            return _gridView.GetRevision(row);
        }

        /// <summary>
        /// Get the actual GitRevision from grid or use GitModule if parents may be rewritten or the commit is not in the grid.
        /// </summary>
        /// <returns>The GitRevision or null if not found</returns>
        public GitRevision? GetActualRevision(ObjectId objectId)
        {
            GitRevision revision = GetRevision(objectId);
            if (revision is not null)
            {
                return GetActualRevision(revision);
            }

            // Revision is not in grid, try get from Git
            return Module.GetRevision(objectId, shortFormat: true, loadRefs: true);
        }

        /// <summary>
        /// Get the GitRevision with the actual parents as they may be rewritten in filtered grids.
        /// </summary>
        /// <param name="revision">The revision, likely from the grid.</param>
        /// <returns>The input GitRevision if no changes and a clone with actual parents if parents are rewritten.</returns>
        public GitRevision GetActualRevision(GitRevision revision)
        {
            // Index commits must have HEAD as parent already
            if (ParentsAreRewritten && !revision.IsArtificial)
            {
                // Grid is filtered and revision may have incorrect parents
                revision = revision.Clone();
                revision.ParentIds = Module.GetParents(revision.ObjectId).ToList();
            }

            return revision;
        }

        public override bool ProcessHotkey(Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.A))
            {
                if (FindForm() is GitExtensionsFormBase form)
                {
                    form.ProcessHotkey(keyData);
                }

                return true; // never select all revisions
            }

            return base.ProcessHotkey(keyData);
        }

        public void ReloadHotkeys()
        {
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            MenuCommands.CreateOrUpdateMenuCommands();
        }

        public void ReloadTranslation()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        /// <summary>
        /// Show spinner (in the synchronous part of loading revisions
        /// and creating the control), or the "Loading..." text when first revision is
        /// handled and the grid is being updated.
        /// Note that these controls are removed by SetPage() when the grid is loaded.
        /// </summary>
        /// <param name="showSpinner">Show the spinner or the text controls.</param>
        private void ShowLoading(bool showSpinner = true)
        {
            _loadingControlSpinner.Visible = showSpinner;
            _loadingControlSpinner.Left = (ClientSize.Width - _loadingControlSpinner.Width) / 2;
            _loadingControlSpinner.Top = (ClientSize.Height - _loadingControlSpinner.Height) / 2;

            _loadingControlText.Visible = !showSpinner;
            _loadingControlText.Left = ClientSize.Width - _loadingControlSpinner.Width - SystemInformation.VerticalScrollBarWidth;

            _loadingControlSpinner.BringToFront();
            _loadingControlText.BringToFront();
        }

        /// <summary>
        ///  Indicates whether the revision grid can be refreshed, i.e. it is not currently being refreshed
        ///  or it is not in a middle of reconfiguration process guarded by <see cref="SuspendRefreshRevisions"/>
        ///  and <see cref="ResumeRefreshRevisions"/>.
        /// </summary>
        private bool CanRefresh => !_isRefreshingRevisions && _updatingFilters == 0;

        #region PerformRefreshRevisions

        /// <summary>
        ///  Queries git for the new set of revisions and refreshes the grid.
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <param name="forceRefresh">Refresh may be required as references may be changed.</param>
        public void PerformRefreshRevisions(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs = null, bool forceRefresh = false)
        {
            ThreadHelper.AssertOnUIThread();

            if (!CanRefresh)
            {
                Trace.WriteLine("Ignoring refresh as RefreshRevisions() is already running.");
                return;
            }

            GitModule capturedModule = Module;

            // Reset the "cache" for current branch
            CurrentBranch = new(() => Module.IsValidGitWorkingDir()
                      ? Module.GetSelectedBranch(setDefaultIfEmpty: false)
                      : "");

            // Revision info is read in three parallel steps:
            // 1. Read current commit, refs, prepare grid etc.
            // 2. Read stashes (if enabled).
            // 3. Read all revisions with git-log.
            //    Git will provide log information when available (so slower to first revision if sorting).
            // These operations can take a long time and is done async in parallel.
            // Step 3. requires that the information in 1 and 2 is available when adding revisions,
            // and is therefore protected with a semaphore.
            SemaphoreSlim semaphoreUpdateGrid = new(initialCount: 0, maxCount: 2);
            CancellationToken cancellationToken = _refreshRevisionsSequence.Next();
            _isRefreshingRevisions = true;

            ILookup<ObjectId, IGitRef> refsByObjectId = null;
            bool firstRevisionReceived = false;
            bool headIsHandled = false;
            Dictionary<ObjectId, GitRevision> stashesById = null;
            Dictionary<ObjectId, GitRevision> untrackedByStashId = null;
            ILookup<ObjectId, GitRevision> stashesByParentId = null;

            // getRefs (refreshing from Browse) is Lazy already, but not from RevGrid (updating filters etc)
            Lazy<IReadOnlyList<IGitRef>> getUnfilteredRefs = new(() => (getRefs ?? capturedModule.GetRefs)(RefsFilter.NoFilter));
            _ambiguousRefs = new(() => GitRef.GetAmbiguousRefNames(getUnfilteredRefs.Value));

            // Get the "main" stash commit, including the reflog selector
            Lazy<IReadOnlyCollection<GitRevision>> getStashRevs = new(() =>
                !AppSettings.ShowStashes
                ? Array.Empty<GitRevision>()
                : new RevisionReader(capturedModule, hasReflogSelector: true).GetStashes(cancellationToken));

            try
            {
                _revisionGraphColumnProvider.RevisionGraphDrawStyle = RevisionGraphDrawStyleEnum.DrawNonRelativesGray;

                // Apply checkboxes changes also to FormBrowse main menu
                MenuCommands.TriggerMenuChanged();

                FilterChanged?.Invoke(this, new FilterChangedEventArgs(_filterInfo));

                _buildServerWatcher.CancelBuildStatusFetchOperation();

                base.Refresh();

                _superprojectCurrentCheckout = null;
                Subject<GitRevision> observeRevisions = new();
                _revisionSubscription?.Dispose();
                _revisionSubscription = observeRevisions
                    .ObserveOn(ThreadPoolScheduler.Instance)
                    .Subscribe(OnRevisionRead, OnRevisionReaderError, OnRevisionReadCompleted);

                IReadOnlyList<ObjectId>? currentlySelectedObjectIds = _gridView.SelectedObjectIds;
                _gridView.SuspendLayout();
                _gridView.SelectionChanged -= OnGridViewSelectionChanged;
                _gridView.ClearSelection();
                _gridView.Clear();
                _gridView.Enabled = true;
                _gridView.Focus();
                _gridView.SelectionChanged += OnGridViewSelectionChanged;
                _gridView.MarkAsDataLoading();

                // Add the spinner controls, removed by SetPage()
                Controls.Add(_loadingControlSpinner);
                Controls.Add(_loadingControlText);
                ShowLoading();
                _gridView.ResumeLayout();

                IndexWatcher.Reset();

                // Selected is null: Update to trigger reset the tabs etc.
                _selectionTimer.Stop();
                _selectionTimer.Start();

                cancellationToken.ThrowIfCancellationRequested();

                // Evaluate GitRefs and current commit
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await TaskScheduler.Default;
                    cancellationToken.ThrowIfCancellationRequested();

                    IGitRef? headRef = !string.IsNullOrEmpty(CurrentBranch.Value)
                        ? getUnfilteredRefs.Value.FirstOrDefault(i => i.CompleteName == $"{GitRefName.RefsHeadsPrefix}{CurrentBranch.Value}")
                        : null;
                    ObjectId? newCurrentCheckout = headRef?.ObjectId ?? capturedModule.GetCurrentCheckout();

                    // If the current checkout (HEAD) is changed, don't get the currently selected rows,
                    // select the new current checkout instead.
                    if (newCurrentCheckout != CurrentCheckout && newCurrentCheckout is not null)
                    {
                        currentlySelectedObjectIds = new List<ObjectId> { newCurrentCheckout };
                    }

                    CurrentCheckout = newCurrentCheckout;

                    // Exclude the 'stash' ref, it is specially handled when stashes are shown
                    refsByObjectId = (AppSettings.ShowStashes
                        ? getUnfilteredRefs.Value.Where(r => r.CompleteName != GitRefName.RefsStashPrefix)
                        : getUnfilteredRefs.Value)
                        .ToLookup(gitRef => gitRef.ObjectId);
                    ResetNavigationHistory();
                    UpdateSelectedRef(capturedModule, getUnfilteredRefs.Value, headRef);
                    _gridView.ToBeSelectedObjectIds = GetToBeSelectedRevisions(newCurrentCheckout, currentlySelectedObjectIds);

                    _gridView._revisionGraph.OnlyFirstParent = _filterInfo.ShowOnlyFirstParent;
                    _gridView._revisionGraph.HeadId = CurrentCheckout;

                    // Allow add revisions to the grid
                    semaphoreUpdateGrid.Release();

                    _superprojectCurrentCheckout = await GetSuperprojectCheckoutAsync(capturedModule, noLocks: true);
                    if (firstRevisionReceived && _superprojectCurrentCheckout is not null)
                    {
                        // Revision may be displayed already, explicit refresh needed
                        await this.SwitchToMainThreadAsync(cancellationToken);
                        Refresh();
                    }
                }).FileAndForget();

                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    if (!AppSettings.ShowStashes)
                    {
                        semaphoreUpdateGrid.Release();
                        return;
                    }

                    await TaskScheduler.Default;

                    stashesById = getStashRevs.Value.ToDictionary(r => r.ObjectId);

                    // Git stores stashes in 2 or 3 commits. The (first) "stash" commit is listed by git-stash-list,
                    // does not include untracked files, may be stored in the third "untracked" commit.
                    // The second "index" commit are ignored (can be seen with reflog).
                    // Git creates the "untracked" commit also if there are no changes, these are filtered (adds marginal time to getting revisions).
                    // This command to list "untracked" commits is quite slow, why max number of "stash" commits
                    // to evaluate for untracked files is limited.
                    if (AppSettings.ShowReflogReferences)
                    {
                        // the "untracked" commits are already shown in the grid
                        semaphoreUpdateGrid.Release();
                        return;
                    }

                    // "stash" commits to insert (before regular commit)
                    stashesByParentId = getStashRevs.Value.ToLookup(r => r.FirstParentId);

                    // "untracked" commits to insert (parent to "stash" commits)
                    Dictionary<ObjectId, ObjectId> untrackedChildIds = getStashRevs.Value.Where(stash => stash.ParentIds.Count >= 3)
                        .Take(AppSettings.MaxStashesWithUntrackedFiles)
                        .ToDictionary(stash => stash.ParentIds[2], stash => stash.ObjectId);
                    IReadOnlyCollection<GitRevision> untrackedRevs = new RevisionReader(capturedModule, hasReflogSelector: false)
                        .GetRevisionsFromList(untrackedChildIds.Keys.ToList(), cancellationToken);
                    untrackedByStashId = untrackedRevs.ToDictionary(r => untrackedChildIds[r.ObjectId]);

                    // Remove parents not included ("index" and empty "untracked" commits).
                    foreach (GitRevision stash in getStashRevs.Value)
                    {
                        if (!untrackedByStashId.ContainsKey(stash.ObjectId))
                        {
                            stash.ParentIds = new List<ObjectId>() { stash.FirstParentId };
                        }
                        else
                        {
                            stash.ParentIds = new List<ObjectId>() { stash.FirstParentId, stash.ParentIds[2] };
                        }
                    }

                    // Allow add revisions to the grid
                    semaphoreUpdateGrid.Release();
                }).FileAndForget();

                // Get info about all Git commits, update the grid
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await TaskScheduler.Default;

                    RevisionReader reader = new(capturedModule, hasReflogSelector: false);
                    string pathFilter = BuildPathFilter(_filterInfo.PathFilter);
                    ParentsAreRewritten = _filterInfo.HasRevisionFilter;

                    cancellationToken.ThrowIfCancellationRequested();
                    reader.GetLog(
                        observeRevisions,
                        _filterInfo.GetRevisionFilter(CurrentBranch),
                        pathFilter,
                        cancellationToken);
                }).FileAndForget(
                    ex =>
                    {
                        observeRevisions.OnError(ex);
                        return false;
                    });

                // Initiate update side panel
                RevisionsLoading?.Invoke(this, new RevisionLoadEventArgs(this, UICommands, getUnfilteredRefs, getStashRevs, forceRefresh));
            }
            catch
            {
                SetPage(new ErrorControl());
                throw;
            }

            return;

            static void UpdateSelectedRef(GitModule module, IReadOnlyList<IGitRef> gitRefs, IGitRef? selectedRef)
            {
                if (selectedRef is null)
                {
                    return;
                }

                selectedRef.IsSelected = true;

                var localConfigFile = module.LocalConfigFile;
                var selectedRemote = selectedRef.GetTrackingRemote(localConfigFile);
                var selectedMerge = selectedRef.GetMergeWith(localConfigFile);
                var selectedHeadMergeSource = gitRefs.FirstOrDefault(
                    gitRef => gitRef.IsRemote
                         && selectedRemote == gitRef.Remote
                         && selectedMerge == gitRef.LocalName);

                if (selectedHeadMergeSource is not null)
                {
                    selectedHeadMergeSource.IsSelectedHeadMergeSource = true;
                }
            }

            string BuildPathFilter(string? path)
            {
                FilePathByObjectId?.Clear();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return "";
                }

                // Manual arguments must be quoted if needed (internal paths are quoted)
                // except for simple arguments without any quotes or spaces
                path = path.Trim();
                bool multpleArgs = false;
                if (!path.Any(c => c == '"') && !path.Any(c => c == '\''))
                {
                    if (!path.Any(c => c == ' '))
                    {
                        path = path.Quote();
                    }
                    else
                    {
                        multpleArgs = true;
                    }
                }
                else if (path.Count(c => c == '"') + path.Count(c => c == '\'') > 2)
                {
                    // Basic detection of multiple quoted strings (let the Git command fail for more advanced usage)
                    multpleArgs = true;
                }

                if (!AppSettings.FollowRenamesInFileHistory

                    // The command line can be very long for folders, just ignore.
                    || path.EndsWith("/")
                    || path.EndsWith("/\"")

                    // --follow only accepts exactly one argument, error for all other
                    || multpleArgs)
                {
                    return path;
                }

                // git log --follow is not working as expected (see  https://stackoverflow.com/questions/46487476/git-log-follow-graph-skips-commits)
                //
                // But we can take a more complicated path to get reasonable results:
                //  1. use git log --follow to get all previous filenames of the file we are interested in
                //  2. use git log "list of files names" to get the history graph

                const string startOfObjectId = "????";
                GitArgumentBuilder args = new("log")
                {
                    // --name-only will list each filename on a separate line, ending with an empty line
                    // Find start of a new commit with a sequence impossible in a filename
                    $"--format=\"{startOfObjectId}%H\"",
                    "--name-only",
                    "--follow",
                    FindRenamesAndCopiesOpts(),
                    "--",
                    path
                };

                HashSet<string?> setOfFileNames = new();
                ExecutionResult result = Module.GitExecutable.Execute(args, outputEncoding: GitModule.LosslessEncoding, throwOnErrorExit: false);
                var lines = result.StandardOutput.LazySplit('\n');

                // TODO Check the exit code and warn the user that rename detection could not be done.

                ObjectId currentObjectId = null;
                foreach (var line in lines.Select(GitModule.ReEncodeFileNameFromLossless))
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        // empty line after sha
                        continue;
                    }

                    if (line.StartsWith(startOfObjectId))
                    {
                        if (line.Length < ObjectId.Sha1CharCount + startOfObjectId.Length
                            || !ObjectId.TryParse(line, offset: startOfObjectId.Length, out currentObjectId))
                        {
                            // Parse error, ignore
                            currentObjectId = null;
                        }

                        continue;
                    }

                    if (currentObjectId == null)
                    {
                        // Parsing has failed, ignore
                        continue;
                    }

                    // Add only the first file to the dictionary
                    FilePathByObjectId?.TryAdd(currentObjectId, line);
                    setOfFileNames.Add(line);
                }

                // Add path in case of no matches so result is never empty
                // This also occurs if Git detects more than one path argument
                return setOfFileNames.Count == 0
                    ? path
                    : string.Join("", setOfFileNames.Select(s => @$" ""{s}"""));
            }

            void OnRevisionRead(GitRevision revision)
            {
                if (!firstRevisionReceived)
                {
                    // Wait for refs,CurrentCheckout and stashes as second step
                    this.InvokeAsync(() => { ShowLoading(showSpinner: false); }).FileAndForget();
                    semaphoreUpdateGrid.Wait(cancellationToken);
                    semaphoreUpdateGrid.Wait(cancellationToken);
                    firstRevisionReceived = true;
                }

                if (stashesById is not null)
                {
                    if (stashesById.TryGetValue(revision.ObjectId, out GitRevision gridStash))
                    {
                        revision.ReflogSelector = gridStash.ReflogSelector;

                        // Do not add this again (when the main commit is handled)
                        stashesById.Remove(revision.ObjectId);
                    }
                    else if (stashesByParentId?.Contains(revision.ObjectId) is true)
                    {
                        foreach (GitRevision stash in stashesByParentId[revision.ObjectId])
                        {
                            // Add if not already added (reflogs etc list before parent commit)
                            if (stashesById.ContainsKey(stash.ObjectId))
                            {
                                _gridView.Add(stash);
                                if (untrackedByStashId.TryGetValue(stash.ObjectId, out GitRevision untracked))
                                {
                                    _gridView.Add(untracked);
                                }
                            }
                        }
                    }
                }

                // Look up any refs associated with this revision
                revision.Refs = refsByObjectId[revision.ObjectId].AsReadOnlyList();

                if (!headIsHandled && (revision.ObjectId.Equals(CurrentCheckout) || CurrentCheckout is null))
                {
                    // Insert artificial worktree/index just before HEAD (CurrentCheckout)
                    // If grid is filtered and HEAD not visible, insert in OnRevisionReadCompleted()
                    headIsHandled = true;
                    AddArtificialRevisions();
                }

                _gridView.Add(revision);

                return;
            }

            bool ShowArtificialRevisions()
            {
                return ShowUncommittedChangesIfPossible
                    && AppSettings.RevisionGraphShowArtificialCommits
                    && !Module.IsBareRepository();
            }

            bool AddArtificialRevisions(IEnumerable<ObjectId> headParents = null)
            {
                if (!ShowArtificialRevisions())
                {
                    return false;
                }

                bool insertWithMatch = headParents is not null;
                var userName = Module.GetEffectiveSetting(SettingKeyString.UserName);
                var userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail);

                // Add working directory as an artificial commit
                GitRevision workTreeRev = new(ObjectId.WorkTreeId)
                {
                    Author = userName,
                    AuthorUnixTime = 0,
                    AuthorEmail = userEmail,
                    Committer = userName,
                    CommitUnixTime = 0,
                    CommitterEmail = userEmail,
                    Subject = ResourceManager.TranslatedStrings.Workspace,
                    ParentIds = new[] { ObjectId.IndexId },
                    HasNotes = true
                };
                _gridView.Add(workTreeRev, insertWithMatch: insertWithMatch, insertRange: 2, parents: headParents);

                // Add index as an artificial commit
                GitRevision indexRev = new(ObjectId.IndexId)
                {
                    Author = userName,
                    AuthorUnixTime = 0,
                    AuthorEmail = userEmail,
                    Committer = userName,
                    CommitUnixTime = 0,
                    CommitterEmail = userEmail,
                    Subject = ResourceManager.TranslatedStrings.Index,
                    ParentIds = CurrentCheckout is null ? null : new[] { CurrentCheckout },
                    HasNotes = true
                };

                // headParents is not needed for Index, already handled by WorkTree insertion
                _gridView.Add(indexRev, insertWithMatch: insertWithMatch, insertRange: 0, parents: null);
                return true;
            }

            void OnRevisionReaderError(Exception exception)
            {
                _refreshRevisionsSequence.CancelCurrent();

                _gridView.MarkAsDataLoadingComplete();
                this.InvokeAsync(() => SetPage(new ErrorControl()))
                    .FileAndForget();
                _isRefreshingRevisions = false;

                // Rethrow the exception on the UI thread
                this.InvokeAsync(() => throw exception)
                    .FileAndForget();
            }

            void OnRevisionReadCompleted()
            {
                if (!firstRevisionReceived && !FilterIsApplied())
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        await TaskScheduler.Default;

                        // No revisions at all received without any filter
                        await semaphoreUpdateGrid.WaitAsync(cancellationToken);

                        bool showArtificial = AddArtificialRevisions();
                        _gridView.LoadingCompleted();

                        await this.SwitchToMainThreadAsync(cancellationToken);
                        if (showArtificial)
                        {
                            // There is a context to show something
                            SetPage(_gridView);
                        }
                        else
                        {
                            SetPage(new EmptyRepoControl(Module.IsBareRepository()));
                        }

                        _isRefreshingRevisions = false;
                        RevisionsLoaded?.Invoke(this, new RevisionLoadEventArgs(this, UICommands, getUnfilteredRefs, getStashRevs, forceRefresh));
                    }).FileAndForget();
                    return;
                }

                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await TaskScheduler.Default;
                    if (!firstRevisionReceived)
                    {
                        await semaphoreUpdateGrid.WaitAsync(cancellationToken);
                    }

                    IEnumerable<ObjectId> headParents = null;
                    bool refresh = false;
                    if (!headIsHandled && ShowArtificialRevisions())
                    {
                        if (CurrentCheckout is not null)
                        {
                            // Not found, so search for its parents
                            headParents = TryGetParents(Module, _filterInfo, CurrentCheckout);
                        }

                        // If parents are rewritten HEAD may not be included
                        // Insert the artificial commits where relevant if possible, otherwise first
                        refresh = AddArtificialRevisions(headParents);
                    }

                    // All revisions are loaded (but maybe not yet the grid)
                    if (!_gridView.PendingToBeSelected &&

                        // objectIds that were not selected after revisions were loaded
                        _gridView.ToBeSelectedObjectIds.Count > 0)
                    {
                        ObjectId notSelectedId = _gridView.ToBeSelectedObjectIds[0];
                        IEnumerable<ObjectId> parents = null;

                        // Try to reuse headParents for parents to the commit to be selected
                        if (headParents is not null && notSelectedId == CurrentCheckout)
                        {
                            parents = headParents;
                        }
                        else if (headParents is not null && headParents.ToList().IndexOf(notSelectedId) is int index && index >= 0)
                        {
                            parents = headParents.Skip(index + 1).ToList();
                        }
                        else if (!notSelectedId.IsArtificial)
                        {
                            // Ignore if the not selected is artificial, it is likely that the settings was changed
                            parents = TryGetParents(Module, _filterInfo, notSelectedId);
                        }

                        // Try to select the first of the parents
                        _gridView.SetToBeSelectedFromParents(parents);
                    }

                    _gridView.LoadingCompleted();
                    await this.SwitchToMainThreadAsync(cancellationToken);
                    if (refresh)
                    {
                        _gridView.Refresh();
                    }

                    SetPage(_gridView);
                    _isRefreshingRevisions = false;
                    RevisionsLoaded?.Invoke(this, new RevisionLoadEventArgs(this, UICommands, getUnfilteredRefs, getStashRevs, forceRefresh));
                    HighlightRevisionsByAuthor(GetSelectedRevisions());

                    await TaskScheduler.Default;

                    // optimize for no notes at all in repo
                    // Improvement: set .HasNotes for commits not included in git-notes
                    bool hasAnyNotes = AppSettings.ShowGitNotes && getUnfilteredRefs.Value.Any(i => i.CompleteName == GitRefName.RefsNotesPrefix);
                    if (!hasAnyNotes)
                    {
                        // No notes at all in this repo
                        _gridView._revisionGraph.SetHasNotesForRevisions();
                    }

                    if (ShowBuildServerInfo)
                    {
                        await _buildServerWatcher.LaunchBuildServerInfoFetchOperationAsync();
                    }
                }).FileAndForget();
            }

            static async Task<SuperProjectInfo?> GetSuperprojectCheckoutAsync(GitModule gitModule, bool noLocks = false)
            {
                if (gitModule.SuperprojectModule is null)
                {
                    return null;
                }

                SuperProjectInfo spi = new();
                var (code, commit) = await gitModule.GetSuperprojectCurrentCheckoutAsync().ConfigureAwait(false);
                if (code == 'U')
                {
                    // return local and remote hashes
                    var array = await gitModule.SuperprojectModule.GetConflictAsync(gitModule.SubmodulePath)
                        .ConfigureAwaitRunInline();
                    spi.ConflictBase = array.Base.ObjectId;
                    spi.ConflictLocal = array.Local.ObjectId;
                    spi.ConflictRemote = array.Remote.ObjectId;
                }
                else
                {
                    spi.CurrentCommit = commit;
                }

                var refs = await gitModule.SuperprojectModule.GetSubmoduleItemsForEachRefAsync(gitModule.SubmodulePath, noLocks: noLocks);

                if (refs is not null)
                {
                    spi.Refs = refs
                        .Where(a => a.Value is not null && a.Value.ObjectId is not null)
                        .GroupBy(a => a.Value!.ObjectId!)
                        .ToDictionary(gr => gr.Key, gr => gr.Select(a => a.Key).AsReadOnlyList());
                }

                return spi;
            }

#pragma warning disable CS1587 // XML comment is not placed on a valid language element
            /// <summary>
            /// Select initial revision(s) in the grid.
            /// Get the revision(s) currently selected in the grid, to be selected at next refresh.
            /// If SelectedId is set, select that revision instead.
            /// The SelectedId is the last selected commit in the grid (with related CommitInfo in Browse).
            /// The FirstId is first selected, the first commit in a diff.
            /// </summary>
            IReadOnlyList<ObjectId>? GetToBeSelectedRevisions(ObjectId? currentCheckout, IReadOnlyList<ObjectId>? currentlySelectedObjectIds)
#pragma warning restore CS1587 // XML comment is not placed on a valid language element
            {
                if (SelectedId is not null)
                {
                    IReadOnlyList<ObjectId>? toBeSelectedObjectIds;
                    if (FirstId is not null)
                    {
                        toBeSelectedObjectIds = new ObjectId[] { FirstId, SelectedId };
                        FirstId = null;
                    }
                    else
                    {
                        toBeSelectedObjectIds = new ObjectId[] { SelectedId };
                    }

                    SelectedId = null;
                    return toBeSelectedObjectIds;
                }

                if (currentlySelectedObjectIds is null || currentlySelectedObjectIds.Count == 0)
                {
                    return currentCheckout is null ? Array.Empty<ObjectId>() : new ObjectId[] { currentCheckout };
                }

                return currentlySelectedObjectIds;
            }

            static IEnumerable<ObjectId> TryGetParents(GitModule module, FilterInfo filterInfo, ObjectId objectId)
            {
                GitArgumentBuilder args = new("rev-list")
                {
                    { filterInfo.HasCommitsLimit, $"--max-count={filterInfo.CommitsLimit}" },
                    objectId
                };

                ExecutionResult result = module.GitExecutable.Execute(args, throwOnErrorExit: false);
                foreach (var line in result.StandardOutput.LazySplit('\n'))
                {
                    if (ObjectId.TryParse(line, out var parentId))
                    {
                        yield return parentId;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// The parents for commits are replaced with the parent in the graph (as all commits may not be included)
        /// See https://git-scm.com/docs/git-log#Documentation/git-log.txt---parents
        /// </summary>
        private bool ParentsAreRewritten { get; set; } = false;

        internal bool FilterIsApplied()
        {
            return _filterInfo.HasFilter;
        }

        #region Graph event handlers

        private void OnGridViewSelectionChanged(object sender, EventArgs e)
        {
            _parentChildNavigationHistory.RevisionsSelectionChanged();

            if (_gridView.SelectedRows.Count > 0)
            {
                _latestSelectedRowIndex = _gridView.SelectedRows[0].Index;
            }

            _selectionTimer.Stop();
            _selectionTimer.Start();

            var (first, selected) = getFirstAndSelected();

            compareToWorkingDirectoryMenuItem.Enabled = selected is not null && selected.ObjectId != ObjectId.WorkTreeId;
            compareWithCurrentBranchToolStripMenuItem.Enabled = !string.IsNullOrWhiteSpace(CurrentBranch.Value);
            compareSelectedCommitsMenuItem.Enabled = first is not null && selected is not null;
            openCommitsWithDiffToolMenuItem.Enabled = first is not null && selected is not null;

            var selectedRevisions = GetSelectedRevisions();
            HighlightRevisionsByAuthor(selectedRevisions);

            if (selectedRevisions.Count == 1 && selected is not null)
            {
                _navigationHistory.Push(selected.ObjectId);
            }
        }

        private void HighlightRevisionsByAuthor(in IReadOnlyList<GitRevision> selectedRevisions)
        {
            if (Parent is not null &&
                !_gridView.UpdatingVisibleRows &&
                _authorHighlighting.ProcessRevisionSelectionChange(Module, selectedRevisions))
            {
                Refresh();
            }
        }

        private void RenameRef()
        {
            GitRevision? selectedRevision = LatestSelectedRevision;
            if (selectedRevision is null)
            {
                return;
            }

            InitiateRefAction(
                new GitRefListsForRevision(selectedRevision).GetRenameableLocalBranches(),
                gitRef => UICommands.StartRenameDialog(ParentForm, gitRef.Name),
                FormQuickGitRefSelector.Action.Rename);
        }

        private void DeleteRef()
        {
            GitRevision? selectedRevision = LatestSelectedRevision;
            if (selectedRevision is null)
            {
                return;
            }

            InitiateRefAction(
                new GitRefListsForRevision(selectedRevision).GetDeletableRefs(CurrentBranch.Value),
                gitRef =>
                {
                    if (gitRef.IsTag)
                    {
                        UICommands.StartDeleteTagDialog(ParentForm, gitRef.Name);
                    }
                    else if (gitRef.IsRemote)
                    {
                        UICommands.StartDeleteRemoteBranchDialog(ParentForm, gitRef.Name);
                    }
                    else
                    {
                        UICommands.StartDeleteBranchDialog(ParentForm, gitRef.Name);
                    }
                },
                FormQuickGitRefSelector.Action.Delete);
        }

        private void OnGridViewMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.XButton1: NavigateBackward(); break;
                case MouseButtons.XButton2: NavigateForward(); break;
                case MouseButtons.Left when _maximizedColumn is not null && _lastVisibleResizableColumn is not null:
                    // make resizing of the maximized column work and restore the settings afterwards
                    _gridView.MouseCaptureChanged += OnGridViewMouseCaptureChanged;
                    _maximizedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // None must be set before Fill
                    _lastVisibleResizableColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    break;

                    void OnGridViewMouseCaptureChanged(object ignoredSender, EventArgs ignoredArgs)
                    {
                        _gridView.MouseCaptureChanged -= OnGridViewMouseCaptureChanged;
                        _lastVisibleResizableColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // None must be set before Fill
                        _maximizedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
            }
        }

        internal bool TryGetSuperProjectInfo([NotNullWhen(returnValue: true)] out SuperProjectInfo? spi)
        {
            // If _superprojectCurrentCheckout is not yet calculated when the grid is shown,
            // a separate Refresh() will be invoked to update the row later.
            spi = _superprojectCurrentCheckout;
            return spi is not null;
        }

        private void OnGridViewDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            DoubleClickRevision?.Invoke(this, new DoubleClickRevisionEventArgs(GetSelectedRevisions().FirstOrDefault()));

            if (!DoubleClickDoesNotOpenCommitInfo)
            {
                ViewSelectedRevisions();
            }
        }

        private void OnGridViewMouseClick(object sender, MouseEventArgs e)
        {
            var hti = _gridView.HitTest(e.X, e.Y);
            _latestSelectedRowIndex = hti.RowIndex;

            if (Control.ModifierKeys.HasFlag(Keys.Alt))
            {
                HighlightSelectedBranch();
            }
        }

        private void OnGridViewCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Right)
                {
                    return;
                }

                if (_latestSelectedRowIndex == e.RowIndex
                    && _latestSelectedRowIndex < _gridView.Rows.Count
                    && _gridView.Rows[_latestSelectedRowIndex].Selected)
                {
                    return;
                }

                _latestSelectedRowIndex = e.RowIndex;
                _gridView.ClearSelection();

                if (IsValidRevisionIndex(_latestSelectedRowIndex))
                {
                    _gridView.Rows[_latestSelectedRowIndex].Selected = true;
                }
            }
            catch (Exception)
            {
                // Checks for bounds seems not enough. See https://github.com/gitextensions/gitextensions/issues/8475
            }
        }

        #endregion

        public void ViewSelectedRevisions()
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count > 0 && !selectedRevisions[0].IsArtificial)
            {
                Form ProvideForm()
                {
                    return new FormCommitDiff(UICommands, selectedRevisions[0].ObjectId);
                }

                UICommands.ShowModelessForm(ParentForm, false, null, null, ProvideForm);
            }
            else if (!selectedRevisions.Any())
            {
                UICommands.StartCompareRevisionsDialog(ParentForm);
            }
        }

        private void CreateTagToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revision = LatestSelectedRevision;

            UICommands.DoActionOnRepo(() =>
            {
                using FormCreateTag form = new(UICommands, revision?.ObjectId);
                return form.ShowDialog(ParentForm) == DialogResult.OK;
            });
        }

        private void ResetCurrentBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.DoActionOnRepo(() =>
            {
                using var form = FormResetCurrentBranch.Create(UICommands, LatestSelectedRevision);
                return form.ShowDialog(ParentForm) == DialogResult.OK;
            });
        }

        private void ResetAnotherBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.DoActionOnRepo(() =>
            {
                using var form = FormResetAnotherBranch.Create(UICommands, LatestSelectedRevision);
                return form.ShowDialog(ParentForm) == DialogResult.OK;
            });
        }

        private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revision = LatestSelectedRevision;

            UICommands.DoActionOnRepo(() =>
            {
                using FormCreateBranch form = new(UICommands, revision?.ObjectId);
                return form.ShowDialog(ParentForm) == DialogResult.OK;
            });
        }

        public void ShowReflog()
        {
            if (_filterInfo.ShowReflogReferences)
            {
                return;
            }

            // Do not unset ByBranchFilter or ShowCurrentBranchOnly,
            // see FilterInfo.ShowReflogReferences.
            _filterInfo.ShowReflogReferences = true;

            PerformRefreshRevisions();
        }

        public void ShowAllBranches()
        {
            if (_filterInfo.IsShowAllBranchesChecked)
            {
                return;
            }

            _filterInfo.ByBranchFilter = false;
            _filterInfo.ShowCurrentBranchOnly = false;
            _filterInfo.ShowReflogReferences = false;

            PerformRefreshRevisions();
        }

        public void ShowFilteredBranches()
        {
            if (_filterInfo.IsShowFilteredBranchesChecked)
            {
                return;
            }

            // Must be able to set ByBranchFilter without a filter to edit it
            _filterInfo.ByBranchFilter = true;
            _filterInfo.ShowCurrentBranchOnly = false;
            _filterInfo.ShowReflogReferences = false;

            PerformRefreshRevisions();
        }

        public void ShowCurrentBranchOnly()
        {
            if (_filterInfo.IsShowCurrentBranchOnlyChecked)
            {
                return;
            }

            _filterInfo.ByBranchFilter = false;
            _filterInfo.ShowCurrentBranchOnly = true;
            _filterInfo.ShowReflogReferences = false;

            PerformRefreshRevisions();
        }

        public void ShowRevisionFilterDialog()
        {
            using FormRevisionFilter form = new(UICommands, _filterInfo);
            if (form.ShowDialog(ParentForm) == DialogResult.OK)
            {
                PerformRefreshRevisions();
            }
        }

        private void ContextMenuOpening(object sender, CancelEventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            var inTheMiddleOfBisect = Module.InTheMiddleOfBisect();
            SetEnabled(markRevisionAsBadToolStripMenuItem, inTheMiddleOfBisect);
            SetEnabled(markRevisionAsGoodToolStripMenuItem, inTheMiddleOfBisect);
            SetEnabled(bisectSkipRevisionToolStripMenuItem, inTheMiddleOfBisect);
            SetEnabled(stopBisectToolStripMenuItem, inTheMiddleOfBisect);
            SetEnabled(bisectSeparator, inTheMiddleOfBisect);

            ContextMenuStrip deleteTagDropDown = new();
            ContextMenuStrip deleteBranchDropDown = new();
            ContextMenuStrip checkoutBranchDropDown = new();
            ContextMenuStrip mergeBranchDropDown = new();
            ContextMenuStrip renameDropDown = new();

            var revision = LatestSelectedRevision;
            GitRefListsForRevision gitRefListsForRevision = new(revision);
            _rebaseOnTopOf = null;

            foreach (var head in gitRefListsForRevision.AllTags)
            {
                AddBranchMenuItem(deleteTagDropDown, head, delegate { UICommands.StartDeleteTagDialog(ParentForm, head.Name); });

                var refUnambiguousName = GetRefUnambiguousName(head);
                var mergeItem = AddBranchMenuItem(mergeBranchDropDown, head,
                    delegate { UICommands.StartMergeBranchDialog(ParentForm, refUnambiguousName); });
                mergeItem.Tag = refUnambiguousName;
            }

            // For now there is no action that could be done on currentBranch
            string currentBranchRef = GitRefName.RefsHeadsPrefix + CurrentBranch.Value;
            var branchesWithNoIdenticalRemotes = gitRefListsForRevision.BranchesWithNoIdenticalRemotes;

            bool currentBranchPointsToRevision = false;
            foreach (var head in branchesWithNoIdenticalRemotes)
            {
                if (head.CompleteName == currentBranchRef)
                {
                    currentBranchPointsToRevision = !revision.IsArtificial;
                }
                else
                {
                    var toolStripItem = AddBranchMenuItem(mergeBranchDropDown, head,
                        delegate { UICommands.StartMergeBranchDialog(ParentForm, GetRefUnambiguousName(head)); });

                    _rebaseOnTopOf ??= (string)toolStripItem.Tag;
                }
            }

            // if there is no branch to rebase on, then allow user to rebase on selected commit
            if (_rebaseOnTopOf is null && !currentBranchPointsToRevision)
            {
                _rebaseOnTopOf = revision.Guid;
            }

            // if there is no branch to merge, then let user to merge selected commit into current branch
            if (mergeBranchDropDown.Items.Count == 0 && !currentBranchPointsToRevision)
            {
                ToolStripMenuItem toolStripItem = new(revision.Guid);
                toolStripItem.Click += delegate { UICommands.StartMergeBranchDialog(ParentForm, revision.Guid); };
                mergeBranchDropDown.Items.Add(toolStripItem);
                _rebaseOnTopOf ??= toolStripItem.Tag as string;
            }

            var allBranches = gitRefListsForRevision.AllBranches;
            bool isHeadOfCurrentBranch = false;
            bool firstRemoteBranchForCheckout = false;
            foreach (var head in allBranches)
            {
                // skip remote branches - they can not be deleted this way
                if (!head.IsRemote)
                {
                    if (head.CompleteName == currentBranchRef)
                    {
                        isHeadOfCurrentBranch = true;
                    }
                    else
                    {
                        AddBranchMenuItem(deleteBranchDropDown, head, delegate { UICommands.StartDeleteBranchDialog(ParentForm, head.Name); });
                    }

                    AddBranchMenuItem(renameDropDown, head, delegate { UICommands.StartRenameDialog(ParentForm, head.Name); });
                }

                if (head.CompleteName != currentBranchRef)
                {
                    if (!head.IsRemote)
                    {
                        firstRemoteBranchForCheckout = true;
                    }
                    else if (firstRemoteBranchForCheckout)
                    {
                        checkoutBranchDropDown.Items.Add(new ToolStripSeparator());
                        firstRemoteBranchForCheckout = false;
                    }

                    AddBranchMenuItem(checkoutBranchDropDown, head, delegate
                    {
                        if (head.IsRemote)
                        {
                            UICommands.StartCheckoutRemoteBranch(ParentForm, head.Name);
                        }
                        else
                        {
                            UICommands.StartCheckoutBranch(ParentForm, head.Name);
                        }
                    });
                }
            }

            bool firstRemoteBranchForDelete = true;
            foreach (var head in allBranches)
            {
                if (head.IsRemote)
                {
                    if (firstRemoteBranchForDelete)
                    {
                        firstRemoteBranchForDelete = false;
                        if (deleteBranchDropDown.Items.Count > 0)
                        {
                            deleteBranchDropDown.Items.Add(new ToolStripSeparator());
                        }
                    }

                    AddBranchMenuItem(deleteBranchDropDown, head, delegate { UICommands.StartDeleteRemoteBranchDialog(ParentForm, head.Name); });
                }
            }

            bool bareRepositoryOrArtificial = Module.IsBareRepository() || revision.IsArtificial;
            deleteTagToolStripMenuItem.DropDown = deleteTagDropDown;
            SetEnabled(deleteTagToolStripMenuItem, deleteTagDropDown.Items.Count > 0);

            deleteBranchToolStripMenuItem.DropDown = deleteBranchDropDown;
            SetEnabled(deleteBranchToolStripMenuItem, deleteBranchDropDown.Items.Count > 0 && !Module.IsBareRepository());
            if (isHeadOfCurrentBranch)
            {
                deleteBranchToolStripMenuItem.Visible = true;
            }

            checkoutBranchToolStripMenuItem.DropDown = checkoutBranchDropDown;
            SetEnabled(checkoutBranchToolStripMenuItem, !bareRepositoryOrArtificial && HasEnabledItem(checkoutBranchDropDown) && !Module.IsBareRepository());

            mergeBranchToolStripMenuItem.DropDown = mergeBranchDropDown;
            SetEnabled(mergeBranchToolStripMenuItem, !bareRepositoryOrArtificial && HasEnabledItem(mergeBranchDropDown) && !Module.IsBareRepository());

            SetEnabled(rebaseOnToolStripMenuItem, !bareRepositoryOrArtificial && !Module.IsBareRepository());

            renameBranchToolStripMenuItem.DropDown = renameDropDown;
            SetEnabled(renameBranchToolStripMenuItem, renameDropDown.Items.Count > 0);

            SetEnabled(checkoutRevisionToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(revertCommitToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(cherryPickCommitToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(manipulateCommitToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(amendCommitToolStripMenuItem, !bareRepositoryOrArtificial && Module.GitVersion.SupportAmendCommits);

            SetEnabled(copyToClipboardToolStripMenuItem, !revision.IsArtificial);
            SetEnabled(createNewBranchToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(resetCurrentBranchToHereToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(resetAnotherBranchToHereToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(archiveRevisionToolStripMenuItem, !revision.IsArtificial);
            SetEnabled(createTagToolStripMenuItem, !revision.IsArtificial);

            bool showStash = !bareRepositoryOrArtificial && revision.IsStash;
            SetEnabled(applyStashToolStripMenuItem, showStash);
            SetEnabled(popStashToolStripMenuItem, showStash);
            SetEnabled(dropStashToolStripMenuItem, showStash);

            SetEnabled(openBuildReportToolStripMenuItem, !string.IsNullOrWhiteSpace(revision.BuildStatus?.Url));

            SetEnabled(openPullRequestPageStripMenuItem, !string.IsNullOrWhiteSpace(revision.BuildStatus?.PullRequestUrl));

            mainContextMenu.AddUserScripts(runScriptToolStripMenuItem, ((IRunScript)this).Execute);

            UpdateSeparators();

            return;

            void SetEnabled(ToolStripItem item, bool isEnabled)
            {
                // NOTE we have to set 'enabled' in order to filter separators because
                // setting 'visible' to true sets some internal flag, yet the property still returns
                // false, presumably because the menu item is not actually yet visible on screen.
                item.Visible = isEnabled;
                item.Enabled = isEnabled;
            }

            bool HasEnabledItem(ToolStrip item)
            {
                return item.Items.Count != 0 && item.Items.Cast<ToolStripItem>().Any(i => i.Enabled);
            }

            void UpdateSeparators()
            {
                var seenItem = false;
                foreach (var item in mainContextMenu.Items.Cast<ToolStripItem>())
                {
                    if (item is ToolStripSeparator separator)
                    {
                        separator.Visible = seenItem;
                        seenItem = false;
                    }
                    else if (item.Enabled)
                    {
                        seenItem = true;
                    }
                }
            }

            ToolStripMenuItem AddBranchMenuItem(ContextMenuStrip menu, IGitRef gitRef, EventHandler action)
            {
                ToolStripMenuItem menuItem = new(gitRef.Name)
                {
                    Image = gitRef.IsRemote ? Images.BranchRemote : Images.BranchLocal
                };
                menuItem.Click += action;
                menu.Items.Add(menuItem);
                return menuItem;
            }
        }

        private string GetRefUnambiguousName(IGitRef gitRef)
        {
            return _ambiguousRefs.Value.Contains(gitRef.Name)
                ? gitRef.CompleteName
                : gitRef.Name;
        }

        private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf is null)
            {
                return;
            }

            if (AppSettings.DontConfirmRebase)
            {
                UICommands.StartRebase(ParentForm, _rebaseOnTopOf);
                return;
            }

            TaskDialogPage page = new()
            {
                Text = _areYouSureRebase.Text,
                Caption = _rebaseConfirmTitle.Text,
                Heading = _rebaseBranch.Text,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Information,
                Verification = new TaskDialogVerificationCheckBox
                {
                    Text = _dontShowAgain.Text
                },
                SizeToContent = true
            };

            TaskDialogButton result = TaskDialog.ShowDialog(Handle, page);

            if (page.Verification.Checked)
            {
                AppSettings.DontConfirmRebase = true;
            }

            if (result == TaskDialogButton.Yes)
            {
                UICommands.StartRebase(ParentForm, _rebaseOnTopOf);
            }
        }

        private void OnRebaseInteractivelyClicked(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf is null)
            {
                return;
            }

            if (AppSettings.DontConfirmRebase)
            {
                UICommands.StartInteractiveRebase(ParentForm, _rebaseOnTopOf);
                return;
            }

            TaskDialogPage page = new()
            {
                Text = _areYouSureRebase.Text,
                Caption = _rebaseConfirmTitle.Text,
                Heading = _rebaseBranchInteractive.Text,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Information,
                Verification = new TaskDialogVerificationCheckBox
                {
                    Text = _dontShowAgain.Text
                },
                SizeToContent = true
            };

            TaskDialogButton result = TaskDialog.ShowDialog(Handle, page);

            if (page.Verification.Checked)
            {
                AppSettings.DontConfirmRebase = true;
            }

            if (result == TaskDialogButton.Yes)
            {
                UICommands.StartInteractiveRebase(ParentForm, _rebaseOnTopOf);
            }
        }

        private void OnRebaseWithAdvOptionsClicked(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf is not null)
            {
                UICommands.StartRebaseDialogWithAdvOptions(ParentForm, _rebaseOnTopOf);
            }
        }

        private void CheckoutRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is not null)
            {
                UICommands.StartCheckoutRevisionDialog(ParentForm, LatestSelectedRevision.Guid);
            }
        }

        private void ArchiveRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count > 2)
            {
                MessageBox.Show(this, "Select only one or two revisions. Abort.", "Archive revision", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            GitRevision mainRevision = selectedRevisions.FirstOrDefault();
            GitRevision? diffRevision = null;
            if (selectedRevisions.Count == 2)
            {
                diffRevision = selectedRevisions.Last();
            }

            UICommands.StartArchiveDialog(ParentForm, mainRevision, diffRevision);
        }

        internal void ToggleShowAuthorDate()
        {
            AppSettings.ShowAuthorDate = !AppSettings.ShowAuthorDate;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleShowRemoteBranches()
        {
            AppSettings.ShowRemoteBranches = !AppSettings.ShowRemoteBranches;
            MenuCommands.TriggerMenuChanged();
            _gridView.Invalidate();
        }

        internal void ToggleShowArtificialCommits()
        {
            AppSettings.RevisionGraphShowArtificialCommits = !AppSettings.RevisionGraphShowArtificialCommits;
            PerformRefreshRevisions();
        }

        internal void ToggleAuthorDateSort()
        {
            AppSettings.RevisionSortOrder = AppSettings.RevisionSortOrder != RevisionSortOrder.AuthorDate ? RevisionSortOrder.AuthorDate : RevisionSortOrder.GitDefault;
            PerformRefreshRevisions();
        }

        internal void ToggleTopoOrder()
        {
            AppSettings.RevisionSortOrder = AppSettings.RevisionSortOrder != RevisionSortOrder.Topology ? RevisionSortOrder.Topology : RevisionSortOrder.GitDefault;
            PerformRefreshRevisions();
        }

        public void ToggleShowReflogReferences()
        {
            _filterInfo.ShowReflogReferences = !_filterInfo.ShowReflogReferences;
            PerformRefreshRevisions();
        }

        internal void ToggleShowStashes()
        {
            AppSettings.ShowStashes = !AppSettings.ShowStashes;
            PerformRefreshRevisions();
        }

        internal void ToggleShowSuperprojectTags()
        {
            AppSettings.ShowSuperprojectTags = !AppSettings.ShowSuperprojectTags;
            PerformRefreshRevisions();
        }

        internal void ShowSuperprojectBranches_ToolStripMenuItemClick()
        {
            AppSettings.ShowSuperprojectBranches = !AppSettings.ShowSuperprojectBranches;
            PerformRefreshRevisions();
        }

        internal void ShowSuperprojectRemoteBranches_ToolStripMenuItemClick()
        {
            AppSettings.ShowSuperprojectRemoteBranches = !AppSettings.ShowSuperprojectRemoteBranches;
            PerformRefreshRevisions();
        }

        private void RevertCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Ascending);
            foreach (var rev in revisions)
            {
                UICommands.StartRevertCommitDialog(ParentForm, rev);
            }
        }

        private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Descending);
            UICommands.StartCherryPickDialog(ParentForm, revisions);
        }

        private void ApplyStashToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StashApply(this, LatestSelectedRevision.ObjectId.ToString());
            PerformRefreshRevisions();
        }

        private void PopStashToolStripMenuItemClick(object sender, EventArgs e)
        {
            string stashName = LatestSelectedRevision.ReflogSelector;
            if (string.IsNullOrEmpty(stashName))
            {
                return;
            }

            UICommands.StashPop(this, stashName);
            PerformRefreshRevisions();
        }

        private void DropStashToolStripMenuItemClick(object sender, EventArgs e)
        {
            var stashName = LatestSelectedRevision.ReflogSelector;
            if (string.IsNullOrEmpty(stashName))
            {
                return;
            }

            using (new WaitCursorScope())
            {
                TaskDialogButton result;
                if (AppSettings.DontConfirmStashDrop)
                {
                    result = TaskDialogButton.Yes;
                }
                else
                {
                    TaskDialogPage page = new()
                    {
                        Text = TranslatedStrings.AreYouSure,
                        Caption = TranslatedStrings.StashDropConfirmTitle,
                        Heading = TranslatedStrings.CannotBeUndone,
                        Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                        Icon = TaskDialogIcon.Information,
                        Verification = new TaskDialogVerificationCheckBox
                        {
                            Text = TranslatedStrings.DontShowAgain
                        },
                        SizeToContent = true
                    };

                    result = TaskDialog.ShowDialog(Handle, page);

                    if (page.Verification.Checked)
                    {
                        AppSettings.DontConfirmStashDrop = true;
                    }
                }

                if (result == TaskDialogButton.Yes)
                {
                    UICommands.StashDrop(this, stashName);
                    PerformRefreshRevisions();
                }
            }
        }

        private void FixupCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.StartFixupCommitDialog(ParentForm, LatestSelectedRevision);
        }

        private void SquashCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.StartSquashCommitDialog(ParentForm, LatestSelectedRevision);
        }

        private void AmendCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.StartAmendCommitDialog(ParentForm, LatestSelectedRevision);
        }

        internal void ToggleShowRelativeDate(EventArgs e)
        {
            AppSettings.RelativeDate = !AppSettings.RelativeDate;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        #region Artificial commit change counters

        /// <summary>
        /// Gets the counter for artificial changes.
        /// </summary>
        /// <param name="objectId">The commit for which to get the count.</param>
        /// <returns>The count object if the commit is official and count is enabled.</returns>
        public ArtificialCommitChangeCount? GetChangeCount(ObjectId objectId)
        {
            return objectId == ObjectId.WorkTreeId
                ? _workTreeChangeCount
                : objectId == ObjectId.IndexId
                    ? _indexChangeCount
                    : null;
        }

        public void UpdateArtificialCommitCount(IReadOnlyList<GitItemStatus>? status)
        {
            // Note that the count is updated also if AppSettings.ShowGitStatusForArtificialCommits is not set
            UpdateChangeCount(ObjectId.WorkTreeId, status);
            UpdateChangeCount(ObjectId.IndexId, status);

            _gridView.Invalidate();
            return;

            void UpdateChangeCount(ObjectId objectId, IReadOnlyList<GitItemStatus>? status)
            {
                Debug.Assert(objectId == ObjectId.WorkTreeId || objectId == ObjectId.IndexId,
                    $"Unexpected Git object id {objectId}");
                var changeCount = GetChangeCount(objectId);
                Validates.NotNull(changeCount);
                StagedStatus staged = objectId == ObjectId.WorkTreeId ? StagedStatus.WorkTree : StagedStatus.Index;
                var items = status?.Where(item => item.Staged == staged).ToList();
                changeCount.Update(items);
            }
        }

        #endregion

        internal void ToggleDrawNonRelativesGray()
        {
            AppSettings.RevisionGraphDrawNonRelativesGray = !AppSettings.RevisionGraphDrawNonRelativesGray;
            MenuCommands.TriggerMenuChanged();
            _gridView.Refresh();
        }

        #region Bisect

        private void MarkRevisionAsBadToolStripMenuItemClick(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Bad);
        }

        private void MarkRevisionAsGoodToolStripMenuItemClick(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Good);
        }

        private void BisectSkipRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Skip);
        }

        private void ContinueBisect(GitBisectOption bisectOption)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            string? command = GitCommandHelpers.ContinueBisectCmd(bisectOption, LatestSelectedRevision.ObjectId);
            FormProcess.ShowDialog(ParentForm, arguments: command, Module.WorkingDir, input: null, useDialogSettings: false);
            PerformRefreshRevisions();
        }

        private void StopBisectToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(ParentForm, arguments: GitCommandHelpers.StopBisectCmd(), Module.WorkingDir, input: null, useDialogSettings: true);
            PerformRefreshRevisions();
        }

        #endregion

        internal void ToggleShowGitNotes()
        {
            AppSettings.ShowGitNotes = !AppSettings.ShowGitNotes;
            PerformRefreshRevisions();
        }

        internal void ToggleShowMergeCommits()
        {
            AppSettings.ShowMergeCommits = !AppSettings.ShowMergeCommits;
            PerformRefreshRevisions();
        }

        internal void ToggleShowCommitBodyInRevisionGrid()
        {
            AppSettings.ShowCommitBodyInRevisionGrid = !AppSettings.ShowCommitBodyInRevisionGrid;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        public void ToggleShowOnlyFirstParent()
        {
            _filterInfo.ShowOnlyFirstParent = !_filterInfo.ShowOnlyFirstParent;
            PerformRefreshRevisions();
        }

        public void ToggleFullHistory()
        {
            AppSettings.FullHistoryInFileHistory = !AppSettings.FullHistoryInFileHistory;
            PerformRefreshRevisions();
        }

        public void ToggleSimplifyMerges()
        {
            AppSettings.SimplifyMergesInFileHistory = !AppSettings.SimplifyMergesInFileHistory;
            PerformRefreshRevisions();
        }

        internal void ToggleBetweenArtificialAndHeadCommits()
        {
            GoToRef(GetIdToSelect()?.ToString(), showNoRevisionMsg: false);
            ToggledBetweenArtificialAndHeadCommits?.Invoke(this, EventArgs.Empty);
            return;

            ObjectId? GetIdToSelect()
            {
                // Try up to 3 possibilities in the circle: WorkTree -> Index -> Head -> (WorkTree).
                ObjectId? idToSelect = LatestSelectedRevision?.ObjectId;
                for (int i = 0; i < 3; ++i)
                {
                    idToSelect = GetNextIdToSelect(idToSelect);
                    if (idToSelect is null

                        // WorkTree and Index are skipped if and only if we do retrieve the ChangeCount info (only for artificial) and HasChanges returns false.
                        || (AppSettings.ShowGitStatusForArtificialCommits && (GetChangeCount(idToSelect)?.HasChanges is false)))
                    {
                        continue;
                    }

                    if (idToSelect == CurrentCheckout && AppSettings.RevisionGraphShowArtificialCommits && _gridView.GetRevision(idToSelect) is null)
                    {
                        // HEAD is not in revision grid (filtered)
                        return ObjectId.WorkTreeId;
                    }

                    return idToSelect;
                }

                return CurrentCheckout;

                ObjectId? GetNextIdToSelect(ObjectId? id)
                    => id == ObjectId.WorkTreeId ? ObjectId.IndexId
                     : id == ObjectId.IndexId ? CurrentCheckout
                     : ObjectId.WorkTreeId;
            }
        }

        #region Column visibilities

        internal void ToggleRevisionGraphColumn()
        {
            AppSettings.ShowRevisionGridGraphColumn = !AppSettings.ShowRevisionGridGraphColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleAuthorAvatarColumn()
        {
            AppSettings.ShowAuthorAvatarColumn = !AppSettings.ShowAuthorAvatarColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleAuthorNameColumn()
        {
            AppSettings.ShowAuthorNameColumn = !AppSettings.ShowAuthorNameColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleDateColumn()
        {
            AppSettings.ShowDateColumn = !AppSettings.ShowDateColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleObjectIdColumn()
        {
            AppSettings.ShowObjectIdColumn = !AppSettings.ShowObjectIdColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleBuildStatusIconColumn()
        {
            ////Module.EffectiveSettings.BuildServer.ShowBuildIconInGrid.Value = !Module.EffectiveSettings.BuildServer.ShowBuildIconInGrid.Value;
            AppSettings.ShowBuildStatusIconColumn = !AppSettings.ShowBuildStatusIconColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleBuildStatusTextColumn()
        {
            ////Module.EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.Value = !Module.EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.Value;
            AppSettings.ShowBuildStatusTextColumn = !AppSettings.ShowBuildStatusTextColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        #endregion

        internal void ToggleShowTags()
        {
            AppSettings.ShowTags = !AppSettings.ShowTags;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal CommandStatus ExecuteCommand(Command cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        private void HighlightSelectedBranch()
        {
            GitRevision revision = GetSelectedRevisions().FirstOrDefault();
            if (revision is null)
            {
                return;
            }

            HighlightBranch(revision.ObjectId);
            Refresh();
        }

        private void renameBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if (item.DropDown is not null && item.DropDown.Items.Count == 1)
            {
                item.DropDown.Items[0].PerformClick();
            }
        }

        private void deleteBranchTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if (item.DropDown is not null && item.DropDown.Items.Count == 1)
            {
                item.DropDown.Items[0].PerformClick();
            }
        }

        private void goToParentToolStripMenuItem_Click()
        {
            var r = LatestSelectedRevision;
            if (r is not null)
            {
                if (_parentChildNavigationHistory.HasPreviousParent)
                {
                    _parentChildNavigationHistory.NavigateToPreviousParent(r.ObjectId);
                }
                else if (r.HasParent)
                {
                    _parentChildNavigationHistory.NavigateToParent(r.ObjectId, r.FirstParentId);
                }
            }
        }

        private void SelectNextForkPointAsDiffBase()
        {
            IReadOnlyList<GitRevision> revisions = GetSelectedRevisions();
            if (revisions.Count == 0)
            {
                return;
            }

            GitRevision revision = revisions[revisions.Count - 1];
            while (revision.IsArtificial)
            {
                revision = GetRevision(revision.FirstParentId);
            }

            do
            {
                GitRevision prevRev = GetRevision(revision.FirstParentId);
                if (prevRev == null)
                {
                    break;
                }

                revision = prevRev;
            }
            while (!revision.Refs.Any(r => r.IsHead || r.IsRemote) && GetRevisionChildren(revision.ObjectId).Count == 1);

            SetSelectedRevision(revision.ObjectId, toggleSelection: false, updateNavigationHistory: false);
            foreach (GitRevision selectedRevision in revisions.Take(Math.Max(1, revisions.Count - 1)))
            {
                SetSelectedRevision(selectedRevision.ObjectId, toggleSelection: true, updateNavigationHistory: false);
            }
        }

        private void GoToMergeBase()
        {
            // Artificial commits are replaced with HEAD
            // If only one revision is selected, compare to HEAD
            // => Fill with HEAD to if less than two normal revisions (it is OK to compare HEAD HEAD)
            var revisions = GetSelectedRevisions().Select(i => i.ObjectId).Where(i => !i.IsArtificial).ToList();
            bool hasArtificial = GetSelectedRevisions().Any(i => i.IsArtificial);
            ObjectId? headId = Module.RevParse("HEAD");
            if (headId is null || (revisions.Count == 0 && !hasArtificial))
            {
                return;
            }

            GitArgumentBuilder args = new("merge-base")
            {
                { revisions.Count > 2 || (revisions.Count == 2 && hasArtificial), "--octopus" },
                { revisions.Count < 1, headId.ToString() },
                { revisions.Count < 2, headId.ToString() },
                revisions
            };

            var mergeBaseCommitId = UICommands.GitModule.GitExecutable.GetOutput(args).TrimEnd('\n');
            if (string.IsNullOrWhiteSpace(mergeBaseCommitId))
            {
                MessageBox.Show(_noMergeBaseCommit.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ObjectId objectId = ObjectId.Parse(mergeBaseCommitId);
            if (!SetSelectedRevision(objectId))
            {
                MessageBoxes.RevisionFilteredInGrid(this, objectId);
            }
        }

        private void goToChildToolStripMenuItem_Click()
        {
            var r = LatestSelectedRevision;
            if (r is not null)
            {
                var children = GetRevisionChildren(r.ObjectId);

                if (_parentChildNavigationHistory.HasPreviousChild)
                {
                    _parentChildNavigationHistory.NavigateToPreviousChild(r.ObjectId);
                }
                else if (children.Any())
                {
                    _parentChildNavigationHistory.NavigateToChild(r.ObjectId, children[0]);
                }
            }
        }

        public void GoToRef(string? refName, bool showNoRevisionMsg, bool toggleSelection = false)
        {
            if (string.IsNullOrEmpty(refName))
            {
                return;
            }

            if (DetachedHeadParser.TryParse(refName, out var sha1))
            {
                refName = sha1;
            }

            ObjectId? revisionId = Module.RevParse(refName);
            if (revisionId is not null)
            {
                if (!SetSelectedRevision(revisionId, toggleSelection) && showNoRevisionMsg)
                {
                    MessageBoxes.RevisionFilteredInGrid(this, revisionId);
                }
            }
            else if (showNoRevisionMsg)
            {
                MessageBox.Show(this, _noRevisionFoundError.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal void SetFilterShortcutKeys(FilterToolBar filterBar)
        {
            filterBar.SetShortcutKeys(SetShortcutString);
        }

        internal void SetShortcutKeys()
        {
            SetShortcutString(fixupCommitToolStripMenuItem, Command.CreateFixupCommit);
            SetShortcutString(squashCommitToolStripMenuItem, Command.CreateSquashCommit);
            SetShortcutString(amendCommitToolStripMenuItem, Command.CreateAmendCommit);
            SetShortcutString(selectAsBaseToolStripMenuItem, Command.SelectAsBaseToCompare);
            SetShortcutString(openCommitsWithDiffToolMenuItem, Command.OpenCommitsWithDifftool);
            SetShortcutString(compareToBaseToolStripMenuItem, Command.CompareToBase);
            SetShortcutString(compareToWorkingDirectoryMenuItem, Command.CompareToWorkingDirectory);
            SetShortcutString(compareSelectedCommitsMenuItem, Command.CompareSelectedCommits);
        }

        private void SetShortcutString(ToolStripMenuItem item, Command command)
        {
            item.ShortcutKeyDisplayString = GetShortcutKeys(command)
                .ToShortcutKeyDisplayString();
        }

        internal Keys GetShortcutKeys(Command cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        private void CompareToBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var headCommit = GetSelectedRevisions().FirstOrDefault();
            if (headCommit is null)
            {
                return;
            }

            using FormCompareToBranch form = new(UICommands, headCommit.ObjectId);
            if (form.ShowDialog(ParentForm) == DialogResult.OK)
            {
                Validates.NotNull(form.BranchName);
                var baseCommit = Module.RevParse(form.BranchName);
                Validates.NotNull(baseCommit);
                UICommands.ShowFormDiff(baseCommit, headCommit.ObjectId, form.BranchName, headCommit.Subject);
            }
        }

        private void CompareWithCurrentBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CurrentBranch.Value) || CurrentCheckout is null)
            {
                MessageBox.Show(this, "No branch is currently selected", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var baseCommit = GetSelectedRevisions().FirstOrDefault();
            if (baseCommit is null)
            {
                return;
            }

            UICommands.ShowFormDiff(baseCommit.ObjectId, CurrentCheckout, baseCommit.Subject, CurrentBranch.Value);
        }

        private void selectAsBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _baseCommitToCompare = GetSelectedRevisions().FirstOrDefault();
            compareToBaseToolStripMenuItem.Enabled = _baseCommitToCompare is not null;
        }

        private void compareToBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_baseCommitToCompare is null)
            {
                MessageBox.Show(this, _baseForCompareNotSelectedError.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var headCommit = GetSelectedRevisions().FirstOrDefault();
            if (headCommit is null)
            {
                return;
            }

            UICommands.ShowFormDiff(_baseCommitToCompare.ObjectId, headCommit.ObjectId, _baseCommitToCompare.Subject, headCommit.Subject);
        }

        private void compareToWorkingDirectoryMenuItem_Click(object sender, EventArgs e)
        {
            var baseCommit = GetSelectedRevisions().FirstOrDefault();
            if (baseCommit is null)
            {
                return;
            }

            if (baseCommit.ObjectId == ObjectId.WorkTreeId)
            {
                MessageBox.Show(this, "Cannot diff working directory to itself", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UICommands.ShowFormDiff(baseCommit.ObjectId, ObjectId.WorkTreeId, baseCommit.Subject, "Working directory");
        }

        private void compareSelectedCommitsMenuItem_Click(object sender, EventArgs e)
        {
            var (first, selected) = getFirstAndSelected();

            if (selected is not null && first is not null)
            {
                var firstRev = GetRevision(first);
                if (firstRev is not null)
                {
                    UICommands.ShowFormDiff(first, selected.ObjectId, firstRev.Subject, selected.Subject);
                }
            }
            else
            {
                MessageBox.Show(this, "You must have two commits selected to compare", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void diffSelectedCommitsMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default difftool
                item.HideDropDown();
            }

            var toolName = item?.Tag as string;
            DiffSelectedCommitsWithDifftool(toolName);
        }

        public void DiffSelectedCommitsWithDifftool(string? customTool = null)
        {
            var (first, selected) = getFirstAndSelected();
            if (selected is not null)
            {
                Module.OpenWithDifftoolDirDiff(first?.ToString(), selected.ObjectId.ToString(), customTool: customTool);
            }
        }

        private void getHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(
                UserManual.UserManual.UrlFor("modify_history", "using-autosquash-rebase-feature"));
        }

        private void openBuildReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var revision = GetSelectedRevisions().FirstOrDefault();

            if (revision is not null && !string.IsNullOrWhiteSpace(revision.BuildStatus?.Url))
            {
                OsShellUtil.OpenUrlInDefaultBrowser(revision.BuildStatus.Url);
            }
        }

        private void openPullRequestPageStripMenuItem_Click(object sender, EventArgs e)
        {
            var revision = GetSelectedRevisions().FirstOrDefault();

            if (revision is not null && !string.IsNullOrWhiteSpace(revision.BuildStatus?.PullRequestUrl))
            {
                OsShellUtil.OpenUrlInDefaultBrowser(revision.BuildStatus.PullRequestUrl);
            }
        }

        private void editCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchRebase("e");
        }

        private void rewordCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchRebase("r");
        }

        private void LaunchRebase(string command)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            string rebaseCmd = GitCommandHelpers.RebaseCmd(
                GetActualRevision(LatestSelectedRevision)?.FirstParentId?.ToString(), interactive: true, preserveMerges: false,
                autosquash: false, autoStash: true, ignoreDate: false, committerDateIsAuthorDate: false, supportRebaseMerges: Module.GitVersion.SupportRebaseMerges);

            using FormProcess formProcess = new(UICommands, arguments: rebaseCmd, Module.WorkingDir, input: null, useDialogSettings: true);
            formProcess.ProcessEnvVariables.Add("GIT_SEQUENCE_EDITOR", string.Format("sed -i -re '0,/pick/s//{0}/'", command));
            formProcess.ShowDialog(ParentForm);
            PerformRefreshRevisions();
        }

        #region Drag/drop patch files on revision grid

        private void OnGridViewDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is Array fileNameArray)
            {
                if (fileNameArray.Length > 10)
                {
                    // Some users need to be protected against themselves!
                    MessageBox.Show(this, _droppingFilesBlocked.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (object fileNameObject in fileNameArray)
                {
                    var fileName = fileNameObject as string;

                    if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Start apply patch dialog for each dropped patch file...
                        UICommands.StartApplyPatchDialog(ParentForm, fileName);
                    }
                }
            }
        }

        private static void OnGridViewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is Array fileNameArray)
            {
                foreach (object fileNameObject in fileNameArray)
                {
                    var fileName = fileNameObject as string;

                    if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Allow drop (copy, not move) patch files
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        // When a non-patch file is dragged, do not allow it
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                }
            }
        }

        #endregion

        #region Hotkey commands

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.ToggleRevisionGraph: ToggleRevisionGraphColumn(); break;
                case Command.RevisionFilter: ShowRevisionFilterDialog(); break;
                case Command.ResetRevisionFilter: ResetAllFiltersAndRefresh(); break;
                case Command.ResetRevisionPathFilter: SetAndApplyPathFilter(""); break;
                case Command.ToggleAuthorDateCommitDate: ToggleShowAuthorDate(); break;
                case Command.ToggleShowRelativeDate: ToggleShowRelativeDate(EventArgs.Empty); break;
                case Command.ToggleDrawNonRelativesGray: ToggleDrawNonRelativesGray(); break;
                case Command.ToggleShowGitNotes: ToggleShowGitNotes(); break;
                case Command.ToggleShowMergeCommits: ToggleShowMergeCommits(); break;
                case Command.ToggleShowTags: ToggleShowTags(); break;
                case Command.ShowAllBranches: ShowAllBranches(); break;
                case Command.ShowCurrentBranchOnly: ShowCurrentBranchOnly(); break;
                case Command.ShowFilteredBranches: ShowFilteredBranches(); break;
                case Command.ShowReflogReferences: ToggleShowReflogReferences(); break;
                case Command.ShowRemoteBranches: ToggleShowRemoteBranches(); break;
                case Command.ShowFirstParent: ToggleShowOnlyFirstParent(); break;
                case Command.ToggleBetweenArtificialAndHeadCommits: ToggleBetweenArtificialAndHeadCommits(); break;
                case Command.SelectCurrentRevision:
                    if (!SetSelectedRevision(CurrentCheckout))
                    {
                        MessageBoxes.RevisionFilteredInGrid(this, CurrentCheckout);
                    }

                    break;
                case Command.GoToCommit: MenuCommands.GotoCommitExecute(); break;
                case Command.GoToParent: goToParentToolStripMenuItem_Click(); break;
                case Command.SelectNextForkPointAsDiffBase: SelectNextForkPointAsDiffBase(); break;
                case Command.GoToMergeBase: GoToMergeBase(); break;
                case Command.GoToChild: goToChildToolStripMenuItem_Click(); break;
                case Command.ToggleHighlightSelectedBranch: HighlightSelectedBranch(); break;
                case Command.NextQuickSearch: _quickSearchProvider.NextResult(down: true); break;
                case Command.PrevQuickSearch: _quickSearchProvider.NextResult(down: false); break;
                case Command.NavigateBackward:
                case Command.NavigateBackward_AlternativeHotkey: NavigateBackward(); break;
                case Command.NavigateForward:
                case Command.NavigateForward_AlternativeHotkey: NavigateForward(); break;
                case Command.SelectAsBaseToCompare: selectAsBaseToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CompareToBase: compareToBaseToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CreateFixupCommit: FixupCommitToolStripMenuItemClick(this, EventArgs.Empty); break;
                case Command.CreateSquashCommit: SquashCommitToolStripMenuItemClick(this, EventArgs.Empty); break;
                case Command.CreateAmendCommit: AmendCommitToolStripMenuItemClick(this, EventArgs.Empty); break;
                case Command.OpenCommitsWithDifftool: DiffSelectedCommitsWithDifftool(); break;
                case Command.CompareToWorkingDirectory: compareToWorkingDirectoryMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CompareToCurrentBranch: CompareWithCurrentBranchToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CompareToBranch: CompareToBranchToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CompareSelectedCommits: compareSelectedCommitsMenuItem_Click(this, EventArgs.Empty); break;
                case Command.DeleteRef: DeleteRef(); break;
                case Command.RenameRef: RenameRef(); break;
                default:
                    {
                        var result = base.ExecuteCommand(cmd);
                        if (result.NeedsGridRefresh)
                        {
                            PerformRefreshRevisions();
                        }

                        return result;
                    }
            }

            return true;
        }
        #endregion

        #region IScriptHostControl

        GitRevision IScriptHostControl.GetCurrentRevision()
            => GetActualRevision(CurrentCheckout);

        GitRevision? IScriptHostControl.GetLatestSelectedRevision()
            => LatestSelectedRevision;

        IReadOnlyList<GitRevision> IScriptHostControl.GetSelectedRevisions()
            => GetSelectedRevisions();

        Point IScriptHostControl.GetQuickItemSelectorLocation()
            => GetQuickItemSelectorLocation();

        #endregion

        bool ICheckRefs.Contains(ObjectId objectId) => _gridView.Contains(objectId);

        void IRunScript.Execute(string name)
        {
            if (ScriptRunner.RunScript(this, Module, name, UICommands, this).NeedsGridRefresh)
            {
                PerformRefreshRevisions();
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionGridControl _revisionGridControl;

            public TestAccessor(RevisionGridControl revisionGridControl)
            {
                _revisionGridControl = revisionGridControl;
            }

            public int VisibleRevisionCount => _revisionGridControl._gridView.RowCount;

            public bool IsDataLoadComplete =>
                _revisionGridControl._gridView.IsDataLoadComplete;

            public void ClearSelection()
            {
                _revisionGridControl._gridView.ClearSelection();
                _revisionGridControl._latestSelectedRowIndex = -1;
            }
        }

        public void OnRepositoryChanged()
        {
            _buildServerWatcher.OnRepositoryChanged();
        }
    }
}
