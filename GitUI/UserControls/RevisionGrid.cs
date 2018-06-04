using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Editor; // For ColorHelper
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUI.RevisionGridClasses;
using GitUI.Script;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGridClasses;
using GitUIPluginInterfaces;
using Gravatar;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI
{
    public enum RevisionGridLayout
    {
        FilledBranchesSmall = 1,
        FilledBranchesSmallWithGraph = 2,
        Small = 3,
        SmallWithGraph = 4,
        Card = 5,
        CardWithGraph = 6,
        LargeCard = 7,
        LargeCardWithGraph = 8
    }

    public enum RevisionGraphDrawStyleEnum
    {
        Normal,
        DrawNonRelativesGray,
        HighlightSelected
    }

    [DefaultEvent("DoubleClick")]
    public sealed partial class RevisionGrid : GitModuleControl
    {
        public event EventHandler<GitModuleEventArgs> GitModuleChanged;
        public event EventHandler<DoubleClickRevisionEventArgs> DoubleClickRevision;
        public event EventHandler<EventArgs> ShowFirstParentsToggled;
        public event EventHandler SelectionChanged;

        private readonly TranslationString _droppingFilesBlocked = new TranslationString("For you own protection dropping more than 10 patch files at once is blocked!");
        private readonly TranslationString _cannotHighlightSelectedBranch = new TranslationString("Cannot highlight selected branch when revision graph is loading.");
        private readonly TranslationString _noRevisionFoundError = new TranslationString("No revision found.");
        private readonly TranslationString _baseForCompareNotSelectedError = new TranslationString("Base commit for compare is not selected.");
        private readonly TranslationString _strError = new TranslationString("Error");

        private const int MaxSuperprojectRefs = 4;

        private readonly FormRevisionFilter _revisionFilter = new FormRevisionFilter();
        private readonly NavigationHistory _navigationHistory = new NavigationHistory();
        private readonly IImageCache _avatarCache;
        private readonly IAvatarService _gravatarService;
        private readonly IImageNameProvider _avatarImageNameProvider;
        private readonly IGitRevisionTester _gitRevisionTester;
        private readonly ParentChildNavigationHistory _parentChildNavigationHistory;
        private readonly AuthorEmailBasedRevisionHighlighting _revisionHighlighting;
        private readonly Lazy<IndexWatcher> _indexWatcher;

        private RefFilterOptions _refFilterOptions = RefFilterOptions.All | RefFilterOptions.Boundary;
        private IEnumerable<IGitRef> _latestRefs = Enumerable.Empty<IGitRef>();
        private string _lastQuickSearchString = string.Empty;
        private bool _initialLoad = true;

        /// <summary>Tracks status for the artificial commits while the revision graph is reloading</summary>
        private IReadOnlyList<GitItemStatus> _artificialStatus;
        private Brush _selectedItemBrush;
        private SolidBrush _authoredRevisionsBrush;
        private Brush _filledItemBrush; // disposable brush
        private string _initialSelectedRevision;
        private Label _quickSearchLabel;
        private string _quickSearchString = "";
        private RevisionReader _revisionReader;
        private IDisposable _revisionSubscription;
        private RevisionGridLayout _layout;
        private int _rowHeigth;
        private GitRevision _baseCommitToCompare;
        private string _rebaseOnTopOf;
        private bool _isRefreshingRevisions;
        private bool _isLoading;
        private BuildServerWatcher _buildServerWatcher;
        private Font _normalFont;
        private Font _headFont;
        private Font _superprojectFont;
        private Font _refsFont;
        private string[] _lastSelectedRows;
        private string _fixedRevisionFilter = "";
        private string _fixedPathFilter = "";
        private string _branchFilter = string.Empty;
        private JoinableTask<SuperProjectInfo> _superprojectCurrentCheckout;
        private int _latestSelectedRowIndex;
        private string _filtredCurrentCheckout;
        private string[] _currentCheckoutParents;
        private bool _settingsLoaded;
        private Font _fontOfSHAColumn;

        [Browsable(false)] internal RevisionGridMenuCommands MenuCommands { get; }
        [Browsable(false)] public Action OnToggleBranchTreePanelRequested { get; set; }
        [Browsable(false)] public string QuickRevisionFilter { get; set; } = "";
        [Browsable(false)] public bool InMemFilterIgnoreCase { get; set; } = true;
        [Browsable(false)] public string InMemAuthorFilter { get; set; } = "";
        [Browsable(false)] public string InMemCommitterFilter { get; set; } = "";
        [Browsable(false)] public string InMemMessageFilter { get; set; } = "";
        [Browsable(false)] public bool AllowGraphWithFilter { get; set; }
        [Browsable(false)] public string CurrentCheckout { get; private set; }

        /// <summary>
        /// Refs loaded while the latest processing of git log
        /// </summary>
        private IEnumerable<IGitRef> LatestRefs
        {
            get { return _latestRefs; }
            set
            {
                _latestRefs = value;
                AmbiguousRefs = null;
            }
        }

        public RevisionGrid()
        {
            InitLayout();
            InitializeComponent();

            // Parent-child navigation can expect that SetSelectedRevision is always successfull since it always uses first-parents
            _parentChildNavigationHistory = new ParentChildNavigationHistory(revision => SetSelectedRevision(revision));
            _revisionHighlighting = new AuthorEmailBasedRevisionHighlighting();
            _indexWatcher = new Lazy<IndexWatcher>(() => new IndexWatcher(UICommandsSource));

            Loading.Image = Resources.loadingpanel;

            Translate();

            _avatarImageNameProvider = new AvatarImageNameProvider();
            _avatarCache = new DirectoryImageCache(AppSettings.GravatarCachePath, AppSettings.AuthorImageCacheDays);
            _avatarCache.Invalidated += (s, e) => Revisions.Invalidate();
            _gravatarService = new GravatarService(_avatarCache, _avatarImageNameProvider);
            _gitRevisionTester = new GitRevisionTester(new FullPathResolver(() => Module.WorkingDir));

            copyToClipboardToolStripMenuItem.GetViewModel = () => new CopyContextMenuViewModel(LatestSelectedRevision);

            MenuCommands = new RevisionGridMenuCommands(this);
            MenuCommands.CreateOrUpdateMenuCommands();

            // fill View context menu from MenuCommands
            FillMenuFromMenuCommands(MenuCommands.GetViewMenuCommands(), viewToolStripMenuItem);

            // fill Navigate context menu from MenuCommands
            FillMenuFromMenuCommands(MenuCommands.GetNavigateMenuCommands(), navigateToolStripMenuItem);

            NormalFont = AppSettings.Font;
            Loading.Paint += Loading_Paint;

            Revisions.CellPainting += RevisionsCellPainting;
            Revisions.CellFormatting += RevisionsCellFormatting;
            Revisions.KeyPress += RevisionsKeyPress;
            Revisions.KeyUp += RevisionsKeyUp;
            Revisions.KeyDown += RevisionsKeyDown;
            Revisions.MouseDown += RevisionsMouseDown;
            Revisions.ShowCellToolTips = false;
            Revisions.MouseEnter += RevisionsMouseEnter;
            Revisions.CellMouseMove += RevisionsMouseMove;

            showMergeCommitsToolStripMenuItem.Checked = AppSettings.ShowMergeCommits;

            SetShowBranches();

            quickSearchTimer.Tick += QuickSearchTimerTick;

            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            HotkeysEnabled = true;

            Revisions.Loading += RevisionsLoading;

            // Allow to drop patch file on revisiongrid
            Revisions.DragEnter += Revisions_DragEnter;
            Revisions.DragDrop += Revisions_DragDrop;
            Revisions.AllowDrop = true;

            Revisions.ColumnHeadersVisible = false;
            Revisions.IdColumn.Visible = AppSettings.ShowIds;

            IsMessageMultilineDataGridViewColumn.DisplayIndex = 2;
            IsMessageMultilineDataGridViewColumn.Resizable = DataGridViewTriState.False;

            GraphDataGridViewColumn.Width = DpiUtil.Scale(70);
            AuthorDataGridViewColumn.Width = DpiUtil.Scale(150);
            DateDataGridViewColumn.Width = DpiUtil.Scale(135);

            try
            {
                AppSettings.RevisionGraphLayout = (int)(RevisionGridLayout)AppSettings.RevisionGraphLayout;
                SetRevisionsLayout();
            }
            catch
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.SmallWithGraph;
                SetRevisionsLayout();
            }

            compareToBaseToolStripMenuItem.Enabled = false;
            fixupCommitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.CreateFixupCommit).ToShortcutKeyDisplayString();

            _buildServerWatcher = new BuildServerWatcher(this, Revisions);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _revisionSubscription?.Dispose();
                _revisionReader?.Dispose();
                _buildServerWatcher?.Dispose();
                _filledItemBrush?.Dispose();
                _authoredRevisionsBrush?.Dispose();
                _fontOfSHAColumn?.Dispose();

                if (_indexWatcher.IsValueCreated)
                {
                    _indexWatcher.Value.Dispose();
                }
            }

            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region ToolTip

        private readonly ToolTip _toolTip = new ToolTip();
        private readonly Dictionary<Point, bool> _showCellToolTip = new Dictionary<Point, bool>();

        private void RevisionsMouseEnter(object sender, EventArgs e)
        {
            _toolTip.Active = false;
            _toolTip.AutoPopDelay = 32767;
        }

        private void RevisionsMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            var revision = Revisions.GetRowData(e.RowIndex);

            if (revision == null)
            {
                return;
            }

            string oldTooltip = _toolTip.GetToolTip(Revisions);

            string newToolTip;
            if (e.ColumnIndex == GraphDataGridViewColumn.Index)
            {
                newToolTip = Revisions.GetLaneInfo(e.RowIndex, e.X, Module);
            }
            else if (e.ColumnIndex == IsMessageMultilineDataGridViewColumn.Index)
            {
                newToolTip = revision.HasMultiLineMessage ? revision.Body : string.Empty;
            }
            else if (e.ColumnIndex == MessageDataGridViewColumn.Index && revision.HasMultiLineMessage)
            {
                newToolTip = revision.Body;
            }
            else if (_showCellToolTip.TryGetValue(new Point(e.ColumnIndex, e.RowIndex), out var showToolTip) && showToolTip)
            {
                newToolTip = e.ColumnIndex == IdDataGridViewColumn.Index
                             ? revision.Guid
                             : GetCellText(e.RowIndex, e.ColumnIndex);
            }
            else
            {
                // no tooltip unless always active or truncated
                newToolTip = string.Empty;
            }

            if (newToolTip != oldTooltip)
            {
                _toolTip.SetToolTip(Revisions, newToolTip);
            }

            if (!_toolTip.Active)
            {
                _toolTip.Active = true;
            }

            return;

            string GetCellText(int row, int col)
            {
                return Revisions.Rows[row].Cells[col].FormattedValue?.ToString() ?? "";
            }
        }

        internal void DrawColumnText(DataGridViewCellPaintingEventArgs e, string text, Font font, Color color)
        {
            DrawColumnText(e, text, font, color, RevisionGridUtils.GetCellRectangle(e));
        }

        internal void DrawColumnText(DataGridViewCellPaintingEventArgs e, string text, Font font, Color color, Rectangle bounds)
        {
            bool truncated = RevisionGridUtils.DrawColumnTextTruncated(e.Graphics, text, font, color, bounds);
            _showCellToolTip[new Point(e.ColumnIndex, e.RowIndex)] = truncated;
        }

        #endregion

        private static void FillMenuFromMenuCommands(IEnumerable<MenuCommand> menuCommands, ToolStripDropDownItem targetMenuItem)
        {
            foreach (var menuCommand in menuCommands)
            {
                var toolStripItem = MenuCommand.CreateToolStripItem(menuCommand);

                if (toolStripItem is ToolStripMenuItem toolStripMenuItem)
                {
                    menuCommand.RegisterMenuItem(toolStripMenuItem);
                }

                targetMenuItem.DropDownItems.Add(toolStripItem);
            }
        }

        private void Loading_Paint(object sender, PaintEventArgs e)
        {
            // If our loading state has changed since the last paint, update it.
            if (Loading != null)
            {
                if (Loading.Visible != _isLoading)
                {
                    Loading.Visible = _isLoading;
                }
            }
        }

        private Font NormalFont
        {
            set
            {
                _normalFont = value;
                MessageDataGridViewColumn.DefaultCellStyle.Font = _normalFont;
                DateDataGridViewColumn.DefaultCellStyle.Font = _normalFont;
                _fontOfSHAColumn = new Font("Consolas", _normalFont.SizeInPoints);
                IdDataGridViewColumn.DefaultCellStyle.Font = _fontOfSHAColumn;
                IsMessageMultilineDataGridViewColumn.DefaultCellStyle.Font = _normalFont;
                IsMessageMultilineDataGridViewColumn.Width = TextRenderer.MeasureText(MultilineMessageIndicator, NormalFont).Width;

                _refsFont = IsFilledBranchesLayout ? _normalFont : new Font(_normalFont, FontStyle.Bold);
                _headFont = new Font(_normalFont, FontStyle.Bold);
                _superprojectFont = new Font(_normalFont, FontStyle.Underline);
            }
        }

        [CanBeNull]
        [Browsable(false)]
        public GitRevision LatestSelectedRevision => IsValidRevisionIndex(_latestSelectedRowIndex) ? GetRevision(_latestSelectedRowIndex) : null;

        [Description("Indicates whether the user is allowed to select more than one commit at a time.")]
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool MultiSelect
        {
            get => Revisions.MultiSelect;
            set => Revisions.MultiSelect = value;
        }

        [Description("Show uncommited changes in revision grid if enabled in settings.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ShowUncommitedChangesIfPossible { get; set; }

        [Description("Show build server information in revision grid if enabled in settings.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ShowBuildServerInfo { get; set; }

        [Description("Do not open the commit info dialog on double click. This is used if the double click event is handled elseswhere.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool DoubleClickDoesNotOpenCommitInfo { get; set; }

        [Browsable(false)]
        public IndexWatcher IndexWatcher => _indexWatcher.Value;

        public void SetFilters((string revision, string path) filter)
        {
            _fixedRevisionFilter = filter.revision;
            _fixedPathFilter = filter.path;
        }

        public void SetInitialRevision(string initialSelectedRevision)
        {
            _initialSelectedRevision = initialSelectedRevision;
        }

        private void RevisionsLoading(object sender, DvcsGraph.LoadingEventArgs e)
        {
            // Since this can happen on a background thread, we'll just set a
            // flag and deal with it next time we paint (a bit of a hack, but
            // it works)
            _isLoading = e.IsLoading;
        }

        private void ShowQuickSearchString()
        {
            if (_quickSearchLabel == null)
            {
                _quickSearchLabel
                    = new Label
                    {
                        Location = new Point(10, 10),
                        BorderStyle = BorderStyle.FixedSingle,
                        ForeColor = SystemColors.InfoText,
                        BackColor = SystemColors.Info
                    };
                Controls.Add(_quickSearchLabel);
            }

            _quickSearchLabel.Visible = true;
            _quickSearchLabel.BringToFront();
            _quickSearchLabel.Text = _quickSearchString;
            _quickSearchLabel.AutoSize = true;
        }

        private void HideQuickSearchString()
        {
            if (_quickSearchLabel != null)
            {
                _quickSearchLabel.Visible = false;
            }
        }

        private void QuickSearchTimerTick(object sender, EventArgs e)
        {
            quickSearchTimer.Stop();
            _quickSearchString = "";
            HideQuickSearchString();
        }

        private void RestartQuickSearchTimer()
        {
            quickSearchTimer.Stop();
            quickSearchTimer.Interval = AppSettings.RevisionGridQuickSearchTimeout;
            quickSearchTimer.Start();
        }

        private void RevisionsKeyPress(object sender, KeyPressEventArgs e)
        {
            var curIndex = -1;
            if (Revisions.SelectedRows.Count > 0)
            {
                curIndex = Revisions.SelectedRows[0].Index;
            }

            curIndex = curIndex >= 0 ? curIndex : 0;
            if (e.KeyChar == 8 && _quickSearchString.Length > 1)
            {
                // backspace
                RestartQuickSearchTimer();

                _quickSearchString = _quickSearchString.Substring(0, _quickSearchString.Length - 1);

                FindNextMatch(curIndex, _quickSearchString, false);
                _lastQuickSearchString = _quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else if (!char.IsControl(e.KeyChar))
            {
                RestartQuickSearchTimer();

                // The code below is meant to fix the weird keyvalues when pressing keys e.g. ".".
                _quickSearchString = string.Concat(_quickSearchString, char.ToLower(e.KeyChar));

                FindNextMatch(curIndex, _quickSearchString, false);
                _lastQuickSearchString = _quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else
            {
                _quickSearchString = "";
                HideQuickSearchString();
                e.Handled = false;
            }
        }

        private void RevisionsKeyDown(object sender, KeyEventArgs e)
        {
            // BrowserBack/BrowserForward keys and additional handling for Alt+Right/Left sent by some keyboards
            if ((e.KeyCode == Keys.BrowserBack) || ((e.KeyCode == Keys.Left) && e.Modifiers.HasFlag(Keys.Alt)))
            {
                NavigateBackward();
            }
            else if ((e.KeyCode == Keys.BrowserForward) || ((e.KeyCode == Keys.Right) && e.Modifiers.HasFlag(Keys.Alt)))
            {
                NavigateForward();
            }
        }

        private void RevisionsKeyUp(object sender, KeyEventArgs e)
        {
            var selectedRevision = LatestSelectedRevision;
            if (selectedRevision == null)
            {
                return;
            }

            if (e.KeyCode != Keys.F2 && e.KeyCode != Keys.Delete)
            {
                return;
            }

            var gitRefListsForRevision = new GitRefListsForRevision(selectedRevision);

            switch (e.KeyCode)
            {
                case Keys.F2:
                    {
                        InitiateRefAction(gitRefListsForRevision.GetRenameableLocalBranches(),
                                          gitRef => UICommands.StartRenameDialog(this, gitRef.Name),
                                          FormQuickGitRefSelector.Action.Rename);
                    }

                    break;

                case Keys.Delete:
                    {
                        string currentBranch = Module.GetSelectedBranch();
                        InitiateRefAction(gitRefListsForRevision.GetDeletableLocalRefs(currentBranch),
                                          gitRef =>
                                          {
                                              if (gitRef.IsTag)
                                              {
                                                  UICommands.StartDeleteTagDialog(this, gitRef.Name);
                                              }
                                              else
                                              {
                                                  UICommands.StartDeleteBranchDialog(this, gitRef.Name);
                                              }
                                          },
                                          FormQuickGitRefSelector.Action.Delete);
                    }

                    break;
            }
        }

        private void InitiateRefAction(IGitRef[] refs, Action<IGitRef> action, FormQuickGitRefSelector.Action actionLabel)
        {
            if (refs == null || refs.Length < 1)
            {
                return;
            }

            if (refs.Length == 1)
            {
                action(refs[0]);
                return;
            }

            var rect = Revisions.GetCellDisplayRectangle(0, _latestSelectedRowIndex, true);
            using (var dlg = new FormQuickGitRefSelector())
            {
                dlg.Init(actionLabel, refs);
                dlg.Location = PointToScreen(new Point(rect.Right, rect.Bottom));
                if (dlg.ShowDialog(this) != DialogResult.OK || dlg.SelectedRef == null)
                {
                    return;
                }

                action(dlg.SelectedRef);
            }
        }

        private void RevisionsMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
            {
                NavigateBackward();
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                NavigateForward();
            }
        }

        private void ResetNavigationHistory()
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                _navigationHistory.Push(selectedRevisions[0].Guid);
            }
            else
            {
                _navigationHistory.Clear();
            }
        }

        public void NavigateBackward()
        {
            if (_navigationHistory.CanNavigateBackward)
            {
                SetSelectedRevision(_navigationHistory.NavigateBackward());
            }
        }

        public void NavigateForward()
        {
            if (_navigationHistory.CanNavigateForward)
            {
                SetSelectedRevision(_navigationHistory.NavigateForward());
            }
        }

        private void ToggleBranchTreePanel()
        {
            OnToggleBranchTreePanelRequested();
        }

        private void FindNextMatch(int startIndex, string searchString, bool reverse)
        {
            if (Revisions.RowCount == 0)
            {
                return;
            }

            int? searchResult = reverse
                ? SearchInReverseOrder(startIndex, searchString)
                : SearchForward(startIndex, searchString);

            if (searchResult.HasValue)
            {
                Revisions.ClearSelection();
                Revisions.Rows[searchResult.Value].Selected = true;

                Revisions.CurrentCell = Revisions.Rows[searchResult.Value].Cells[1];
            }
        }

        private int? SearchForward(int startIndex, string searchString)
        {
            // Check for out of bounds roll over if required
            int index;
            if (startIndex < 0 || startIndex >= Revisions.RowCount)
            {
                startIndex = 0;
            }

            for (index = startIndex; index < Revisions.RowCount; ++index)
            {
                if (_gitRevisionTester.Matches(GetRevision(index), searchString))
                {
                    return index;
                }
            }

            // We didn't find it so start searching from the top
            for (index = 0; index < startIndex; ++index)
            {
                if (_gitRevisionTester.Matches(GetRevision(index), searchString))
                {
                    return index;
                }
            }

            return null;
        }

        private int? SearchInReverseOrder(int startIndex, string searchString)
        {
            // Check for out of bounds roll over if required
            int index;
            if (startIndex < 0 || startIndex >= Revisions.RowCount)
            {
                startIndex = Revisions.RowCount - 1;
            }

            for (index = startIndex; index >= 0; --index)
            {
                if (_gitRevisionTester.Matches(GetRevision(index), searchString))
                {
                    return index;
                }
            }

            // We didn't find it so start searching from the bottom
            for (index = Revisions.RowCount - 1; index > startIndex; --index)
            {
                if (_gitRevisionTester.Matches(GetRevision(index), searchString))
                {
                    return index;
                }
            }

            return null;
        }

        public void DisableContextMenu()
        {
            Revisions.ContextMenuStrip = null;
        }

        public void FormatQuickFilter(string filter,
                                      bool filterCommit,
                                      bool filterCommitter,
                                      bool filterAuthor,
                                      bool filterDiffContent,
                                      out string revListArgs,
                                      out string inMemMessageFilter,
                                      out string inMemCommitterFilter,
                                      out string inMemAuthorFilter)
        {
            revListArgs = string.Empty;
            inMemMessageFilter = string.Empty;
            inMemCommitterFilter = string.Empty;
            inMemAuthorFilter = string.Empty;
            if (!string.IsNullOrEmpty(filter))
            {
                // hash filtering only possible in memory
                var cmdLineSafe = GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(filter);
                revListArgs = " --regexp-ignore-case ";
                if (filterCommit)
                {
                    if (cmdLineSafe && !MessageFilterCouldBeSHA(filter))
                    {
                        revListArgs += "--grep=\"" + filter + "\" ";
                    }
                    else
                    {
                        inMemMessageFilter = filter;
                    }
                }

                if (filterCommitter && !filter.IsNullOrWhiteSpace())
                {
                    if (cmdLineSafe)
                    {
                        revListArgs += "--committer=\"" + filter + "\" ";
                    }
                    else
                    {
                        inMemCommitterFilter = filter;
                    }
                }

                if (filterAuthor && !filter.IsNullOrWhiteSpace())
                {
                    if (cmdLineSafe)
                    {
                        revListArgs += "--author=\"" + filter + "\" ";
                    }
                    else
                    {
                        inMemAuthorFilter = filter;
                    }
                }

                if (filterDiffContent)
                {
                    if (cmdLineSafe)
                    {
                        revListArgs += "\"-S" + filter + "\" ";
                    }
                    else
                    {
                        throw new InvalidOperationException("Filter text not valid for \"Diff contains\" filter.");
                    }
                }
            }
        }

        public bool SetAndApplyBranchFilter(string filter)
        {
            if (filter == _revisionFilter.GetBranchFilter())
            {
                return false;
            }

            if (filter == "")
            {
                AppSettings.BranchFilterEnabled = false;
                AppSettings.ShowCurrentBranchOnly = true;
            }
            else
            {
                AppSettings.BranchFilterEnabled = true;
                AppSettings.ShowCurrentBranchOnly = false;
                _revisionFilter.SetBranchFilter(filter);
            }

            SetShowBranches();
            return true;
        }

        public override void Refresh()
        {
            if (IsDisposed)
            {
                return;
            }

            SetRevisionsLayout();

            base.Refresh();

            _showCellToolTip.Clear();

            Revisions.Refresh();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            _isLoading = true;
            Error.Visible = false;
            NoCommits.Visible = false;
            NoGit.Visible = false;
            Revisions.Visible = false;
            Loading.Visible = true;
            Loading.BringToFront();
        }

        public new void Load()
        {
            if (!DesignMode)
            {
                ReloadHotkeys();
            }

            ForceRefreshRevisions();
        }

        private void SetSelectedIndex(int index)
        {
            if (Revisions.Rows[index].Selected)
            {
                return;
            }

            Revisions.ClearSelection();

            Revisions.Rows[index].Selected = true;
            Revisions.CurrentCell = Revisions.Rows[index].Cells[1];

            Revisions.Select();
        }

        // Selects row containing revision given its revisionId
        // Returns whether the required revision was found and selected
        private bool InternalSetSelectedRevision(string revision)
        {
            int index = FindRevisionIndex(revision);
            if (index >= 0 && index < Revisions.RowCount)
            {
                SetSelectedIndex(index);
                return true;
            }
            else
            {
                Revisions.ClearSelection();
                Revisions.Select();
                return false;
            }
        }

        /// <summary>
        /// Find specified revision in known to the grid revisions
        /// </summary>
        /// <param name="revision">Revision to lookup</param>
        /// <returns>Index of the found revision or -1 if nothing was found</returns>
        private int FindRevisionIndex(string revision)
        {
            return Revisions.TryGetRevisionIndex(revision) ?? -1;
        }

        public bool SetSelectedRevision(string revision)
        {
            var found = InternalSetSelectedRevision(revision);
            if (found)
            {
                _navigationHistory.Push(revision);
            }

            return found;
        }

        public GitRevision GetRevision(string guid)
        {
            return Revisions.GetRevision(guid);
        }

        public bool SetSelectedRevision([CanBeNull] GitRevision revision)
        {
            return SetSelectedRevision(revision?.Guid);
        }

        private void HighlightBranch(string id)
        {
            RevisionGraphDrawStyle = RevisionGraphDrawStyleEnum.HighlightSelected;
            Revisions.HighlightBranch(id);
        }

        private void RevisionsSelectionChanged(object sender, EventArgs e)
        {
            _parentChildNavigationHistory.RevisionsSelectionChanged();

            if (Revisions.SelectedRows.Count > 0)
            {
                _latestSelectedRowIndex = Revisions.SelectedRows[0].Index;

                // if there was selected a new revision while data is being loaded
                // then don't change the new selection when restoring selected revisions after data is loaded
                if (_isRefreshingRevisions && !Revisions.UpdatingVisibleRows)
                {
                    _lastSelectedRows = Revisions.SelectedIds;
                }
            }

            SelectionTimer.Enabled = false;
            SelectionTimer.Stop();
            SelectionTimer.Enabled = true;
            SelectionTimer.Start();

            var selectedRevisions = GetSelectedRevisions();
            var firstSelectedRevision = selectedRevisions.FirstOrDefault();
            if (selectedRevisions.Count == 1 && firstSelectedRevision != null)
            {
                _navigationHistory.Push(firstSelectedRevision.Guid);
            }

            if (Parent != null && !Revisions.UpdatingVisibleRows &&
                _revisionHighlighting.ProcessRevisionSelectionChange(Module, selectedRevisions) ==
                AuthorEmailBasedRevisionHighlighting.SelectionChangeAction.RefreshUserInterface)
            {
                Refresh();
            }
        }

        public RevisionGraphDrawStyleEnum RevisionGraphDrawStyle
        {
            get => Revisions.RevisionGraphDrawStyle;
            set => Revisions.RevisionGraphDrawStyle = value;
        }

        public string DescribeRevision(GitRevision revision, int maxLength = 0)
        {
            var gitRefListsForRevision = new GitRefListsForRevision(revision);

            var descriptiveRef = gitRefListsForRevision.AllBranches
                .Concat(gitRefListsForRevision.AllTags)
                .FirstOrDefault();

            var description = descriptiveRef != null
                ? GetRefUnambiguousName(descriptiveRef)
                : revision.Subject;

            if (!revision.IsArtificial)
            {
                description += " @" + revision.Guid.Substring(0, 4);
            }

            if (maxLength > 0)
            {
                description = description.ShortenTo(maxLength);
            }

            return description;
        }

        public IReadOnlyList<GitRevision> GetSelectedRevisions(SortDirection? direction = null)
        {
            var rows = Revisions
                .SelectedRows
                .Cast<DataGridViewRow>()
                .Where(row => Revisions.RowCount > row.Index);

            if (direction.HasValue)
            {
                int d = direction.Value == SortDirection.Ascending ? 1 : -1;
                rows = rows.OrderBy((row) => row.Index, (r1, r2) => d * (r1 - r2));
            }

            return rows
                .Select(row => GetRevision(row.Index))
                .ToList();
        }

        public bool IsFirstParentValid()
        {
            var revisions = GetSelectedRevisions();

            // Parents to First (A) are only known if A is explicitly selected (there is no explicit search for parents to parents of a single selected revision)
            return revisions != null && revisions.Count > 1;
        }

        public IReadOnlyList<string> GetRevisionChildren(string revision)
        {
            return Revisions.GetRevisionChildren(revision);
        }

        private bool IsValidRevisionIndex(int index)
        {
            return index >= 0 && index < Revisions.RowCount;
        }

        private GitRevision GetRevision(int row)
        {
            return Revisions.GetRowData(row);
        }

        public GitRevision GetCurrentRevision()
        {
            return Module.GetRevision(CurrentCheckout, shortFormat: true, loadRefs: true);
        }

        public void RefreshRevisions()
        {
            if (IndexWatcher.IndexChanged)
            {
                ForceRefreshRevisions();
            }
        }

        private sealed class RevisionGridInMemFilter
        {
            private readonly string _authorFilter;
            private readonly Regex _authorFilterRegex;
            private readonly string _committerFilter;
            private readonly Regex _committerFilterRegex;
            private readonly string _messageFilter;
            private readonly Regex _messageFilterRegex;
            private readonly string _shaFilter;
            private readonly Regex _shaFilterRegex;

            private RevisionGridInMemFilter(string authorFilter, string committerFilter, string messageFilter, bool ignoreCase)
            {
                (_authorFilter, _authorFilterRegex) = SetUpVars(authorFilter, ignoreCase);
                (_committerFilter, _committerFilterRegex) = SetUpVars(committerFilter, ignoreCase);
                (_messageFilter, _messageFilterRegex) = SetUpVars(messageFilter, ignoreCase);

                if (!string.IsNullOrEmpty(_messageFilter) && MessageFilterCouldBeSHA(_messageFilter))
                {
                    (_shaFilter, _shaFilterRegex) = SetUpVars(messageFilter, false);
                }

                (string filterStr, Regex filterRegex) SetUpVars(string filterValue, bool ignoreKase)
                {
                    var filterStr = filterValue?.Trim() ?? string.Empty;

                    try
                    {
                        var options = ignoreKase ? RegexOptions.IgnoreCase : RegexOptions.None;
                        return (filterStr, new Regex(filterStr, options));
                    }
                    catch (ArgumentException)
                    {
                        return (filterStr, null);
                    }
                }
            }

            public bool Predicate(GitRevision rev)
            {
                return CheckCondition(_authorFilter, _authorFilterRegex, rev.Author) &&
                       CheckCondition(_committerFilter, _committerFilterRegex, rev.Committer) &&
                       (CheckCondition(_messageFilter, _messageFilterRegex, rev.Body) ||
                        (_shaFilter != null && CheckCondition(_shaFilter, _shaFilterRegex, rev.Guid)));

                bool CheckCondition(string filter, Regex regex, string value)
                {
                    return string.IsNullOrEmpty(filter) ||
                           (regex != null && value != null && regex.IsMatch(value));
                }
            }

            [CanBeNull]
            public static RevisionGridInMemFilter CreateIfNeeded([CanBeNull] string authorFilter,
                                                                 [CanBeNull] string committerFilter,
                                                                 [CanBeNull] string messageFilter,
                                                                 bool ignoreCase)
            {
                if (string.IsNullOrEmpty(authorFilter) &&
                    string.IsNullOrEmpty(committerFilter) &&
                    string.IsNullOrEmpty(messageFilter) &&
                    !MessageFilterCouldBeSHA(messageFilter))
                {
                    return null;
                }

                return new RevisionGridInMemFilter(
                    authorFilter,
                    committerFilter,
                    messageFilter,
                    ignoreCase);
            }
        }

        public void ReloadHotkeys()
        {
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            MenuCommands.CreateOrUpdateMenuCommands();
        }

        public void ReloadTranslation()
        {
            Translate();
        }

        private static bool ShowRemoteRef(IGitRef r)
        {
            if (r.IsTag)
            {
                return AppSettings.ShowSuperprojectTags;
            }

            if (r.IsHead)
            {
                return AppSettings.ShowSuperprojectBranches;
            }

            if (r.IsRemote)
            {
                return AppSettings.ShowSuperprojectRemoteBranches;
            }

            return false;
        }

        public void ForceRefreshRevisions()
        {
            try
            {
                RevisionGraphDrawStyle = RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
                IsMessageMultilineDataGridViewColumn.Visible = AppSettings.ShowIndicatorForMultilineMessage;

                ApplyFilterFromRevisionFilterDialog();

                _initialLoad = true;

                _buildServerWatcher.CancelBuildStatusFetchOperation();

                DisposeRevisionReader();

                var newCurrentCheckout = Module.GetCurrentCheckout();
                GitModule capturedModule = Module;
                JoinableTask<SuperProjectInfo> newSuperPrjectInfo =
                    ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                        return GetSuperprojectCheckout(ShowRemoteRef, capturedModule);
                    });
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    try
                    {
                        await newSuperPrjectInfo;
                    }
                    finally
                    {
                        await this.SwitchToMainThreadAsync();
                        Refresh();
                    }
                }).FileAndForget();

                // If the current checkout changed, don't get the currently selected rows, select the
                // new current checkout instead.
                if (newCurrentCheckout == CurrentCheckout)
                {
                    _lastSelectedRows = Revisions.SelectedIds;
                }
                else
                {
                    // This is a new checkout, so ensure the variable is cleared out.
                    _lastSelectedRows = null;
                }

                Revisions.ClearSelection();
                CurrentCheckout = newCurrentCheckout;
                _filtredCurrentCheckout = null;
                _currentCheckoutParents = null;
                _superprojectCurrentCheckout = newSuperPrjectInfo;
                Revisions.Clear();
                Error.Visible = false;

                if (!Module.IsValidGitWorkingDir())
                {
                    Revisions.Visible = false;
                    NoCommits.Visible = true;
                    Loading.Visible = false;
                    NoGit.Visible = true;
                    string dir = Module.WorkingDir;
                    if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir) ||
                        (Directory.GetDirectories(dir).Length == 0 &&
                        Directory.GetFiles(dir).Length == 0))
                    {
                        CloneRepository.Show();
                    }
                    else
                    {
                        CloneRepository.Hide();
                    }

                    NoGit.BringToFront();
                    return;
                }

                NoCommits.Visible = false;
                NoGit.Visible = false;
                Revisions.Visible = true;
                Revisions.BringToFront();
                Revisions.Enabled = false;
                Loading.Visible = true;
                Loading.BringToFront();
                _isLoading = true;
                _isRefreshingRevisions = true;
                base.Refresh();

                IndexWatcher.Reset();

                if (!AppSettings.ShowGitNotes && _refFilterOptions.HasFlag(RefFilterOptions.All | RefFilterOptions.Boundary))
                {
                    _refFilterOptions |= RefFilterOptions.ShowGitNotes;
                }

                if (AppSettings.ShowGitNotes)
                {
                    _refFilterOptions &= ~RefFilterOptions.ShowGitNotes;
                }

                if (!AppSettings.ShowMergeCommits)
                {
                    _refFilterOptions |= RefFilterOptions.NoMerges;
                }

                if (AppSettings.ShowFirstParent)
                {
                    _refFilterOptions |= RefFilterOptions.FirstParent;
                }

                if (AppSettings.ShowSimplifyByDecoration)
                {
                    _refFilterOptions |= RefFilterOptions.SimplifyByDecoration;
                }

                var formFilter = RevisionGridInMemFilter.CreateIfNeeded(
                    _revisionFilter.GetInMemAuthorFilter(),
                    _revisionFilter.GetInMemCommitterFilter(),
                    _revisionFilter.GetInMemMessageFilter(),
                    _revisionFilter.GetIgnoreCase());

                var toolStripFilter = RevisionGridInMemFilter.CreateIfNeeded(
                    InMemAuthorFilter,
                    InMemCommitterFilter,
                    InMemMessageFilter,
                    InMemFilterIgnoreCase);

                Func<GitRevision, bool> predicate;
                if (formFilter != null && toolStripFilter != null)
                {
                    // either or
                    predicate = r => formFilter.Predicate(r) || toolStripFilter.Predicate(r);
                }
                else if (formFilter != null)
                {
                    predicate = formFilter.Predicate;
                }
                else if (toolStripFilter != null)
                {
                    predicate = toolStripFilter.Predicate;
                }
                else
                {
                    predicate = null;
                }

                var revisions = new Subject<GitRevision>();
                _revisionSubscription?.Dispose();
                _revisionSubscription = revisions
                    .ObserveOn(ThreadPoolScheduler.Instance)
                    .Subscribe(OnRevisionRead, OnRevisionReaderError, OnRevisionReadCompleted);

                if (_revisionReader == null)
                {
                    _revisionReader = new RevisionReader();
                }

                _revisionReader.Execute(
                    Module,
                    revisions,
                    _refFilterOptions,
                    _branchFilter,
                    _revisionFilter.GetRevisionFilter() + QuickRevisionFilter + _fixedRevisionFilter,
                    _revisionFilter.GetPathFilter() + _fixedPathFilter,
                    predicate);

                LoadRevisions();
                SetRevisionsLayout();
                ResetNavigationHistory();
            }
            catch (Exception)
            {
                Error.Visible = true;
                Error.BringToFront();
                throw;
            }
        }

        private class SuperProjectInfo
        {
            public string CurrentBranch;
            public string Conflict_Base;
            public string Conflict_Remote;
            public string Conflict_Local;
            public Dictionary<string, List<IGitRef>> Refs;
        }

        [CanBeNull]
        private static SuperProjectInfo GetSuperprojectCheckout(Func<IGitRef, bool> showRemoteRef, GitModule gitModule)
        {
            if (gitModule.SuperprojectModule == null)
            {
                return null;
            }

            SuperProjectInfo spi = new SuperProjectInfo();
            var (code, commit) = gitModule.GetSuperprojectCurrentCheckout();
            if (code == 'U')
            {
                // return local and remote hashes
                var array = gitModule.SuperprojectModule.GetConflict(gitModule.SubmodulePath);
                spi.Conflict_Base = array.Base.Hash;
                spi.Conflict_Local = array.Local.Hash;
                spi.Conflict_Remote = array.Remote.Hash;
            }
            else
            {
                spi.CurrentBranch = commit;
            }

            var refs = gitModule.SuperprojectModule.GetSubmoduleItemsForEachRef(gitModule.SubmodulePath, showRemoteRef);

            if (refs != null)
            {
                spi.Refs = refs.Where(a => a.Value != null).GroupBy(a => a.Value.Guid).ToDictionary(gr => gr.Key, gr => gr.Select(a => a.Key).ToList());
            }

            return spi;
        }

        private static readonly Regex PotentialShaPattern = new Regex(@"^[a-f0-9]{5,}", RegexOptions.Compiled);

        public static bool MessageFilterCouldBeSHA(string filter)
        {
            bool result = PotentialShaPattern.IsMatch(filter);

            return result;
        }

        private void OnRevisionReaderError(Exception exception)
        {
            // This has to happen on the UI thread
            this.InvokeAsync(
                    () =>
                    {
                        Error.Visible = true;
                        ////Error.BringToFront();
                        NoGit.Visible = false;
                        NoCommits.Visible = false;
                        Revisions.Visible = false;
                        Loading.Visible = false;
                    })
                .FileAndForget();

            DisposeRevisionReader();
            this.InvokeAsync(() => throw new AggregateException(exception)).FileAndForget();
        }

        internal bool FilterIsApplied(bool inclBranchFilter)
        {
            return (inclBranchFilter && !string.IsNullOrEmpty(_branchFilter)) ||
                   !(string.IsNullOrEmpty(QuickRevisionFilter) &&
                     !_revisionFilter.FilterEnabled() &&
                     string.IsNullOrEmpty(InMemAuthorFilter) &&
                     string.IsNullOrEmpty(InMemCommitterFilter) &&
                     string.IsNullOrEmpty(InMemMessageFilter));
        }

        private bool ShouldHideGraph(bool inclBranchFilter)
        {
            return (inclBranchFilter && !string.IsNullOrEmpty(_branchFilter)) ||
                   !(!_revisionFilter.ShouldHideGraph() &&
                     string.IsNullOrEmpty(InMemAuthorFilter) &&
                     string.IsNullOrEmpty(InMemCommitterFilter) &&
                     string.IsNullOrEmpty(InMemMessageFilter));
        }

        private void DisposeRevisionReader()
        {
            if (_revisionReader != null)
            {
                LatestRefs = _revisionReader.LatestRefs;

                _revisionReader.Dispose();
                _revisionReader = null;
            }
        }

        private void OnRevisionReadCompleted()
        {
            _isLoading = false;

            if (_revisionReader.RevisionCount == 0 &&
                !FilterIsApplied(true))
            {
                // This has to happen on the UI thread
                this.InvokeAsync(
                        () =>
                        {
                            NoGit.Visible = false;
                            NoCommits.Visible = true;
                            ////NoCommits.BringToFront();
                            Revisions.Visible = false;
                            Loading.Visible = false;
                            _isRefreshingRevisions = false;
                        })
                    .FileAndForget();
            }
            else
            {
                // This has to happen on the UI thread
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await this.SwitchToMainThreadAsync();
                    OnRevisionRead();
                    Loading.Visible = false;
                    _isRefreshingRevisions = false;
                    SelectInitialRevision();
                    if (ShowBuildServerInfo)
                    {
                        await _buildServerWatcher.LaunchBuildServerInfoFetchOperationAsync();
                    }
                }).FileAndForget();
            }

            DisposeRevisionReader();
        }

        private void SelectInitialRevision()
        {
            string filtredCurrentCheckout = _filtredCurrentCheckout;
            string[] lastSelectedRows = _lastSelectedRows ?? Array.Empty<string>();

            // filter out all unavailable commits from LastSelectedRows.
            lastSelectedRows = lastSelectedRows.Where(revision => FindRevisionIndex(revision) >= 0).ToArray();

            if (lastSelectedRows.Any())
            {
                Revisions.SelectedIds = lastSelectedRows;
                _lastSelectedRows = null;
            }
            else
            {
                if (!string.IsNullOrEmpty(_initialSelectedRevision))
                {
                    int index = SearchRevision(_initialSelectedRevision);
                    if (index >= 0)
                    {
                        SetSelectedIndex(index);
                    }
                }
                else
                {
                    SetSelectedRevision(filtredCurrentCheckout);
                }
            }

            if (string.IsNullOrEmpty(filtredCurrentCheckout))
            {
                return;
            }

            if (!Revisions.IsRevisionRelative(filtredCurrentCheckout))
            {
                HighlightBranch(filtredCurrentCheckout);
            }
        }

        private string[] GetAllParents(string initRevision)
        {
            var args = new ArgumentBuilder
            {
                "rev-list",
                { AppSettings.OrderRevisionByDate, "--date-order", "--topo-order" },
                { AppSettings.MaxRevisionGraphCommits > 0, $"--max-count=\"{AppSettings.MaxRevisionGraphCommits}\" " },
                initRevision
            };

            return Module.ReadGitOutputLines(args.ToString()).ToArray();
        }

        private int SearchRevision(string initRevision)
        {
            var exactIndex = Revisions.TryGetRevisionIndex(initRevision);
            if (exactIndex.HasValue)
            {
                return exactIndex.Value;
            }

            foreach (var parentHash in GetAllParents(initRevision))
            {
                var parentIndex = Revisions.TryGetRevisionIndex(parentHash);
                if (parentIndex.HasValue)
                {
                    return parentIndex.Value;
                }
            }

            return -1;
        }

        private static string GetDateHeaderText()
        {
            return AppSettings.ShowAuthorDate ? Strings.GetAuthorDateText() : Strings.GetCommitDateText();
        }

        private void LoadRevisions()
        {
            if (_revisionReader == null)
            {
                return;
            }

            Revisions.SuspendLayout();

            Revisions.MessageColumn.HeaderText = Strings.GetMessageText();
            Revisions.AuthorColumn.HeaderText = Strings.GetAuthorText();
            Revisions.DateColumn.HeaderText = GetDateHeaderText();

            Revisions.SelectionChanged -= RevisionsSelectionChanged;

            Revisions.Enabled = true;
            Revisions.Focus();
            Revisions.SelectionChanged += RevisionsSelectionChanged;

            Revisions.ResumeLayout();

            if (!_initialLoad)
            {
                return;
            }

            _initialLoad = false;

            SelectionTimer.Enabled = false;
            SelectionTimer.Stop();
            SelectionTimer.Enabled = true;
            SelectionTimer.Start();
        }

        public struct DrawRefArgs
        {
            public Graphics Graphics;
            public Rectangle CellBounds;
            public bool IsRowSelected;
            public Font RefsFont;
        }

        private void RevisionsCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // If our loading state has changed since the last paint, update it.
            if (Loading != null)
            {
                if (Loading.Visible != _isLoading)
                {
                    Loading.Visible = _isLoading;
                }
            }

            var columnIndex = e.ColumnIndex;

            int graphColIndex = GraphDataGridViewColumn.Index;
            int messageColIndex = MessageDataGridViewColumn.Index;
            int authorColIndex = AuthorDataGridViewColumn.Index;
            int dateColIndex = DateDataGridViewColumn.Index;
            int idColIndex = IdDataGridViewColumn.Index;
            int isMsgMultilineColIndex = IsMessageMultilineDataGridViewColumn.Index;

            if (e.RowIndex < 0 || (e.State & DataGridViewElementStates.Visible) == 0)
            {
                return;
            }

            if (Revisions.RowCount <= e.RowIndex)
            {
                return;
            }

            var revision = GetRevision(e.RowIndex);
            if (revision == null)
            {
                return;
            }

            var spi = _superprojectCurrentCheckout.Task.CompletedOrDefault();
            var superprojectRefs = new List<IGitRef>();
            if (spi?.Refs != null && spi.Refs.ContainsKey(revision.Guid))
            {
                superprojectRefs.AddRange(spi.Refs[revision.Guid].Where(ShowRemoteRef));
            }

            e.Handled = true;

            var drawRefArgs = new DrawRefArgs
            {
                Graphics = e.Graphics,
                CellBounds = e.CellBounds,
                IsRowSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected
            };

            // Determine background colour for cell
            Brush cellBackgroundBrush;
            if (drawRefArgs.IsRowSelected /*&& !showRevisionCards*/)
            {
                cellBackgroundBrush = _selectedItemBrush;
            }
            else if (ShouldHighlightRevisionByAuthor(revision))
            {
                cellBackgroundBrush = _authoredRevisionsBrush;
            }
            else if (ShouldRenderAlternateBackColor(e.RowIndex))
            {
                cellBackgroundBrush = new SolidBrush(ColorHelper.MakeColorDarker(e.CellStyle.BackColor));

                // TODO if default background is nearly black, we should make it lighter instead
            }
            else
            {
                cellBackgroundBrush = new SolidBrush(e.CellStyle.BackColor);
            }

            // Draw cell background
            e.Graphics.FillRectangle(cellBackgroundBrush, e.CellBounds);

            var backColor = cellBackgroundBrush is SolidBrush brush
                ? brush.Color
                : (Color?)null;

            // Draw graphics column
            if (e.ColumnIndex == graphColIndex)
            {
                Revisions.dataGrid_CellPainting(sender, e);
                return;
            }

            // Determine cell foreground (text) colour for other columns
            Color foreColor;
            if (drawRefArgs.IsRowSelected)
            {
                foreColor = SystemColors.HighlightText;
            }
            else if (AppSettings.RevisionGraphDrawNonRelativesTextGray && !Revisions.RowIsRelative(e.RowIndex))
            {
                Debug.Assert(backColor != null, "backColor != null");
                foreColor = Color.Gray;

                // TODO: If the background colour is close to being Gray, we should adjust the gray until there is a bit more contrast.
                while (ColorHelper.GetColorBrightnessDifference(foreColor, backColor.Value) < 125)
                {
                    foreColor = ColorHelper.IsLightColor(backColor.Value) ? ColorHelper.MakeColorDarker(foreColor) : ColorHelper.MakeColorLighter(foreColor);
                }
            }
            else
            {
                Debug.Assert(backColor != null, "backColor != null");
                foreColor = ColorHelper.GetForeColorForBackColor(backColor.Value);
            }

            /*
            if (!AppSettings.RevisionGraphDrawNonRelativesTextGray || Revisions.RowIsRelative(e.RowIndex))
            {
                foreColor = drawRefArgs.IsRowSelected && IsFilledBranchesLayout()
                    ? SystemColors.HighlightText
                    : e.CellStyle.ForeColor;
            }
            else
            {
                foreColor = drawRefArgs.IsRowSelected ? SystemColors.HighlightText : Color.Gray;
            }
            */

            using (Brush foreBrush = new SolidBrush(foreColor))
            {
                var rowFont = _normalFont;
                if (revision.Guid == CurrentCheckout /*&& !showRevisionCards*/)
                {
                    rowFont = _headFont;
                }
                else if (spi != null && spi.CurrentBranch == revision.Guid)
                {
                    rowFont = _superprojectFont;
                }

                if (columnIndex == messageColIndex)
                {
                    int baseOffset = 0;
                    if (IsCardLayout)
                    {
                        baseOffset = 5;

                        Rectangle cellRectangle = new Rectangle(e.CellBounds.Left + baseOffset, e.CellBounds.Top + 1, e.CellBounds.Width - (baseOffset * 2), e.CellBounds.Height - 4);

                        if (!AppSettings.RevisionGraphDrawNonRelativesGray || Revisions.RowIsRelative(e.RowIndex))
                        {
                            e.Graphics.FillRectangle(
                                new LinearGradientBrush(cellRectangle,
                                                        Color.FromArgb(255, 220, 220, 231),
                                                        Color.FromArgb(255, 240, 240, 250), 90, false), cellRectangle);
                            using (var pen = new Pen(Color.FromArgb(255, 200, 200, 200), 1))
                            {
                                e.Graphics.DrawRectangle(pen, cellRectangle);
                            }
                        }
                        else
                        {
                            e.Graphics.FillRectangle(
                                new LinearGradientBrush(cellRectangle,
                                                        Color.FromArgb(255, 240, 240, 240),
                                                        Color.FromArgb(255, 250, 250, 250), 90, false), cellRectangle);
                        }

                        if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                        {
                            using (var penSelectionBackColor = new Pen(Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor, 1))
                            {
                                e.Graphics.DrawRectangle(penSelectionBackColor, cellRectangle);
                            }
                        }
                    }

                    float offset = baseOffset;

                    drawRefArgs.RefsFont = IsFilledBranchesLayout ? rowFont : _refsFont;

                    if (spi != null)
                    {
                        if (spi.Conflict_Base == revision.Guid)
                        {
                            offset = DrawRef(drawRefArgs, offset, "Base", Color.OrangeRed, ArrowType.NotFilled);
                        }

                        if (spi.Conflict_Local == revision.Guid)
                        {
                            offset = DrawRef(drawRefArgs, offset, "Local", Color.OrangeRed, ArrowType.NotFilled);
                        }

                        if (spi.Conflict_Remote == revision.Guid)
                        {
                            offset = DrawRef(drawRefArgs, offset, "Remote", Color.OrangeRed, ArrowType.NotFilled);
                        }
                    }

                    if (revision.Refs.Count != 0)
                    {
                        var gitRefs = revision.Refs.ToList();
                        gitRefs.Sort(
                            (left, right) =>
                            {
                                int leftTypeRank = RefTypeRank(left);
                                int rightTypeRank = RefTypeRank(right);
                                if (leftTypeRank == rightTypeRank)
                                {
                                    return left.Name.CompareTo(right.Name);
                                }

                                return leftTypeRank.CompareTo(rightTypeRank);
                            });

                        foreach (var gitRef in gitRefs.Where(head => (!head.IsRemote || AppSettings.ShowRemoteBranches)))
                        {
                            if (gitRef.IsTag)
                            {
                                if (!AppSettings.ShowTags)
                                {
                                    continue;
                                }
                            }

                            Color headColor = GetHeadColor(gitRef);

                            ArrowType arrowType = gitRef.Selected ? ArrowType.Filled :
                                                  gitRef.SelectedHeadMergeSource ? ArrowType.NotFilled : ArrowType.None;
                            drawRefArgs.RefsFont = gitRef.Selected ? rowFont : _refsFont;

                            var superprojectRef = superprojectRefs.FirstOrDefault(superGitRef => gitRef.CompleteName == superGitRef.CompleteName);
                            if (superprojectRef != null)
                            {
                                superprojectRefs.Remove(superprojectRef);
                            }

                            string name = gitRef.Name;
                            if (gitRef.IsTag
                                 && gitRef.IsDereference // see note on using IsDereference in CommitInfo class.
                                 && AppSettings.ShowAnnotatedTagsMessages
                                 && AppSettings.ShowIndicatorForMultilineMessage)
                            {
                                name = name + "  " + MultilineMessageIndicator;
                            }

                            offset = DrawRef(drawRefArgs, offset, name, headColor, arrowType, superprojectRef != null, true);
                        }
                    }

                    for (int i = 0; i < Math.Min(MaxSuperprojectRefs, superprojectRefs.Count); i++)
                    {
                        var gitRef = superprojectRefs[i];
                        Color headColor = GetHeadColor(gitRef);
                        var gitRefName = i < (MaxSuperprojectRefs - 1) ? gitRef.Name : "…";

                        ArrowType arrowType = gitRef.Selected ? ArrowType.Filled :
                                              gitRef.SelectedHeadMergeSource ? ArrowType.NotFilled : ArrowType.None;
                        drawRefArgs.RefsFont = gitRef.Selected ? rowFont : _refsFont;

                        offset = DrawRef(drawRefArgs, offset, gitRefName, headColor, arrowType, true);
                    }

                    if (IsCardLayout)
                    {
                        offset = baseOffset;
                    }

                    var text = (string)e.FormattedValue;
                    var bounds = AdjustCellBounds(e.CellBounds, offset);
                    DrawColumnText(e, text, rowFont, foreColor, bounds);

                    if (IsCardLayout)
                    {
                        int textHeight = (int)e.Graphics.MeasureString(text, rowFont).Height;
                        int gravatarSize = _rowHeigth - textHeight - 12;
                        int gravatarTop = e.CellBounds.Top + textHeight + 6;
                        int gravatarLeft = e.CellBounds.Left + baseOffset + 2;

                        var imageName = _avatarImageNameProvider.Get(revision.AuthorEmail);
                        var gravatar = _avatarCache.GetImage(imageName, null);
                        if (gravatar == null)
                        {
                            gravatar = Resources.User;

                            // kick off download operation, will likely display the avatar during the next round of repaint
                            _gravatarService.GetAvatarAsync(revision.AuthorEmail, AppSettings.AuthorImageSize, AppSettings.GravatarDefaultImageType);
                        }

                        e.Graphics.DrawImage(gravatar, gravatarLeft + 1, gravatarTop + 1, gravatarSize, gravatarSize);
                        e.Graphics.DrawRectangle(Pens.Black, gravatarLeft, gravatarTop, gravatarSize + 1, gravatarSize + 1);

                        string authorText;
                        string timeText;

                        if (_rowHeigth >= 60)
                        {
                            authorText = revision.Author;
                            timeText = TimeToString(AppSettings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate);
                        }
                        else
                        {
                            timeText = string.Concat(revision.Author, " (", TimeToString(AppSettings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate), ")");
                            authorText = string.Empty;
                        }

                        e.Graphics.DrawString(authorText, rowFont, foreBrush,
                                              new PointF(gravatarLeft + gravatarSize + 5, gravatarTop + 6));
                        e.Graphics.DrawString(timeText, rowFont, foreBrush,
                                              new PointF(gravatarLeft + gravatarSize + 5, e.CellBounds.Bottom - textHeight - 4));
                    }

                    if (revision.IsArtificial)
                    {
                        // Get offset for "count" text
                        SizeF textSize = drawRefArgs.Graphics.MeasureString(text, rowFont);

                        offset += 1 + textSize.Width;
                        offset = DrawRef(drawRefArgs, offset, revision.Subject, AppSettings.OtherTagColor, ArrowType.None, false, true);
                    }
                }
                else if (columnIndex == authorColIndex)
                {
                    if (!revision.IsArtificial)
                    {
                        var text = (string)e.FormattedValue;
                        DrawColumnText(e, text, rowFont, foreColor);
                    }
                }
                else if (columnIndex == dateColIndex)
                {
                    var time = AppSettings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
                    var text = TimeToString(time);
                    DrawColumnText(e, text, rowFont, foreColor);
                }
                else if (columnIndex == idColIndex)
                {
                    if (!revision.IsArtificial)
                    {
                        // do not show artificial GUID
                        var text = revision.Guid;
                        DrawColumnText(e, text, _fontOfSHAColumn, foreColor);
                    }
                }
                else if (columnIndex == _buildServerWatcher.BuildStatusImageColumnIndex)
                {
                    BuildInfoDrawingLogic.BuildStatusImageColumnCellPainting(e, revision);
                }
                else if (columnIndex == _buildServerWatcher.BuildStatusMessageColumnIndex)
                {
                    var isSelected = Revisions.Rows[e.RowIndex].Selected;
                    BuildInfoDrawingLogic.BuildStatusMessageCellPainting(e, revision, foreColor, rowFont, isSelected, this);
                }
                else if (AppSettings.ShowIndicatorForMultilineMessage && columnIndex == isMsgMultilineColIndex)
                {
                    var text = (string)e.FormattedValue;
                    DrawColumnText(e, text, rowFont, foreColor);
                }
            }

            return;

            int RefTypeRank(IGitRef gitRef)
            {
                if (gitRef.IsBisect)
                {
                    return 0;
                }

                if (gitRef.Selected)
                {
                    return 1;
                }

                if (gitRef.SelectedHeadMergeSource)
                {
                    return 2;
                }

                if (gitRef.IsHead)
                {
                    return 3;
                }

                if (gitRef.IsRemote)
                {
                    return 4;
                }

                return 5;
            }
        }

        private bool ShouldHighlightRevisionByAuthor(GitRevision revision)
        {
            return AppSettings.HighlightAuthoredRevisions &&
                   AuthorEmailEqualityComparer.Instance.Equals(revision.AuthorEmail,
                                                               _revisionHighlighting.AuthorEmailToHighlight);
        }

        private static bool ShouldRenderAlternateBackColor(int rowIndex)
        {
            return AppSettings.RevisionGraphDrawAlternateBackColor && rowIndex % 2 == 0;
        }

        private float DrawRef(DrawRefArgs drawRefArgs, float offset, string name, Color headColor, ArrowType arrowType, bool dashedLine = false, bool fill = false)
        {
            var textColor = fill ? headColor : Lerp(headColor, Color.White, 0.5f);

            if (IsCardLayout)
            {
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    string headName = name;
                    offset += drawRefArgs.Graphics.MeasureString(headName, drawRefArgs.RefsFont).Width + 6;
                    var location = new PointF(drawRefArgs.CellBounds.Right - offset, drawRefArgs.CellBounds.Top + 4);
                    var size = new SizeF(drawRefArgs.Graphics.MeasureString(headName, drawRefArgs.RefsFont).Width,
                                     drawRefArgs.Graphics.MeasureString(headName, drawRefArgs.RefsFont).Height);
                    if (fill)
                    {
                        drawRefArgs.Graphics.FillRectangle(SystemBrushes.Info, location.X - 1,
                                             location.Y - 1, size.Width + 3, size.Height + 2);
                    }

                    drawRefArgs.Graphics.DrawRectangle(SystemPens.InfoText, location.X - 1,
                                         location.Y - 1, size.Width + 3, size.Height + 2);
                    drawRefArgs.Graphics.DrawString(headName, drawRefArgs.RefsFont, textBrush, location);
                }
            }
            else
            {
                string headName = IsFilledBranchesLayout
                               ? name
                               : string.Concat("[", name, "] ");

                var headBounds = AdjustCellBounds(drawRefArgs.CellBounds, offset);
                SizeF textSize = drawRefArgs.Graphics.MeasureString(headName, drawRefArgs.RefsFont);

                offset += textSize.Width;

                if (IsFilledBranchesLayout)
                {
                    offset += 9;

                    float extraOffset = DrawHeadBackground(drawRefArgs.IsRowSelected, drawRefArgs.Graphics,
                                                           headColor, headBounds.X,
                                                           headBounds.Y,
                                                           RoundToEven(textSize.Width + 3),
                                                           RoundToEven(textSize.Height), 3,
                                                           arrowType, dashedLine, fill);

                    offset += extraOffset;
                    headBounds.Offset((int)(extraOffset + 1), 0);
                }

                RevisionGridUtils.DrawColumnTextTruncated(drawRefArgs.Graphics, headName, drawRefArgs.RefsFont, textColor, headBounds);
            }

            return offset;
        }

        private const string MultilineMessageIndicator = "[...]";

        private void RevisionsCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var columnIndex = e.ColumnIndex;
            if (e.RowIndex < 0)
            {
                return;
            }

            if (Revisions.RowCount <= e.RowIndex)
            {
                return;
            }

            var revision = GetRevision(e.RowIndex);
            if (revision == null)
            {
                return;
            }

            e.FormattingApplied = true;

            int graphColIndex = GraphDataGridViewColumn.Index;
            int messageColIndex = MessageDataGridViewColumn.Index;
            int authorColIndex = AuthorDataGridViewColumn.Index;
            int dateColIndex = DateDataGridViewColumn.Index;
            int isMsgMultilineColIndex = IsMessageMultilineDataGridViewColumn.Index;

            if (columnIndex == graphColIndex && !revision.IsArtificial)
            {
                // Do not show artificial guid
                e.Value = revision.Guid;
            }
            else if (columnIndex == messageColIndex)
            {
                if (revision.IsArtificial)
                {
                    e.Value = revision.SubjectCount;
                }
                else
                {
                    e.Value = revision.Subject;
                }
            }
            else if (columnIndex == authorColIndex)
            {
                e.Value = revision.Author ?? "";
            }
            else if (columnIndex == dateColIndex)
            {
                var time = AppSettings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
                if (time == DateTime.MinValue || time == DateTime.MaxValue)
                {
                    e.Value = "";
                }
                else
                {
                    e.Value = string.Format("{0} {1}", time.ToShortDateString(), time.ToLongTimeString());
                }
            }
            else if (columnIndex == _buildServerWatcher.BuildStatusImageColumnIndex)
            {
                BuildInfoDrawingLogic.BuildStatusImageColumnCellFormatting(e, Revisions, revision);
            }
            else if (columnIndex == _buildServerWatcher.BuildStatusMessageColumnIndex)
            {
                BuildInfoDrawingLogic.BuildStatusMessageCellFormatting(e, revision);
            }
            else if (AppSettings.ShowIndicatorForMultilineMessage && columnIndex == isMsgMultilineColIndex)
            {
                e.Value = revision.HasMultiLineMessage ? MultilineMessageIndicator : "";
            }
            else
            {
                e.FormattingApplied = false;
            }
        }

        private static Rectangle AdjustCellBounds(Rectangle cellBounds, float offset)
        {
            return new Rectangle((int)(cellBounds.Left + offset), cellBounds.Top + 4,
                                 cellBounds.Width - (int)offset, cellBounds.Height);
        }

        private static Color GetHeadColor(IGitRef gitRef)
        {
            if (gitRef.IsTag)
            {
                return AppSettings.TagColor;
            }

            if (gitRef.IsHead)
            {
                return AppSettings.BranchColor;
            }

            if (gitRef.IsRemote)
            {
                return AppSettings.RemoteBranchColor;
            }

            return AppSettings.OtherTagColor;
        }

        private static float RoundToEven(float value)
        {
            int result = ((int)value / 2) * 2;
            return result < value ? result + 2 : result;
        }

        private enum ArrowType
        {
            None,
            Filled,
            NotFilled
        }

        private static readonly float[] dashPattern = { 4, 4 };

        private float DrawHeadBackground(bool isSelected, Graphics graphics, Color color,
            float x, float y, float width, float height, float radius, ArrowType arrowType, bool dashedLine, bool fill)
        {
            float additionalOffset = arrowType != ArrowType.None ? height - 6 : 0;
            width += additionalOffset;
            var oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                // shade
                if (fill)
                {
                    using (var shadePath = CreateRoundRectPath(x + 1, y + 1, width, height, radius))
                    {
                        var shadeBrush = isSelected ? Brushes.Black : Brushes.Gray;
                        graphics.FillPath(shadeBrush, shadePath);
                    }
                }

                using (var forePath = CreateRoundRectPath(x, y, width, height, radius))
                {
                    Color fillColor = Lerp(color, Color.White, 0.92F);

                    if (fill)
                    {
                        using (var fillBrush = new LinearGradientBrush(new RectangleF(x, y, width, height), fillColor, Lerp(fillColor, Color.White, 0.9F), 90))
                        {
                            graphics.FillPath(fillBrush, forePath);
                        }
                    }
                    else if (isSelected)
                    {
                        graphics.FillPath(Brushes.White, forePath);
                    }

                    // frame
                    using (var pen = new Pen(Lerp(color, Color.White, 0.83F)))
                    {
                        if (dashedLine)
                        {
                            pen.DashPattern = dashPattern;
                        }

                        graphics.DrawPath(pen, forePath);
                    }

                    // arrow if the head is the current branch
                    if (arrowType != ArrowType.None)
                    {
                        DrawArrow(graphics, x, y, height, color, arrowType == ArrowType.Filled);
                    }
                }
            }
            finally
            {
                graphics.SmoothingMode = oldMode;
            }

            return additionalOffset;
        }


        private static void DrawArrow(Graphics graphics, float x, float y, float rowHeight, Color color, bool filled)
        {
            const float horShift = 4;
            const float verShift = 3;
            float height = rowHeight - (verShift * 2);
            float width = height / 2;

            var points = new[]
            {
                new PointF(x + horShift, y + verShift),
                new PointF(x + horShift + width, y + verShift + (height / 2)),
                new PointF(x + horShift, y + verShift + height),
                new PointF(x + horShift, y + verShift)
            };

            if (filled)
            {
                using (var solidBrush = new SolidBrush(color))
                {
                    graphics.FillPolygon(solidBrush, points);
                }
            }
            else
            {
                using (var pen = new Pen(color))
                {
                    graphics.DrawPolygon(pen, points);
                }
            }
        }

        private static GraphicsPath CreateRoundRectPath(float x, float y, float width, float height, float radius)
        {
            var path = new GraphicsPath();
            path.AddLine(x + radius, y, x + width - (radius * 2), y);
            path.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
            path.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
            path.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);
            path.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            path.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            path.AddLine(x, y + height - (radius * 2), x, y + radius);
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            path.CloseFigure();
            return path;
        }

        private static float Lerp(float start, float end, float amount)
        {
            float difference = end - start;
            float adjusted = difference * amount;
            return start + adjusted;
        }

        private static Color Lerp(Color colour, Color to, float amount)
        {
            // start colours as lerp-able floats
            float sr = colour.R, sg = colour.G, sb = colour.B;

            // end colours as lerp-able floats
            float er = to.R, eg = to.G, eb = to.B;

            // lerp the colours to get the difference
            byte r = (byte)Lerp(sr, er, amount),
                 g = (byte)Lerp(sg, eg, amount),
                 b = (byte)Lerp(sb, eb, amount);

            // return the new colour
            return Color.FromArgb(r, g, b);
        }

        private void RevisionsDoubleClick(object sender, MouseEventArgs e)
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

        public void ViewSelectedRevisions()
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Any(rev => !rev.IsArtificial))
            {
                Form ProvideForm()
                {
                    return new FormCommitDiff(UICommands, selectedRevisions[0].Guid);
                }

                UICommands.ShowModelessForm(this, false, null, null, ProvideForm);
            }
            else if (!selectedRevisions.Any())
            {
                UICommands.StartCompareRevisionsDialog(this);
            }
        }

        private void SelectionTimerTick(object sender, EventArgs e)
        {
            SelectionTimer.Enabled = false;
            SelectionTimer.Stop();
            SelectionChanged?.Invoke(this, e);
        }

        private void CreateTagToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision == null)
            {
                return;
            }

            UICommands.DoActionOnRepo(() =>
                {
                    using (var frm = new FormCreateTag(UICommands, LatestSelectedRevision))
                    {
                        return frm.ShowDialog(this) == DialogResult.OK;
                    }
                });
        }

        private void ResetCurrentBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision == null)
            {
                return;
            }

            var frm = new FormResetCurrentBranch(UICommands, LatestSelectedRevision);
            frm.ShowDialog(this);
        }

        private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision == null)
            {
                return;
            }

            UICommands.DoActionOnRepo(() =>
                {
                    var frm = new FormCreateBranch(UICommands, LatestSelectedRevision);

                    return frm.ShowDialog(this) == DialogResult.OK;
                });
        }

        private void RevisionsMouseClick(object sender, MouseEventArgs e)
        {
            var pt = Revisions.PointToClient(Cursor.Position);
            var hti = Revisions.HitTest(pt.X, pt.Y);
            _latestSelectedRowIndex = hti.RowIndex;
        }

        private void RevisionsCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            var pt = Revisions.PointToClient(Cursor.Position);
            var hti = Revisions.HitTest(pt.X, pt.Y);

            if (_latestSelectedRowIndex == hti.RowIndex)
            {
                return;
            }

            _latestSelectedRowIndex = hti.RowIndex;
            Revisions.ClearSelection();

            if (IsValidRevisionIndex(_latestSelectedRowIndex))
            {
                Revisions.Rows[_latestSelectedRowIndex].Selected = true;
            }
        }

        private void CommitClick(object sender, EventArgs e)
        {
            UICommands.StartCommitDialog(this);
        }

        private void GitIgnoreClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this, false);
        }

        internal void InvalidateRevisions()
        {
            Revisions.Invalidate();
        }

        internal void ShowCurrentBranchOnly_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ShowCurrentBranchOnly_ToolStripMenuItemChecked)
            {
                return;
            }

            AppSettings.BranchFilterEnabled = true;
            AppSettings.ShowCurrentBranchOnly = true;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        internal void ShowAllBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ShowAllBranches_ToolStripMenuItemChecked)
            {
                return;
            }

            AppSettings.BranchFilterEnabled = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        internal void ShowFilteredBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ShowFilteredBranches_ToolStripMenuItemChecked)
            {
                return;
            }

            AppSettings.BranchFilterEnabled = true;
            AppSettings.ShowCurrentBranchOnly = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        internal bool ShowCurrentBranchOnly_ToolStripMenuItemChecked { get; private set; }
        internal bool ShowAllBranches_ToolStripMenuItemChecked { get; private set; }
        internal bool ShowFilteredBranches_ToolStripMenuItemChecked { get; private set; }

        private void SetShowBranches()
        {
            ShowAllBranches_ToolStripMenuItemChecked = !AppSettings.BranchFilterEnabled;
            ShowCurrentBranchOnly_ToolStripMenuItemChecked = AppSettings.BranchFilterEnabled && AppSettings.ShowCurrentBranchOnly;
            ShowFilteredBranches_ToolStripMenuItemChecked = AppSettings.BranchFilterEnabled && !AppSettings.ShowCurrentBranchOnly;

            _branchFilter = _revisionFilter.GetBranchFilter();

            if (!AppSettings.BranchFilterEnabled)
            {
                _refFilterOptions = RefFilterOptions.All | RefFilterOptions.Boundary;
            }
            else if (AppSettings.ShowCurrentBranchOnly)
            {
                _refFilterOptions = 0;
            }
            else
            {
                _refFilterOptions = _branchFilter.Length > 0
                    ? 0
                    : RefFilterOptions.All | RefFilterOptions.Boundary;
            }

            // Apply checkboxes changes also to FormBrowse main menu
            MenuCommands.TriggerMenuChanged();
        }

        internal void FilterToolStripMenuItemClick(object sender, EventArgs e)
        {
            _revisionFilter.ShowDialog(this);
            ForceRefreshRevisions();
        }

        private void ApplyFilterFromRevisionFilterDialog()
        {
            _branchFilter = _revisionFilter.GetBranchFilter();
            SetShowBranches();
        }

        private void ContextMenuOpening(object sender, CancelEventArgs e)
        {
            if (LatestSelectedRevision == null)
            {
                return;
            }

            var inTheMiddleOfBisect = Module.InTheMiddleOfBisect();
            markRevisionAsBadToolStripMenuItem.Visible = inTheMiddleOfBisect;
            markRevisionAsGoodToolStripMenuItem.Visible = inTheMiddleOfBisect;
            bisectSkipRevisionToolStripMenuItem.Visible = inTheMiddleOfBisect;
            stopBisectToolStripMenuItem.Visible = inTheMiddleOfBisect;
            bisectSeparator.Visible = inTheMiddleOfBisect;
            compareWithCurrentBranchToolStripMenuItem.Visible = Module.GetSelectedBranch().IsNotNullOrWhitespace();

            var deleteTagDropDown = new ContextMenuStrip();
            var deleteBranchDropDown = new ContextMenuStrip();
            var checkoutBranchDropDown = new ContextMenuStrip();
            var mergeBranchDropDown = new ContextMenuStrip();
            var renameDropDown = new ContextMenuStrip();

            var revision = LatestSelectedRevision;
            var gitRefListsForRevision = new GitRefListsForRevision(revision);
            _rebaseOnTopOf = null;
            foreach (var head in gitRefListsForRevision.AllTags)
            {
                var deleteItem = new ToolStripMenuItem(head.Name) { Tag = head.Name };
                deleteItem.Click += ToolStripItemClickDeleteTag;
                deleteTagDropDown.Items.Add(deleteItem);

                var mergeItem = new ToolStripMenuItem(head.Name) { Tag = GetRefUnambiguousName(head) };
                mergeItem.Click += ToolStripItemClickMergeBranch;
                mergeBranchDropDown.Items.Add(mergeItem);
            }

            // For now there is no action that could be done on currentBranch
            string currentBranchRef = GitRefName.RefsHeadsPrefix + Module.GetSelectedBranch();
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
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Tag = GetRefUnambiguousName(head);
                    toolStripItem.Click += ToolStripItemClickMergeBranch;
                    mergeBranchDropDown.Items.Add(toolStripItem);
                    if (_rebaseOnTopOf == null)
                    {
                        _rebaseOnTopOf = toolStripItem.Tag as string;
                    }
                }
            }

            // if there is no branch to rebase on, then allow user to rebase on selected commit
            if (_rebaseOnTopOf == null && !currentBranchPointsToRevision)
            {
                _rebaseOnTopOf = revision.Guid;
            }

            // if there is no branch to merge, then let user to merge selected commit into current branch
            if (mergeBranchDropDown.Items.Count == 0 && !currentBranchPointsToRevision)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(revision.Guid);
                toolStripItem.Tag = revision.Guid;
                toolStripItem.Click += ToolStripItemClickMergeBranch;
                mergeBranchDropDown.Items.Add(toolStripItem);
                if (_rebaseOnTopOf == null)
                {
                    _rebaseOnTopOf = toolStripItem.Tag as string;
                }
            }

            var allBranches = gitRefListsForRevision.AllBranches;
            foreach (var head in allBranches)
            {
                ToolStripItem toolStripItem;

                // skip remote branches - they can not be deleted this way
                if (!head.IsRemote)
                {
                    if (head.CompleteName != currentBranchRef)
                    {
                        toolStripItem = new ToolStripMenuItem(head.Name);
                        toolStripItem.Tag = head.Name;
                        toolStripItem.Click += ToolStripItemClickDeleteBranch;
                        deleteBranchDropDown.Items.Add(toolStripItem); // Add to delete branch
                    }

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Tag = head.Name;
                    toolStripItem.Click += ToolStripItemClickRenameBranch;
                    renameDropDown.Items.Add(toolStripItem); // Add to rename branch
                }

                if (head.CompleteName != currentBranchRef)
                {
                    toolStripItem = new ToolStripMenuItem(head.Name);
                    if (head.IsRemote)
                    {
                        toolStripItem.Click += ToolStripItemClickCheckoutRemoteBranch;
                    }
                    else
                    {
                        toolStripItem.Click += ToolStripItemClickCheckoutBranch;
                    }

                    checkoutBranchDropDown.Items.Add(toolStripItem);
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

                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClickDeleteRemoteBranch;
                    deleteBranchDropDown.Items.Add(toolStripItem); // Add to delete branch
                }
            }

            bool bareRepositoryOrArtificial = Module.IsBareRepository() || revision.IsArtificial;
            deleteTagToolStripMenuItem.DropDown = deleteTagDropDown;
            deleteTagToolStripMenuItem.Enabled = deleteTagDropDown.Items.Count > 0;

            deleteBranchToolStripMenuItem.DropDown = deleteBranchDropDown;
            deleteBranchToolStripMenuItem.Enabled = deleteBranchDropDown.Items.Count > 0 && !Module.IsBareRepository();

            checkoutBranchToolStripMenuItem.DropDown = checkoutBranchDropDown;
            checkoutBranchToolStripMenuItem.Enabled = !bareRepositoryOrArtificial && checkoutBranchDropDown.Items.Count > 0 && !Module.IsBareRepository();

            mergeBranchToolStripMenuItem.DropDown = mergeBranchDropDown;
            mergeBranchToolStripMenuItem.Enabled = !bareRepositoryOrArtificial && mergeBranchDropDown.Items.Count > 0 && !Module.IsBareRepository();

            rebaseOnToolStripMenuItem.Enabled = !bareRepositoryOrArtificial && !Module.IsBareRepository();

            renameBranchToolStripMenuItem.DropDown = renameDropDown;
            renameBranchToolStripMenuItem.Enabled = renameDropDown.Items.Count > 0;

            checkoutRevisionToolStripMenuItem.Enabled = !bareRepositoryOrArtificial;
            revertCommitToolStripMenuItem.Enabled = !bareRepositoryOrArtificial;
            cherryPickCommitToolStripMenuItem.Enabled = !bareRepositoryOrArtificial;
            manipulateCommitToolStripMenuItem.Enabled = !bareRepositoryOrArtificial;

            copyToClipboardToolStripMenuItem.Enabled = !revision.IsArtificial;
            createNewBranchToolStripMenuItem.Enabled = !bareRepositoryOrArtificial;
            resetCurrentBranchToHereToolStripMenuItem.Enabled = !bareRepositoryOrArtificial;
            archiveRevisionToolStripMenuItem.Enabled = !revision.IsArtificial;
            createTagToolStripMenuItem.Enabled = !revision.IsArtificial;

            openBuildReportToolStripMenuItem.Visible = !string.IsNullOrWhiteSpace(revision.BuildStatus?.Url);

            RefreshOwnScripts();
        }

        private ISet<string> _ambiguousRefs;
        private ISet<string> AmbiguousRefs
        {
            get
            {
                if (_ambiguousRefs == null)
                {
                    _ambiguousRefs = GitRef.GetAmbiguousRefNames(LatestRefs);
                }

                return _ambiguousRefs;
            }

            set
            {
                _ambiguousRefs = value;
            }
        }

        private string GetRefUnambiguousName(IGitRef gitRef)
        {
            if (AmbiguousRefs.Contains(gitRef.Name))
            {
                return gitRef.CompleteName;
            }

            return gitRef.Name;
        }

        private void ToolStripItemClickDeleteTag(object sender, EventArgs e)
        {
            if (sender is ToolStripItem toolStripItem)
            {
                UICommands.StartDeleteTagDialog(this, toolStripItem.Tag as string);
            }
        }

        private void ToolStripItemClickDeleteBranch(object sender, EventArgs e)
        {
            if (sender is ToolStripItem toolStripItem)
            {
                UICommands.StartDeleteBranchDialog(this, toolStripItem.Tag as string);
            }
        }

        private void ToolStripItemClickDeleteRemoteBranch(object sender, EventArgs e)
        {
            if (sender is ToolStripItem toolStripItem)
            {
                UICommands.StartDeleteRemoteBranchDialog(this, toolStripItem.Text);
            }
        }

        private void ToolStripItemClickCheckoutBranch(object sender, EventArgs e)
        {
            if (sender is ToolStripItem toolStripItem)
            {
                string branch = toolStripItem.Text;
                UICommands.StartCheckoutBranch(this, branch);
            }
        }

        private void ToolStripItemClickCheckoutRemoteBranch(object sender, EventArgs e)
        {
            if (sender is ToolStripItem toolStripItem)
            {
                UICommands.StartCheckoutRemoteBranch(this, toolStripItem.Text);
            }
        }

        private void ToolStripItemClickMergeBranch(object sender, EventArgs e)
        {
            if (sender is ToolStripItem toolStripItem)
            {
                UICommands.StartMergeBranchDialog(this, toolStripItem.Tag as string);
            }
        }

        private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf != null)
            {
                UICommands.StartRebase(this, _rebaseOnTopOf);
            }
        }

        private void OnRebaseInteractivelyClicked(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf != null)
            {
                UICommands.StartInteractiveRebase(this, _rebaseOnTopOf);
            }
        }

        private void OnRebaseWithAdvOptionsClicked(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf != null)
            {
                UICommands.StartRebaseDialogWithAdvOptions(this, _rebaseOnTopOf);
            }
        }

        private void ToolStripItemClickRenameBranch(object sender, EventArgs e)
        {
            if (sender is ToolStripItem toolStripItem)
            {
                UICommands.StartRenameDialog(this, toolStripItem.Tag as string);
            }
        }

        private void CheckoutRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision != null)
            {
                string revision = LatestSelectedRevision.Guid;
                UICommands.StartCheckoutRevisionDialog(this, revision);
            }
        }

        private void ArchiveRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count > 2)
            {
                MessageBox.Show(this, "Select only one or two revisions. Abort.", "Archive revision");
                return;
            }

            GitRevision mainRevision = selectedRevisions.First();
            GitRevision diffRevision = null;
            if (selectedRevisions.Count == 2)
            {
                diffRevision = selectedRevisions.Last();
            }

            UICommands.StartArchiveDialog(this, mainRevision, diffRevision);
        }

        internal void ShowAuthorDate_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowAuthorDate = !AppSettings.ShowAuthorDate;
            ForceRefreshRevisions();
        }

        internal void OrderRevisionsByDate_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.OrderRevisionByDate = !AppSettings.OrderRevisionByDate;
            ForceRefreshRevisions();
        }

        internal void ShowRemoteBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowRemoteBranches = !AppSettings.ShowRemoteBranches;
            InvalidateRevisions();
            MenuCommands.TriggerMenuChanged(); // check/uncheck ToolStripMenuItem
        }

        internal void ShowReflogReferences_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowReflogReferences = !AppSettings.ShowReflogReferences;
            ForceRefreshRevisions();
        }

        internal void ShowSuperprojectTags_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowSuperprojectTags = !AppSettings.ShowSuperprojectTags;
            ForceRefreshRevisions();
        }

        internal void ShowSuperprojectBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowSuperprojectBranches = !AppSettings.ShowSuperprojectBranches;
            ForceRefreshRevisions();
        }

        internal void ShowSuperprojectRemoteBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowSuperprojectRemoteBranches = !AppSettings.ShowSuperprojectRemoteBranches;
            ForceRefreshRevisions();
        }

        private void RevertCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Ascending);
            foreach (var rev in revisions)
            {
                UICommands.StartRevertCommitDialog(this, rev);
            }
        }

        private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Descending);
            UICommands.StartCherryPickDialog(this, revisions);
        }

        private void FixupCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision == null)
            {
                return;
            }

            UICommands.StartFixupCommitDialog(this, LatestSelectedRevision);
        }

        private void SquashCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision == null)
            {
                return;
            }

            UICommands.StartSquashCommitDialog(this, LatestSelectedRevision);
        }

        internal void ShowRelativeDate_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.RelativeDate = !AppSettings.RelativeDate;
            ForceRefreshRevisions();
        }

        private static string TimeToString(DateTime time)
        {
            if (time == DateTime.MinValue || time == DateTime.MaxValue)
            {
                return "";
            }

            if (!AppSettings.RelativeDate)
            {
                return string.Format("{0} {1}", time.ToShortDateString(), time.ToLongTimeString());
            }

            return LocalizationHelpers.GetRelativeDateString(DateTime.Now, time, false);
        }

        private bool ShowUncommitedChanges()
        {
            return ShowUncommitedChangesIfPossible && AppSettings.RevisionGraphShowWorkingDirChanges;
        }

        private void OnRevisionRead([CanBeNull] GitRevision rev = null)
        {
            if (rev == null)
            {
                // Prune the graph and make sure the row count matches reality
                Revisions.Prune();
                return;
            }

            if (_filtredCurrentCheckout == null)
            {
                if (rev.Guid == CurrentCheckout)
                {
                    _filtredCurrentCheckout = CurrentCheckout;
                }
                else
                {
                    if (_currentCheckoutParents == null)
                    {
                        _currentCheckoutParents = GetAllParents(CurrentCheckout);
                    }

                    _filtredCurrentCheckout = _currentCheckoutParents.FirstOrDefault(parent => parent == rev.Guid);
                }
            }

            if (_filtredCurrentCheckout == rev.Guid && ShowUncommitedChanges() && !Module.IsBareRepository())
            {
                CheckUncommitedChanged(_filtredCurrentCheckout);
            }

            DvcsGraph.DataTypes dataTypes;
            if (rev.Guid == _filtredCurrentCheckout)
            {
                dataTypes = DvcsGraph.DataTypes.Active;
            }
            else if (rev.Refs.Count != 0)
            {
                dataTypes = DvcsGraph.DataTypes.Special;
            }
            else
            {
                dataTypes = DvcsGraph.DataTypes.Normal;
            }

            Revisions.Add(rev, dataTypes);
        }

        public void InvalidateCount()
        {
            _artificialStatus = null;
        }

        public void UpdateArtificialCommitCount(IReadOnlyList<GitItemStatus> status)
        {
            GitRevision unstagedRev = GetRevision(GitRevision.UnstagedGuid);
            GitRevision stagedRev = GetRevision(GitRevision.IndexGuid);
            UpdateArtificialCommitCount(status, unstagedRev, stagedRev);
        }

        private void UpdateArtificialCommitCount([CanBeNull] IReadOnlyList<GitItemStatus> status, GitRevision unstagedRev, GitRevision stagedRev)
        {
            if (status == null)
            {
                return;
            }

            if (unstagedRev != null)
            {
                var count = status.Count(item => item.Staged == StagedStatus.WorkTree);
                unstagedRev.SubjectCount = "(" + count + ") ";
            }

            if (stagedRev != null)
            {
                var count = status.Count(item => item.Staged == StagedStatus.Index);
                stagedRev.SubjectCount = "(" + count + ") ";
            }

            // cache the status, if commits do not exist or for a refresh
            _artificialStatus = status;

            Revisions.Invalidate();
        }

        private void CheckUncommitedChanged(string filtredCurrentCheckout)
        {
            var userName = Module.GetEffectiveSetting(SettingKeyString.UserName);
            var userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail);

            // Add working directory as virtual commit
            var unstagedRev = new GitRevision(GitRevision.UnstagedGuid)
            {
                Author = userName,
                AuthorDate = DateTime.MaxValue,
                AuthorEmail = userEmail,
                Committer = userName,
                CommitDate = DateTime.MaxValue,
                CommitterEmail = userEmail,
                Subject = Strings.GetCurrentUnstagedChanges(),
                ParentGuids = new[] { GitRevision.IndexGuid }
            };
            Revisions.Add(unstagedRev, DvcsGraph.DataTypes.Normal);

            // Add index as virtual commit
            var stagedRev = new GitRevision(GitRevision.IndexGuid)
            {
                Author = userName,
                AuthorDate = DateTime.MaxValue,
                AuthorEmail = userEmail,
                Committer = userName,
                CommitDate = DateTime.MaxValue,
                CommitterEmail = userEmail,
                Subject = Strings.GetCurrentIndex(),
                ParentGuids = new[] { filtredCurrentCheckout }
            };
            Revisions.Add(stagedRev, DvcsGraph.DataTypes.Normal);

            UpdateArtificialCommitCount(_artificialStatus, unstagedRev, stagedRev);
        }

        internal void DrawNonrelativesGray_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.RevisionGraphDrawNonRelativesGray = !AppSettings.RevisionGraphDrawNonRelativesGray;
            MenuCommands.TriggerMenuChanged();
            Revisions.Refresh();
        }

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
            if (LatestSelectedRevision == null)
            {
                return;
            }

            FormProcess.ShowDialog(this, Module, GitCommandHelpers.ContinueBisectCmd(bisectOption, LatestSelectedRevision.Guid), false);
            RefreshRevisions();
        }

        private void StopBisectToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, Module, GitCommandHelpers.StopBisectCmd());
            RefreshRevisions();
        }

        private void RefreshOwnScripts()
        {
            RemoveOwnScripts();
            AddOwnScripts();
        }

        private void AddOwnScripts()
        {
            var scripts = ScriptManager.GetScripts();

            if (scripts == null)
            {
                return;
            }

            int lastIndex = mainContextMenu.Items.Count;

            foreach (ScriptInfo scriptInfo in scripts)
            {
                if (scriptInfo.Enabled)
                {
                    ToolStripItem item = new ToolStripMenuItem(scriptInfo.Name);
                    item.Name = item.Text + "_ownScript";
                    item.Click += RunScript;
                    item.Image = scriptInfo.GetIcon();
                    if (scriptInfo.AddToRevisionGridContextMenu)
                    {
                        mainContextMenu.Items.Add(item);
                    }
                    else
                    {
                        runScriptToolStripMenuItem.DropDown.Items.Add(item);
                    }
                }
            }

            if (lastIndex != mainContextMenu.Items.Count)
            {
                mainContextMenu.Items.Insert(lastIndex, new ToolStripSeparator());
            }

            bool showScriptsMenu = runScriptToolStripMenuItem.DropDown.Items.Count > 0;
            runScriptToolStripMenuItem.Visible = showScriptsMenu;
        }

        private void RemoveOwnScripts()
        {
            runScriptToolStripMenuItem.DropDown.Items.Clear();
            List<ToolStripItem> list = new List<ToolStripItem>();
            foreach (ToolStripItem item in mainContextMenu.Items)
            {
                list.Add(item);
            }

            foreach (ToolStripItem item in list)
            {
                if (item.Name.Contains("_ownScript"))
                {
                    mainContextMenu.Items.RemoveByKey(item.Name);
                }
            }

            if (mainContextMenu.Items[mainContextMenu.Items.Count - 1] is ToolStripSeparator)
            {
                mainContextMenu.Items.RemoveAt(mainContextMenu.Items.Count - 1);
            }
        }

        private void RunScript(object sender, EventArgs e)
        {
            if (_settingsLoaded == false)
            {
                new FormSettings(UICommands).LoadSettings();
                _settingsLoaded = true;
            }

            if (ScriptRunner.RunScript(this, Module, sender.ToString(), this))
            {
                RefreshRevisions();
            }
        }

        #region Drag/drop patch files on revision grid

        private void Revisions_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is Array fileNameArray)
            {
                if (fileNameArray.Length > 10)
                {
                    // Some users need to be protected against themselves!
                    MessageBox.Show(this, _droppingFilesBlocked.Text);
                    return;
                }

                foreach (object fileNameObject in fileNameArray)
                {
                    var fileName = fileNameObject as string;

                    if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Start apply patch dialog for each dropped patch file...
                        UICommands.StartApplyPatchDialog(this, fileName);
                    }
                }
            }
        }

        private static void Revisions_DragEnter(object sender, DragEventArgs e)
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

        internal void ShowGitNotes_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowGitNotes = !AppSettings.ShowGitNotes;
            ForceRefreshRevisions();
        }

        internal void ShowMergeCommits_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowMergeCommits = !showMergeCommitsToolStripMenuItem.Checked;
            showMergeCommitsToolStripMenuItem.Checked = AppSettings.ShowMergeCommits;

            // hide revision graph when hiding merge commits, reasons:
            // 1, revison graph is no longer relevant, as we are not sohwing all commits
            // 2, performance hit when both revision graph and no merge commits are enabled
            if (IsGraphLayout && !AppSettings.ShowMergeCommits)
            {
                ToggleRevisionGraph();
                SetRevisionsLayout();
            }

            ForceRefreshRevisions();
        }

        internal void ShowFirstParent()
        {
            AppSettings.ShowFirstParent = !AppSettings.ShowFirstParent;

            ShowFirstParentsToggled?.Invoke(this, EventArgs.Empty);

            ForceRefreshRevisions();
        }

        private void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            GitModuleChanged?.Invoke(this, e);
        }

        private void InitRepository_Click(object sender, EventArgs e)
        {
            UICommands.StartInitializeDialog(this, Module.WorkingDir, OnModuleChanged);
        }

        private void CloneRepository_Click(object sender, EventArgs e)
        {
            if (UICommands.StartCloneDialog(this, null, OnModuleChanged))
            {
                ForceRefreshRevisions();
            }
        }

        internal void ShowRevisionGraph_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            ToggleRevisionGraph();
            SetRevisionsLayout();
            MenuCommands.TriggerMenuChanged();

            // must show MergeCommits when showing revision graph
            if (!AppSettings.ShowMergeCommits && IsGraphLayout)
            {
                AppSettings.ShowMergeCommits = true;
                showMergeCommitsToolStripMenuItem.Checked = true;
                ForceRefreshRevisions();
            }
            else
            {
                Refresh();
            }
        }

        private static void ToggleRevisionGraph()
        {
            if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.Small)
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.SmallWithGraph;
            }
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.Card)
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.CardWithGraph;
            }
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.LargeCard)
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.LargeCardWithGraph;
            }
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.SmallWithGraph)
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.Small;
            }
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.CardWithGraph)
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.Card;
            }
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.LargeCardWithGraph)
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.LargeCard;
            }
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.FilledBranchesSmall)
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.FilledBranchesSmallWithGraph;
            }
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.FilledBranchesSmallWithGraph)
            {
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.FilledBranchesSmall;
            }
        }

        internal void ShowTags_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowTags = !AppSettings.ShowTags;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ShowIds_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowIds = !AppSettings.ShowIds;
            Revisions.IdColumn.Visible = AppSettings.ShowIds;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        public void ToggleRevisionCardLayout()
        {
            var layouts = (RevisionGridLayout[])Enum.GetValues(typeof(RevisionGridLayout));

            Array.Sort(layouts);

            var maxLayout = (int)layouts[layouts.Length - 1];
            var nextLayout = AppSettings.RevisionGraphLayout + 1;

            if (nextLayout > maxLayout)
            {
                nextLayout = 1;
            }

            AppSettings.RevisionGraphLayout = nextLayout;

            SetRevisionsLayout();
        }

        private void SetRevisionsLayout()
        {
            _layout = Enum.IsDefined(typeof(RevisionGridLayout), AppSettings.RevisionGraphLayout)
                         ? (RevisionGridLayout)AppSettings.RevisionGraphLayout
                         : RevisionGridLayout.SmallWithGraph;

            // TODO why was this removed? if we only set the font when the control is created then it cannot update when settings change
            ////NormalFont = new Font(Settings.Font.Name, Settings.Font.Size + 2); // SystemFonts.DefaultFont.FontFamily, SystemFonts.DefaultFont.Size + 2);

            SetAuthoredRevisionsBrush();

            if (IsCardLayout)
            {
                _rowHeigth = AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.Card ||
                             AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.CardWithGraph
                    ? 45
                    : 70;

                if (_filledItemBrush == null)
                {
                    _filledItemBrush = new LinearGradientBrush(
                        rect: new Rectangle(0, 0, _rowHeigth, _rowHeigth),
                        color1: Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor,
                        color2: Color.LightBlue,
                        angle: 90,
                        isAngleScaleable: false);
                }

                _selectedItemBrush = _filledItemBrush;

                Revisions.ShowAuthor(!IsCardLayout);
                Revisions.SetRowHeight(_rowHeigth);
            }
            else
            {
                if (IsFilledBranchesLayout)
                {
                    using (var graphics = Graphics.FromHwnd(Handle))
                    {
                        _rowHeigth = (int)graphics.MeasureString("By", _normalFont).Height + 9;
                    }

                    _selectedItemBrush = SystemBrushes.Highlight;
                }
                else
                {
                    _rowHeigth = 25;

                    if (_filledItemBrush == null)
                    {
                        _filledItemBrush = new LinearGradientBrush(
                            rect: new Rectangle(0, 0, _rowHeigth, _rowHeigth),
                            color1: Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor,
                            color2: Color.LightBlue,
                            angle: 90,
                            isAngleScaleable: false);
                    }

                    _selectedItemBrush = _filledItemBrush;
                }

                Revisions.ShowAuthor(!IsCardLayout);
                Revisions.SetRowHeight(_rowHeigth);
            }

            // Hide graph column when there it is disabled OR when a filter is active
            // allowing for special case when history of a single file is being displayed
            if (!IsGraphLayout || (ShouldHideGraph(false) && !AllowGraphWithFilter))
            {
                Revisions.HideRevisionGraph();
            }
            else
            {
                Revisions.ShowRevisionGraph();
            }
        }

        private void SetAuthoredRevisionsBrush()
        {
            if (_authoredRevisionsBrush != null && _authoredRevisionsBrush.Color != AppSettings.AuthoredRevisionsColor)
            {
                _authoredRevisionsBrush.Dispose();
                _authoredRevisionsBrush = null;
            }

            if (_authoredRevisionsBrush == null)
            {
                _authoredRevisionsBrush = new SolidBrush(AppSettings.AuthoredRevisionsColor);
            }
        }

        private bool IsFilledBranchesLayout
            => _layout == RevisionGridLayout.FilledBranchesSmall ||
               _layout == RevisionGridLayout.FilledBranchesSmallWithGraph;

        private bool IsCardLayout
            => _layout == RevisionGridLayout.Card ||
               _layout == RevisionGridLayout.CardWithGraph ||
               _layout == RevisionGridLayout.LargeCard ||
               _layout == RevisionGridLayout.LargeCardWithGraph;

        internal bool IsGraphLayout
            => _layout == RevisionGridLayout.SmallWithGraph ||
               _layout == RevisionGridLayout.CardWithGraph ||
               _layout == RevisionGridLayout.LargeCardWithGraph ||
               _layout == RevisionGridLayout.FilledBranchesSmallWithGraph;

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "RevisionGrid";

        internal enum Commands
        {
            ToggleRevisionGraph = 0,
            RevisionFilter = 1,
            ToggleAuthorDateCommitDate = 2,
            ToggleOrderRevisionsByDate = 3,
            ToggleShowRelativeDate = 4,
            ToggleDrawNonRelativesGray = 5,
            ToggleShowGitNotes = 6,
            ToggleRevisionCardLayout = 7,
            ToggleShowMergeCommits = 8,
            ShowAllBranches = 9,
            ShowCurrentBranchOnly = 10,
            ShowFilteredBranches = 11,
            ShowRemoteBranches = 12,
            ShowFirstParent = 13,
            GoToParent = 14,
            GoToChild = 15,
            ToggleHighlightSelectedBranch = 16,
            NextQuickSearch = 17,
            PrevQuickSearch = 18,
            SelectCurrentRevision = 19,
            GoToCommit = 20,
            NavigateBackward = 21,
            NavigateForward = 22,
            SelectAsBaseToCompare = 23,
            CompareToBase = 24,
            CreateFixupCommit = 25,
            ToggleShowTags = 26,
            ToggleBranchTreePanel = 27
        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.ToggleRevisionGraph: ShowRevisionGraph_ToolStripMenuItemClick(null, null); break;
                case Commands.RevisionFilter: FilterToolStripMenuItemClick(null, null); break;
                case Commands.ToggleAuthorDateCommitDate: ShowAuthorDate_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleOrderRevisionsByDate: OrderRevisionsByDate_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleShowRelativeDate: ShowRelativeDate_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleDrawNonRelativesGray: DrawNonrelativesGray_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleShowGitNotes: ShowGitNotes_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleRevisionCardLayout: ToggleRevisionCardLayout(); break;
                case Commands.ToggleShowMergeCommits: ShowMergeCommits_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleShowTags: ShowTags_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowAllBranches: ShowAllBranches_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowCurrentBranchOnly: ShowCurrentBranchOnly_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowFilteredBranches: ShowFilteredBranches_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowRemoteBranches: ShowRemoteBranches_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowFirstParent: ShowFirstParent(); break;
                case Commands.SelectCurrentRevision: SetSelectedRevision(new GitRevision(CurrentCheckout)); break;
                case Commands.GoToCommit: MenuCommands.GotoCommitExcecute(); break;
                case Commands.GoToParent: goToParentToolStripMenuItem_Click(null, null); break;
                case Commands.GoToChild: goToChildToolStripMenuItem_Click(null, null); break;
                case Commands.ToggleHighlightSelectedBranch: ToggleHighlightSelectedBranch(); break;
                case Commands.NextQuickSearch: NextQuickSearch(true); break;
                case Commands.PrevQuickSearch: NextQuickSearch(false); break;
                case Commands.NavigateBackward: NavigateBackward(); break;
                case Commands.NavigateForward: NavigateForward(); break;
                case Commands.SelectAsBaseToCompare: selectAsBaseToolStripMenuItem_Click(null, null); break;
                case Commands.CompareToBase: compareToBaseToolStripMenuItem_Click(null, null); break;
                case Commands.CreateFixupCommit: FixupCommitToolStripMenuItemClick(null, null); break;
                case Commands.ToggleBranchTreePanel: ToggleBranchTreePanel(); break;
                default:
                    {
                        bool result = base.ExecuteCommand(cmd);
                        RefreshRevisions();
                        return result;
                    }
            }

            return true;
        }

        #endregion

        internal bool ExecuteCommand(Commands cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        private void NextQuickSearch(bool down)
        {
            var curIndex = -1;
            if (Revisions.SelectedRows.Count > 0)
            {
                curIndex = Revisions.SelectedRows[0].Index;
            }

            RestartQuickSearchTimer();

            bool reverse = !down;
            var nextIndex = 0;
            if (curIndex >= 0)
            {
                nextIndex = reverse ? curIndex - 1 : curIndex + 1;
            }

            _quickSearchString = _lastQuickSearchString;
            FindNextMatch(nextIndex, _quickSearchString, reverse);
            ShowQuickSearchString();
        }

        private void ToggleHighlightSelectedBranch()
        {
            if (_revisionReader != null)
            {
                MessageBox.Show(_cannotHighlightSelectedBranch.Text);
                return;
            }

            var revision = GetSelectedRevisions().FirstOrDefault();

            if (revision != null)
            {
                HighlightBranch(revision.Guid);
                Refresh();
            }
        }

        private void deleteBranchTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if (item.DropDown != null && item.DropDown.Items.Count == 1)
            {
                item.DropDown.Items[0].PerformClick();
            }
        }

        private void goToParentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = LatestSelectedRevision;
            if (r != null)
            {
                if (_parentChildNavigationHistory.HasPreviousParent)
                {
                    _parentChildNavigationHistory.NavigateToPreviousParent(r.Guid);
                }
                else if (r.HasParent)
                {
                    _parentChildNavigationHistory.NavigateToParent(r.Guid, r.FirstParentGuid);
                }
            }
        }

        private void goToChildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = LatestSelectedRevision;
            if (r != null)
            {
                var children = GetRevisionChildren(r.Guid);

                if (_parentChildNavigationHistory.HasPreviousChild)
                {
                    _parentChildNavigationHistory.NavigateToPreviousChild(r.Guid);
                }
                else if (children.Any())
                {
                    _parentChildNavigationHistory.NavigateToChild(r.Guid, children[0]);
                }
            }
        }

        public void GoToRef(string refName, bool showNoRevisionMsg)
        {
            string sha1;
            if (DetachedHeadParser.TryParse(refName, out sha1))
            {
                refName = sha1;
            }

            var revisionGuid = Module.RevParse(refName);
            if (revisionGuid != null)
            {
                if (_isLoading || !SetSelectedRevision(new GitRevision(revisionGuid.ToString())))
                {
                    _initialSelectedRevision = revisionGuid.ToString();
                    Revisions.SelectedIds = null;
                    _lastSelectedRows = null;
                }
            }
            else if (showNoRevisionMsg)
            {
                MessageBox.Show((ParentForm as IWin32Window) ?? this, _noRevisionFoundError.Text);
            }
        }

        internal Keys GetShortcutKeys(Commands cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        private void CompareToBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var headCommit = GetSelectedRevisions().First();
            using (var form = new FormCompareToBranch(UICommands, headCommit.Guid))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var baseCommit = Module.RevParse(form.BranchName);
                    UICommands.ShowFormDiff(IsFirstParentValid(), baseCommit, headCommit.ObjectId,
                        form.BranchName, headCommit.Subject);
                }
            }
        }

        private void CompareWithCurrentBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var baseCommit = GetSelectedRevisions().First();
            var headBranch = Module.GetSelectedBranch();
            var headBranchName = Module.RevParse(headBranch);
            UICommands.ShowFormDiff(IsFirstParentValid(), baseCommit.ObjectId, headBranchName,
                baseCommit.Subject, headBranch);
        }

        private void selectAsBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _baseCommitToCompare = GetSelectedRevisions().First();
            compareToBaseToolStripMenuItem.Enabled = true;
        }

        private void compareToBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_baseCommitToCompare == null)
            {
                MessageBox.Show(this, _baseForCompareNotSelectedError.Text, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var headCommit = GetSelectedRevisions().First();
            UICommands.ShowFormDiff(IsFirstParentValid(), _baseCommitToCompare.ObjectId, headCommit.ObjectId,
                _baseCommitToCompare.Subject, headCommit.Subject);
        }

        private void getHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenManual();
        }

        private static void OpenManual()
        {
            string url = UserManual.UserManual.UrlFor("modify_history", "using-autosquash-rebase-feature");
            OsShellUtil.OpenUrlInDefaultBrowser(url);
        }

        private void openBuildReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var revision = GetSelectedRevisions().First();

            if (!string.IsNullOrWhiteSpace(revision.BuildStatus?.Url))
            {
                Process.Start(revision.BuildStatus.Url);
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
            if (LatestSelectedRevision == null)
            {
                return;
            }

            string rebaseCmd = GitCommandHelpers.RebaseCmd(LatestSelectedRevision.FirstParentGuid,
                interactive: true, preserveMerges: false, autosquash: false, autostash: true);

            using (var formProcess = new FormProcess(null, rebaseCmd, Module.WorkingDir, null, true))
            {
                formProcess.ProcessEnvVariables.Add("GIT_SEQUENCE_EDITOR", string.Format("sed -i -re '0,/pick/s//{0}/'", command));
                formProcess.ShowDialog(this);
            }

            RefreshRevisions();
        }
    }
}
