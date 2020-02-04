using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI
{
    public delegate string DescribeRevisionDelegate(ObjectId objectId);

    public sealed partial class FileStatusList : GitModuleControl
    {
        private static readonly TimeSpan SelectedIndexChangeThrottleDuration = TimeSpan.FromMilliseconds(50);
        private readonly TranslationString _diffWithParent = new TranslationString("Diff with: a/");
        public readonly TranslationString CombinedDiff = new TranslationString("Combined Diff");
        private readonly IGitRevisionTester _revisionTester;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly SortDiffListContextMenuItem _sortByContextMenu;
        private int _nextIndexToSelect = -1;
        private bool _groupByRevision;
        private bool _enableSelectedIndexChangeEvent = true;
        private bool _mouseEntered;
        private ToolStripItem _openSubmoduleMenuItem;
        private Rectangle _dragBoxFromMouseDown;
        private IReadOnlyList<(GitRevision revision, IReadOnlyList<GitItemStatus> statuses)> _itemsWithParent = Array.Empty<(GitRevision, IReadOnlyList<GitItemStatus>)>();
        [CanBeNull] private IDisposable _selectedIndexChangeSubscription;
        [CanBeNull] private IDisposable _diffListSortSubscription;

        private bool _updatingColumnWidth;

        public delegate void EnterEventHandler(object sender, EnterEventArgs e);

        public event EventHandler SelectedIndexChanged;
        public event EventHandler DataSourceChanged;

        public new event EventHandler DoubleClick;
        public new event KeyEventHandler KeyDown;
        public new event EnterEventHandler Enter;

        public FileStatusList()
        {
            InitializeComponent();
            InitialiseFiltering();
            CreateOpenSubmoduleMenuItem();
            _sortByContextMenu = CreateSortByContextMenuItem();
            SetupUnifiedDiffListSorting();
            lblSplitter.Height = DpiUtil.Scale(1);
            InitializeComplete();
            FilterVisible = true;

            SelectFirstItemOnSetItems = true;

            FileStatusListView.SmallImageList = CreateImageList();
            FileStatusListView.LargeImageList = CreateImageList();
            FileStatusListView.AllowCollapseGroups = true;
            FileStatusListView.Scroll += FileStatusListView_Scroll;

            HandleVisibility_NoFilesLabel_FilterComboBox(filesPresent: true);
            Controls.SetChildIndex(NoFiles, 0);
            NoFiles.Font = new Font(NoFiles.Font, FontStyle.Italic);
            FilterWatermarkLabel.Font = new Font(FilterWatermarkLabel.Font, FontStyle.Italic);
            FilterComboBox.Font = new Font(FilterComboBox.Font, FontStyle.Bold);

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _revisionTester = new GitRevisionTester(_fullPathResolver);

            base.Enter += FileStatusList_Enter;

            return;

            ImageList CreateImageList()
            {
                const int rowHeight = 18;

                return new ImageList
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = DpiUtil.Scale(new Size(16, rowHeight)), // Scale ImageSize and images scale automatically
                    Images =
                    {
                        ScaleHeight(Images.FileStatusRemoved), // 0
                        ScaleHeight(Images.FileStatusAdded), // 1
                        ScaleHeight(Images.FileStatusModified), // 2
                        ScaleHeight(Images.FileStatusRenamed.AdaptLightness()), // 3
                        ScaleHeight(Images.FileStatusCopied), // 4
                        ScaleHeight(Images.SubmoduleDirty), // 5
                        ScaleHeight(Images.SubmoduleRevisionUp), // 6
                        ScaleHeight(Images.SubmoduleRevisionUpDirty), // 7
                        ScaleHeight(Images.SubmoduleRevisionDown), // 8
                        ScaleHeight(Images.SubmoduleRevisionDownDirty), // 9
                        ScaleHeight(Images.SubmoduleRevisionSemiUp), // 10
                        ScaleHeight(Images.SubmoduleRevisionSemiUpDirty), // 11
                        ScaleHeight(Images.SubmoduleRevisionSemiDown), // 12
                        ScaleHeight(Images.SubmoduleRevisionSemiDownDirty), // 13
                        ScaleHeight(Images.FileStatusUnknown) // 14
                    }
                };

                Bitmap ScaleHeight(Bitmap input)
                {
                    Debug.Assert(input.Height < rowHeight, "Can only increase row height");
                    var scaled = new Bitmap(input.Width, rowHeight, input.PixelFormat);
                    using (var g = Graphics.FromImage(scaled))
                    {
                        g.DrawImageUnscaled(input, 0, (rowHeight - input.Height) / 2);
                    }

                    return scaled;
                }
            }
        }

        private void SetupUnifiedDiffListSorting()
        {
            // cleanup the previous subscription if it exists.
            _diffListSortSubscription?.Dispose();

            _diffListSortSubscription = DiffListSortService.Instance.CurrentAndFutureSorting()
                .Do(sortingMethod =>
                {
                    switch (sortingMethod)
                    {
                        case DiffListSortType.FilePath:
                            SortByFilePath();
                            break;

                        case DiffListSortType.FileExtension:
                            SortByFileExtension();
                            break;

                        case DiffListSortType.FileStatus:
                            SortByFileStatus();
                            break;

                        default:
                            throw new NotSupportedException(sortingMethod.ToString() + " is not a supported sorting method.");
                    }
                })
                .Catch<DiffListSortType, NotSupportedException>(e =>
                {
                    // TODO log the error can we display it to the user somehow?
                    return Observable.Empty<DiffListSortType>();
                })
                .Subscribe();
        }

        private static SortDiffListContextMenuItem CreateSortByContextMenuItem()
        {
            return new SortDiffListContextMenuItem(DiffListSortService.Instance)
            {
                Name = "sortListByContextMenuItem"
            };
        }

        // Properties

        [Browsable(false)]
        public IEnumerable<GitItemStatus> AllItems =>
            FileStatusListView.ItemTags<GitItemStatus>();

        public int AllItemsCount => FileStatusListView.Items.Count;

        public override ContextMenu ContextMenu
        {
            get => FileStatusListView.ContextMenu;
            set => FileStatusListView.ContextMenu = value;
        }

        public override ContextMenuStrip ContextMenuStrip
        {
            get { return FileStatusListView.ContextMenuStrip; }
            set
            {
                FileStatusListView.ContextMenuStrip = value;
                if (FileStatusListView.ContextMenuStrip != null)
                {
                    FileStatusListView.ContextMenuStrip.Opening += FileStatusListView_ContextMenu_Opening;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DescribeRevisionDelegate DescribeRevision { get; set; }

        public bool FilterFocused => FilterComboBox.Focused;

        public bool ShouldSerializeFilterVisible => FilterVisible != true;

        public bool FilterVisible
        {
            get { return _filterVisible; }
            set
            {
                if (value == _filterVisible)
                {
                    return;
                }

                _filterVisible = value;
                FilterVisibleInternal = value;
            }
        }

        private bool FilterVisibleInternal
        {
            get => FilterComboBox.Visible;
            set
            {
                FilterComboBox.Visible = value;
                SetDeleteFilterButtonVisibility();
                SetFilterWatermarkLabelVisibility();

                int top = value
                    ? FileStatusListView.Margin.Top + FilterComboBox.Bottom + FilterComboBox.Margin.Bottom
                    : FileStatusListView.Margin.Top;

                int height = ClientRectangle.Height - FileStatusListView.Margin.Bottom - top;

                FileStatusListView.SetBounds(0, top, 0, height, BoundsSpecified.Y | BoundsSpecified.Height);
            }
        }

        public override bool Focused => FileStatusListView.Focused;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<GitItemStatus> GitItemFilteredStatuses =>
            AllItems.AsReadOnlyList();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<GitItemStatus> GitItemStatuses
        {
            get
            {
                return GitItemStatusesWithParents?.SelectMany(tuple => tuple.statuses).AsReadOnlyList()
                       ?? Array.Empty<GitItemStatus>();
            }
        }

        // Parents is used as the "first selected" (not always the parent) for GitItemStatus
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<(GitRevision revision, IReadOnlyList<GitItemStatus> statuses)> GitItemStatusesWithParents
        {
            get { return _itemsWithParent; }
            private set
            {
                _itemsWithParent = value ?? throw new ArgumentNullException(nameof(value));
                UpdateFileStatusListView();
            }
        }

        public bool GroupByRevision
        {
            get => _groupByRevision;
            set => _groupByRevision = value;
        }

        [DefaultValue(true)]
        public bool MultiSelect
        {
            get => FileStatusListView.MultiSelect;
            set => FileStatusListView.MultiSelect = value;
        }

        [Browsable(false)]
        [DefaultValue(true)]
        public bool IsEmpty => GitItemStatuses == null || !GitItemStatuses.Any();

        /// <summary>
        /// Gets or sets the revision.
        /// </summary>
        /// <value>The revision.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitRevision Revision { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int SelectedIndex
        {
            get
            {
                foreach (int i in FileStatusListView.SelectedIndices)
                {
                    return i;
                }

                return -1;
            }
            set => SelectItems(item => item.Index == value);
        }

        [CanBeNull]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitItemStatus SelectedItem
        {
            get => FileStatusListView.LastSelectedItem()?.Tag<GitItemStatus>();
            set
            {
                var itemToBeSelected = GetItemByStatus(value);
                SelectItems(item => item == itemToBeSelected);
                return;

                ListViewItem GetItemByStatus(GitItemStatus status)
                {
                    if (status == null)
                    {
                        return null;
                    }

                    ListViewItem newSelected = null;
                    foreach (ListViewItem item in FileStatusListView.Items)
                    {
                        var gitItemStatus = item.Tag<GitItemStatus>();

                        if (status.CompareTo(gitItemStatus) == 0)
                        {
                            if (newSelected == null)
                            {
                                newSelected = item;
                            }
                            else if (gitItemStatus == status)
                            {
                                return item;
                            }
                        }
                    }

                    return newSelected;
                }
            }
        }

        [CanBeNull]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitRevision SelectedItemParent
            => FileStatusListView.LastSelectedItem()?.Group?.Tag<GitRevision>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<GitRevision> SelectedItemParents
            => FileStatusListView.SelectedItems()
                .Select(i => i.Group?.Tag<GitRevision>())
                .Where(r => r != null)
                .Distinct(); // Parents should be distinct to avoid iterating same items

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<GitItemStatus> SelectedItems
        {
            get => FileStatusListView.SelectedItemTags<GitItemStatus>();
            set
            {
                if (value == null)
                {
                    ClearSelected();
                    return;
                }

                SelectItems(item => value.Contains(item.Tag<GitItemStatus>()));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<GitItemStatusWithParent> SelectedItemsWithParent
            => FileStatusListView.SelectedItems()
                .Where(i => i.Group?.Tag is GitRevision)
                .Select(i => new GitItemStatusWithParent(i.Group.Tag<GitRevision>(), i.Tag<GitItemStatus>()));

        [DefaultValue(true)]
        public bool SelectFirstItemOnSetItems { get; set; }

        public int UnfilteredItemsCount => GitItemStatusesWithParents?.Sum(tuple => tuple.statuses.Count) ?? 0;

        // Public methods

        public void ClearSelected()
        {
            foreach (ListViewItem item in FileStatusListView.SelectedItems)
            {
                item.Selected = false;
            }
        }

        public new void Focus()
        {
            if (FileStatusListView.Items.Count > 0)
            {
                if (SelectedItem == null)
                {
                    SelectedIndex = 0;
                }

                FileStatusListView.Focus();
            }
        }

        private static (Image image, string prefix, string text, string suffix, int prefixTextStartX, int textWidth, int textMaxWidth)
            FormatListViewItem(ListViewItem item, PathFormatter formatter, int itemWidth)
        {
            var gitItemStatus = item.Tag<GitItemStatus>();
            var image = item.Image();
            int itemLeft = item.Position.X;

            var prefixTextStartX = itemLeft + (image?.Width ?? 0);
            var textMaxWidth = itemWidth - prefixTextStartX;
            var (prefix, text, suffix, textWidth) = formatter.FormatTextForDrawing(textMaxWidth, gitItemStatus.Name, gitItemStatus.OldName);
            text = AppendItemSubmoduleStatus(text, gitItemStatus);

            return (image, prefix, text, suffix, prefixTextStartX, textWidth, textMaxWidth);
        }

        public int GetNextIndex(bool searchBackward, bool loop)
        {
            int curIdx = SelectedIndex;

            if (curIdx < 0)
            {
                return -1;
            }

            ListViewItem currentItem = FileStatusListView.Items[curIdx];
            var currentGroup = currentItem.Group;

            if (searchBackward)
            {
                var nextItem = FindPrevItemInGroups();
                if (nextItem == null)
                {
                    return loop ? GetLastIndex() : curIdx;
                }

                return nextItem.Index;
            }
            else
            {
                var nextItem = FindNextItemInGroups();
                if (nextItem == null)
                {
                    return loop ? 0 : curIdx;
                }

                return nextItem.Index;
            }

            ListViewItem FindPrevItemInGroups()
            {
                var searchInGroups = new List<ListViewGroup>();
                var foundCurrentGroup = false;
                for (var i = FileStatusListView.Groups.Count - 1; i >= 0; i--)
                {
                    if (FileStatusListView.Groups[i] == currentGroup)
                    {
                        foundCurrentGroup = true;
                    }

                    if (foundCurrentGroup)
                    {
                        searchInGroups.Add(FileStatusListView.Groups[i]);
                    }
                }

                var idx = curIdx;
                foreach (var grp in searchInGroups)
                {
                    for (var i = idx - 1; i >= 0; i--)
                    {
                        if (FileStatusListView.Items[i].Group == grp)
                        {
                            return FileStatusListView.Items[i];
                        }
                    }

                    idx = FileStatusListView.Items.Count;
                }

                return null;
            }

            ListViewItem FindNextItemInGroups()
            {
                var searchInGroups = new List<ListViewGroup>();
                var foundCurrentGroup = false;
                for (var i = 0; i < FileStatusListView.Groups.Count; i++)
                {
                    if (FileStatusListView.Groups[i] == currentGroup)
                    {
                        foundCurrentGroup = true;
                    }

                    if (foundCurrentGroup)
                    {
                        searchInGroups.Add(FileStatusListView.Groups[i]);
                    }
                }

                var idx = curIdx;
                foreach (var grp in searchInGroups)
                {
                    for (var i = idx + 1; i < FileStatusListView.Items.Count; i++)
                    {
                        if (FileStatusListView.Items[i].Group == grp)
                        {
                            return FileStatusListView.Items[i];
                        }
                    }

                    idx = -1;
                }

                return null;
            }

            int GetLastIndex()
            {
                if (FileStatusListView.Items.Count == 0)
                {
                    return -1;
                }

                if (FileStatusListView.Groups.Count < 2)
                {
                    return FileStatusListView.Items.Count - 1;
                }

                ListViewGroup lastNonEmptyGroup = null;
                for (int i = FileStatusListView.Groups.Count - 1; i >= 0; i--)
                {
                    if (FileStatusListView.Groups[i].Items.Count > 0)
                    {
                        lastNonEmptyGroup = FileStatusListView.Groups[i];
                        break;
                    }
                }

                for (int i = FileStatusListView.Items.Count - 1; i >= 0; i--)
                {
                    if (FileStatusListView.Items[i].Group == lastNonEmptyGroup)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        public void SelectAll() => SelectItems(_ => true);

        public void SelectFirstVisibleItem()
        {
            if (FileStatusListView.Items.Count == 0)
            {
                return;
            }

            var group = FileStatusListView.Groups().FirstOrDefault(gr => gr.Items.Count > 0);
            if (group != null)
            {
                ListViewItem sortedFirstGroupItem = FileStatusListView.Items().FirstOrDefault(item => item.Group == group);
                if (sortedFirstGroupItem != null)
                {
                    SelectedIndex = sortedFirstGroupItem.Index;
                }
            }
            else
            {
                SelectedIndex = 0;
            }
        }

        public void SelectStoredNextIndex(int defaultIndex = -1)
        {
            _nextIndexToSelect = Math.Min(_nextIndexToSelect, FileStatusListView.Items.Count - 1);
            if (_nextIndexToSelect < 0 && defaultIndex > -1)
            {
                _nextIndexToSelect = Math.Min(defaultIndex, FileStatusListView.Items.Count - 1);
            }

            if (_nextIndexToSelect > -1)
            {
                SelectedIndex = _nextIndexToSelect;
            }

            _nextIndexToSelect = -1;
        }

        public void SetDiffs(IReadOnlyList<GitRevision> revisions)
        {
            Revision = revisions.FirstOrDefault();

            var tuples = new List<(GitRevision revision, IReadOnlyList<GitItemStatus> statuses)>();

            if (Revision != null)
            {
                GitRevision[] parentRevs;
                if (revisions.Count == 1)
                {
                    // Get the parents for the selected revision
                    parentRevs = Revision.ParentIds?.Select(item => new GitRevision(item)).ToArray();
                }
                else if (revisions.Count == 2 || revisions.Count > 4)
                {
                    // only first -> selected is interesting
                    parentRevs = new[] { revisions.Last() };
                    if (AppSettings.ShowDiffForAllParents)
                    {
                        // Get base commit, add as parent if unique
                        Lazy<ObjectId> head = new Lazy<ObjectId>(() => Module.RevParse("HEAD"));
                        var revA = parentRevs[0].ObjectId;
                        var revB = Revision.ObjectId;
                        ObjectId baseRevGuid = Module.GetMergeBase(GetRevisionOrHead(revA, head), GetRevisionOrHead(revB, head));
                        if (baseRevGuid != null
                            && baseRevGuid != revA
                            && baseRevGuid != revB)
                        {
                            Array.Resize(ref parentRevs, 2);
                            parentRevs[1] = new GitRevision(baseRevGuid);
                        }
                    }
                }
                else
                {
                    // Limited selections: Show multi selection
                    parentRevs = revisions.Skip(1).ToArray();
                }

                if (parentRevs == null || parentRevs.Length == 0)
                {
                    // No parent, will set null as parent
                    tuples.Add((null, Module.GetTreeFiles(Revision.TreeGuid, full: true)));
                }
                else
                {
                    if (!AppSettings.ShowDiffForAllParents)
                    {
                        parentRevs = new[] { parentRevs[0] };
                    }

                    foreach (var rev in parentRevs)
                    {
                        tuples.Add((rev, Module.GetDiffFilesWithSubmodulesStatus(rev.Guid, Revision.Guid, Revision.ParentIds?.FirstOrDefault()?.ToString())));
                    }

                    // Show combined (merge conflicts) only when all first (A) are parents to selected (B)
                    // (a single merge commit is selected with parents explicit or implicit selected)
                    var isMergeCommit = AppSettings.ShowDiffForAllParents &&
                                        Revision.ParentIds != null && Revision.ParentIds.Count > 1 &&
                                        _revisionTester.AllFirstAreParentsToSelected(parentRevs, Revision);
                    if (isMergeCommit)
                    {
                        var conflicts = Module.GetCombinedDiffFileList(Revision.Guid);
                        if (conflicts.Count != 0)
                        {
                            // Create an artificial commit
                            tuples.Add((new GitRevision(ObjectId.CombinedDiffId), conflicts));
                        }
                    }
                }
            }

            GitItemStatusesWithParents = tuples;

            return;

            ObjectId GetRevisionOrHead(ObjectId rev, Lazy<ObjectId> head)
            {
                return rev.IsArtificial ? head.Value : rev;
            }
        }

        public void SetDiffs(GitRevision selectedRev = null, GitRevision parentRev = null, IReadOnlyList<GitItemStatus> items = null)
        {
            Revision = selectedRev;
            GitItemStatusesWithParents = items == null
                ? Array.Empty<(GitRevision, IReadOnlyList<GitItemStatus>)>()
                : new[] { (parentRev, items) };
        }

        public void SetNoFilesText(string text)
        {
            NoFiles.Text = text;
        }

        public void SetSelectedIndex(int idx, bool notify)
        {
            _enableSelectedIndexChangeEvent = notify;
            SelectedIndex = idx;
            _enableSelectedIndexChangeEvent = true;
        }

        public int SetSelectionFilter(string selectionFilter)
        {
            var regex = RegexForSelecting(selectionFilter);
            SelectItems(item => regex.IsMatch(item.Name));
            return FileStatusListView.SelectedIndices.Count;

            Regex RegexForSelecting(string value)
            {
                return string.IsNullOrEmpty(value)
                    ? new Regex("^$", RegexOptions.Compiled)
                    : new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public void StoreNextIndexToSelect()
        {
            _nextIndexToSelect = -1;
            foreach (int idx in FileStatusListView.SelectedIndices)
            {
                if (idx > _nextIndexToSelect)
                {
                    _nextIndexToSelect = idx;
                }
            }

            _nextIndexToSelect = _nextIndexToSelect - FileStatusListView.SelectedIndices.Count + 1;
        }

        // Protected methods

        protected override void DisposeCustomResources()
        {
            _selectedIndexChangeSubscription?.Dispose();
            _diffListSortSubscription?.Dispose();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                _mouseEntered = !Focused;
            }

            base.WndProc(ref m);
        }

        // Private methods

        private void CreateOpenSubmoduleMenuItem()
        {
            _openSubmoduleMenuItem = new ToolStripMenuItem
            {
                Name = "openSubmoduleMenuItem",
                Tag = "1",
                Text = "Open with Git Extensions",
                Image = Images.GitExtensionsLogo16
            };
            _openSubmoduleMenuItem.Click += (s, ea) => { ThreadHelper.JoinableTaskFactory.RunAsync(() => OpenSubmoduleAsync()); };
        }

        private static string AppendItemSubmoduleStatus(string text, GitItemStatus item)
        {
            if (item.IsSubmodule &&
                item.GetSubmoduleStatusAsync() != null &&
                item.GetSubmoduleStatusAsync().IsCompleted &&
                item.GetSubmoduleStatusAsync().CompletedResult() != null)
            {
                text += item.GetSubmoduleStatusAsync().CompletedResult().AddedAndRemovedString();
            }

            return text;
        }

        private async Task OpenSubmoduleAsync()
        {
            var submoduleName = SelectedItem.Name;

            var status = await SelectedItem.GetSubmoduleStatusAsync().ConfigureAwait(false);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "browse -commit=" + status.Commit,
                    WorkingDirectory = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator())
                }
            };

            process.Start();
        }

        private void SelectItems(Func<ListViewItem, bool> predicate)
        {
            try
            {
                FileStatusListView.BeginUpdate();

                ListViewItem firstSelectedItem = null;
                foreach (var item in FileStatusListView.Items())
                {
                    item.Selected = predicate(item);
                    if (item.Selected && firstSelectedItem == null)
                    {
                        firstSelectedItem = item;
                    }
                }

                if (firstSelectedItem != null)
                {
                    firstSelectedItem.Focused = true;
                    firstSelectedItem.EnsureVisible();
                }
            }
            finally
            {
                FileStatusListView.EndUpdate();
            }

            StoreNextIndexToSelect();
        }

        private void SetDeleteFilterButtonVisibility()
        {
            DeleteFilterButton.Visible = FilterVisibleInternal && !FilterComboBox.Text.IsNullOrEmpty();
        }

        private void SetFilterWatermarkLabelVisibility()
        {
            FilterWatermarkLabel.Visible = FilterVisibleInternal && !FilterComboBox.Focused && string.IsNullOrEmpty(FilterComboBox.Text);
        }

        private void UpdateFileStatusListView(bool updateCausedByFilter = false)
        {
            if (!GitItemStatuses.Any())
            {
                HandleVisibility_NoFilesLabel_FilterComboBox(filesPresent: false);
            }
            else
            {
                EnsureSelectedIndexChangeSubscription();
                HandleVisibility_NoFilesLabel_FilterComboBox(filesPresent: true);
            }

            HashSet<GitItemStatus> previouslySelectedItems = null;

            if (updateCausedByFilter)
            {
                previouslySelectedItems = FileStatusListView.SelectedItems()
                    .ToHashSet(i => i.Tag<GitItemStatus>());

                DataSourceChanged?.Invoke(this, EventArgs.Empty);
            }

            FileStatusListView.BeginUpdate();
            FileStatusListView.ShowGroups = GitItemStatusesWithParents.Count > 1 || _groupByRevision;
            FileStatusListView.Groups.Clear();
            FileStatusListView.Items.Clear();

            var list = new List<ListViewItem>();
            foreach (var (revision, statuses) in GitItemStatusesWithParents)
            {
                ListViewGroup group = null;
                if (revision != null)
                {
                    string groupName;
                    if (revision.Guid == GitRevision.CombinedDiffGuid)
                    {
                        groupName = CombinedDiff.Text;
                    }
                    else
                    {
                        groupName = _diffWithParent.Text + GetDescriptionForRevision(revision.ObjectId);
                    }

                    group = new ListViewGroup(groupName)
                    {
                        Tag = revision
                    };

                    FileStatusListView.Groups.Add(group);
                }

                foreach (var item in statuses)
                {
                    if (!IsFilterMatch(item))
                    {
                        continue;
                    }

                    var listItem = new ListViewItem(string.Empty, group)
                    {
                        ImageIndex = GetItemImageIndex(item)
                    };

                    if (item.GetSubmoduleStatusAsync() != null && !item.GetSubmoduleStatusAsync().IsCompleted)
                    {
                        var capturedItem = item;

                        ThreadHelper.JoinableTaskFactory.RunAsync(
                            async () =>
                            {
                                await capturedItem.GetSubmoduleStatusAsync();

                                await this.SwitchToMainThreadAsync();

                                listItem.ImageIndex = GetItemImageIndex(capturedItem);
                            });
                    }

                    if (previouslySelectedItems?.Contains(item) == true)
                    {
                        listItem.Selected = true;
                    }

                    listItem.Tag = item;
                    list.Add(listItem);
                }
            }

            FileStatusListView.Items.AddRange(list.ToArray());

            if (updateCausedByFilter == false)
            {
                FileStatusListView_SelectedIndexChanged();
                DataSourceChanged?.Invoke(this, EventArgs.Empty);
                if (SelectFirstItemOnSetItems)
                {
                    SelectFirstVisibleItem();
                }
            }

            FileStatusListView.EndUpdate();
            UpdateColumnWidth();
            return;

            void EnsureSelectedIndexChangeSubscription()
            {
                if (_selectedIndexChangeSubscription == null)
                {
                    _selectedIndexChangeSubscription = Observable.FromEventPattern(
                            h => FileStatusListView.SelectedIndexChanged += h,
                            h => FileStatusListView.SelectedIndexChanged -= h)
                        .Where(x => _enableSelectedIndexChangeEvent)
                        .Throttle(SelectedIndexChangeThrottleDuration, MainThreadScheduler.Instance)
                        .ObserveOn(MainThreadScheduler.Instance)
                        .Subscribe(_ => FileStatusListView_SelectedIndexChanged());
                }
            }

            int GetItemImageIndex(GitItemStatus gitItemStatus)
            {
                if (gitItemStatus.IsDeleted)
                {
                    return 0;
                }

                if (gitItemStatus.IsNew || (!gitItemStatus.IsTracked && !gitItemStatus.IsSubmodule))
                {
                    return 1;
                }

                if (gitItemStatus.IsChanged || gitItemStatus.IsConflict || gitItemStatus.IsSubmodule)
                {
                    if (!gitItemStatus.IsSubmodule || gitItemStatus.GetSubmoduleStatusAsync() == null ||
                        !gitItemStatus.GetSubmoduleStatusAsync().IsCompleted)
                    {
                        return 2;
                    }

                    var status = gitItemStatus.GetSubmoduleStatusAsync().CompletedResult();
                    if (status == null)
                    {
                        return 2;
                    }

                    if (status.Status == SubmoduleStatus.FastForward)
                    {
                        return 6 + (status.IsDirty ? 1 : 0);
                    }

                    if (status.Status == SubmoduleStatus.Rewind)
                    {
                        return 8 + (status.IsDirty ? 1 : 0);
                    }

                    if (status.Status == SubmoduleStatus.NewerTime)
                    {
                        return 10 + (status.IsDirty ? 1 : 0);
                    }

                    if (status.Status == SubmoduleStatus.OlderTime)
                    {
                        return 12 + (status.IsDirty ? 1 : 0);
                    }

                    return !status.IsDirty ? 2 : 5;
                }

                if (gitItemStatus.IsRenamed)
                {
                    if (gitItemStatus.RenameCopyPercentage == "100")
                    {
                        return 3; // Rename icon
                    }

                    return 2; // Modified icon
                }

                if (gitItemStatus.IsCopied)
                {
                    return 4;
                }

                return 14; // icon unknown
            }

            string GetDescriptionForRevision(ObjectId objectId)
            {
                if (DescribeRevision != null)
                {
                    return DescribeRevision(objectId);
                }

                return objectId.ToShortString();
            }
        }

        private void UpdateColumnWidth()
        {
            // prevent infinite recursions such as
            // ClientSizeChanged -> UpdateColumnWidth -> ScrollBar visibility changed -> ClientSizeChanged
            if (!_updatingColumnWidth)
            {
                _updatingColumnWidth = true;
                columnHeader.Width = GetWidth();
                _updatingColumnWidth = false;
            }

            int GetWidth()
            {
                var pathFormatter = new PathFormatter(FileStatusListView.CreateGraphics(), FileStatusListView.Font);
                var controlWidth = FileStatusListView.ClientSize.Width;

                var contentWidth = FileStatusListView.Items()
                    .Where(item => item.BoundsOrEmpty().IntersectsWith(FileStatusListView.ClientRectangle))
                    .Select(item =>
                    {
                        (_, _, _, _, int textStart, int textWidth, _) = FormatListViewItem(item, pathFormatter, FileStatusListView.ClientSize.Width);
                        return textStart + textWidth;
                    })
                    .DefaultIfEmpty(controlWidth)
                    .Max();

                return Math.Max(contentWidth, controlWidth);
            }
        }

        private void HandleVisibility_NoFilesLabel_FilterComboBox(bool filesPresent)
        {
            NoFiles.Visible = !filesPresent;
            FilterVisibleInternal = FilterVisible && filesPresent;
        }

        // Event handlers

        private void FileStatusListView_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateColumnWidth();
        }

        private void FileStatusListView_ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var cm = (ContextMenuStrip)sender;

            if (!cm.Items.Find(_openSubmoduleMenuItem.Name, true).Any())
            {
                cm.Items.Insert(1, _openSubmoduleMenuItem);
            }

            bool isSubmoduleSelected = SelectedItem != null && SelectedItem.IsSubmodule;

            _openSubmoduleMenuItem.Visible = isSubmoduleSelected;

            if (isSubmoduleSelected)
            {
                _openSubmoduleMenuItem.Font = AppSettings.OpenSubmoduleDiffInSeparateWindow ?
                    new Font(_openSubmoduleMenuItem.Font, FontStyle.Bold) :
                    new Font(_openSubmoduleMenuItem.Font, FontStyle.Regular);
            }

            if (!cm.Items.Find(_sortByContextMenu.Name, true).Any())
            {
                cm.Items.Add(new ToolStripSeparator());
                cm.Items.Add(_sortByContextMenu);
            }
        }

        private void FileStatusListView_DoubleClick(object sender, EventArgs e)
        {
            if (DoubleClick == null)
            {
                if (AppSettings.OpenSubmoduleDiffInSeparateWindow && SelectedItem.IsSubmodule)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(() => OpenSubmoduleAsync());
                }
                else
                {
                    UICommands.StartFileHistoryDialog(this, SelectedItem.Name, Revision);
                }
            }
            else
            {
                DoubleClick?.Invoke(sender, e);
            }
        }

        private void FileStatusListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var item = e.Item;
            var formatter = new PathFormatter(e.Graphics, FileStatusListView.Font);

            var (image, prefix, text, suffix, prefixTextStartX, _, textMaxWidth) = FormatListViewItem(item, formatter, item.Bounds.Width);

            if (image != null)
            {
                e.Graphics.DrawImageUnscaled(image, item.Position.X, item.Position.Y);
            }

            if (!string.IsNullOrEmpty(text))
            {
                var textRect = new Rectangle(prefixTextStartX, item.Bounds.Top, textMaxWidth, item.Bounds.Height);

                if (!string.IsNullOrEmpty(prefix))
                {
                    DrawString(textRect, prefix, SystemColors.GrayText);
                    var prefixSize = formatter.MeasureString(prefix);
                    textRect.Offset(prefixSize.Width, 0);
                }

                DrawString(textRect, text, SystemColors.ControlText);

                if (!string.IsNullOrEmpty(suffix))
                {
                    var textSize = formatter.MeasureString(text);
                    textRect.Offset(textSize.Width, 0);
                    DrawString(textRect, suffix, SystemColors.GrayText);
                }
            }

            void DrawString(Rectangle rect, string s, Color color)
            {
                rect.Intersect(Rectangle.Round(e.Graphics.ClipBounds));
                if (rect.Width != 0 && rect.Height != 0)
                {
                    formatter.DrawString(s, rect, color);
                }
            }
        }

        private void FileStatusListView_GroupMouseDown(object sender, ListViewGroupMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // prevent selecting all sub-items when left-clicking group
                e.Handled = true;
            }
        }

        private void FileStatusListView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Control | Keys.A:
                    {
                        SelectAll();
                        e.Handled = true;
                        break;
                    }

                default:
                    {
                        KeyDown?.Invoke(sender, e);
                        break;
                    }
            }
        }

        private void FileStatusListView_MouseDown(object sender, MouseEventArgs e)
        {
            // SELECT
            if (e.Button == MouseButtons.Right)
            {
                var hover = FileStatusListView.HitTest(e.Location);

                if (hover.Item != null && !hover.Item.Selected)
                {
                    SelectedIndex = hover.Item.Index;
                }
            }

            // DRAG
            if (e.Button == MouseButtons.Left)
            {
                if (SelectedItems.Any())
                {
                    // Remember the point where the mouse down occurred.
                    // The DragSize indicates the size that the mouse can move
                    // before a drag event should be started.
                    Size dragSize = SystemInformation.DragSize;

                    // Create a rectangle using the DragSize, with the mouse position being
                    // at the center of the rectangle.
                    _dragBoxFromMouseDown = new Rectangle(
                        new Point(
                            e.X - (dragSize.Width / 2),
                            e.Y - (dragSize.Height / 2)),
                        dragSize);
                }
                else
                {
                    // Reset the rectangle if the mouse is not over an item in the ListView.
                    _dragBoxFromMouseDown = Rectangle.Empty;
                }
            }
        }

        private void FileStatusListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListView listView = sender as ListView;

            // DRAG
            // If the mouse moves outside the rectangle, start the drag.
            if (_dragBoxFromMouseDown != Rectangle.Empty &&
                !_dragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                if (SelectedItems.Any())
                {
                    var fileList = new StringCollection();

                    foreach (GitItemStatus item in SelectedItems)
                    {
                        string fileName = _fullPathResolver.Resolve(item.Name);

                        fileList.Add(fileName.ToNativePath());
                    }

                    var obj = new DataObject();
                    obj.SetFileDropList(fileList);

                    // Proceed with the drag and drop, passing in the list item.
                    DoDragDrop(obj, DragDropEffects.Copy);
                    _dragBoxFromMouseDown = Rectangle.Empty;
                }
            }

            // TOOLTIP
            if (listView != null)
            {
                ListViewItem hoveredItem;
                try
                {
                    var point = new Point(e.X, e.Y);
                    hoveredItem = listView.HitTest(point).Item;
                }
                catch (ArgumentOutOfRangeException)
                {
                    hoveredItem = null;
                }

                var gitItemStatus = hoveredItem?.Tag<GitItemStatus>();

                if (gitItemStatus != null)
                {
                    string text;
                    if (gitItemStatus.IsRenamed || gitItemStatus.IsCopied)
                    {
                        text = string.Concat(gitItemStatus.Name, " (", gitItemStatus.OldName, ")");
                    }
                    else
                    {
                        text = gitItemStatus.Name;
                    }

                    float textWidth;
                    using (var graphics = listView.CreateGraphics())
                    {
                        textWidth = graphics.MeasureString(text, listView.Font).Width + 17;
                    }

                    // Use width-itemheight because the icon drawn in front of the text is the itemheight
                    if (textWidth > (FileStatusListView.Width - FileStatusListView.GetItemRect(hoveredItem.Index).Height))
                    {
                        if (hoveredItem.ToolTipText != gitItemStatus.ToString())
                        {
                            hoveredItem.ToolTipText = gitItemStatus.ToString();
                        }
                    }
                    else
                    {
                        hoveredItem.ToolTipText = "";
                    }
                }
            }
        }

        private void FileStatusListView_SelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private void FileStatusListView_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type == ScrollEventType.ThumbTrack)
            {
                return;
            }

            UpdateColumnWidth();
        }

        private void FileStatusList_Enter(object sender, EventArgs e)
        {
            Enter?.Invoke(this, new EnterEventArgs(_mouseEntered));
            _mouseEntered = false;
        }

        #region Filtering

        private string _toolTipText = "";
        private readonly Subject<string> _filterSubject = new Subject<string>();
        [CanBeNull] private Regex _filter;
        private bool _filterVisible = false;

        public void SetFilter(string value)
        {
            FilterComboBox.Text = value;
            FilterFiles(value);
        }

        private void DeleteFilterButton_Click(object sender, EventArgs e)
        {
            SetFilter(string.Empty);
        }

        private int FilterFiles(string value)
        {
            StoreFilter(value);

            UpdateFileStatusListView(updateCausedByFilter: true);
            return FileStatusListView.Items.Count;
        }

        private bool IsFilterMatch(GitItemStatus item)
        {
            if (_filter == null)
            {
                return true;
            }

            string name = item.Name.TrimEnd(PathUtil.PosixDirectorySeparatorChar);
            string oldName = item.OldName;

            if (AppSettings.TruncatePathMethod == TruncatePathMethod.FileNameOnly)
            {
                name = Path.GetFileName(name);
                oldName = Path.GetFileName(oldName);
            }

            if (_filter.IsMatch(name))
            {
                return true;
            }

            return oldName != null && _filter.IsMatch(oldName);
        }

        private void InitialiseFiltering()
        {
            // TODO this code is very similar to code in FormCommit
            _filterSubject
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(
                    filterText =>
                    {
                        _toolTipText = "";
                        var fileCount = 0;
                        try
                        {
                            fileCount = FilterFiles(filterText);
                        }
                        catch (ArgumentException ae)
                        {
                            _toolTipText = ae.Message;
                        }

                        if (fileCount > 0)
                        {
                            AddToSelectionFilter(filterText);
                        }
                    });

            void AddToSelectionFilter(string filter)
            {
                if (FilterComboBox.Items.Cast<string>().Any(candidate => candidate == filter))
                {
                    return;
                }

                const int SelectionFilterMaxLength = 10;
                if (FilterComboBox.Items.Count == SelectionFilterMaxLength)
                {
                    FilterComboBox.Items.RemoveAt(SelectionFilterMaxLength - 1);
                }

                FilterComboBox.Items.Insert(0, filter);
            }
        }

        private void FilterComboBox_TextUpdate(object sender, EventArgs e)
        {
            // show DeleteFilterButton at once
            SetDeleteFilterButtonVisibility();

            string filterText = FilterComboBox.Text;

            // workaround for text getting selected if it matches the start of the combobox items
            if (FilterComboBox.SelectionLength == filterText.Length && FilterComboBox.SelectionStart == 0)
            {
                FilterComboBox.SelectionLength = 0;
                FilterComboBox.SelectionStart = filterText.Length;
            }

            _filterSubject.OnNext(filterText);
        }

        private void FilterComboBox_MouseEnter(object sender, EventArgs e)
        {
            FilterToolTip.SetToolTip(FilterComboBox, _toolTipText);
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterFiles(FilterComboBox.Text);
        }

        private void FilterComboBox_GotFocus(object sender, EventArgs e)
        {
            SetFilterWatermarkLabelVisibility();
        }

        private void FilterComboBox_LostFocus(object sender, EventArgs e)
        {
            SetFilterWatermarkLabelVisibility();
        }

        private void FilterWatermarkLabel_Click(object sender, EventArgs e)
        {
            FilterComboBox.Focus();
        }

        private void FilterComboBox_SizeChanged(object sender, EventArgs e)
        {
            // strangely it does not invalidate itself on resize so its look becomes distorted
            FilterComboBox.Invalidate();
        }

        private void SortByFilePath()
        {
            FileStatusListView.ListViewItemSorter = new GitStatusListSorter(Comparer<GitItemStatus>.Default);
            FileStatusListView.Sort();
        }

        private void SortByFileExtension()
        {
            FileStatusListView.ListViewItemSorter = new GitStatusListSorter(new GitItemStatusFileExtensionComparer());
            FileStatusListView.Sort();
        }

        private void SortByFileStatus()
        {
            FileStatusListView.ListViewItemSorter = new ImageIndexListSorter();
            FileStatusListView.Sort();
        }

        private void StoreFilter(string value)
        {
            SetDeleteFilterButtonVisibility();
            if (string.IsNullOrEmpty(value))
            {
                FilterComboBox.BackColor = SystemColors.Window;
                _filter = null;
                return;
            }

            try
            {
                _filter = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                FilterComboBox.BackColor = _activeInputColor;
            }
            catch
            {
                FilterComboBox.BackColor = _invalidInputColor;
                throw;
            }
        }

        private class GitStatusListSorter : Comparer<ListViewItem>
        {
            public IComparer<GitItemStatus> StatusComparer { get; }

            public GitStatusListSorter(IComparer<GitItemStatus> gitStatusItemSorter = null)
            {
                StatusComparer = gitStatusItemSorter ?? Comparer<GitItemStatus>.Default;
            }

            public override int Compare(ListViewItem x, ListViewItem y)
            {
                return StatusComparer.Compare(x.Tag as GitItemStatus, y.Tag as GitItemStatus);
            }
        }

        private class ImageIndexListSorter : Comparer<ListViewItem>
        {
            /// <summary>
            /// Secondary sort should be by file path.
            /// </summary>
            private static readonly GitStatusListSorter ThenBy = new GitStatusListSorter(Comparer<GitItemStatus>.Default);

            public override int Compare(ListViewItem x, ListViewItem y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }
                else if (x == null)
                {
                    return -1;
                }
                else if (y == null)
                {
                    return 1;
                }

                var statusResult = x.ImageIndex.CompareTo(y.ImageIndex);

                if (statusResult == 0)
                {
                    return ThenBy.Compare(x, y);
                }

                return statusResult;
            }
        }

        #endregion

        #region private Color constants
        //// Do not declare the colors "static" because Color.FromArgb() will not work at their initialization.
        private readonly Color _activeInputColor = Color.FromArgb(0xC8, 0xFF, 0xC8).AdaptBackColor();
        private readonly Color _invalidInputColor = Color.FromArgb(0xFF, 0xC8, 0xC8).AdaptBackColor();
        #endregion

        internal TestAccessor GetTestAccessor() => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly FileStatusList _fileStatusList;

            internal TestAccessor(FileStatusList fileStatusList)
            {
                _fileStatusList = fileStatusList;
            }

            internal Color ActiveInputColor => _fileStatusList._activeInputColor;
            internal Color InvalidInputColor => _fileStatusList._invalidInputColor;
            internal Label DeleteFilterButton => _fileStatusList.DeleteFilterButton;
            internal ListView FileStatusListView => _fileStatusList.FileStatusListView;
            internal ComboBox FilterComboBox => _fileStatusList.FilterComboBox;
            internal Regex Filter => _fileStatusList._filter;
            internal bool FilterWatermarkLabelVisible => _fileStatusList.FilterWatermarkLabel.Visible;
            internal void StoreFilter(string value) => _fileStatusList.StoreFilter(value);
        }
    }
}
