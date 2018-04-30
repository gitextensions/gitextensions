using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitUI.Properties;
using GitUI.UserControls;
using ResourceManager;

namespace GitUI
{
    // Parents is used as the "first selected" (not always the parent) for GitItemStatus
    using IGitItemsWithParents = IDictionary<GitRevision, IReadOnlyList<GitItemStatus>>;
    using GitItemsWithParents = Dictionary<GitRevision, IReadOnlyList<GitItemStatus>>;

    public delegate string DescribeRevisionDelegate(string sha1);

    public sealed partial class FileStatusList : GitModuleControl
    {
        private readonly TranslationString _diffWithParent = new TranslationString("Diff with:");
        public readonly TranslationString CombinedDiff = new TranslationString("Combined Diff");

        // Artificial commit for the combined diff, similar to GitRevision.UnstagedGuid
        public readonly string CombinedDiffGuid = "2222222222222222222222222222222222222222";

        private IDisposable _selectedIndexChangeSubscription;
        private static readonly TimeSpan SelectedIndexChangeThrottleDuration = TimeSpan.FromMilliseconds(50);

        private bool _filterVisible;
        private ToolStripItem _openSubmoduleMenuItem;
        private bool _alwaysRevisionGroups = false;

        private readonly IGitRevisionTester _revisionTester;
        private readonly IFullPathResolver _fullPathResolver;
        public DescribeRevisionDelegate DescribeRevision;

        public FileStatusList()
        {
            InitializeComponent();
            CreateOpenSubmoduleMenuItem();
            Translate();
            FilterVisible = false;

            SelectFirstItemOnSetItems = true;
            FileStatusListView.MouseMove += FileStatusListView_MouseMove;
            FileStatusListView.MouseDown += FileStatusListView_MouseDown;

            if (_images == null)
            {
                _images = new ImageList
                {
                    ImageSize = DpiUtil.Scale(new Size(16, 16)), // Scale ImageSize and images scale automatically
                    Images =
                    {
                        Resources.Removed, // 0
                        Resources.Added, // 1
                        Resources.Modified, // 2
                        Resources.Renamed, // 3
                        Resources.Copied, // 4
                        Resources.IconSubmoduleDirty, // 5
                        Resources.IconSubmoduleRevisionUp, // 6
                        Resources.IconSubmoduleRevisionUpDirty, // 7
                        Resources.IconSubmoduleRevisionDown, // 8
                        Resources.IconSubmoduleRevisionDownDirty, // 9
                        Resources.IconSubmoduleRevisionSemiUp, // 10
                        Resources.IconSubmoduleRevisionSemiUpDirty, // 11
                        Resources.IconSubmoduleRevisionSemiDown, // 12
                        Resources.IconSubmoduleRevisionSemiDownDirty, // 13
                        Resources.IconFileStatusUnknown // 14
                    }
                };
            }

            FileStatusListView.SmallImageList = _images;
            FileStatusListView.LargeImageList = _images;

            HandleVisibility_NoFilesLabel_FilterComboBox(filesPresent: true);
            Controls.SetChildIndex(NoFiles, 0);
            NoFiles.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);

            _filter = new Regex(".*");
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _revisionTester = new GitRevisionTester(_fullPathResolver);
        }

        public bool AlwaysRevisionGroups
        {
            set
            {
                _alwaysRevisionGroups = value;
            }
        }

        private void CreateOpenSubmoduleMenuItem()
        {
            _openSubmoduleMenuItem = new ToolStripMenuItem
            {
                Name = "openSubmoduleMenuItem",
                Tag = "1",
                Text = "Open with Git Extensions",
                Image = Resources.gitex
            };
            _openSubmoduleMenuItem.Click += (s, ea) => { ThreadHelper.JoinableTaskFactory.RunAsync(() => OpenSubmoduleAsync()); };
        }

        protected override void DisposeCustomResources()
        {
            _selectedIndexChangeSubscription?.Dispose();
        }

        private bool _enableSelectedIndexChangeEvent = true;

        public void SetSelectedIndex(int idx, bool notify)
        {
            _enableSelectedIndexChangeEvent = notify;
            SelectedIndex = idx;
            _enableSelectedIndexChangeEvent = true;
        }

