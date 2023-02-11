using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.NBugReports;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public sealed partial class FileStatusList : GitModuleControl
    {
        private static readonly TimeSpan SelectedIndexChangeThrottleDuration = TimeSpan.FromMilliseconds(50);
        private readonly IFullPathResolver _fullPathResolver;
        private readonly FileStatusDiffCalculator _diffCalculator;
        private readonly SortDiffListContextMenuItem _sortByContextMenu;
        private readonly IReadOnlyList<GitItemStatus> _noItemStatuses;

        private int _nextIndexToSelect = -1;
        private bool _enableSelectedIndexChangeEvent = true;
        private bool _mouseEntered;
        private readonly ToolStripItem _NO_TRANSLATE_openSubmoduleMenuItem;
        private readonly ToolStripItem _openInVisualStudioSeparator = new ToolStripSeparator();
        private readonly ToolStripItem _NO_TRANSLATE_openInVisualStudioMenuItem;
        private Rectangle _dragBoxFromMouseDown;
        private IReadOnlyList<FileStatusWithDescription> _itemsWithDescription = new List<FileStatusWithDescription>();
        private IDisposable? _selectedIndexChangeSubscription;
        private IDisposable? _diffListSortSubscription;

        // Enable menu item to disable AppSettings.ShowDiffForAllParents in some forms
        private bool _enableDisablingShowDiffForAllParents = false;

        private bool _updatingColumnWidth;

        public delegate void EnterEventHandler(object sender, EnterEventArgs e);

        public event EventHandler? SelectedIndexChanged;
        public event EventHandler? DataSourceChanged;
        public event EventHandler? FilterChanged;

        public new event EventHandler? DoubleClick;
        public new event KeyEventHandler? KeyDown;
        public new event EnterEventHandler? Enter;

        [Description("Disable showing open submodule menu items as bold")]
        [DefaultValue(false)]
        public bool DisableSubmoduleMenuItemBold { get; set; }

        private readonly Dictionary<string, int> _stateImageIndexDict = new();

        public FileStatusList()
        {
            InitializeComponent();
            InitialiseFiltering();
            _NO_TRANSLATE_openSubmoduleMenuItem = CreateOpenSubmoduleMenuItem();
            _NO_TRANSLATE_openInVisualStudioMenuItem = CreateOpenInVisualStudioMenuItem();
            _sortByContextMenu = new SortDiffListContextMenuItem(DiffListSortService.Instance)
            {
                Name = "sortListByContextMenuItem"
            };

            SetupUnifiedDiffListSorting();
            lblSplitter.Height = DpiUtil.Scale(1);
            InitializeComplete();
            FilterVisible = true;

            SelectFirstItemOnSetItems = true;

            FileStatusListView.SmallImageList = CreateImageList();
            FileStatusListView.LargeImageList = FileStatusListView.SmallImageList;

            HandleVisibility_NoFilesLabel_FilterComboBox(filesPresent: true);
            NoFiles.Text = TranslatedStrings.NoChanges;
            LoadingFiles.Text = TranslatedStrings.LoadingData;
            Controls.SetChildIndex(NoFiles, 0);
            Controls.SetChildIndex(LoadingFiles, 0);
            NoFiles.Font = new Font(NoFiles.Font, FontStyle.Italic);
            LoadingFiles.Font = new Font(LoadingFiles.Font, FontStyle.Italic);
            FilterWatermarkLabel.Font = new Font(FilterWatermarkLabel.Font, FontStyle.Italic);
            FilterComboBox.Font = new Font(FilterComboBox.Font, FontStyle.Bold);

            _diffCalculator = new FileStatusDiffCalculator(() => Module);
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _noItemStatuses = new[]
            {
                new GitItemStatus(name: $"     - {NoFiles.Text} -")
                {
                    IsStatusOnly = true,
                    ErrorMessage = string.Empty
                }
            };

            base.Enter += FileStatusList_Enter;

            return;

            ImageList CreateImageList()
            {
                const int rowHeight = 18;

                ImageList list = new()
                {
                    ColorDepth = ColorDepth.Depth32Bit,
                    ImageSize = DpiUtil.Scale(new Size(16, rowHeight)), // Scale ImageSize and images scale automatically
                };

                var images = new (string imageKey, Bitmap icon)[]
                {
                    (nameof(Images.FileStatusUnknown), ScaleHeight(Images.FileStatusUnknown)),
                    (nameof(Images.FileStatusModified), ScaleHeight(Images.FileStatusModified)),
                    (nameof(Images.FileStatusModifiedOnlyA), ScaleHeight(Images.FileStatusModifiedOnlyA)),
                    (nameof(Images.FileStatusModifiedOnlyB), ScaleHeight(Images.FileStatusModifiedOnlyB)),
                    (nameof(Images.FileStatusModifiedSame), ScaleHeight(Images.FileStatusModifiedSame)),
                    (nameof(Images.FileStatusModifiedUnequal), ScaleHeight(Images.FileStatusModifiedUnequal)),
                    (nameof(Images.FileStatusAdded), ScaleHeight(Images.FileStatusAdded)),
                    (nameof(Images.FileStatusAddedOnlyA), ScaleHeight(Images.FileStatusAddedOnlyA)),
                    (nameof(Images.FileStatusAddedOnlyB), ScaleHeight(Images.FileStatusAddedOnlyB)),
                    (nameof(Images.FileStatusAddedSame), ScaleHeight(Images.FileStatusAddedSame)),
                    (nameof(Images.FileStatusAddedUnequal), ScaleHeight(Images.FileStatusAddedUnequal)),
                    (nameof(Images.FileStatusRemoved), ScaleHeight(Images.FileStatusRemoved)),
                    (nameof(Images.FileStatusRemovedOnlyA), ScaleHeight(Images.FileStatusRemovedOnlyA)),
                    (nameof(Images.FileStatusRemovedOnlyB), ScaleHeight(Images.FileStatusRemovedOnlyB)),
                    (nameof(Images.FileStatusRemovedSame), ScaleHeight(Images.FileStatusRemovedSame)),
                    (nameof(Images.FileStatusRemovedUnequal), ScaleHeight(Images.FileStatusRemovedUnequal)),
                    (nameof(Images.Conflict), ScaleHeight(Images.Conflict)),
                    (nameof(Images.FileStatusRenamed), ScaleHeight(Images.FileStatusRenamed.AdaptLightness())),
                    (nameof(Images.FileStatusRenamedOnlyA), ScaleHeight(Images.FileStatusRenamedOnlyA)),
                    (nameof(Images.FileStatusRenamedOnlyB), ScaleHeight(Images.FileStatusRenamedOnlyB)),
                    (nameof(Images.FileStatusRenamedSame), ScaleHeight(Images.FileStatusRenamedSame)),
                    (nameof(Images.FileStatusRenamedUnequal), ScaleHeight(Images.FileStatusRenamedUnequal)),
                    (nameof(Images.FileStatusCopied), ScaleHeight(Images.FileStatusCopied)),
                    (nameof(Images.FileStatusCopiedOnlyA), ScaleHeight(Images.FileStatusCopiedOnlyA)),
                    (nameof(Images.FileStatusCopiedOnlyB), ScaleHeight(Images.FileStatusCopiedOnlyB)),
                    (nameof(Images.FileStatusCopiedSame), ScaleHeight(Images.FileStatusCopiedSame)),
                    (nameof(Images.FileStatusCopiedUnequal), ScaleHeight(Images.FileStatusCopiedUnequal)),
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
                    (nameof(Images.SubmoduleRevisionSemiDownDirty), ScaleHeight(Images.SubmoduleRevisionSemiDownDirty)),
                    (nameof(Images.Diff), ScaleHeight(Images.Diff))
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
                    Bitmap scaled = new(input.Width, rowHeight, input.PixelFormat);
                    using var g = Graphics.FromImage(scaled);
                    g.DrawImageUnscaled(input, 0, (rowHeight - input.Height) / 2);

                    return scaled;
                }
            }

            ToolStripMenuItem CreateOpenSubmoduleMenuItem()
            {
                ToolStripMenuItem item = new()
                {
                    Name = "openSubmoduleMenuItem",
                    Tag = "1",
                    Text = TranslatedStrings.OpenWithGitExtensions,
                    Image = Images.GitExtensionsLogo16
                };
                item.Click += (_, _) => { ThreadHelper.JoinableTaskFactory.RunAsync(OpenSubmoduleAsync); };
                return item;
            }

            ToolStripMenuItem CreateOpenInVisualStudioMenuItem()
            {
                ToolStripMenuItem item = new()
                {
                    Name = "openInVisualStudioMenuItem",
                    Text = TranslatedStrings.OpenInVisualStudio,
                    Image = Images.VisualStudio16
                };
                item.Click += (_, _) =>
                {
                    var itemName = SelectedItemAbsolutePath;
                    if (itemName != null && !VisualStudioIntegration.TryOpenFile(itemName))
                    {
                        MessageBox.Show(
                            TranslatedStrings.OpenInVisualStudioFailureText,
                            TranslatedStrings.OpenInVisualStudioFailureCaption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                };
                return item;
            }
        }

        public void Bind(Func<ObjectId, string> describeRevision, Func<GitRevision, GitRevision> getActualRevision)
        {
            DescribeRevision = describeRevision;
            _diffCalculator.DescribeRevision = describeRevision;
            _diffCalculator.GetActualRevision = getActualRevision;
        }

        private string? SelectedItemAbsolutePath => _fullPathResolver.Resolve(SelectedItem?.Item.Name)?.NormalizePath();

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

        // Properties

        [Browsable(false)]
        public IEnumerable<FileStatusItem> AllItems => FileStatusListView.ItemTags<FileStatusItem>();

        public int AllItemsCount => FileStatusListView.Items.Count;

        public override ContextMenuStrip ContextMenuStrip
        {
            get { return FileStatusListView.ContextMenuStrip; }
            set
            {
                if (FileStatusListView.ContextMenuStrip == value)
                {
                    return;
                }

                if (FileStatusListView.ContextMenuStrip is not null)
                {
                    FileStatusListView.ContextMenuStrip.Opening -= FileStatusListView_ContextMenu_Opening;
                }

                FileStatusListView.ContextMenuStrip = value;

                if (FileStatusListView.ContextMenuStrip is not null)
                {
                    FileStatusListView.ContextMenuStrip.Opening += FileStatusListView_ContextMenu_Opening;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Func<ObjectId?, string>? DescribeRevision { get; set; }

        public bool FilterFocused => FilterComboBox.Focused;

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
                return GitItemStatusesWithDescription?.SelectMany(tuple => tuple.Statuses).AsReadOnlyList()
                       ?? Array.Empty<GitItemStatus>();
            }
        }

        private IReadOnlyList<FileStatusWithDescription> GitItemStatusesWithDescription
        {
            get => _itemsWithDescription;
            set
            {
                _itemsWithDescription = value ?? throw new ArgumentNullException(nameof(value));
                UpdateFileStatusListView();
            }
        }

        public bool GroupByRevision { get; set; }

        [Browsable(false)]
        [DefaultValue(true)]
        public bool IsEmpty => GitItemStatuses is null || !GitItemStatuses.Any();

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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitItemStatus? SelectedGitItem
        {
            get => SelectedItem?.Item;
            set
            {
                var itemToBeSelected = GetItemByStatus(value);
                SelectItems(item => item == itemToBeSelected);
                return;

                ListViewItem? GetItemByStatus(GitItemStatus? status)
                {
                    if (status is null)
                    {
                        return null;
                    }

                    ListViewItem? newSelected = null;
                    foreach (ListViewItem item in FileStatusListView.Items)
                    {
                        var gitItemStatus = item.Tag<FileStatusItem>();
                        if (gitItemStatus.Item == status)
                        {
                            return item;
                        }

                        if (status.CompareName(gitItemStatus.Item) == 0 && newSelected is null)
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
                if (value is null)
                {
                    ClearSelected();
                    return;
                }

                SelectItems(item => value.Contains(item.Tag<FileStatusItem>().Item));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public FileStatusItem? SelectedItem => FileStatusListView.LastSelectedItem()?.Tag<FileStatusItem>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<FileStatusItem> SelectedItems
        {
            get => FileStatusListView.SelectedItemTags<FileStatusItem>();
            set
            {
                if (value is null)
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

        public int UnfilteredItemsCount => GitItemStatusesWithDescription?.Sum(tuple => tuple.Statuses.Count) ?? 0;

        public bool IsFilterActive => !string.IsNullOrEmpty(FilterComboBox.Text);

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
                if (SelectedItem is null)
                {
                    SelectedIndex = 0;
                }

                FileStatusListView.Focus();
            }
        }

        private static (Image? image, string? prefix, string text, string? suffix, int prefixTextStartX, int textWidth, int textMaxWidth)
            FormatListViewItem(ListViewItem item, PathFormatter formatter, int itemWidth)
        {
            var gitItemStatus = item.Tag<FileStatusItem>().Item;
            var image = item.Image();
            int itemLeft = item.Position.X;

            var prefixTextStartX = itemLeft + (image?.Width ?? 0);
            var textMaxWidth = itemWidth - prefixTextStartX;
            var (prefix, text, suffix, textWidth) = formatter.FormatTextForDrawing(textMaxWidth, gitItemStatus.Name, gitItemStatus.OldName);
            text = AppendItemSubmoduleStatus(text ?? "", gitItemStatus);

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
            ListViewGroup currentGroup = currentItem.Group;

            if (searchBackward)
            {
                ListViewItem? nextItem = FindPrevItemInGroups();
                if (nextItem is null)
                {
                    return loop ? GetLastIndex() : curIdx;
                }

                return nextItem.Index;
            }
            else
            {
                ListViewItem? nextItem = FindNextItemInGroups();
                if (nextItem is null)
                {
                    return loop ? GetFirstIndex() : curIdx;
                }

                return nextItem.Index;
            }

            ListViewItem? FindPrevItemInGroups()
            {
                List<ListViewGroup> searchInGroups = new();
                bool foundCurrentGroup = false;
                for (int i = FileStatusListView.Groups.Count - 1; i >= 0; i--)
                {
                    if (FileStatusListView.Groups[i] == currentGroup)
                    {
                        foundCurrentGroup = true;
                    }

                    if (foundCurrentGroup && ContainsSearchableItem(FileStatusListView.Groups[i]))
                    {
                        searchInGroups.Add(FileStatusListView.Groups[i]);
                    }
                }

                int idx = ContainsSearchableItem(currentGroup) ? curIdx : FileStatusListView.Items.Count;
                foreach (ListViewGroup grp in searchInGroups)
                {
                    for (int i = idx - 1; i >= 0; i--)
                    {
                        ListViewItem item = FileStatusListView.Items[i];
                        if (item.Group == grp && IsSearchableItem(item))
                        {
                            return item;
                        }
                    }

                    idx = FileStatusListView.Items.Count;
                }

                return null;
            }

            ListViewItem? FindNextItemInGroups()
            {
                List<ListViewGroup> searchInGroups = new();
                bool foundCurrentGroup = false;
                for (int i = 0; i < FileStatusListView.Groups.Count; i++)
                {
                    if (FileStatusListView.Groups[i] == currentGroup)
                    {
                        foundCurrentGroup = true;
                    }

                    if (foundCurrentGroup && ContainsSearchableItem(FileStatusListView.Groups[i]))
                    {
                        searchInGroups.Add(FileStatusListView.Groups[i]);
                    }
                }

                int idx = ContainsSearchableItem(currentGroup) ? curIdx : -1;
                foreach (ListViewGroup grp in searchInGroups)
                {
                    for (int i = idx + 1; i < FileStatusListView.Items.Count; i++)
                    {
                        ListViewItem item = FileStatusListView.Items[i];
                        if (item.Group == grp && IsSearchableItem(item))
                        {
                            return item;
                        }
                    }

                    idx = -1;
                }

                return null;
            }

            int GetFirstIndex()
            {
                if (FileStatusListView.Items.Count == 0)
                {
                    return -1;
                }

                if (FileStatusListView.Groups.Count < 2)
                {
                    return 0;
                }

                ListViewGroup? firstNonEmptyGroup = null;
                foreach (ListViewGroup group in FileStatusListView.Groups)
                {
                    if (ContainsSearchableItem(group))
                    {
                        firstNonEmptyGroup = group;
                        break;
                    }
                }

                for (int i = 0; i < FileStatusListView.Items.Count; ++i)
                {
                    ListViewItem item = FileStatusListView.Items[i];
                    if (item.Group == firstNonEmptyGroup && IsSearchableItem(item))
                    {
                        return i;
                    }
                }

                return -1;
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

                ListViewGroup? lastNonEmptyGroup = null;
                for (int i = FileStatusListView.Groups.Count - 1; i >= 0; i--)
                {
                    if (ContainsSearchableItem(FileStatusListView.Groups[i]))
                    {
                        lastNonEmptyGroup = FileStatusListView.Groups[i];
                        break;
                    }
                }

                for (int i = FileStatusListView.Items.Count - 1; i >= 0; i--)
                {
                    ListViewItem item = FileStatusListView.Items[i];
                    if (item.Group == lastNonEmptyGroup && IsSearchableItem(item))
                    {
                        return i;
                    }
                }

                return -1;
            }

            static bool IsSearchableItem(ListViewItem item)
            {
                return item.Tag is FileStatusItem fileStatusItem
                    && !fileStatusItem.Item.IsStatusOnly
                    && !fileStatusItem.Item.IsRangeDiff;
            }

            static bool ContainsSearchableItem(ListViewGroup group)
            {
                foreach (ListViewItem item in group.Items)
                {
                    if (IsSearchableItem(item))
                    {
                        return true;
                    }
                }

                return false;
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
            if (group is not null)
            {
                ListViewItem? sortedFirstGroupItem = FileStatusListView.Items().FirstOrDefault(item => item.Group == group);
                if (sortedFirstGroupItem is not null)
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
            _enableDisablingShowDiffForAllParents = true;
            GitItemStatusesWithDescription = _diffCalculator.SetDiffs(revisions, headId: null, allowMultiDiff: false, cancellationToken: default);
        }

        public async Task SetDiffsAsync(IReadOnlyList<GitRevision> revisions, ObjectId? headId, CancellationToken cancellationToken)
        {
            _enableDisablingShowDiffForAllParents = true;
            await this.SwitchToMainThreadAsync(cancellationToken);
            FileStatusListLoading();

            await TaskScheduler.Default;
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<FileStatusWithDescription> gitItemStatusesWithDescription = _diffCalculator.SetDiffs(revisions, headId, allowMultiDiff: true, cancellationToken);

            await this.SwitchToMainThreadAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            GitItemStatusesWithDescription = gitItemStatusesWithDescription;
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
            GroupByRevision = true;
            GitItemStatusesWithDescription = new List<FileStatusWithDescription>
            {
                new FileStatusWithDescription(
                    firstRev: indexRev,
                    secondRev: workTreeRev,
                    summary: workTreeDesc,
                    statuses: workTreeItems),
                new FileStatusWithDescription(
                    firstRev: headRev,
                    secondRev: indexRev,
                    summary: indexDesc,
                    statuses: indexItems)
            };
        }

        public void SetDiffs(GitRevision? firstRev, GitRevision secondRev, IReadOnlyList<GitItemStatus> items)
        {
            GroupByRevision = false;
            GitItemStatusesWithDescription = new List<FileStatusWithDescription>
            {
                new FileStatusWithDescription(
                    firstRev: firstRev,
                    secondRev: secondRev,
                    summary: TranslatedStrings.DiffWithParent + GetDescriptionForRevision(firstRev?.ObjectId),
                    statuses: items)
            };
        }

        public void ClearDiffs()
        {
            GitItemStatusesWithDescription = new List<FileStatusWithDescription>();
        }

        private string? GetDescriptionForRevision(ObjectId? objectId) =>
            DescribeRevision is not null ? DescribeRevision(objectId) : objectId?.ToShortString();

        public void SetNoFilesText(string text)
        {
            NoFiles.Text = text;
        }

        public void SetSelectedIndex(int idx, bool notify)
        {
            _enableSelectedIndexChangeEvent = notify;
            try
            {
                SelectedIndex = idx;

                ListViewGroup? group = FileStatusListView.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.Group;
                if (group?.CollapsedState is ListViewGroupCollapsedState.Collapsed)
                {
                    group.CollapsedState = ListViewGroupCollapsedState.Expanded;
                }
            }
            finally
            {
                _enableSelectedIndexChangeEvent = true;
            }
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

        private static string AppendItemSubmoduleStatus(string text, GitItemStatus item)
        {
            if (item.IsSubmodule
                && item.GetSubmoduleStatusAsync() is Task<GitSubmoduleStatus> task
                && task is not null
                && task.IsCompleted
                && task.CompletedResult() is not null)
            {
                text += task.CompletedResult()!.AddedAndRemovedString();
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
            Validates.NotNull(SelectedItem);

            var submoduleName = SelectedItem.Item.Name;

            Task<GitSubmoduleStatus?>? task = SelectedItem.Item.GetSubmoduleStatusAsync();
            GitSubmoduleStatus? status = task is not null
                ? await task.ConfigureAwait(false)
                : null;

            ObjectId? selectedId = SelectedItem.SecondRevision?.ObjectId == ObjectId.WorkTreeId
                ? ObjectId.WorkTreeId
                : status?.Commit;
            ObjectId? firstId = status?.OldCommit;

            string path = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator()) ?? "";
            if (!Directory.Exists(path))
            {
                MessageBoxes.SubmoduleDirectoryDoesNotExist(this, path, submoduleName);
                return;
            }

            GitUICommands.LaunchBrowse(workingDir: path, selectedId, firstId);
        }

        private void SelectItems(Func<ListViewItem, bool> predicate)
        {
            try
            {
                FileStatusListView.BeginUpdate();

                ListViewItem? firstSelectedItem = null;
                foreach (var item in FileStatusListView.Items())
                {
                    item.Selected = predicate(item);
                    if (item.Selected && firstSelectedItem is null)
                    {
                        firstSelectedItem = item;
                    }
                }

                if (firstSelectedItem is not null)
                {
                    firstSelectedItem.Focused = true;
                    firstSelectedItem.Selected = true;
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

        private void FileStatusListLoading()
        {
            LoadingFiles.Visible = true;
            NoFiles.Visible = false;

            FileStatusListView.BeginUpdate();
            FileStatusListView.Groups.Clear();
            FileStatusListView.Items.Clear();
            FileStatusListView.EndUpdate();
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

            HashSet<GitItemStatus>? previouslySelectedItems = null;

            if (updateCausedByFilter)
            {
                previouslySelectedItems = FileStatusListView.SelectedItems()
                    .Select(i => i.Tag<FileStatusItem>().Item)
                    .ToHashSet();

                DataSourceChanged?.Invoke(this, EventArgs.Empty);
            }

            FileStatusListView.BeginUpdate();
            FileStatusListView.ShowGroups = GitItemStatusesWithDescription.Count > 1 || GroupByRevision;
            FileStatusListView.Groups.Clear();
            FileStatusListView.Items.Clear();

            bool hasChanges = GitItemStatusesWithDescription.Any(x => x.Statuses.Count > 0);

            List<ListViewItem> list = new();
            foreach (var i in GitItemStatusesWithDescription)
            {
                string name = i.Statuses.Count == 1 && i.Statuses[0].IsRangeDiff
                    ? i.Summary
                    : $"({i.Statuses.Count}) {i.Summary}";
                ListViewGroup group = new(name)
                {
                    // Collapse some groups for diffs with common BASE
                    CollapsedState = i.Statuses.Count <= 7 || GitItemStatusesWithDescription.Count < 3 || i == GitItemStatusesWithDescription[0]
                        ? ListViewGroupCollapsedState.Expanded
                        : ListViewGroupCollapsedState.Collapsed,
                    Tag = i.FirstRev
                };
                FileStatusListView.Groups.Add(group);

                IReadOnlyList<GitItemStatus> itemStatuses;
                if (hasChanges && i.Statuses.Count == 0)
                {
                    itemStatuses = _noItemStatuses;
                    if (group is not null)
                    {
                        group.CollapsedState = ListViewGroupCollapsedState.Collapsed;
                    }
                }
                else
                {
                    itemStatuses = i.Statuses;
                }

                foreach (var item in itemStatuses)
                {
                    if (!IsFilterMatch(item))
                    {
                        continue;
                    }

                    ListViewItem listItem = new(string.Empty, group);

                    if (!item.IsStatusOnly || !string.IsNullOrWhiteSpace(item.ErrorMessage))
                    {
                        listItem.ImageIndex = GetItemImageIndex(item);
                    }

                    if (item.IsSubmodule
                        && item.GetSubmoduleStatusAsync() is Task<GitSubmoduleStatus> task
                        && task is not null)
                    {
                        var capturedItem = item;

                        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                        {
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                            await task;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

                            await this.SwitchToMainThreadAsync();

                            listItem.ImageIndex = GetItemImageIndex(capturedItem);
                        });
                    }

                    if (previouslySelectedItems?.Contains(item) == true)
                    {
                        listItem.Selected = true;
                    }

                    listItem.Tag = new FileStatusItem(i.FirstRev, i.SecondRev, item, i.BaseA, i.BaseB);
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
                _selectedIndexChangeSubscription ??= Observable.FromEventPattern(
                        h => FileStatusListView.SelectedIndexChanged += h,
                        h => FileStatusListView.SelectedIndexChanged -= h)
                    .Where(x => _enableSelectedIndexChangeEvent)
                    .Throttle(SelectedIndexChangeThrottleDuration, MainThreadScheduler.Instance)
                    .ObserveOn(MainThreadScheduler.Instance)
                    .Subscribe(_ => FileStatusListView_SelectedIndexChanged());
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
                    return nameof(Images.Diff);
                }

                if (gitItemStatus.IsNew || (!gitItemStatus.IsTracked && !gitItemStatus.IsSubmodule))
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

                if (gitItemStatus.IsConflict)
                {
                    return nameof(Images.Conflict);
                }

                if (gitItemStatus.IsSubmodule)
                {
                    if (gitItemStatus.GetSubmoduleStatusAsync() is not Task<GitSubmoduleStatus> task
                        || task is null
                        || !task.IsCompleted
                        || task.CompletedResult() is not GitSubmoduleStatus status
                        || status is null)
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
                PathFormatter pathFormatter = new(FileStatusListView.CreateGraphics(), FileStatusListView.Font);
                int controlWidth = FileStatusListView.ClientSize.Width;

                int contentWidth = 0;
                try
                {
                    contentWidth = FileStatusListView.Items()
                        .Where(item => item.BoundsOrEmpty().IntersectsWith(FileStatusListView.ClientRectangle))
                        .Select(item =>
                        {
                            (_, _, _, _, int textStart, int textWidth, _) = FormatListViewItem(item, pathFormatter, FileStatusListView.ClientSize.Width);
                            return textStart + textWidth;
                        })
                        .DefaultIfEmpty(controlWidth)
                        .Max();
                }
                catch (ExternalException exception)
                {
                    // See https://github.com/gitextensions/gitextensions/issues/9166#issuecomment-849567022
                    // A rather obscure bug report, which may be causing random app crashes
                    BugReportInvoker.LogError(exception);
                }

                return Math.Max(contentWidth, controlWidth);
            }
        }

        private void HandleVisibility_NoFilesLabel_FilterComboBox(bool filesPresent)
        {
            LoadingFiles.Visible = false;
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

            ListViewItem? selectedItemFound = null;
            for (int i = FileStatusListView.Groups.Count - 1; i >= 0; i--)
            {
                var group = FileStatusListView.Groups[i];
                var groupItems = FileStatusListView.Items
                    .Cast<ListViewItem>()
                    .Where(item => item.Group == group)
                    .Reverse();
                foreach (var item in groupItems)
                {
                    if (selectedItemFound is not null)
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

            ListViewItem? selectedItemFound = null;
            foreach (var group in FileStatusListView.Groups)
            {
                var groupItems = FileStatusListView.Items
                    .Cast<ListViewItem>()
                    .Where(item => item.Group == group);
                foreach (var item in groupItems)
                {
                    if (selectedItemFound is not null)
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
            if (SelectedItem?.Item.IsStatusOnly ?? false)
            {
                e.Cancel = true;
                return;
            }

            var cm = (ContextMenuStrip)sender;

            // TODO The handling of _NO_TRANSLATE_openSubmoduleMenuItem need to be revised
            // This code handles the 'bold' in the menu for submodules. Other default actions are not set to bold.
            // The actual implementation of the default handling with doubleclick is in each form,
            // separate from this menu item

            if (!cm.Items.Find(_NO_TRANSLATE_openSubmoduleMenuItem.Name, true).Any())
            {
                cm.Items.Insert(0, _NO_TRANSLATE_openSubmoduleMenuItem);
            }

            bool isSubmoduleSelected = SelectedItem?.Item.IsSubmodule ?? false;

            _NO_TRANSLATE_openSubmoduleMenuItem.Visible = isSubmoduleSelected;
            if (isSubmoduleSelected && !DisableSubmoduleMenuItemBold)
            {
                _NO_TRANSLATE_openSubmoduleMenuItem.Font = AppSettings.OpenSubmoduleDiffInSeparateWindow
                    ? new Font(_NO_TRANSLATE_openSubmoduleMenuItem.Font, FontStyle.Bold)
                    : new Font(_NO_TRANSLATE_openSubmoduleMenuItem.Font, FontStyle.Regular);
            }

            if (!cm.Items.Find(_NO_TRANSLATE_openInVisualStudioMenuItem.Name, true).Any())
            {
                cm.Items.Add(_openInVisualStudioSeparator);
                cm.Items.Add(_NO_TRANSLATE_openInVisualStudioMenuItem);
            }

            _NO_TRANSLATE_openInVisualStudioMenuItem.Visible = _openInVisualStudioSeparator.Visible = VisualStudioIntegration.IsVisualStudioRunning;
            _NO_TRANSLATE_openInVisualStudioMenuItem.Enabled = File.Exists(SelectedItemAbsolutePath);

            if (!cm.Items.Find(_sortByContextMenu.Name, true).Any())
            {
                cm.Items.Add(new ToolStripSeparator());
                cm.Items.Add(_sortByContextMenu);
            }

            // Show 'Show file differences for all parents' menu item if it is possible that there are multiple first revisions
            var mayBeMultipleRevs = _enableDisablingShowDiffForAllParents && _itemsWithDescription.Count > 1;

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
                ToolStripMenuItem showAllDiferencesItem = new(TranslatedStrings.ShowDiffForAllParentsText)
                {
                    Checked = AppSettings.ShowDiffForAllParents,
                    ToolTipText = TranslatedStrings.ShowDiffForAllParentsTooltip,
                    Name = showAllDifferencesItemName,
                    CheckOnClick = true,
                    Visible = mayBeMultipleRevs
                };
                showAllDiferencesItem.CheckedChanged += (s, e) =>
                {
                    AppSettings.ShowDiffForAllParents = showAllDiferencesItem.Checked;
                    FileStatusListLoading();
                    ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        await TaskScheduler.Default;
                        IReadOnlyList<FileStatusWithDescription> gitItemStatusesWithDescription = _diffCalculator.Reload();

                        await this.SwitchToMainThreadAsync();
                        GitItemStatusesWithDescription = gitItemStatusesWithDescription;
                    });
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
            if (DoubleClick is null)
            {
                if (SelectedItem?.Item is null)
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
            PathFormatter formatter = new(e.Graphics, FileStatusListView.Font);

            var (image, prefix, text, suffix, prefixTextStartX, _, textMaxWidth) = FormatListViewItem(item, formatter, item.Bounds.Width);

            if (item.Selected)
            {
                e.Graphics.FillRectangle(Focused ? SystemBrushes.Highlight : OtherColors.InactiveSelectionHighlightBrush, e.Bounds);
            }

            if (image is not null)
            {
                e.Graphics.DrawImageUnscaled(image, item.Position.X, item.Position.Y);
            }

            if (!string.IsNullOrEmpty(text))
            {
                Rectangle textRect = new(prefixTextStartX, item.Bounds.Top, textMaxWidth, item.Bounds.Height);

                Color grayTextColor = item.Selected && Focused
                    ? ColorHelper.GetHighlightGrayTextColor(
                        backgroundColorName: KnownColor.Window,
                        textColorName: KnownColor.WindowText,
                        highlightColorName: KnownColor.Highlight)
                    : SystemColors.GrayText;

                Color textColor = item.Selected && Focused
                    ? SystemColors.HighlightText
                    : SystemColors.WindowText;

                if (!string.IsNullOrEmpty(prefix))
                {
                    DrawString(textRect, prefix, grayTextColor);
                    var prefixSize = formatter.MeasureString(prefix);
                    textRect.Offset(prefixSize.Width, 0);
                }

                DrawString(textRect, text, textColor);

                if (!string.IsNullOrEmpty(suffix))
                {
                    var textSize = formatter.MeasureString(text);
                    textRect.Offset(textSize.Width, 0);
                    DrawString(textRect, suffix, grayTextColor);
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

                if (hover.Item is not null && !hover.Item.Selected)
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

        private void FileStatusListView_MouseUp(object sender, MouseEventArgs e)
        {
            // Release the drag capture
            if (e.Button == MouseButtons.Left)
            {
                _dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        private void FileStatusListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListView? listView = sender as ListView;

            // DRAG
            // If the mouse moves outside the rectangle, start the drag.
            if (_dragBoxFromMouseDown != Rectangle.Empty &&
                !_dragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                if (SelectedItems.Any())
                {
                    StringCollection fileList = new();

                    foreach (FileStatusItem item in SelectedItems)
                    {
                        string? fileName = _fullPathResolver.Resolve(item.Item.Name);

                        if (!string.IsNullOrWhiteSpace(fileName))
                        {
                            fileList.Add(fileName.ToNativePath());
                        }
                    }

                    DataObject obj = new();
                    obj.SetFileDropList(fileList);

                    // Proceed with the drag and drop, passing in the list item.
                    DoDragDrop(obj, DragDropEffects.Copy);
                    _dragBoxFromMouseDown = Rectangle.Empty;
                }
            }

            // TOOLTIP
            if (listView is not null)
            {
                ListViewItem? hoveredItem;
                try
                {
                    Point point = new(e.X, e.Y);
                    hoveredItem = listView.HitTest(point).Item;
                }
                catch (ArgumentOutOfRangeException)
                {
                    hoveredItem = null;
                }

                var gitItemStatus = hoveredItem?.Tag<FileStatusItem>();

                if (gitItemStatus is not null)
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
                    if (textWidth > (FileStatusListView.Width - FileStatusListView.GetItemRect(hoveredItem!.Index).Height))
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
        private readonly Subject<string> _filterSubject = new();
        private Regex? _filter;
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
            FilterChanged?.Invoke(this, EventArgs.Empty);
            return FileStatusListView.Items.Count;
        }

        private bool IsFilterMatch(GitItemStatus item)
        {
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

            if (_filter.IsMatch(name))
            {
                return true;
            }

            return oldName is not null && _filter.IsMatch(oldName);
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

            // RangeDiff should always be sorted last in the group
            public static int CompareRangeDiff(ListViewItem x, ListViewItem y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }
                else if (x?.Tag is null)
                {
                    return -1;
                }
                else if (y?.Tag is null)
                {
                    return 1;
                }

                if (((FileStatusItem)x.Tag).Item.IsRangeDiff)
                {
                    return 1;
                }
                else if (((FileStatusItem)y.Tag).Item.IsRangeDiff)
                {
                    return -1;
                }

                return 0;
            }

            public override int Compare(ListViewItem x, ListViewItem y)
            {
                int statusResult = CompareRangeDiff(x, y);
                if (statusResult != 0)
                {
                    return statusResult;
                }

                return StatusComparer.Compare(((FileStatusItem)x.Tag).Item, ((FileStatusItem)y.Tag).Item);
            }
        }

        private class ImageIndexListSorter : Comparer<ListViewItem>
        {
            /// <summary>
            /// Secondary sort should be by file path.
            /// </summary>
            private static readonly GitStatusListSorter ThenBy = new(new GitItemStatusNameComparer());

            public override int Compare(ListViewItem x, ListViewItem y)
            {
                int statusResult = GitStatusListSorter.CompareRangeDiff(x, y);
                if (statusResult != 0)
                {
                    return statusResult;
                }

                // All indexes, does not have "overlay", check explicitly
                // Sort in reverse alphabetic order with Unequal first
                statusResult = -((FileStatusItem)x.Tag).Item.DiffStatus.CompareTo(((FileStatusItem)y.Tag).Item.DiffStatus);
                if (statusResult == 0)
                {
                    statusResult = x.ImageIndex.CompareTo(y.ImageIndex);
                    if (statusResult == 0)
                    {
                        return ThenBy.Compare(x, y);
                    }
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

        internal TestAccessor GetTestAccessor() => new(this);

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
            internal Regex? Filter => _fileStatusList._filter;
            internal bool FilterWatermarkLabelVisible => _fileStatusList.FilterWatermarkLabel.Visible;
            internal void StoreFilter(string value) => _fileStatusList.StoreFilter(value);
        }
    }
}
