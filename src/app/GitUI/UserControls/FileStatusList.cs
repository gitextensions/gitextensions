#nullable enable

using System.Collections.Frozen;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitUI
{
    public sealed partial class FileStatusList : GitModuleControl
    {
        private const string _showDiffForAllParentsItemName = nameof(TranslatedStrings.ShowDiffForAllParentsText);

        private static readonly TimeSpan SelectedIndexChangeThrottleDuration = TimeSpan.FromMilliseconds(50);
        private readonly IFullPathResolver _fullPathResolver;
        private readonly FileStatusDiffCalculator _diffCalculator;
        private readonly SortDiffListContextMenuItem _sortByContextMenu;
        private readonly StatusSorter _sorter = new();
        private readonly IReadOnlyList<GitItemStatus> _noItemStatuses;
        private readonly ToolStripItem _NO_TRANSLATE_openSubmoduleMenuItem;
        private readonly ToolStripItem _openInVisualStudioSeparator = new ToolStripSeparator();
        private readonly ToolStripItem _NO_TRANSLATE_openInVisualStudioMenuItem;
        private readonly CancellationTokenSequence _reloadSequence = new();
        private readonly ToolStripItem _showDiffForAllParentsSeparator = new ToolStripSeparator() { Name = $"{_showDiffForAllParentsItemName}Separator" };
        private readonly ToolStripItem _sortBySeparator = new ToolStripSeparator();

        private GitItemStatus? _nextItemToSelect = null;
        private bool _enableSelectedIndexChangeEvent = true;
        private bool _flatList = false;
        private GroupBy? _groupBy = null;
        private bool _mouseEntered;
        private Rectangle _dragBoxFromMouseDown;
        private IDisposable? _selectedIndexChangeSubscription;
        private IDisposable? _diffListSortSubscription;
        private FormFindInCommitFilesGitGrep? _formFindInCommitFilesGitGrep;
        private bool _showDiffGroups = false;

        // Enable menu item to disable AppSettings.ShowDiffForAllParents in some forms
        private bool _enableDisablingShowDiffForAllParents = false;

        [GeneratedRegex(@"(^|\s)-e(\s|\s+['""])", RegexOptions.ExplicitCapture)]
        private static partial Regex GrepStringRegex();

        public delegate void EnterEventHandler(object? sender, EnterEventArgs e);

        public event EventHandler? SelectedIndexChanged;
        public event EventHandler? DataSourceChanged;
        public event EventHandler? FilterChanged;

        public new event EventHandler? DoubleClick;
        public new event KeyEventHandler? KeyDown;
        public new event EnterEventHandler? Enter;

        [Description("Disable showing open submodule menu items as bold")]
        [DefaultValue(false)]
        public bool DisableSubmoduleMenuItemBold { get; set; }

        private record ImageListData(ImageList ImageList, FrozenDictionary<string, int> StateImageIndexMap);

        private static readonly ImageListData _imageListData = CreateImageListData();

        public FileStatusList()
        {
            InitializeComponent();
            InitialiseFiltering();
            Disposed += (sender, e) =>
            {
                _formFindInCommitFilesGitGrep?.Dispose();
            };

            CreateTreeContextMenuItems();
            _NO_TRANSLATE_openSubmoduleMenuItem = CreateOpenSubmoduleMenuItem();
            _NO_TRANSLATE_openInVisualStudioMenuItem = CreateOpenInVisualStudioMenuItem();
            _sortByContextMenu = new SortDiffListContextMenuItem(DiffListSortService.Instance)
            {
                Name = "sortListByContextMenuItem"
            };

            SetupUnifiedDiffListSorting();
            lblSplitter.Height = DpiUtil.Scale(1);
            InitializeComplete();

            SelectFirstItemOnSetItems = true;

            FileStatusListView.Indent = DpiUtil.Scale(14);
            FileStatusListView.ImageList = _imageListData.ImageList;
            FileStatusListView.StateImageList = _imageListData.ImageList;

            NoFiles.Text = TranslatedStrings.NoChanges;
            LoadingFiles.Text = TranslatedStrings.LoadingData;

            NoFiles.Font = new Font(NoFiles.Font, FontStyle.Italic);
            LoadingFiles.Font = new Font(LoadingFiles.Font, FontStyle.Italic);
            FilterWatermarkLabel.Font = new Font(FilterWatermarkLabel.Font, FontStyle.Italic);
            _NO_TRANSLATE_FilterComboBox.Font = new Font(_NO_TRANSLATE_FilterComboBox.Font, FontStyle.Bold);
            _NO_TRANSLATE_FilterComboBox.Items.Add("^(?!.*NotThisWord)");
            _NO_TRANSLATE_FilterComboBox.Items.Add(@"^(?!.*\bg?tests?/)");
            lblFindInCommitFilesGitGrepWatermark.Font = new Font(lblFindInCommitFilesGitGrepWatermark.Font, FontStyle.Italic);
            cboFindInCommitFilesGitGrep.Font = new Font(cboFindInCommitFilesGitGrep.Font, FontStyle.Bold);

            // Trigger initialisation of Search and Filter boxes
            NoFiles.Visible = true;
            CanUseFindInCommitFilesGitGrep = false;
            SetFindInCommitFilesGitGrepVisibilityImpl(visible: false);

            _diffCalculator = new FileStatusDiffCalculator(() => Module);
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _noItemStatuses =
            [
                new GitItemStatus(name: $"- {NoFiles.Text} -")
                {
                    IsStatusOnly = true,
                    ErrorMessage = string.Empty
                }
            ];

            base.Enter += FileStatusList_Enter;

            return;

            ToolStripMenuItem CreateOpenSubmoduleMenuItem()
            {
                ToolStripMenuItem item = new()
                {
                    Name = "openSubmoduleMenuItem",
                    Tag = "1",
                    Text = TranslatedStrings.OpenWithGitExtensions,
                    Image = Images.GitExtensionsLogo16
                };
                item.Click += (_, _) => this.InvokeAndForget(OpenSubmoduleAsync);
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
                    string? itemName = SelectedItemAbsolutePath;
                    if (itemName is not null)
                    {
                        VisualStudioIntegration.OpenFile(itemName);
                    }
                };
                return item;
            }
        }

        private static ImageListData CreateImageListData()
        {
            const int rowHeight = 18;
            const int imageWidth = rowHeight;

            ImageList list = new()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = DpiUtil.Scale(new Size(imageWidth, rowHeight)), // Scale ImageSize and images scale automatically
            };

            (string imageKey, Bitmap icon)[] images =
            [
                (nameof(Images.FolderClosed), Pad(Images.FolderClosed)),
                (nameof(Images.FileStatusUnknown), Pad(Images.FileStatusUnknown)),
                (nameof(Images.FileStatusModified), Pad(Images.FileStatusModified)),
                (nameof(Images.FileStatusModifiedOnlyA), Pad(Images.FileStatusModifiedOnlyA)),
                (nameof(Images.FileStatusModifiedOnlyB), Pad(Images.FileStatusModifiedOnlyB)),
                (nameof(Images.FileStatusModifiedSame), Pad(Images.FileStatusModifiedSame)),
                (nameof(Images.FileStatusModifiedUnequal), Pad(Images.FileStatusModifiedUnequal)),
                (nameof(Images.FileStatusAdded), Pad(Images.FileStatusAdded)),
                (nameof(Images.FileStatusAddedOnlyA), Pad(Images.FileStatusAddedOnlyA)),
                (nameof(Images.FileStatusAddedOnlyB), Pad(Images.FileStatusAddedOnlyB)),
                (nameof(Images.FileStatusAddedSame), Pad(Images.FileStatusAddedSame)),
                (nameof(Images.FileStatusAddedUnequal), Pad(Images.FileStatusAddedUnequal)),
                (nameof(Images.FileStatusRemoved), Pad(Images.FileStatusRemoved)),
                (nameof(Images.FileStatusRemovedOnlyA), Pad(Images.FileStatusRemovedOnlyA)),
                (nameof(Images.FileStatusRemovedOnlyB), Pad(Images.FileStatusRemovedOnlyB)),
                (nameof(Images.FileStatusRemovedSame), Pad(Images.FileStatusRemovedSame)),
                (nameof(Images.FileStatusRemovedUnequal), Pad(Images.FileStatusRemovedUnequal)),
                (nameof(Images.Unmerged), Pad(Images.Unmerged)),
                (nameof(Images.FileStatusRenamed), Pad(Images.FileStatusRenamed.AdaptLightness())),
                (nameof(Images.FileStatusRenamedOnlyA), Pad(Images.FileStatusRenamedOnlyA)),
                (nameof(Images.FileStatusRenamedOnlyB), Pad(Images.FileStatusRenamedOnlyB)),
                (nameof(Images.FileStatusRenamedSame), Pad(Images.FileStatusRenamedSame)),
                (nameof(Images.FileStatusRenamedUnequal), Pad(Images.FileStatusRenamedUnequal)),
                (nameof(Images.FileStatusCopied), Pad(Images.FileStatusCopied)),
                (nameof(Images.FileStatusCopiedOnlyA), Pad(Images.FileStatusCopiedOnlyA)),
                (nameof(Images.FileStatusCopiedOnlyB), Pad(Images.FileStatusCopiedOnlyB)),
                (nameof(Images.FileStatusCopiedSame), Pad(Images.FileStatusCopiedSame)),
                (nameof(Images.FileStatusCopiedUnequal), Pad(Images.FileStatusCopiedUnequal)),
                (nameof(Images.SubmodulesManage), Pad(Images.SubmodulesManage)),
                (nameof(Images.FolderSubmodule), Pad(Images.FolderSubmodule)),
                (nameof(Images.SubmoduleDirty), Pad(Images.SubmoduleDirty)),
                (nameof(Images.SubmoduleRevisionUp), Pad(Images.SubmoduleRevisionUp)),
                (nameof(Images.SubmoduleRevisionUpDirty), Pad(Images.SubmoduleRevisionUpDirty)),
                (nameof(Images.SubmoduleRevisionDown), Pad(Images.SubmoduleRevisionDown)),
                (nameof(Images.SubmoduleRevisionDownDirty), Pad(Images.SubmoduleRevisionDownDirty)),
                (nameof(Images.SubmoduleRevisionSemiUp), Pad(Images.SubmoduleRevisionSemiUp)),
                (nameof(Images.SubmoduleRevisionSemiUpDirty), Pad(Images.SubmoduleRevisionSemiUpDirty)),
                (nameof(Images.SubmoduleRevisionSemiDown), Pad(Images.SubmoduleRevisionSemiDown)),
                (nameof(Images.SubmoduleRevisionSemiDownDirty), Pad(Images.SubmoduleRevisionSemiDownDirty)),
                (nameof(Images.ViewFile), Pad(Images.ViewFile)),
                (nameof(Images.File), Pad(Images.File)),
                (nameof(Images.Diff), Pad(Images.Diff)),
                (nameof(Images.DiffA), Pad(Images.DiffA)),
                (nameof(Images.DiffB), Pad(Images.DiffB)),
                (nameof(Images.DiffC), Pad(Images.DiffC)),
                (nameof(Images.DiffR), Pad(Images.DiffR)),
            ];

            Dictionary<string, int> stateImageIndexDict = [];
            for (int i = 0; i < images.Length; i++)
            {
                list.Images.Add(images[i].icon);
                stateImageIndexDict.Add(images[i].imageKey, i);
            }

            return new ImageListData(list, stateImageIndexDict.ToFrozenDictionary());

            static Bitmap Pad(Bitmap input, int width = imageWidth, int height = rowHeight, int offsetX = 0, int offsetY = 1)
            {
                int deltaWidth = width - input.Width;
                int deltaHeight = height - input.Height;
                DebugHelpers.Assert(deltaWidth >= 0, "Can only increase image width");
                DebugHelpers.Assert(deltaHeight >= 0, "Can only increase image height");
                Bitmap padded = new(width, height, input.PixelFormat);
                using Graphics g = Graphics.FromImage(padded);
                g.DrawImageUnscaled(input, (deltaWidth / 2) + offsetX, (deltaHeight / 2) + offsetY);

                return padded;
            }
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();
            VisualStudioIntegration.Init();
        }

        // Wire up events to respond to Settings changes
        protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
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
                    commandSource.UICommands.PostSettings += UICommands_PostSettings;
                    UICommands_PostSettings(commandSource.UICommands, null);
                }
            }

            // Show/hide the search box if settings are changed
            void UICommands_PostSettings(object? sender, GitUIPostActionEventArgs? e)
            {
                if (CanUseFindInCommitFilesGitGrep && Visible)
                {
                    BeginInvoke(() => SetFindInCommitFilesGitGrepVisibility(AppSettings.ShowFindInCommitFilesGitGrep.Value));
                }
            }
        }

        public void Bind(Func<ObjectId?, string> describeRevision, Func<GitRevision, GitRevision> getActualRevision)
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
                .Do(sortType =>
                {
                    _groupBy = sortType switch
                    {
                        DiffListSortType.FilePath or DiffListSortType.FilePathFlat
                            => null,
                        DiffListSortType.FileExtension or DiffListSortType.FileExtensionFlat
                            => new GroupBy(status => Path.GetExtension(status.Name), GetImageKey: _ => nameof(Images.File), GetLabel: k => k),
                        DiffListSortType.FileStatus or DiffListSortType.FileStatusFlat
                            => new GroupBy(GetStatusKey, GetImageKey: GetItemImageKey, GetLabel: _ => ""),
                        _ => throw new NotSupportedException($"{sortType} is not a supported sorting method.")
                    };

                    _flatList = sortType.ToString().EndsWith("Flat");

                    UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true);
                })
                .Catch<DiffListSortType, Exception>(ex =>
                {
                    Trace.WriteLine(ex);
                    return Observable.Empty<DiffListSortType>();
                })
                .Subscribe();

            return;

            static string GetStatusKey(GitItemStatus status)
            {
                char inverseDiffStatus = (char)((int)'Z' - (int)status.DiffStatus);
                int imageIndex = _imageListData.StateImageIndexMap[GetItemImageKey(status)];
                DebugHelpers.Assert(inverseDiffStatus >= 0, $"offset of {nameof(inverseDiffStatus)} needs to be adapted");
                DebugHelpers.Assert(imageIndex < 100, $"width of {nameof(imageIndex)} needs to be adapted");
                return $"{inverseDiffStatus}{imageIndex:D02}";
            }
        }

        // Properties

        [Browsable(false)]
        public IEnumerable<FileStatusItem> AllItems => FileStatusListView.ItemTags<FileStatusItem>();

        public int AllItemsCount => AllItems.Count();

        public override ContextMenuStrip? ContextMenuStrip
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

        public bool FilterFilesByNameRegexFocused => _NO_TRANSLATE_FilterComboBox.Focused;
        public bool FindInCommitFilesGitGrepFocused => cboFindInCommitFilesGitGrep.Focused;
        public bool FindInCommitFilesGitGrepVisible => cboFindInCommitFilesGitGrep.Visible;

        /// <summary>
        ///  Indicates whether the git-grep search functionality is enabled for this control.
        /// </summary>
        [DefaultValue(false)]
        public bool CanUseFindInCommitFilesGitGrep { get; set; }

        public void SetFindInCommitFilesGitGrepVisibility(bool visible)
        {
            if (!CanUseFindInCommitFilesGitGrep || cboFindInCommitFilesGitGrep.Visible == visible)
            {
                return;
            }

            SetFindInCommitFilesGitGrepVisibilityImpl(visible);
        }

        private void SetFindInCommitFilesGitGrepVisibilityImpl(bool visible)
        {
            _formFindInCommitFilesGitGrep?.SetShowFindInCommitFilesGitGrep(visible);

            cboFindInCommitFilesGitGrep.Visible = visible;
            if (visible)
            {
                if (ActiveControl is not null)
                {
                    ActiveControl = cboFindInCommitFilesGitGrep;
                }

                // Adjust sizes "automatically" changed by visibility
                cboFindInCommitFilesGitGrep.Top = 0;
            }
            else if (_formFindInCommitFilesGitGrep?.Visible is not true && cboFindInCommitFilesGitGrep.Text.Length > 0)
            {
                cboFindInCommitFilesGitGrep.Text = "";
                FindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Text, delay: 0);
            }

            // Adjust locations
            // Note that 'LoadingFiles' location depends on visibility of Filter box, must be set each time made visible
            int top = !visible ? 0 : cboFindInCommitFilesGitGrep.Bottom + cboFindInCommitFilesGitGrep.Margin.Bottom;
            _NO_TRANSLATE_FilterComboBox.Top = top;
            _NO_TRANSLATE_FilterComboBox.Width = FileStatusListView.Width;
            FilterWatermarkLabel.Top = _NO_TRANSLATE_FilterComboBox.Top;
            DeleteFilterButton.Top = _NO_TRANSLATE_FilterComboBox.Top;

            SetFindInCommitFilesGitGrepWatermarkVisibility();
            SetFileStatusListVisibility(filesPresent: !NoFiles.Visible);
        }

        private void SetFileStatusListVisibility(bool filesPresent)
        {
            LoadingFiles.Visible = false;

            // Use variable to prevent bad value retrieved from `Visible` property
            bool filesToFilter = filesPresent || (cboFindInCommitFilesGitGrep.Visible && !string.IsNullOrEmpty(cboFindInCommitFilesGitGrep.Text));
            _NO_TRANSLATE_FilterComboBox.Visible = filesToFilter;
            NoFiles.Visible = !filesToFilter;
            if (!filesToFilter)
            {
                // Workaround for startup issue if set in EnableSearchForList()
                NoFiles.Top = _NO_TRANSLATE_FilterComboBox.Top;
                NoFiles.BringToFront();
            }

            SetDeleteFilterButtonVisibility();
            SetFilterWatermarkLabelVisibility();
            SetDeleteSearchButtonVisibility();
            SetFindInCommitFilesGitGrepWatermarkVisibility();

            int top = GetFileStatusListTop();
            int height = ClientRectangle.Height - top - FileStatusListView.Margin.Top - FileStatusListView.Margin.Bottom;
            FileStatusListView.SetBounds(0, top, 0, height, BoundsSpecified.Y | BoundsSpecified.Height);
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

        private IReadOnlyList<FileStatusWithDescription> GitItemStatusesWithDescription { get; set; } = Array.Empty<FileStatusWithDescription>();

        public bool GroupByRevision { get; set; } = false;

        [Browsable(false)]
        [DefaultValue(true)]
        public bool IsEmpty => GitItemStatuses is null || !GitItemStatuses.Any();

        [Browsable(false)]
        [DefaultValue(true)]
        public bool HasSelection => FileStatusListView.SelectedNodes.Count > 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitItemStatus? SelectedGitItem
        {
            get => SelectedItem?.Item;
            set
            {
                TreeNode? itemToBeSelected = GetItemByStatus(value);
                FileStatusListView.SelectedNode = itemToBeSelected;
                return;

                TreeNode? GetItemByStatus(GitItemStatus? status)
                {
                    if (status is null)
                    {
                        return null;
                    }

                    TreeNode? newSelected = null;
                    foreach (TreeNode node in FileStatusListView.Items())
                    {
                        if (node.Tag is not FileStatusItem gitItemStatus)
                        {
                            continue;
                        }

                        if (gitItemStatus.Item == status)
                        {
                            return node;
                        }

                        if (status.CompareName(gitItemStatus.Item) == 0 && newSelected is null)
                        {
                            newSelected = node;
                        }
                    }

                    return newSelected;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<GitItemStatus> SelectedGitItems
        {
            set
            {
                if (value is null)
                {
                    ClearSelected();
                    return;
                }

                SelectItems(node => node.Tag is FileStatusItem fileStatusItem && value.Contains(fileStatusItem.Item));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public FileStatusItem? SelectedItem => FileStatusListView.LastSelectedNode?.Tag as FileStatusItem;

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

                SelectItems(node => node.Tag is FileStatusItem fileStatusItem && value.Contains(fileStatusItem));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<FileStatusItem> FirstGroupItems
            => FileStatusListView.Nodes.Count == 0
                ? []
                : _showDiffGroups
                    ? FileStatusListView.Nodes[0].ItemTags<FileStatusItem>()
                    : FileStatusListView.ItemTags<FileStatusItem>();

        [DefaultValue(true)]
        public bool SelectFirstItemOnSetItems { get; set; }

        public int UnfilteredItemsCount => GitItemStatusesWithDescription?.Sum(tuple => tuple.Statuses.Count) ?? 0;

        public bool IsFilterActive => !string.IsNullOrEmpty(_NO_TRANSLATE_FilterComboBox.Text);

        // Public methods

        public void ClearSelected()
        {
            FileStatusListView.SelectedNodes = [];
        }

        public new void Focus()
        {
            if (FileStatusListView.Nodes.Count > 0)
            {
                if (SelectedItem is null)
                {
                    SelectFirstVisibleItem();
                }

                FileStatusListView.Focus();
            }
        }

        private static (string? prefix, string text, string? suffix) FormatListViewItem(TreeNode node, PathFormatter formatter, int itemWidth)
        {
            if (node.Tag is not FileStatusItem fileStatusItem || fileStatusItem.Item.IsRangeDiff)
            {
                return (prefix: null, text: node.Text, suffix: null);
            }

            GitItemStatus gitItemStatus = fileStatusItem.Item;

            string name = gitItemStatus.Name;
            string? parentPath = (node.Parent?.Tag as RelativePath)?.Value;
            if (!string.IsNullOrEmpty(parentPath) && name.StartsWith(parentPath))
            {
                name = name[(parentPath.Length + 1)..];
            }

            (string? prefix, string? text, string? suffix) = formatter.FormatTextForDrawing(itemWidth, name, gitItemStatus.OldName);
            text = AppendItemSubmoduleStatus(text ?? "", gitItemStatus);

            return (prefix, text, suffix);
        }

        public FileStatusItem? SelectNextItem(bool backwards, bool loop, bool notify = true)
        {
            TreeNode? currentItem = FileStatusListView.FocusedNode;

            if (currentItem is null)
            {
                return null;
            }

            if (backwards)
            {
                SetSelectedItem(FindPrevItem(currentItem) ?? (loop ? GetLastItem() ?? currentItem : currentItem), notify);
            }
            else
            {
                SetSelectedItem(FindNextItem(currentItem) ?? (loop ? GetFirstItem() ?? currentItem : currentItem), notify);
            }

            return SelectedItem;

            TreeNode? FindPrevItem(TreeNode currentItem)
            {
                TreeNode? prevItem = null;
                foreach (TreeNode item in FileStatusListView.Items())
                {
                    if (item == currentItem)
                    {
                        return prevItem;
                    }

                    if (IsSearchableItem(item))
                    {
                        prevItem = item;
                    }
                }

                throw new ArgumentException(@$"{nameof(currentItem)} ""{currentItem}"" is no tree item of {nameof(FileStatusListView)} tree!");
            }

            TreeNode? FindNextItem(TreeNode currentItem)
            {
                bool currentItemFound = false;
                foreach (TreeNode item in FileStatusListView.Items())
                {
                    if (item == currentItem)
                    {
                        currentItemFound = true;
                        continue;
                    }

                    if (currentItemFound && IsSearchableItem(item))
                    {
                        return item;
                    }
                }

                return null;
            }

            TreeNode? GetFirstItem() => FileStatusListView.Items().FirstOrDefault(IsSearchableItem);

            TreeNode? GetLastItem() => FileStatusListView.Items().LastOrDefault(IsSearchableItem);

            static bool IsSearchableItem(TreeNode item)
            {
                return item.Tag is FileStatusItem fileStatusItem
                    && !fileStatusItem.Item.IsStatusOnly
                    && !fileStatusItem.Item.IsRangeDiff;
            }
        }

        public void SelectAll() => SelectItems(_ => true);

        public void SelectFirstVisibleItem()
        {
            FileStatusListView.SelectedNode = FileStatusListView.Items()
                .FirstOrDefault(node => IsLeaf(node) && !IsCollapsed(node) && node.Tag is FileStatusItem fileStatusItem && fileStatusItem.Item != _noItemStatuses[0]);

            return;

            static bool IsLeaf(TreeNode node) => node.Nodes.Count == 0;

            static bool IsCollapsed(TreeNode node)
            {
                while (true)
                {
                    if (node.Parent is null)
                    {
                        return false;
                    }

                    node = node.Parent;

                    if (!node.IsExpanded)
                    {
                        return true;
                    }
                }
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

        public void SetDiffs(IReadOnlyList<GitRevision> revisions)
        {
            CancellationToken cancellationToken = _reloadSequence.Next();
            FileStatusListLoading();
            _enableDisablingShowDiffForAllParents = true;
            _diffCalculator.SetDiff(revisions, headId: null, allowMultiDiff: false);
            UpdateFileStatusListView(_diffCalculator.Calculate(prevList: [], refreshDiff: true, refreshGrep: false, cancellationToken));
        }

        public async Task SetDiffsAsync(IReadOnlyList<GitRevision> revisions, ObjectId? headId, CancellationToken cancellationToken)
        {
            _enableDisablingShowDiffForAllParents = true;
            await this.SwitchToMainThreadAsync(cancellationToken);
            FileStatusListLoading();

            await TaskScheduler.Default;
            cancellationToken.ThrowIfCancellationRequested();
            _diffCalculator.SetDiff(revisions, headId, allowMultiDiff: true);
            IReadOnlyList<FileStatusWithDescription> gitItemStatusesWithDescription = _diffCalculator.Calculate(prevList: [], refreshDiff: true, refreshGrep: false, cancellationToken);

            await this.SwitchToMainThreadAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            UpdateFileStatusListView(gitItemStatusesWithDescription);

            // git grep, fetched as a separate step
            if (string.IsNullOrEmpty(cboFindInCommitFilesGitGrep.Text))
            {
                return;
            }

            await TaskScheduler.Default;
            cancellationToken.ThrowIfCancellationRequested();
            gitItemStatusesWithDescription = _diffCalculator.Calculate(prevList: GitItemStatusesWithDescription, refreshDiff: false, refreshGrep: true, cancellationToken);

            await this.SwitchToMainThreadAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            UpdateFileStatusListView(gitItemStatusesWithDescription);
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
            UpdateFileStatusListView(new List<FileStatusWithDescription>
            {
                new(
                    firstRev: indexRev,
                    secondRev: workTreeRev,
                    summary: workTreeDesc,
                    statuses: workTreeItems),
                new(
                    firstRev: headRev,
                    secondRev: indexRev,
                    summary: indexDesc,
                    statuses: indexItems)
            });
        }

        public void SetDiffs(GitRevision? firstRev, GitRevision secondRev, IReadOnlyList<GitItemStatus> items)
        {
            FileStatusListLoading();
            UpdateFileStatusListView(new List<FileStatusWithDescription>
            {
                new(
                    firstRev: firstRev,
                    secondRev: secondRev,
                    summary: TranslatedStrings.DiffWithParent + GetDescriptionForRevision(firstRev?.ObjectId),
                    statuses: items)
            });
        }

        public void ClearDiffs()
        {
            UpdateFileStatusListView([]);
        }

        private string? GetDescriptionForRevision(ObjectId? objectId) =>
            DescribeRevision is not null ? DescribeRevision(objectId) : objectId?.ToShortString();

        public void SetNoFilesText(string text)
        {
            NoFiles.Text = text;
        }

        private void SetSelectedItem(TreeNode node, bool notify)
        {
            _enableSelectedIndexChangeEvent = notify;
            try
            {
                FileStatusListView.SelectedNode = node;
            }
            finally
            {
                _enableSelectedIndexChangeEvent = true;
            }
        }

        public int SetSelectionFilter(string selectionFilter)
        {
            SelectItems(item => string.IsNullOrEmpty(selectionFilter) || Regex.IsMatch(item.Name, selectionFilter, RegexOptions.IgnoreCase));
            return FileStatusListView.SelectedNodes.Count;
        }

        public void StoreNextItemToSelect()
        {
            TreeNode? lastSelectedNode = FileStatusListView.LastSelectedNode;
            if (lastSelectedNode is not null)
            {
                bool found = false;
                foreach (TreeNode node in FileStatusListView.Items())
                {
                    if (node == lastSelectedNode)
                    {
                        found = true;
                        continue;
                    }

                    if (found && node.Tag is FileStatusItem fileStatusItem)
                    {
                        _nextItemToSelect = fileStatusItem.Item;
                        return;
                    }
                }
            }

            _nextItemToSelect = FileStatusListView.Items()
                .Select(node => (node.Tag as FileStatusItem)?.Item)
                .FirstOrDefault(item => item is not null);
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
                && item.GetSubmoduleStatusAsync() is Task<GitSubmoduleStatus> { IsCompleted: true } task
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

            string submoduleName = SelectedItem.Item.Name;

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

        private void SelectItems(Func<TreeNode, bool> predicate)
        {
            try
            {
                FileStatusListView.BeginUpdate();

                HashSet<TreeNode> selectedNodes = [];
                TreeNode? firstSelectedItem = null;
                foreach (TreeNode item in FileStatusListView.Items().Where(item => predicate(item)))
                {
                    firstSelectedItem ??= item;
                    selectedNodes.Add(item);
                }

                FileStatusListView.SelectedNodes = selectedNodes;
                if (firstSelectedItem is not null)
                {
                    FileStatusListView.FocusedNode = firstSelectedItem;
                }
            }
            finally
            {
                FileStatusListView.EndUpdate();
            }

            StoreNextItemToSelect();
        }

        private void SetDeleteFilterButtonVisibility()
        {
            DeleteFilterButton.Visible = _NO_TRANSLATE_FilterComboBox.Visible && !string.IsNullOrEmpty(_NO_TRANSLATE_FilterComboBox.Text);
            if (DeleteFilterButton.Visible)
            {
                DeleteFilterButton.BringToFront();
            }
        }

        private void SetFilterWatermarkLabelVisibility()
        {
            FilterWatermarkLabel.Visible = _NO_TRANSLATE_FilterComboBox.Visible && !_NO_TRANSLATE_FilterComboBox.Focused && string.IsNullOrEmpty(_NO_TRANSLATE_FilterComboBox.Text);
            if (FilterWatermarkLabel.Visible)
            {
                FilterWatermarkLabel.BringToFront();
            }
        }

        private void SetFindInCommitFilesGitGrepWatermarkVisibility()
        {
            lblFindInCommitFilesGitGrepWatermark.Visible = cboFindInCommitFilesGitGrep.Visible && !cboFindInCommitFilesGitGrep.Focused && string.IsNullOrEmpty(cboFindInCommitFilesGitGrep.Text);
            if (lblFindInCommitFilesGitGrepWatermark.Visible)
            {
                lblFindInCommitFilesGitGrepWatermark.BringToFront();
            }
        }

        private void SetDeleteSearchButtonVisibility()
        {
            DeleteSearchButton.Visible = cboFindInCommitFilesGitGrep.Visible && !string.IsNullOrEmpty(cboFindInCommitFilesGitGrep.Text);
            if (DeleteSearchButton.Visible)
            {
                DeleteSearchButton.BringToFront();
            }
        }

        private void FileStatusListLoading()
        {
            // Show "Files loading" below the filterbox
            NoFiles.Visible = false;
            int top = GetFileStatusListTop();
            LoadingFiles.Top = top;
            LoadingFiles.Visible = true;
            LoadingFiles.BringToFront();

            FileStatusListView.BeginUpdate();
            ClearSelected();
            FileStatusListView.Nodes.Clear();
            FileStatusListView.EndUpdate();
        }

        private int GetFileStatusListTop()
            => _NO_TRANSLATE_FilterComboBox.Visible ? _NO_TRANSLATE_FilterComboBox.Bottom + _NO_TRANSLATE_FilterComboBox.Margin.Top + _NO_TRANSLATE_FilterComboBox.Margin.Bottom
                : cboFindInCommitFilesGitGrep.Visible ? cboFindInCommitFilesGitGrep.Bottom + cboFindInCommitFilesGitGrep.Margin.Top + cboFindInCommitFilesGitGrep.Margin.Bottom
                : 0;

        private void UpdateFileStatusListView(IReadOnlyList<FileStatusWithDescription> items, bool updateCausedByFilter = false)
        {
            GitItemStatusesWithDescription = items ?? throw new ArgumentNullException(nameof(items));
            bool hasGrepGroup = GitItemStatusesWithDescription.Any(FileStatusDiffCalculator.IsGrepItemStatuses);
            bool filesPresent = GitItemStatusesWithDescription.Any(x => x.Statuses.Count > 0 && !FileStatusDiffCalculator.IsGrepItemStatuses(x));
            bool showGroupLabel = (filesPresent && (GitItemStatusesWithDescription.Count > 1 || GroupByRevision)) || !string.IsNullOrEmpty(cboFindInCommitFilesGitGrep.Text);
            bool hasChangesOrMultipleGroups = filesPresent || GitItemStatusesWithDescription.Count > 1 || GroupByRevision;
            if (filesPresent)
            {
                EnsureSelectedIndexChangeSubscription();
            }

            HashSet<GitItemStatus>? previouslySelectedItems = null;
            HashSet<TreeNode> toBeSelectedItems = [];

            if (updateCausedByFilter)
            {
                previouslySelectedItems = FileStatusListView.SelectedItemTags<FileStatusItem>()
                    .Select(i => i.Item)
                    .ToHashSet();

                DataSourceChanged?.Invoke(this, EventArgs.Empty);
            }

            FileStatusListView.BeginUpdate();
            SetFileStatusListVisibility(filesPresent);
            _showDiffGroups = GitItemStatusesWithDescription.Count > 1 || (GroupByRevision && !(GitItemStatusesWithDescription.Count == 1 && GitItemStatusesWithDescription[0].Statuses.Count == 0));
            FileStatusListView.Nodes.Clear();

            foreach (FileStatusWithDescription i in GitItemStatusesWithDescription)
            {
                int shownCount = 0;

                // Collapse some groups for diffs with common BASE
                // Always expand grep results
                bool expandDiffGroup = hasGrepGroup
                    ? FileStatusDiffCalculator.IsGrepItemStatuses(i)
                    : ((i.Statuses.Count <= 7 && i.IconName == nameof(Images.Diff)) || GitItemStatusesWithDescription.Count < 3 || i == GitItemStatusesWithDescription[0]);

                TreeNode diffGroup;
                {
                    IEnumerable<GitItemStatus> itemStatuses;
                    if (showGroupLabel && i.Statuses.Count == 0)
                    {
                        itemStatuses = _noItemStatuses;
                        expandDiffGroup = false;
                    }
                    else
                    {
                        itemStatuses = i.Statuses.Where(IsFilterMatch);
                    }

                    if (_groupBy is null)
                    {
                        diffGroup = _sorter.CreateTreeSortedByPath(itemStatuses, _flatList, CreateNode);
                    }
                    else
                    {
                        diffGroup = new TreeNode();
                        IOrderedEnumerable<IGrouping<string, GitItemStatus>> grouped = itemStatuses.GroupBy(_groupBy.GetGroupKey).OrderBy(group => group.Key);
                        foreach (IGrouping<string, GitItemStatus> group in grouped)
                        {
                            TreeNode groupNode = _sorter.CreateTreeSortedByPath(group, _flatList, CreateNode);
                            if (groupNode.Nodes.Count == 1 && groupNode.Nodes[0].Nodes.Count == 0)
                            {
                                groupNode = groupNode.Nodes[0];
                            }
                            else
                            {
                                groupNode.Text = _groupBy.GetLabel(group.Key);
                                groupNode.ImageIndex = _imageListData.StateImageIndexMap[_groupBy.GetImageKey(group.First())];
                                groupNode.SelectedImageIndex = groupNode.ImageIndex;
                            }

                            diffGroup.Nodes.Add(groupNode);
                        }

                        if (diffGroup.Nodes.Count == 1 && diffGroup.Nodes[0].Nodes.Count > 0)
                        {
                            diffGroup = diffGroup.Nodes[0];
                        }
                    }

                    diffGroup.ImageIndex = _imageListData.StateImageIndexMap[i.IconName];
                    diffGroup.SelectedImageIndex = diffGroup.ImageIndex;
                }

                TreeNode CreateNode(GitItemStatus item)
                {
                    ++shownCount;

                    // Also set .Text in order to provide accessibility information (needed for partial standard drawing by TreeView control, too)
                    string oldName = item.OldName is null ? "" : $" ({item.OldName})";
                    TreeNode listItem = new(text: $"{item.Name}{oldName}");

                    listItem.ImageIndex = GetItemImageIndex(item);
                    listItem.SelectedImageIndex = listItem.ImageIndex;

                    if (item.IsSubmodule
                        && item.GetSubmoduleStatusAsync() is Task<GitSubmoduleStatus> task)
                    {
                        GitItemStatus capturedItem = item;

                        ThreadHelper.FileAndForget(async () =>
                        {
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                            await task;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

                            await this.SwitchToMainThreadAsync();

                            listItem.ImageIndex = GetItemImageIndex(capturedItem);
                            listItem.SelectedImageIndex = listItem.ImageIndex;
                        });
                    }

                    if (previouslySelectedItems?.Contains(item) is true)
                    {
                        toBeSelectedItems.Add(listItem);
                    }

                    listItem.Tag = new FileStatusItem(i.FirstRev, i.SecondRev, item, i.BaseA, i.BaseB);

                    return listItem;
                }

                if (_showDiffGroups)
                {
                    if (diffGroup.Nodes.Count == 1 && diffGroup.Nodes[0].Tag is FileStatusItem fileStatusItem && fileStatusItem.Item.IsRangeDiff)
                    {
                        diffGroup = diffGroup.Nodes[0];
                    }
                    else
                    {
                        diffGroup.Tag = i.FirstRev;
                        diffGroup.Text = GetGroupName(i, shownCount);
                    }

                    FileStatusListView.Nodes.Add(diffGroup);
                    diffGroup.ExpandAll();
                    if (!expandDiffGroup)
                    {
                        diffGroup.Collapse(ignoreChildren: true);
                    }
                }
                else
                {
                    foreach (TreeNode node in diffGroup.Nodes)
                    {
                        FileStatusListView.Nodes.Add(node);
                        node.ExpandAll();
                    }
                }
            }

            if (FileStatusListView.Nodes.Count > 0)
            {
                FileStatusListView.TopNode = FileStatusListView.Nodes[0];
            }

            if (toBeSelectedItems.Count > 0)
            {
                SelectItems(node => toBeSelectedItems.Contains(node));
            }

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
            return;

            void EnsureSelectedIndexChangeSubscription()
            {
                _selectedIndexChangeSubscription ??= Observable.FromEventPattern(
                        h => FileStatusListView.FocusedNodeChanged += h,
                        h => FileStatusListView.FocusedNodeChanged -= h)
                    .Where(x => _enableSelectedIndexChangeEvent)
                    .Throttle(SelectedIndexChangeThrottleDuration, MainThreadScheduler.Instance)
                    .ObserveOn(MainThreadScheduler.Instance)
                    .Subscribe(_ => FileStatusListView_SelectedIndexChanged());
            }

            static string GetGroupName(FileStatusWithDescription i, int shownCount)
            {
                // Show shown and total number of files only if different; avoid showing "1/0" for "- No changes -"
                string shownDisplay = shownCount >= i.Statuses.Count ? "" : $"{shownCount}/";
                return $"({shownDisplay}{i.Statuses.Count}) {i.Summary}";
            }

            int GetItemImageIndex(GitItemStatus gitItemStatus)
            {
                string imageKey = gitItemStatus.IsStatusOnly || !string.IsNullOrWhiteSpace(gitItemStatus.ErrorMessage)
                    ? gitItemStatus == _noItemStatuses[0] ? nameof(Images.FileStatusCopiedSame) : nameof(Images.FileStatusUnknown)
                    : GetItemImageKey(gitItemStatus);
                return _imageListData.StateImageIndexMap.TryGetValue(imageKey, out int value)
                    ? value
                    : _imageListData.StateImageIndexMap[nameof(Images.FileStatusUnknown)];
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

            if (!string.IsNullOrWhiteSpace(gitItemStatus.GrepString))
            {
                return nameof(Images.ViewFile);
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

            if (gitItemStatus.IsUnmerged)
            {
                return nameof(Images.Unmerged);
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

        public void SelectPreviousVisibleItem()
        {
            if (SelectNextItem(backwards: true, loop: true) is null)
            {
                SelectFirstVisibleItem();
            }
        }

        public void SelectNextVisibleItem()
        {
            if (SelectNextItem(backwards: false, loop: true) is null)
            {
                SelectFirstVisibleItem();
            }
        }

        // Event handlers

        private void FileStatusListView_ContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            if (sender is null || (SelectedItem?.Item.IsStatusOnly ?? false))
            {
                e.Cancel = true;
                return;
            }

            ContextMenuStrip cm = (ContextMenuStrip)sender;

            InsertTreeContextMenuItems(cm.Items, index: 0);
            UpdateStatusOfTreeContextMenuItems();

            // TODO The handling of _NO_TRANSLATE_openSubmoduleMenuItem need to be revised
            // This code handles the 'bold' in the menu for submodules. Other default actions are not set to bold.
            // The actual implementation of the default handling with doubleclick is in each form,
            // separate from this menu item

            if (!cm.Items.Find(_NO_TRANSLATE_openSubmoduleMenuItem.Name!, true).Any())
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

            if (!cm.Items.Find(_NO_TRANSLATE_openInVisualStudioMenuItem.Name!, true).Any())
            {
                cm.Items.Add(_openInVisualStudioSeparator);
                cm.Items.Add(_NO_TRANSLATE_openInVisualStudioMenuItem);
            }

            bool canOpenInVisualStudio = File.Exists(SelectedItemAbsolutePath) && VisualStudioIntegration.IsVisualStudioInstalled;
            _NO_TRANSLATE_openInVisualStudioMenuItem.Enabled = canOpenInVisualStudio;
            _NO_TRANSLATE_openInVisualStudioMenuItem.Visible = canOpenInVisualStudio;
            _openInVisualStudioSeparator.Visible = canOpenInVisualStudio;

            if (!cm.Items.Find(_sortByContextMenu.Name!, true).Any())
            {
                cm.Items.Add(_sortBySeparator);
                cm.Items.Add(_sortByContextMenu);
            }

            // Show 'Show file differences for all parents' menu item if it is possible that there are multiple first revisions
            bool mayBeMultipleRevs = _enableDisablingShowDiffForAllParents && GitItemStatusesWithDescription.Count > 1;

            ToolStripItem[] diffItem = cm.Items.Find(_showDiffForAllParentsItemName, true);
            if (diffItem.Length == 0)
            {
                cm.Items.Add(_showDiffForAllParentsSeparator);
                _showDiffForAllParentsSeparator.Visible = mayBeMultipleRevs;

                ToolStripMenuItem showAllDifferencesItem = new(TranslatedStrings.ShowDiffForAllParentsText)
                {
                    Checked = AppSettings.ShowDiffForAllParents,
                    ToolTipText = TranslatedStrings.ShowDiffForAllParentsTooltip,
                    Name = _showDiffForAllParentsItemName,
                    CheckOnClick = true,
                    Visible = mayBeMultipleRevs
                };
                showAllDifferencesItem.CheckedChanged += (s, e) =>
                {
                    AppSettings.ShowDiffForAllParents = showAllDifferencesItem.Checked;
                    CancellationToken cancellationToken = _reloadSequence.Next();
                    FileStatusListLoading();
                    ThreadHelper.FileAndForget(async () =>
                    {
                        IReadOnlyList<FileStatusWithDescription> gitItemStatusesWithDescription = _diffCalculator.Calculate(prevList: GitItemStatusesWithDescription, refreshDiff: true, refreshGrep: false, cancellationToken);

                        await this.SwitchToMainThreadAsync(cancellationToken);
                        UpdateFileStatusListView(gitItemStatusesWithDescription);
                    });
                };
                cm.Items.Add(showAllDifferencesItem);
            }
            else
            {
                diffItem[0].Visible = mayBeMultipleRevs;

                ToolStripItem[] sepItem = cm.Items.Find(_showDiffForAllParentsSeparator.Name!, true);
                if (sepItem.Length > 0)
                {
                    sepItem[0].Visible = mayBeMultipleRevs;
                }
            }
        }

        public void ShowFindInCommitFileGitGrepDialog(string text)
        {
            if (!CanUseFindInCommitFilesGitGrep)
            {
                return;
            }

            if (_formFindInCommitFilesGitGrep?.IsDisposed is true)
            {
                _formFindInCommitFilesGitGrep = null;
            }

            Validates.NotNull(TopLevelControl);
            _formFindInCommitFilesGitGrep ??= new FormFindInCommitFilesGitGrep(UICommands)
            {
                FilesGitGrepLocator = (text, delay) =>
                {
                    FindInCommitFilesGitGrep(text, delay);
                    cboFindInCommitFilesGitGrep.Text = text;
                },

                FindInCommitFilesGitGrepToggle = SetFindInCommitFilesGitGrepVisibility,

                Owner = (Form)TopLevelControl,

                // offset a few pixels compared to FindAndReplaceForm
                Location = new Point(TopLevelControl.Location.X + 90, TopLevelControl.Location.Y + 110)
            };

            _formFindInCommitFilesGitGrep.GitGrepExpressionText = !string.IsNullOrEmpty(text) ? text : (cboFindInCommitFilesGitGrep.Visible && !string.IsNullOrWhiteSpace(cboFindInCommitFilesGitGrep.Text) ? cboFindInCommitFilesGitGrep.Text : null);
            _formFindInCommitFilesGitGrep.SetSearchItems(cboFindInCommitFilesGitGrep.Items);
            _formFindInCommitFilesGitGrep.SetShowFindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Visible);
            _formFindInCommitFilesGitGrep.Show();
            _formFindInCommitFilesGitGrep.Focus();
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
                    UICommands.StartFileHistoryDialog(this, SelectedItem.Item.Name, SelectedItem.SecondRevision);
                }
            }
            else
            {
                DoubleClick?.Invoke(sender, e);
            }
        }

        private void FileStatusListView_DrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            Validates.NotNull(sender);
            TreeNode? item = e.Node;
            Validates.NotNull(item);
            MultiSelectTreeView treeView = (MultiSelectTreeView)sender;
            bool selected = treeView.SelectedNodes.Contains(item);

            PathFormatter formatter = new(e.Graphics, FileStatusListView.Font);

            (string? prefix, string text, string? suffix) = FormatListViewItem(item, formatter, item.Bounds.Width);

            if (selected)
            {
                e.Graphics.FillRectangle(Focused ? SystemBrushes.Highlight : OtherColors.InactiveSelectionHighlightBrush, e.Bounds);
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Rectangle textRect = new(item.Bounds.X - 1, item.Bounds.Top - 1, item.Bounds.Width, item.Bounds.Height);

            Color grayTextColor = selected && Focused
                ? ColorHelper.GetHighlightGrayTextColor(
                    backgroundColorName: KnownColor.Window,
                    textColorName: KnownColor.WindowText,
                    highlightColorName: KnownColor.Highlight)
                : SystemColors.GrayText;

            Color textColor = selected && Focused
                ? SystemColors.HighlightText
                : SystemColors.WindowText;

            if (!string.IsNullOrEmpty(prefix))
            {
                DrawString(textRect, prefix, grayTextColor);
                Size prefixSize = formatter.MeasureString(prefix);
                textRect.Offset(prefixSize.Width, 0);
            }

            DrawString(textRect, text, textColor);

            if (!string.IsNullOrEmpty(suffix))
            {
                Size textSize = formatter.MeasureString(text);
                textRect.Offset(textSize.Width, 0);
                DrawString(textRect, suffix, grayTextColor);
            }

            return;

            void DrawString(Rectangle rect, string s, Color color)
            {
                rect.Intersect(Rectangle.Round(e.Graphics.ClipBounds));
                if (rect.Width != 0 && rect.Height != 0)
                {
                    formatter.DrawString(s, rect, color);
                }
            }
        }

        private void FileStatusListView_KeyDown(object? sender, KeyEventArgs e)
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

        private void FileStatusListView_MouseDown(object? sender, MouseEventArgs e)
        {
            // SELECT
            if (e.Button == MouseButtons.Right)
            {
                TreeViewHitTestInfo hover = FileStatusListView.HitTest(e.Location);

                if (hover.Node is not null && !FileStatusListView.SelectedNodes.Contains(hover.Node))
                {
                    FileStatusListView.SelectedNode = hover.Node;
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

        private void FileStatusListView_MouseUp(object? sender, MouseEventArgs e)
        {
            // Release the drag capture
            if (e.Button == MouseButtons.Left)
            {
                _dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        private void FileStatusListView_MouseMove(object? sender, MouseEventArgs e)
        {
            // DRAG
            // If the mouse moves outside the rectangle, start the drag.
            if (_dragBoxFromMouseDown != Rectangle.Empty &&
                !_dragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                if (SelectedItems.Any())
                {
                    StringCollection fileList = [];

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
        }

        private void FileStatusListView_SelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private void FileStatusList_Enter(object? sender, EventArgs e)
        {
            Enter?.Invoke(this, new EnterEventArgs(_mouseEntered));
            _mouseEntered = false;
        }

        #region Filtering

        private string _toolTipText = "";
        private readonly Subject<string> _filterSubject = new();
        private Regex? _filter;

        public void SetFilter(string value)
        {
            _NO_TRANSLATE_FilterComboBox.Text = value;
            FilterFiles(value);
        }

        private void DeleteFilterButton_Click(object? sender, EventArgs e)
        {
            SetFilter(string.Empty);
        }

        private int FilterFiles(string value)
        {
            StoreFilter(value);

            // Feed back the current list of files
            UpdateFileStatusListView(GitItemStatusesWithDescription, updateCausedByFilter: true);
            FilterChanged?.Invoke(this, EventArgs.Empty);
            return AllItemsCount;
        }

        private bool IsFilterMatch(GitItemStatus item)
        {
            if (_filter is null || item.IsRangeDiff)
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
            SynchronizationContext? synchronizationContext = SynchronizationContext.Current;
            Validates.NotNull(synchronizationContext);
            _filterSubject
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(synchronizationContext)
                .Subscribe(
                    filterText =>
                    {
                        _toolTipText = "";
                        int fileCount = 0;
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
                if (_NO_TRANSLATE_FilterComboBox.Items.Cast<string>().Any(candidate => candidate == filter))
                {
                    return;
                }

                const int SelectionFilterMaxLength = 10;
                if (_NO_TRANSLATE_FilterComboBox.Items.Count == SelectionFilterMaxLength)
                {
                    _NO_TRANSLATE_FilterComboBox.Items.RemoveAt(SelectionFilterMaxLength - 1);
                }

                _NO_TRANSLATE_FilterComboBox.Items.Insert(0, filter);
            }
        }

        private void FilterComboBox_TextUpdate(object? sender, EventArgs e)
        {
            // show DeleteFilterButton at once
            SetDeleteFilterButtonVisibility();

            string filterText = _NO_TRANSLATE_FilterComboBox.Text;

            // workaround for text getting selected if it matches the start of the combobox items
            if (_NO_TRANSLATE_FilterComboBox.SelectionLength == filterText.Length && _NO_TRANSLATE_FilterComboBox.SelectionStart == 0)
            {
                _NO_TRANSLATE_FilterComboBox.SelectionLength = 0;
                _NO_TRANSLATE_FilterComboBox.SelectionStart = filterText.Length;
            }

            _filterSubject.OnNext(filterText);
        }

        private void FilterComboBox_MouseEnter(object? sender, EventArgs e)
        {
            FilterToolTip.SetToolTip(_NO_TRANSLATE_FilterComboBox, _toolTipText);
        }

        private void FilterComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            FilterFiles(_NO_TRANSLATE_FilterComboBox.Text);
        }

        private void FilterComboBox_GotFocus(object? sender, EventArgs e)
        {
            SetFilterWatermarkLabelVisibility();
        }

        private void FilterComboBox_LostFocus(object? sender, EventArgs e)
        {
            SetFilterWatermarkLabelVisibility();
        }

        private void FilterWatermarkLabel_Click(object? sender, EventArgs e)
        {
            _NO_TRANSLATE_FilterComboBox.Focus();
        }

        private void FilterComboBox_SizeChanged(object? sender, EventArgs e)
        {
            // strangely it does not invalidate itself on resize so its look becomes distorted
            _NO_TRANSLATE_FilterComboBox.Invalidate();
        }

        private void cboFindInCommitFilesGitGrep_TextUpdate(object? sender, EventArgs e)
        {
            FindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Text);
        }

        private void FindInCommitFilesGitGrep(string search, int delay = 200)
        {
            SetFindInCommitFilesGitGrepWatermarkVisibility();
            SetDeleteSearchButtonVisibility();

            CancellationToken cancellationToken = _reloadSequence.Next();
            ThreadHelper.FileAndForget(async () =>
            {
                // delay to handle keypresses
                await Task.Delay(delay, cancellationToken);
                string searchArg = search;
                if (!string.IsNullOrWhiteSpace(searchArg) && !GrepStringRegex().IsMatch(searchArg))
                {
                    searchArg = $@"-e ""{searchArg}""";
                }

                _diffCalculator.SetGrep(searchArg);
                IReadOnlyList<FileStatusWithDescription> gitItemStatusesWithDescription = _diffCalculator.Calculate(prevList: GitItemStatusesWithDescription, refreshDiff: false, refreshGrep: true, cancellationToken);

                await this.SwitchToMainThreadAsync(cancellationToken);
                cboFindInCommitFilesGitGrep.BackColor = string.IsNullOrEmpty(search) ? SystemColors.Window : _activeInputColor;
                WorkaroundTooEarlyDrawing();
                UpdateFileStatusListView(gitItemStatusesWithDescription);

                if (string.IsNullOrEmpty(search))
                {
                    return;
                }

                AddToSearchFilter(search);
                return;

                void AddToSearchFilter(string search)
                {
                    try
                    {
                        cboFindInCommitFilesGitGrep.SelectedIndexChanged -= cboFindInCommitFilesGitGrep_SelectedIndexChanged;
                        cboFindInCommitFilesGitGrep.BeginUpdate();
                        if (cboFindInCommitFilesGitGrep.Items.IndexOf(search) is int index && index >= 0)
                        {
                            if (index == 0)
                            {
                                return;
                            }

                            cboFindInCommitFilesGitGrep.Items.RemoveAt(index);
                            cboFindInCommitFilesGitGrep.Items.Insert(0, search);
                            cboFindInCommitFilesGitGrep.Text = search;
                            cboFindInCommitFilesGitGrep.SelectionStart = cboFindInCommitFilesGitGrep.Text.Length;
                            cboFindInCommitFilesGitGrep.SelectionLength = 0;
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

                        if (_formFindInCommitFilesGitGrep?.IsDisposed is false)
                        {
                            _formFindInCommitFilesGitGrep.SetSearchItems(cboFindInCommitFilesGitGrep.Items);
                            if (cboFindInCommitFilesGitGrep.Visible)
                            {
                                _formFindInCommitFilesGitGrep.GitGrepExpressionText = search;
                            }
                        }
                    }
                    finally
                    {
                        cboFindInCommitFilesGitGrep.EndUpdate();
                        cboFindInCommitFilesGitGrep.SelectedIndexChanged += cboFindInCommitFilesGitGrep_SelectedIndexChanged;
                    }
                }

                void WorkaroundTooEarlyDrawing()
                {
                    try
                    {
                        FileStatusListView.BeginUpdate();

                        FileStatusListView.CollapseAll();

                        if (FileStatusListView.Nodes.Count > 0)
                        {
                            FileStatusListView.Nodes[^1].Text = "";
                        }
                    }
                    finally
                    {
                        FileStatusListView.EndUpdate();
                        FileStatusListView.Update();
                    }
                }
            });
        }

        private void cboFindInCommitFilesGitGrep_SelectedIndexChanged(object? sender, EventArgs e)
        {
            FindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Text, delay: 0);
        }

        private void cboFindInCommitFilesGitGrep_GotFocus(object? sender, EventArgs e)
        {
            SetFindInCommitFilesGitGrepWatermarkVisibility();
        }

        private void cboFindInCommitFilesGitGrep_LostFocus(object? sender, EventArgs e)
        {
            SetFindInCommitFilesGitGrepWatermarkVisibility();
        }

        private void cboFindInCommitFilesGitGrep_SizeChanged(object? sender, EventArgs e)
        {
            cboFindInCommitFilesGitGrep.Invalidate();
        }

        private void lblFindInCommitFilesGitGrepWatermark_Click(object? sender, EventArgs e)
        {
            cboFindInCommitFilesGitGrep.Focus();
        }

        private void DeleteSearchButton_Click(object? sender, EventArgs e)
        {
            cboFindInCommitFilesGitGrep.Text = "";
            FindInCommitFilesGitGrep(cboFindInCommitFilesGitGrep.Text, delay: 0);
        }

        private void StoreFilter(string value)
        {
            SetDeleteFilterButtonVisibility();
            if (string.IsNullOrEmpty(value))
            {
                _NO_TRANSLATE_FilterComboBox.BackColor = SystemColors.Window;
                _filter = null;
                return;
            }

            try
            {
                _filter = new Regex(value, RegexOptions.IgnoreCase);
                _NO_TRANSLATE_FilterComboBox.BackColor = _activeInputColor;
            }
            catch
            {
                _NO_TRANSLATE_FilterComboBox.BackColor = _invalidInputColor;
                throw;
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
            internal MultiSelectTreeView FileStatusListView => _fileStatusList.FileStatusListView;
            internal ComboBox FilterComboBox => _fileStatusList._NO_TRANSLATE_FilterComboBox;
            internal Regex? Filter => _fileStatusList._filter;
            internal bool FilterWatermarkLabelVisible => _fileStatusList.FilterWatermarkLabel.Visible;
            internal void StoreFilter(string value) => _fileStatusList.StoreFilter(value);
            internal void SetFileStatusListVisibility(bool filesPresent) => _fileStatusList.SetFileStatusListVisibility(filesPresent);
        }
    }
}