        private void EnsureSelectedIndexChangeSubscription()
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

        private static ImageList _images;

        public void SetNoFilesText(string text)
        {
            NoFiles.Text = text;
        }

        public bool FilterVisible
        {
            get
            {
                return _filterVisible;
            }

            set
            {
                _filterVisible = value;
                FilterComboBox.Visible = _filterVisible;
                FilterWatermarkLabel.Visible = _filterVisible;
            }
        }

        public override bool Focused => FileStatusListView.Focused;

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
                var nextItem = FindPrevItemInGroups(curIdx, currentGroup);
                if (nextItem == null)
                {
                    return loop ? GetLastIndex() : curIdx;
                }

                return nextItem.Index;
            }
            else
            {
                var nextItem = FindNextItemInGroups(curIdx, currentGroup);
                if (nextItem == null)
                {
                    return loop ? 0 : curIdx;
                }

                return nextItem.Index;
            }
        }

        private ListViewItem FindPrevItemInGroups(int curIdx, ListViewGroup currentGroup)
        {
            List<ListViewGroup> searchInGroups = new List<ListViewGroup>();
            bool foundCurrentGroup = false;
            for (int i = FileStatusListView.Groups.Count - 1; i >= 0; i--)
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

            foreach (ListViewGroup grp in searchInGroups)
            {
                for (int i = curIdx - 1; i >= 0; i--)
                {
                    if (FileStatusListView.Items[i].Group == grp)
                    {
                        return FileStatusListView.Items[i];
                    }
                }

                curIdx = FileStatusListView.Items.Count;
            }

            return null;
        }

        private ListViewItem FindNextItemInGroups(int curIdx, ListViewGroup currentGroup)
        {
            List<ListViewGroup> searchInGroups = new List<ListViewGroup>();
            bool foundCurrentGroup = false;
            for (int i = 0; i < FileStatusListView.Groups.Count; i++)
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

            foreach (ListViewGroup grp in searchInGroups)
            {
                for (int i = curIdx + 1; i < FileStatusListView.Items.Count; i++)
                {
                    if (FileStatusListView.Items[i].Group == grp)
                    {
                        return FileStatusListView.Items[i];
                    }
                }

                curIdx = -1;
            }

            return null;
        }

        private int GetLastIndex()
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

        private string GetItemText(Graphics graphics, GitItemStatus gitItemStatus, int imageWidth)
        {
            var pathFormatter = new PathFormatter(graphics, FileStatusListView.Font);

            return pathFormatter.FormatTextForDrawing(FileStatusListView.ClientSize.Width - imageWidth,
                                                      gitItemStatus.Name, gitItemStatus.OldName);
        }

        private void FileStatusListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (e?.Item?.Tag is GitItemStatus gitItemStatus)
            {
                var imageWidth = 0;
                if (e.Item.ImageList != null && e.Item.ImageIndex != -1)
                {
                    imageWidth = e.Item.ImageList.Images[e.Item.ImageIndex].Width;
                }

                string text = GetItemText(e.Graphics, gitItemStatus, imageWidth);
                text = AppendItemSubmoduleStatus(text, gitItemStatus);

                e.Item.Text = text;
            }

            e.DrawDefault = true;
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

        private void FileStatusListView_MouseDown(object sender, MouseEventArgs e)
        {
            // SELECT
            if (e.Button == MouseButtons.Right)
            {
                var hover = FileStatusListView.HitTest(e.Location);

                if (hover.Item != null && !hover.Item.Selected)
                {
                    ClearSelected();

                    hover.Item.Selected = true;
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
                    _dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
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

        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return FileStatusListView.ContextMenuStrip;
            }
            set
            {
                FileStatusListView.ContextMenuStrip = value;
                if (FileStatusListView.ContextMenuStrip != null)
                {
                    FileStatusListView.ContextMenuStrip.Opening += FileStatusListView_ContextMenu_Opening;
                }
            }
        }

        public override ContextMenu ContextMenu
        {
            get => FileStatusListView.ContextMenu;
            set => FileStatusListView.ContextMenu = value;
        }

        private Rectangle _dragBoxFromMouseDown;

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
                    StringCollection fileList = new StringCollection();

                    foreach (GitItemStatus item in SelectedItems)
                    {
                        string fileName = _fullPathResolver.Resolve(item.Name);

                        fileList.Add(fileName.ToNativePath());
                    }

                    DataObject obj = new DataObject();
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

                if (hoveredItem?.Tag is GitItemStatus gitItemStatus)
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

        public int UnfilteredItemsCount()
        {
            if (GitItemStatusesWithParents == null)
            {
                return 0;
            }
            else
            {
                return GitItemStatusesWithParents.Sum(pair => pair.Value.Count);
            }
        }

        [Browsable(false)]
        public IEnumerable<GitItemStatus> AllItems
        {
            get
            {
                return FileStatusListView.Items.Cast<ListViewItem>().
                    Select(selectedItem => selectedItem.Tag as GitItemStatus);
            }
        }

        [Browsable(false)]
        public IEnumerable<GitItemStatus> SelectedItems
        {
            get
            {
                return FileStatusListView.SelectedItems.Cast<ListViewItem>().
                    Select(i => i.Tag as GitItemStatus);
            }
            set
            {
                ClearSelected();
                if (value == null)
                {
                    return;
                }

                foreach (var item in FileStatusListView.Items.Cast<ListViewItem>()
                    .Where(i => value.Contains(i.Tag as GitItemStatus)))
                {
                    item.Selected = true;
                }

                var first = FileStatusListView.SelectedItems.Cast<ListViewItem>().FirstOrDefault(x => x.Selected);
                first?.EnsureVisible();
                StoreNextIndexToSelect();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitItemStatus SelectedItem
        {
            get
            {
                return SelectedItems.FirstOrDefault();
            }
            set
            {
                ClearSelected();
                if (value == null)
                {
                    return;
                }

                ListViewItem newSelected = null;
                foreach (ListViewItem item in FileStatusListView.Items)
                {
                    if (value.CompareTo(item.Tag as GitItemStatus) == 0)
                    {
                        if (newSelected == null)
                        {
                            newSelected = item;
                        }
                        else if (item.Tag == value)
                        {
                            newSelected = item;
                            break;
                        }
                    }
                }

                if (newSelected != null)
                {
                    newSelected.Selected = true;
                    newSelected.EnsureVisible();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitRevision SelectedItemParent => SelectedItemParents.FirstOrDefault();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<GitRevision> SelectedItemParents
        {
            get
            {
                return FileStatusListView.SelectedItems.Cast<ListViewItem>()
                    .Where(i => i.Group?.Tag as GitRevision != null)
                    .Select(i => i.Group.Tag as GitRevision);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<GitItemStatusWithParent> SelectedItemsWithParent
        {
            get
            {
                return FileStatusListView.SelectedItems.Cast<ListViewItem>()
                    .Where(i => i.Group?.Tag as GitRevision != null)
                    .Select(i => new GitItemStatusWithParent(i.Group.Tag as GitRevision, i.Tag as GitItemStatus));
            }
        }

        public void ClearSelected()
        {
            foreach (ListViewItem item in FileStatusListView.SelectedItems)
            {
                item.Selected = false;
            }
        }

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
            set
            {
                ClearSelected();
                if (value >= 0)
                {
                    FileStatusListView.Items[value].Selected = true;
                    FileStatusListView.Items[value].Focused = true;
                    FileStatusListView.Items[value].EnsureVisible();
                }
            }
        }

        private int _nextIndexToSelect = -1;

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

        public event EventHandler SelectedIndexChanged;
        public event EventHandler DataSourceChanged;

        public new event EventHandler DoubleClick;
        public new event KeyEventHandler KeyDown;

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
                DoubleClick(sender, e);
            }
        }

        private async Task OpenSubmoduleAsync()
        {
            var submoduleName = SelectedItem.Name;

            var status = await SelectedItem.GetSubmoduleStatusAsync().ConfigureAwait(false);

            Process process = new Process
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
        }

        private void FileStatusListView_SelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private static int GetItemImageIndex(GitItemStatus gitItemStatus)
        {
            if (gitItemStatus.IsDeleted)
            {
                return 0;
            }

            if (gitItemStatus.IsNew || !gitItemStatus.IsTracked)
            {
                return 1;
            }

            if (gitItemStatus.IsChanged || gitItemStatus.IsConflict)
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
                return 3;
            }

            if (gitItemStatus.IsCopied)
            {
                return 4;
            }

            return 14; // icon unknown
        }

        [Browsable(false)]
        [DefaultValue(true)]
        public bool IsEmpty => GitItemStatuses == null || !GitItemStatuses.Any();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<GitItemStatus> GitItemStatuses
        {
            get
            {
                return GitItemStatusesWithParents?.Values.SelectMany(plist => plist).ToList()
                       ?? (IReadOnlyList<GitItemStatus>)Array.Empty<GitItemStatus>();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<GitItemStatus> GitItemFilteredStatuses
        {
            get
            {
                var result = new List<GitItemStatus>(FileStatusListView.Items.Count);

                foreach (ListViewItem listViewItem in FileStatusListView.Items)
                {
                    result.Add(listViewItem.Tag as GitItemStatus);
                }

                return result;
            }
        }

        private IGitItemsWithParents _itemsDictionary = new GitItemsWithParents();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IGitItemsWithParents GitItemStatusesWithParents
        {
            get
            {
                return _itemsDictionary;
            }
            private set
            {
                _itemsDictionary = value ?? new GitItemsWithParents();
                UpdateFileStatusListView();
            }
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

            FileStatusListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            var previouslySelectedItems = new List<GitItemStatus>();
            if (updateCausedByFilter)
            {
                foreach (ListViewItem item in FileStatusListView.SelectedItems)
                {
                    previouslySelectedItems.Add(item.Tag as GitItemStatus);
                }

                DataSourceChanged?.Invoke(this, EventArgs.Empty);
            }

            FileStatusListView.BeginUpdate();
            FileStatusListView.ShowGroups = GitItemStatusesWithParents.Count > 1 || _alwaysRevisionGroups;
            FileStatusListView.Groups.Clear();
            FileStatusListView.Items.Clear();

            var clientSizeWidth = AppSettings.TruncatePathMethod == "compact" || AppSettings.TruncatePathMethod == "trimstart";
            var fileNameOnlyMode = AppSettings.TruncatePathMethod == "fileNameOnly";

            var list = new List<ListViewItem>();
            foreach (var (revision, statuses) in GitItemStatusesWithParents)
            {
                ListViewGroup group = null;
                if (revision != null)
                {
                    string groupName;
                    if (revision.Guid == CombinedDiffGuid)
                    {
                        groupName = CombinedDiff.Text;
                    }
                    else
                    {
                        groupName = _diffWithParent.Text + " " + GetDescriptionForRevision(revision.Guid);
                    }

                    group = new ListViewGroup(groupName)
                    {
                        Tag = revision
                    };

                    FileStatusListView.Groups.Add(group);
                }

                foreach (var item in statuses)
                {
                    if (_filter.IsMatch(item.Name))
                    {
                        var text = item.Name;
                        if (clientSizeWidth)
                        {
                            // list-item has client width, so we don't need horizontal scrollbar (which is determined by this text width)
                            text = string.Empty;
                        }
                        else if (fileNameOnlyMode)
                        {
                            // we need to put filename in list-item text -> then horizontal scrollbar
                            // will have proper width (by the longest filename, and not all path)
                            text = PathFormatter.FormatTextForFileNameOnly(item.Name, item.OldName);
                            text = AppendItemSubmoduleStatus(text, item);
                        }

                        var listItem = new ListViewItem(text, group)
                        {
                            ImageIndex = GetItemImageIndex(item)
                        };

                        if (item.GetSubmoduleStatusAsync() != null && !item.GetSubmoduleStatusAsync().IsCompleted)
                        {
                            var capturedItem = item;

                            ThreadHelper.JoinableTaskFactory.RunAsync(
                                async () =>
                                {
                                    await item.GetSubmoduleStatusAsync();

                                    await this.SwitchToMainThreadAsync();

                                    listItem.ImageIndex = GetItemImageIndex(capturedItem);
                                });
                        }

                        if (previouslySelectedItems.Contains(item))
                        {
                            listItem.Selected = true;
                        }

                        listItem.Tag = item;
                        list.Add(listItem);
                    }
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

            FileStatusListView_SizeChanged(null, null);
            FileStatusListView.SetGroupState(ListViewGroupState.Collapsible);
            FileStatusListView.EndUpdate();
        }

        private string GetDescriptionForRevision(string sha1)
        {
            if (DescribeRevision != null)
            {
                return DescribeRevision(sha1);
            }

            return sha1.ShortenTo(8);
        }

        [DefaultValue(true)]
        public bool SelectFirstItemOnSetItems { get; set; }

        public void SelectFirstVisibleItem()
        {
            if (FileStatusListView.Items.Count == 0)
            {
                return;
            }

            var group = FileStatusListView.Groups.Cast<ListViewGroup>().
                FirstOrDefault(gr => gr.Items.Count > 0);
            if (group != null)
            {
                ListViewItem sortedFirstGroupItem = FileStatusListView.Items.Cast<ListViewItem>().
                    FirstOrDefault(item => item.Group == group);
                if (sortedFirstGroupItem != null)
                {
                    sortedFirstGroupItem.Selected = true;
                }
            }
            else
            {
                FileStatusListView.Items[0].Selected = true;
            }
        }

        /// <summary>
        /// Gets or sets the revision.
        /// </summary>
        /// <value>The revision.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitRevision Revision { get; set; }

        private void FileStatusListView_SizeChanged(object sender, EventArgs e)
        {
            NoFiles.Size = new Size(Size.Width - 10, Size.Height - 10);
            Refresh();
            FileStatusListView.BeginUpdate();

            FileStatusListView.AutoResizeColumn(0,
                ColumnHeaderAutoResizeStyle.HeaderSize);
            FileStatusListView.EndUpdate();
        }

        private void FileStatusListView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    {
                        if (!e.Control)
                        {
                            break;
                        }

                        FileStatusListView.BeginUpdate();
                        try
                        {
                            for (var i = 0; i < FileStatusListView.Items.Count; i++)
                            {
                                FileStatusListView.Items[i].Selected = true;
                            }

                            e.Handled = true;
                        }
                        finally
                        {
                            FileStatusListView.EndUpdate();
                        }

                        break;
                    }

                default:
                    KeyDown?.Invoke(sender, e);
                    break;
            }
        }

        public int SetSelectionFilter(string selectionFilter)
        {
            return SelectFiles(RegexForSelecting(selectionFilter));
        }

        public void SelectAll()
        {
            try
            {
                SuspendLayout();

                var itemCount = AllItems.Count();

                for (var i = 0; i < itemCount; i++)
                {
                    FileStatusListView.Items[i].Selected = true;
                    i++;
                }
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private static Regex RegexForSelecting(string value)
        {
            return string.IsNullOrEmpty(value)
                ? new Regex("^$", RegexOptions.Compiled)
                : new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private int SelectFiles(Regex selctionFilter)
        {
            try
            {
                SuspendLayout();

                var items = AllItems;
                int i = 0;
                foreach (var item in items)
                {
                    FileStatusListView.Items[i].Selected = selctionFilter.IsMatch(item.Name);
                    i++;
                }

                return FileStatusListView.SelectedIndices.Count;
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        public void SetDiffs(GitRevision selectedRev = null, GitRevision parentRev = null, IReadOnlyList<GitItemStatus> items = null)
        {
            Revision = selectedRev;

            if (parentRev == null)
            {
                parentRev = new GitRevision("");
            }

            IGitItemsWithParents dictionary = items == null
                ? null
                : new GitItemsWithParents { { parentRev, items } };

            GitItemStatusesWithParents = dictionary;
        }

        public void SetDiffs(IReadOnlyList<GitRevision> revisions)
        {
            if (revisions == null || revisions.Count == 0)
            {
                Revision = null;
            }
            else
            {
                Revision = revisions[0];
            }

            var dictionary = new GitItemsWithParents();
            if (Revision != null)
            {
                GitRevision[] parentRevs;
                if (revisions.Count == 1)
                {
                    // Note: RevisionGrid could in some forms be used to get the parent guids
                    parentRevs = Revision.ParentGuids.Select(item => new GitRevision(item)).ToArray();
                }
                else
                {
                    parentRevs = revisions.Skip(1).ToArray();
                }

                if (parentRevs.Length == 0)
                {
                    // No parent, will set "" as parent
                    var rev = new GitRevision("");
                    dictionary.Add(rev, Module.GetTreeFiles(Revision.TreeGuid, true));
                }
                else
                {
                    if (!AppSettings.ShowDiffForAllParents)
                    {
                        parentRevs = new[] { parentRevs[0] };
                    }

                    foreach (var rev in parentRevs)
                    {
                        dictionary.Add(rev, Module.GetDiffFilesWithSubmodulesStatus(rev.Guid, Revision.Guid));
                    }

                    // Show combined (merge conflicts) only when all first (A) are parents to selected (B)
                    var isMergeCommit = AppSettings.ShowDiffForAllParents &&
                                        Revision.ParentGuids != null && Revision.ParentGuids.Count() > 1 &&
                                        _revisionTester.AllFirstAreParentsToSelected(parentRevs, Revision);
                    if (isMergeCommit)
                    {
                        var conflicts = Module.GetCombinedDiffFileList(Revision.Guid);
                        if (conflicts.Any())
                        {
                            // Create an artificial commit
                            var rev = new GitRevision(CombinedDiffGuid);
                            dictionary.Add(rev, conflicts);
                        }
                    }
                }
            }

            GitItemStatusesWithParents = dictionary;
        }

        private void HandleVisibility_NoFilesLabel_FilterComboBox(bool filesPresent)
        {
            NoFiles.Visible = !filesPresent;
            if (_filterVisible)
            {
                FilterComboBox.Visible = filesPresent;
            }
        }

        #region Filtering

        private long _lastUserInputTime;
        private string _toolTipText = "";

        private static Regex RegexForFiltering(string value)
        {
            return string.IsNullOrEmpty(value)
                ? new Regex(".", RegexOptions.Compiled)
                : new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public void SetFilter(string value)
        {
            FilterComboBox.Text = value;
            FilterFiles(value);
        }

        private int FilterFiles(string value)
        {
            _filter = RegexForFiltering(value);
            UpdateFileStatusListView(true);
            return FileStatusListView.Items.Count;
        }

        private void FilterComboBox_TextUpdate(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now.Ticks;
            if (_lastUserInputTime == 0)
            {
                long timerLastChanged = currentTime;
                var timer = new Timer { Interval = 250 };
                timer.Tick += (s, a) =>
                {
                    if (NoUserInput(timerLastChanged))
                    {
                        _toolTipText = "";
                        var fileCount = 0;
                        try
                        {
                            fileCount = FilterFiles(FilterComboBox.Text);
                        }
                        catch (ArgumentException ae)
                        {
                            _toolTipText = ae.Message;
                        }

                        if (fileCount > 0)
                        {
                            AddToSelectionFilter(FilterComboBox.Text);
                        }

                        timer.Stop();
                        _lastUserInputTime = 0;
                    }

                    timerLastChanged = _lastUserInputTime;
                };

                timer.Start();
            }

            _lastUserInputTime = currentTime;
        }

        private bool NoUserInput(long timerLastChanged)
        {
            return timerLastChanged == _lastUserInputTime;
        }

        private void AddToSelectionFilter(string filter)
        {
            if (!FilterComboBox.Items.Cast<string>().Any(candiate => candiate == filter))
            {
                const int SelectionFilterMaxLength = 10;
                if (FilterComboBox.Items.Count == SelectionFilterMaxLength)
                {
                    FilterComboBox.Items.RemoveAt(SelectionFilterMaxLength - 1);
                }

                FilterComboBox.Items.Insert(0, filter);
            }
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
            FilterWatermarkLabel.Visible = false;
        }

        private void FilterComboBox_LostFocus(object sender, EventArgs e)
        {
            if (!FilterWatermarkLabel.Visible && string.IsNullOrEmpty(FilterComboBox.Text))
            {
                FilterWatermarkLabel.Visible = true;
            }
        }

        private void FilterWatermarkLabel_Click(object sender, EventArgs e)
        {
            FilterComboBox.Focus();
        }

        private Regex _filter;

        #endregion Filtering

    }

    public class GitItemStatusWithParent
    {
        public readonly GitRevision ParentRevision;
        public readonly GitItemStatus Item;

        public GitItemStatusWithParent(GitRevision parent, GitItemStatus item)
        {
            ParentRevision = parent;
            Item = item;
        }
    }
}
