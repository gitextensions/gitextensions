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
        private readonly TranslationString _diffWithParent = new TranslationString("Diff with a/");
        private readonly TranslationString _diffBaseToB = new TranslationString("Unique diff BASE with b/");
        private readonly TranslationString _diffCommonBase = new TranslationString("Common diff with BASE a/");
        private readonly TranslationString _combinedDiff = new TranslationString("Combined diff");
        private readonly IGitRevisionTester _revisionTester;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly SortDiffListContextMenuItem _sortByContextMenu;
        private readonly IReadOnlyList<GitItemStatus> _noItemStatuses;

        private int _nextIndexToSelect = -1;
        private bool _groupByRevision;
        private bool _enableSelectedIndexChangeEvent = true;
        private bool _mouseEntered;
        private ToolStripItem _openSubmoduleMenuItem;
        private Rectangle _dragBoxFromMouseDown;
        private IReadOnlyList<(GitRevision firstRev, GitRevision secondRev, string summary, IReadOnlyList<GitItemStatus> statuses)> _itemsWithDescription
            = Array.Empty<(GitRevision, GitRevision, string, IReadOnlyList<GitItemStatus>)>();
        [CanBeNull] private IDisposable _selectedIndexChangeSubscription;
        [CanBeNull] private IDisposable _diffListSortSubscription;

        private bool _updatingColumnWidth;

        // Currently bound revisions. Cache so we can reload the view, if AppSettings.ShowDiffForAllParents is changed.
        private IReadOnlyList<GitRevision> _revisions;

        // Function to retrieve revisions. Cache so we can reload the view, if AppSettings.ShowDiffForAllParents is changed.
        private Func<ObjectId, GitRevision> _getRevision;

        public delegate void EnterEventHandler(object sender, EnterEventArgs e);

        public event EventHandler SelectedIndexChanged;
        public event EventHandler DataSourceChanged;

        public new event EventHandler DoubleClick;
        public new event KeyEventHandler KeyDown;
        public new event EnterEventHandler Enter;

        [Description("Disable showing open submodule menu items as bold")]
        [DefaultValue(false)]
        public bool DisableSubmoduleMenuItemBold { get; set; }

        private Dictionary<string, int> _stateImageIndexDict;

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
            FileStatusListView.LargeImageList = FileStatusListView.SmallImageList;

            FileStatusListView.AllowCollapseGroups = true;
            FileStatusListView.Scroll += FileStatusListView_Scroll;

            HandleVisibility_NoFilesLabel_FilterComboBox(filesPresent: true);
            Controls.SetChildIndex(NoFiles, 0);
            NoFiles.Font = new Font(NoFiles.Font, FontStyle.Italic);
            FilterWatermarkLabel.Font = new Font(FilterWatermarkLabel.Font, FontStyle.Italic);
            FilterComboBox.Font = new Font(FilterComboBox.Font, FontStyle.Bold);

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _revisionTester = new GitRevisionTester(_fullPathResolver);
            _noItemStatuses = new[]
            {
                new GitItemStatus
                {
                    Name = $"     - {NoFiles.Text} -",
                    IsStatusOnly = true,
                    ErrorMessage = string.Empty
                }
            };

            base.Enter += FileStatusList_Enter;

            return;

            ImageList CreateImageList()
            {
                const int rowHeight = 18;

                var list = new ImageList
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = DpiUtil.Scale(new Size(16, rowHeight)), // Scale ImageSize and images scale automatically
                };

                _stateImageIndexDict = new Dictionary<string, int>();
                var images = new (string imageKey, Bitmap icon)[]
                {
                    (nameof(Images.FileStatusUnknown), ScaleHeight(Images.FileStatusUnknown)),
                    (nameof(Images.FileStatusModified), ScaleHeight(Images.FileStatusModified)),
                    (nameof(Images.FileStatusAdded), ScaleHeight(Images.FileStatusAdded)),
                    (nameof(Images.FileStatusRemoved), ScaleHeight(Images.FileStatusRemoved)),
                    (nameof(Images.Conflict), ScaleHeight(Images.Conflict)),
                    (nameof(Images.FileStatusRenamed), ScaleHeight(Images.FileStatusRenamed.AdaptLightness())),
                    (nameof(Images.FileStatusCopied), ScaleHeight(Images.FileStatusCopied)),
                    (nameof(Images.SubmodulesManage), ScaleHeight(Images.SubmodulesManage)),
                    (nameof(Images.FolderSubmodule), ScaleHeight(Images.FolderSubmodule)),
                    (nameof(Images.SubmoduleDirty), ScaleHeight(Images.SubmoduleDirty)),
                    (nameof(Images.SubmoduleRevisionUp), ScaleHeight(Images.SubmoduleRevisionUp)),
                    (nameof(Images.SubmoduleRevisionUpDirty), ScaleHeight(Images.SubmoduleRevisionUpDirty)),
                    (nameof(Images.SubmoduleRevisionDown), ScaleHeight(Images.SubmoduleRevisionDown)),
                    (nameof(Images.SubmoduleRevisionDownDirty), ScaleHeight(Images.SubmoduleRevisionDownDirty)),
                    (nameof(Images.SubmoduleRevisionSemiUp), ScaleHeight(Images.SubmoduleRevisionSemiUp)),
                    (nameof(Images.SubmoduleRevisionSemiUpDirty), ScaleHeight(Images.SubmoduleRevisionSemiUpDirty)),
                    (nameof(Images.SubmoduleRevisionSemiDown), ScaleHeight(Images.SubmoduleRevisionSemiDown)),
                    (nameof(Images.SubmoduleRevisionSemiDownDirty), ScaleHeight(Images.SubmoduleRevisionSemiDownDirty))
                };

                for (var i = 0; i < images.Length; i++)
                {
                    list.Images.Add(images[i].icon);
                    _stateImageIndexDict.Add(images[i].imageKey, i);
                }

                return list;

                static Bitmap ScaleHeight(Bitmap input)
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
        public IEnumerable<FileStatusItem> AllItems => FileStatusListView.ItemTags<FileStatusItem>();

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
                if (FileStatusListView.ContextMenuStrip == value)
                {
                    return;
                }

                if (FileStatusListView.ContextMenuStrip != null)
                {
                    FileStatusListView.ContextMenuStrip.Opening -= FileStatusListView_ContextMenu_Opening;
                }

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
        public IReadOnlyList<GitItemStatus> GitItemFilteredStatuses => AllItems.Items().AsReadOnlyList();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<GitItemStatus> GitItemStatuses
        {
            get
            {
                return GitItemStatusesWithDescription?.SelectMany(tuple => tuple.statuses).AsReadOnlyList()
                       ?? Array.Empty<GitItemStatus>();
            }
        }

        private IReadOnlyList<(GitRevision firstRev, GitRevision secondRev, string summary, IReadOnlyList<GitItemStatus> statuses)> GitItemStatusesWithDescription
        {
            get { return _itemsWithDescription; }
            set
            {
                _itemsWithDescription = value ?? throw new ArgumentNullException(nameof(value));
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
        public GitItemStatus SelectedGitItem
        {
            get => SelectedItem?.Item;
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
                        var gitItemStatus = item.Tag<FileStatusItem>();
                        if (gitItemStatus.Item == status)
                        {
                            return item;
                        }

                        if (status.CompareName(gitItemStatus.Item) == 0 && newSelected == null)
                        {
                            newSelected = item;
                        }
                    }

                    return newSelected;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<GitItemStatus> SelectedGitItems
        {
            set
            {
                if (value == null)
                {
                    ClearSelected();
                    return;
                }

                SelectItems(item => value.Contains(item.Tag<FileStatusItem>().Item));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public FileStatusItem SelectedItem => FileStatusListView.LastSelectedItem()?.Tag<FileStatusItem>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<FileStatusItem> SelectedItems
        {
            get => FileStatusListView.SelectedItemTags<FileStatusItem>();
            set
            {
                if (value == null)
                {
                    ClearSelected();
                    return;
                }

                SelectItems(item => value.Contains(item.Tag<FileStatusItem>()));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<FileStatusItem> FirstGroupItems
        {
            get
            {
                if (FileStatusListView.Groups.Count == 0)
                {
                    yield break;
                }

                foreach (ListViewItem item in FileStatusListView.Groups[0].Items)
                {
                    yield return item.Tag<FileStatusItem>();
                }
            }
        }

        [DefaultValue(true)]
        public bool SelectFirstItemOnSetItems { get; set; }

        public int UnfilteredItemsCount => GitItemStatusesWithDescription?.Sum(tuple => tuple.statuses.Count) ?? 0;

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
            var gitItemStatus = item.Tag<FileStatusItem>().Item;
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

        public void SetDiffs(IReadOnlyList<GitRevision> revisions, Func<ObjectId, GitRevision> getRevision = null)
        {
            _revisions = revisions;
            _getRevision = getRevision;

            var tuples = new List<(GitRevision firstRev, GitRevision secondRev, string summary, IReadOnlyList<GitItemStatus> statuses)>();
            var selectedRev = revisions?.FirstOrDefault();
            if (selectedRev == null)
            {
                GitItemStatusesWithDescription = tuples;
                return;
            }

            if (revisions.Count == 1)
            {
                if (selectedRev.ParentIds == null || selectedRev.ParentIds.Count == 0)
                {
                    // No parent for the initial commit
                    tuples.Add((null, selectedRev, GetDescriptionForRevision(selectedRev.ObjectId), Module.GetTreeFiles(selectedRev.TreeGuid, full: true)));
                }
                else
                {
                    // Get the parents for the selected revision
                    var multipleParents = AppSettings.ShowDiffForAllParents ? selectedRev.ParentIds.Count : 1;
                    tuples.AddRange(selectedRev
                        .ParentIds?
                        .Take(multipleParents)
                        .Select(parentId =>
                            ((GitRevision, GitRevision, string, IReadOnlyList<GitItemStatus>))(new GitRevision(parentId),
                                selectedRev,
                                _diffWithParent.Text + GetDescriptionForRevision(parentId),
                                Module.GetDiffFilesWithSubmodulesStatus(parentId, selectedRev.ObjectId, selectedRev.FirstParentId))));
                }

                // Show combined (merge conflicts) when a single merge commit is selected
                var isMergeCommit = tuples.Count > 1;
                if (AppSettings.ShowDiffForAllParents && isMergeCommit)
                {
                    var conflicts = Module.GetCombinedDiffFileList(selectedRev.Guid);
                    if (conflicts.Count != 0)
                    {
                        // Create an artificial commit
                        var desc = _combinedDiff.Text;
                        tuples.Add((new GitRevision(ObjectId.CombinedDiffId), selectedRev, desc, conflicts));
                    }
                }
            }
            else
            {
                // With more than 4, only first -> selected is interesting
                // Limited selections: Show multi selection if more than two selected
                var multipleParents = AppSettings.ShowDiffForAllParents && revisions.Count <= 4 ? revisions.Count - 1 : 1;
                tuples.AddRange(revisions
                    .Skip(1)
                    .Take(multipleParents)
                    .Select(firstRev =>
                        (firstRev,
                            selectedRev,
                            _diffWithParent.Text + GetDescriptionForRevision(firstRev.ObjectId),
                            Module.GetDiffFilesWithSubmodulesStatus(firstRev.ObjectId, selectedRev.ObjectId, selectedRev.FirstParentId))));

                if (AppSettings.ShowDiffForAllParents && revisions.Count == 2)
                {
                    var firstRev = revisions.Last();
                    var allAToB = tuples[0].statuses;

                    // Get base commit, add as parent if unique
                    Lazy<ObjectId> head = getRevision != null
                        ? new Lazy<ObjectId>(() => getRevision(ObjectId.IndexId).FirstParentId)
                        : new Lazy<ObjectId>(() => Module.RevParse("HEAD"));
                    var baseRevGuid = Module.GetMergeBase(GetRevisionOrHead(firstRev, head),
                        GetRevisionOrHead(selectedRev, head));

                    // Add if separate branches (note that artificial commits both have HEAD as BASE)
                    if (baseRevGuid != null
                        && baseRevGuid != GetRevisionOrHead(firstRev, head)
                        && baseRevGuid != GetRevisionOrHead(selectedRev, head))
                    {
                        // Present common files in BASE->B, BASE->A separately
                        // For the following diff:  A->B a,c,d; BASE->B a,b,c; BASE->A a,b,d
                        // (the file a has unique changes, b has the same change and c,d is changed in one of the branches)
                        // The following groups will be shown: A->B a,c,d; BASE->B a,c; BASE->A a,d; Common BASE b
                        var allBaseToB = Module.GetDiffFilesWithSubmodulesStatus(baseRevGuid, selectedRev.ObjectId, selectedRev.FirstParentId);
                        var allBaseToA = Module.GetDiffFilesWithSubmodulesStatus(baseRevGuid, firstRev.ObjectId, firstRev.FirstParentId);

                        var comparer = new GitItemStatusNameEqualityComparer();
                        var commonBaseToAandB = allBaseToB.Intersect(allBaseToA, comparer).Except(allAToB, comparer).ToList();
                        var uniqueBaseToB = allBaseToB.Except(commonBaseToAandB, comparer).ToList();
                        var uniqueBaseToA = allBaseToA.Except(commonBaseToAandB, comparer).ToList();

                        var revBase = new GitRevision(baseRevGuid);
                        tuples.Add((revBase, selectedRev, _diffBaseToB.Text + GetDescriptionForRevision(selectedRev.ObjectId), uniqueBaseToB));
                        tuples.Add((revBase, firstRev, _diffBaseToB.Text + GetDescriptionForRevision(firstRev.ObjectId), uniqueBaseToA));
                        tuples.Add((revBase, selectedRev, _diffCommonBase.Text + GetDescriptionForRevision(baseRevGuid), commonBaseToAandB));
                    }
                }
            }

            GitItemStatusesWithDescription = tuples;

            return;

            static ObjectId GetRevisionOrHead(GitRevision rev, Lazy<ObjectId> head)
                => rev.IsArtificial ? head.Value : rev.ObjectId;
        }

        /// <summary>
        /// FormStash init for WorkTree and Index
        /// </summary>
        /// <param name="headRev">The GitRevision for HEAD</param>
        /// <param name="indexRev">The GitRevision for Index</param>
        /// <param name="indexDesc">The description for Index</param>
        /// <param name="indexItems">The GitItems for Index</param>
        /// <param name="workTreeRev">The GitRevision for WorkTree</param>
        /// <param name="workTreeDesc">The description for WorkTree</param>
        /// <param name="workTreeItems">The GitItems for WorkTree</param>
        public void SetStashDiffs(GitRevision headRev,
            GitRevision indexRev,
            string indexDesc,
            [NotNull] IReadOnlyList<GitItemStatus> indexItems,
            GitRevision workTreeRev,
            string workTreeDesc,
            [NotNull] IReadOnlyList<GitItemStatus> workTreeItems)
        {
            GroupByRevision = true;
            GitItemStatusesWithDescription = new[]
            {
                (indexRev, workTreeRev, workTreeDesc, workTreeItems),
                (headRev, indexRev, indexDesc, indexItems)
            };
        }

        public void SetDiffs([CanBeNull] GitRevision firstRev, [CanBeNull] GitRevision secondRev, [NotNull] IReadOnlyList<GitItemStatus> items)
        {
            GroupByRevision = false;
            GitItemStatusesWithDescription = new[] { (firstRev: firstRev, secondRev: secondRev, _diffWithParent.Text + GetDescriptionForRevision(firstRev?.ObjectId), items) };
        }

        public void ClearDiffs()
        {
            GitItemStatusesWithDescription = Array.Empty<(GitRevision, GitRevision, string, IReadOnlyList<GitItemStatus>)>();
        }

        private string GetDescriptionForRevision(ObjectId objectId)
        {
            if (DescribeRevision != null)
            {
                return DescribeRevision(objectId);
            }

            return objectId?.ToShortString();
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
            try
            {
                _selectedIndexChangeSubscription?.Dispose();
                _diffListSortSubscription?.Dispose();
            }
            catch (InvalidOperationException)
            {
                // System.Reactive causes the app to fail with: 'Invoke or BeginInvoke cannot be called on a control until the window handle has been created.'
            }
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
                Text = Strings.OpenWithGitExtensions,
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
            var submoduleName = SelectedItem.Item.Name;

            var status = await SelectedItem.Item.GetSubmoduleStatusAsync().ConfigureAwait(false);

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
            DeleteFilterButton.Visible = FilterVisibleInternal && !string.IsNullOrEmpty(FilterComboBox.Text);
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
                    .ToHashSet(i => i.Tag<FileStatusItem>().Item);

                DataSourceChanged?.Invoke(this, EventArgs.Empty);
            }

            FileStatusListView.BeginUpdate();
            FileStatusListView.ShowGroups = GitItemStatusesWithDescription.Count > 1 || _groupByRevision;
            FileStatusListView.Groups.Clear();
            FileStatusListView.Items.Clear();

            bool hasChanges = GitItemStatusesWithDescription.Any(x => x.statuses.Count > 0);

            var list = new List<ListViewItem>();
            foreach (var i in GitItemStatusesWithDescription)
            {
                ListViewGroup group = null;
                if (i.firstRev != null)
                {
                    group = new ListViewGroup(i.summary)
                    {
                        Tag = i.firstRev
                    };

                    FileStatusListView.Groups.Add(group);
                }

                IReadOnlyList<GitItemStatus> itemStatuses;
                if (hasChanges && i.statuses.Count == 0)
                {
                    itemStatuses = _noItemStatuses;
                    FileStatusListView.SetGroupState(group, NativeMethods.LVGS.Collapsible | NativeMethods.LVGS.Collapsed);
                }
                else
                {
                    itemStatuses = i.statuses;
                }

                foreach (var item in itemStatuses)
                {
                    if (!IsFilterMatch(item))
                    {
                        continue;
                    }

                    var listItem = new ListViewItem(string.Empty, group);

                    if (!item.IsStatusOnly || !string.IsNullOrWhiteSpace(item.ErrorMessage))
                    {
                        listItem.ImageIndex = GetItemImageIndex(item);
                    }

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

                    listItem.Tag = new FileStatusItem(i.firstRev, i.secondRev, item);
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
                var imageKey = GetItemImageKey(gitItemStatus);
                return _stateImageIndexDict.ContainsKey(imageKey)
                    ? _stateImageIndexDict[imageKey]
                    : _stateImageIndexDict[nameof(Images.FileStatusUnknown)];
            }

            static string GetItemImageKey(GitItemStatus gitItemStatus)
            {
                if (!gitItemStatus.IsNew && !gitItemStatus.IsDeleted && !gitItemStatus.IsTracked)
                {
                    // Illegal combinations, no flags set?
                    return nameof(Images.FileStatusUnknown);
                }

                if (gitItemStatus.IsDeleted)
                {
                    return nameof(Images.FileStatusRemoved);
                }

                if (gitItemStatus.IsNew || (!gitItemStatus.IsTracked && !gitItemStatus.IsSubmodule))
                {
                    return nameof(Images.FileStatusAdded);
                }

                if (gitItemStatus.IsConflict)
                {
                    return nameof(Images.Conflict);
                }

                if (gitItemStatus.IsSubmodule)
                {
                    if (gitItemStatus.GetSubmoduleStatusAsync() == null ||
                        !gitItemStatus.GetSubmoduleStatusAsync().IsCompleted)
                    {
                        return gitItemStatus.IsDirty ? nameof(Images.SubmoduleDirty) : nameof(Images.SubmodulesManage);
                    }

                    var status = gitItemStatus.GetSubmoduleStatusAsync().CompletedResult();
                    if (status == null)
                    {
                        return gitItemStatus.IsDirty ? nameof(Images.SubmoduleDirty) : nameof(Images.SubmodulesManage);
                    }

                    return status.Status switch
                    {
                        SubmoduleStatus.FastForward => status.IsDirty
                            ? nameof(Images.SubmoduleRevisionUpDirty)
                            : nameof(Images.SubmoduleRevisionUp),
                        SubmoduleStatus.Rewind => status.IsDirty
                            ? nameof(Images.SubmoduleRevisionDownDirty)
                            : nameof(Images.SubmoduleRevisionDown),
                        SubmoduleStatus.NewerTime => status.IsDirty
                            ? nameof(Images.SubmoduleRevisionSemiUpDirty)
                            : nameof(Images.SubmoduleRevisionSemiUp),
                        SubmoduleStatus.OlderTime => status.IsDirty
                            ? nameof(Images.SubmoduleRevisionSemiDownDirty)
                            : nameof(Images.SubmoduleRevisionSemiDown),
                        _ => status.IsDirty
                            ? nameof(Images.SubmoduleDirty)
                            : nameof(Images.FolderSubmodule)
                    };
                }

                if (gitItemStatus.IsChanged)
                {
                    return nameof(Images.FileStatusModified);
                }

                if (gitItemStatus.IsRenamed)
                {
                    return gitItemStatus.RenameCopyPercentage == "100"
                        ? nameof(Images.FileStatusRenamed)
                        : nameof(Images.FileStatusModified);
                }

                if (gitItemStatus.IsCopied)
                {
                    return nameof(Images.FileStatusCopied);
                }

                return nameof(Images.FileStatusUnknown);
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

        public void SelectPreviousVisibleItem()
        {
            if (FileStatusListView.Items.Count <= 1)
            {
                return;
            }

            if (FileStatusListView.Groups.Count == 0)
            {
                var index = SelectedIndex == 0 ? FileStatusListView.Items.Count - 1 : SelectedIndex - 1;
                var item = FileStatusListView.Items[index];
                item.Selected = true;
                item.EnsureVisible();
            }

            ListViewItem selectedItemFound = null;
            for (int i = FileStatusListView.Groups.Count - 1; i >= 0; i--)
            {
                var group = FileStatusListView.Groups[i];
                var groupItems = FileStatusListView.Items
                    .Cast<ListViewItem>()
                    .Where(item => item.Group == group)
                    .Reverse();
                foreach (var item in groupItems)
                {
                    if (selectedItemFound != null)
                    {
                        selectedItemFound.Selected = false;
                        item.Selected = true;
                        item.EnsureVisible();
                        return;
                    }

                    if (item.Selected)
                    {
                        selectedItemFound = item;
                    }
                }
            }
        }

        public void SelectNextVisibleItem()
        {
            if (FileStatusListView.Items.Count <= 1)
            {
                return;
            }

            if (FileStatusListView.Groups.Count == 0)
            {
                var index = SelectedIndex >= FileStatusListView.Items.Count - 1 ? 0 : SelectedIndex + 1;
                var item = FileStatusListView.Items[index];
                item.Selected = true;
                item.EnsureVisible();
            }

            ListViewItem selectedItemFound = null;
            foreach (var group in FileStatusListView.Groups)
            {
                var groupItems = FileStatusListView.Items
                    .Cast<ListViewItem>()
                    .Where(item => item.Group == group);
                foreach (var item in groupItems)
                {
                    if (selectedItemFound != null)
                    {
                        selectedItemFound.Selected = false;
                        item.Selected = true;
                        item.EnsureVisible();
                        return;
                    }

                    if (item.Selected)
                    {
                        selectedItemFound = item;
                    }
                }
            }
        }

        // Event handlers

        private void FileStatusListView_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateColumnWidth();
        }

        private void FileStatusListView_ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (SelectedItem?.Item?.IsStatusOnly ?? false)
            {
                e.Cancel = true;
                return;
            }

            var cm = (ContextMenuStrip)sender;

            // TODO The handling of _openSubmoduleMenuItem need to be revised
            // This code handles the 'bold' in the menu for submodules. Other default actions are not set to bold.
            // The actual implementation of the default handling with doubleclick is in each form,
            // separate from this menu item

            if (!cm.Items.Find(_openSubmoduleMenuItem.Name, true).Any())
            {
                cm.Items.Insert(0, _openSubmoduleMenuItem);
            }

            bool isSubmoduleSelected = SelectedItem?.Item.IsSubmodule ?? false;

            _openSubmoduleMenuItem.Visible = isSubmoduleSelected;
            if (isSubmoduleSelected && !DisableSubmoduleMenuItemBold)
            {
                _openSubmoduleMenuItem.Font = AppSettings.OpenSubmoduleDiffInSeparateWindow
                    ? new Font(_openSubmoduleMenuItem.Font, FontStyle.Bold)
                    : new Font(_openSubmoduleMenuItem.Font, FontStyle.Regular);
            }

            if (!cm.Items.Find(_sortByContextMenu.Name, true).Any())
            {
                cm.Items.Add(new ToolStripSeparator());
                cm.Items.Add(_sortByContextMenu);
            }

            // Show 'Show file differences for all parents' menu item if it is possible that there are multiple first revisions
            var mayBeMultipleRevs = _revisions != null &&
                                    (_revisions.Count > 1 || (_revisions.Count == 1 && _revisions[0].ParentIds?.Count > 1));

            const string showAllDifferencesItemName = "ShowDiffForAllParentsText";
            var diffItem = cm.Items.Find(showAllDifferencesItemName, true);
            const string separatorKey = showAllDifferencesItemName + "Separator";
            if (!diffItem.Any())
            {
                cm.Items.Add(new ToolStripSeparator
                {
                    Name = separatorKey,
                    Visible = mayBeMultipleRevs
                });
                var showAllDiferencesItem = new ToolStripMenuItem(Strings.ShowDiffForAllParentsText)
                {
                    Checked = AppSettings.ShowDiffForAllParents,
                    ToolTipText = Strings.ShowDiffForAllParentsTooltip,
                    Name = showAllDifferencesItemName,
                    CheckOnClick = true,
                    Visible = mayBeMultipleRevs
                };
                showAllDiferencesItem.CheckedChanged += (s, e) =>
                {
                    AppSettings.ShowDiffForAllParents = showAllDiferencesItem.Checked;
                    SetDiffs(_revisions, _getRevision);
                };

                cm.Items.Add(showAllDiferencesItem);
            }
            else
            {
                diffItem[0].Visible = mayBeMultipleRevs;

                var sepItem = cm.Items.Find(separatorKey, true);
                if (sepItem.Length > 0)
                {
                    sepItem[0].Visible = mayBeMultipleRevs;
                }
            }
        }

        private void FileStatusListView_DoubleClick(object sender, EventArgs e)
        {
            if (DoubleClick == null)
            {
                if (SelectedItem?.Item == null)
                {
                    return;
                }

                if (AppSettings.OpenSubmoduleDiffInSeparateWindow && SelectedItem.Item.IsSubmodule)
                {
                    ThreadHelper.JoinableTaskFactory.RunAsync(OpenSubmoduleAsync);
                }
                else
                {
                    UICommands.StartFileHistoryDialog(this, SelectedItem.Item.Name, SelectedItem.SecondRevision);
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

                    foreach (FileStatusItem item in SelectedItems)
                    {
                        string fileName = _fullPathResolver.Resolve(item.Item.Name);

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

                var gitItemStatus = hoveredItem?.Tag<FileStatusItem>();

                if (gitItemStatus != null)
                {
                    string text;
                    if (gitItemStatus.Item.IsRenamed || gitItemStatus.Item.IsCopied)
                    {
                        text = string.Concat(gitItemStatus.Item.Name, " (", gitItemStatus.Item.OldName, ")");
                    }
                    else
                    {
                        text = gitItemStatus.Item.Name;
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
            FileStatusListView.ListViewItemSorter = new GitStatusListSorter(new GitItemStatusNameComparer());
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
            private IComparer<GitItemStatus> StatusComparer { get; }

            public GitStatusListSorter(IComparer<GitItemStatus> gitStatusItemSorter)
            {
                StatusComparer = gitStatusItemSorter;
            }

            public override int Compare(ListViewItem x, ListViewItem y)
                => StatusComparer.Compare((x.Tag as FileStatusItem).Item, (y.Tag as FileStatusItem).Item);
        }

        private class ImageIndexListSorter : Comparer<ListViewItem>
        {
            /// <summary>
            /// Secondary sort should be by file path.
            /// </summary>
            private static readonly GitStatusListSorter ThenBy = new GitStatusListSorter(new GitItemStatusNameComparer());

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
