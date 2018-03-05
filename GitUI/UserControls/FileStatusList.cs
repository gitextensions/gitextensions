using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using GitUI.UserControls;
using ResourceManager;

namespace GitUI
{
    // Parents is used as the "first selected" (not always the parent) for GitItemStatus
    using IGitItemsWithParents = IDictionary<GitRevision, IList<GitItemStatus>>;
    using GitItemsWithParents = Dictionary<GitRevision, IList<GitItemStatus>>;

    public delegate string DescribeRevisionDelegate(string sha1);

    public sealed partial class FileStatusList : GitModuleControl
    {
        private readonly TranslationString _UnsupportedMultiselectAction =
            new TranslationString("Operation not supported");
        private readonly TranslationString _DiffWithParent =
            new TranslationString("Diff with:");
        public readonly TranslationString CombinedDiff =
            new TranslationString("Combined Diff");

        private IDisposable selectedIndexChangeSubscription;
        private static readonly TimeSpan SelectedIndexChangeThrottleDuration = TimeSpan.FromMilliseconds(50);

        private bool _filterVisible;
        private ToolStripItem _openSubmoduleMenuItem;

        public DescribeRevisionDelegate DescribeRevision;
        private readonly IFullPathResolver _fullPathResolver;

        public FileStatusList()
        {
            InitializeComponent();
            CreateOpenSubmoduleMenuItem();
            Translate();
            FilterVisible = false;

            SelectFirstItemOnSetItems = true;
            _noDiffFilesChangesDefaultText = NoFiles.Text;
            FileStatusListView.MouseMove += FileStatusListView_MouseMove;
            FileStatusListView.MouseDown += FileStatusListView_MouseDown;
            if (_images == null)
            {
                _images = new ImageList();
                _images.Images.Add(Resources.Removed); // 0
                _images.Images.Add(Resources.Added); // 1
                _images.Images.Add(Resources.Modified); // 2
                _images.Images.Add(Resources.Renamed); // 3
                _images.Images.Add(Resources.Copied); // 4
                _images.Images.Add(Resources.IconSubmoduleDirty); // 5
                _images.Images.Add(Resources.IconSubmoduleRevisionUp); // 6
                _images.Images.Add(Resources.IconSubmoduleRevisionUpDirty); // 7
                _images.Images.Add(Resources.IconSubmoduleRevisionDown); // 8
                _images.Images.Add(Resources.IconSubmoduleRevisionDownDirty); // 9
                _images.Images.Add(Resources.IconSubmoduleRevisionSemiUp); // 10
                _images.Images.Add(Resources.IconSubmoduleRevisionSemiUpDirty); // 11
                _images.Images.Add(Resources.IconSubmoduleRevisionSemiDown); // 12
                _images.Images.Add(Resources.IconSubmoduleRevisionSemiDownDirty); // 13
                _images.Images.Add(Resources.IconFileStatusUnknown); // 14
            }
            FileStatusListView.SmallImageList = _images;
            FileStatusListView.LargeImageList = _images;

            HandleVisibility_NoFilesLabel_FilterComboBox(filesPresent: true);
            this.Controls.SetChildIndex(NoFiles, 0);
            NoFiles.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);

            _filter = new Regex(".*");
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
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
            _openSubmoduleMenuItem.Click += (s, ea) => { OpenSubmodule(); };
        }

        protected override void DisposeCustomResources()
        {
            selectedIndexChangeSubscription?.Dispose();
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
            if (selectedIndexChangeSubscription == null)
            {
                selectedIndexChangeSubscription = Observable.FromEventPattern(
                    h => FileStatusListView.SelectedIndexChanged += h,
                    h => FileStatusListView.SelectedIndexChanged -= h)
                    .Where(x => _enableSelectedIndexChangeEvent)
                    .Throttle(SelectedIndexChangeThrottleDuration)
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(_ => FileStatusListView_SelectedIndexChanged());
            }
        }

        private static ImageList _images;

        private readonly string _noDiffFilesChangesDefaultText;

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
                    SelectedIndex = 0;
                FileStatusListView.Focus();
            }
        }

        public int GetNextIndex(bool searchBackward, bool loop)
        {
            int curIdx = SelectedIndex;
            if (curIdx >= 0)
            {
                ListViewItem currentItem = FileStatusListView.Items[curIdx];
                var currentGroup = currentItem.Group;

                int maxIdx = GitItemStatuses.Count() - 1;

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
            else
            {
                return -1;
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
            var gitItemStatus = e?.Item?.Tag as GitItemStatus;

            if (gitItemStatus != null)
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

        private string AppendItemSubmoduleStatus(string text, GitItemStatus item)
        {
            if (item.IsSubmodule &&
                item.SubmoduleStatus != null &&
                item.SubmoduleStatus.IsCompleted &&
                item.SubmoduleStatus.Result != null)
            {
                text += item.SubmoduleStatus.Result.AddedAndRemovedString();
            }
            return text;
        }

        void FileStatusListView_MouseDown(object sender, MouseEventArgs e)
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
                    dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                                   e.Y - (dragSize.Height / 2)),
                                                            dragSize);
                }
                else
                    // Reset the rectangle if the mouse is not over an item in the ListView.
                    dragBoxFromMouseDown = Rectangle.Empty;
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
                    FileStatusListView.ContextMenuStrip.Opening += new CancelEventHandler(FileStatusListView_ContextMenu_Opening);
                }
            }
        }

        public override ContextMenu ContextMenu
        {
            get
            {
                return FileStatusListView.ContextMenu;
            }
            set
            {
                FileStatusListView.ContextMenu = value;
            }
        }

        private Rectangle dragBoxFromMouseDown;

        void FileStatusListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListView listView = sender as ListView;

            // DRAG
            // If the mouse moves outside the rectangle, start the drag.
            if (dragBoxFromMouseDown != Rectangle.Empty &&
                !dragBoxFromMouseDown.Contains(e.X, e.Y))
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
                    dragBoxFromMouseDown = Rectangle.Empty;
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

                var gitItemStatus = hoveredItem?.Tag as GitItemStatus;
                if (gitItemStatus != null)
                {
                    string text;
                    if (gitItemStatus.IsRenamed || gitItemStatus.IsCopied)
                        text = string.Concat(gitItemStatus.Name, " (", gitItemStatus.OldName, ")");
                    else
                        text = gitItemStatus.Name;

                    float fTextWidth = listView.CreateGraphics().MeasureString(text, listView.Font).Width + 17;

                    // Use width-itemheight because the icon drawn in front of the text is the itemheight
                    if (fTextWidth > (FileStatusListView.Width - FileStatusListView.GetItemRect(hoveredItem.Index).Height))
                    {
                        if (!hoveredItem.ToolTipText.Equals(gitItemStatus.ToString()))
                            hoveredItem.ToolTipText = gitItemStatus.ToString();
                    }
                    else
                        hoveredItem.ToolTipText = "";
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
                return GitItemStatusesWithParents.SelectMany(pair => pair.Value).Count();
            }
        }

        [Browsable(false)]
        public IEnumerable<GitItemStatus> AllItems
        {
            get
            {
                return (FileStatusListView.Items.Cast<ListViewItem>().
                    Select(selectedItem => selectedItem.Tag as GitItemStatus));
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
                    return;

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
                    return;
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
        public GitRevision SelectedItemParent
        {
            get
            {
                return SelectedItemParents.FirstOrDefault();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IEnumerable<GitRevision> SelectedItemParents
        {
            get
            {
                return FileStatusListView.SelectedItems.Cast<ListViewItem>()
                    .Where(i => i.Group != null && i.Group.Tag as GitRevision != null)
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
                    .Where(i => i.Group != null && i.Group.Tag as GitRevision != null)
                    .Select(i => new GitItemStatusWithParent(i.Group.Tag as GitRevision, i.Tag as GitItemStatus));
            }
        }

        public void ClearSelected()
        {
            foreach (ListViewItem item in FileStatusListView.SelectedItems)
                item.Selected = false;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int SelectedIndex
        {
            get
            {
                foreach (int i in FileStatusListView.SelectedIndices)
                    return i;
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
                if (idx > _nextIndexToSelect)
                    _nextIndexToSelect = idx;
            _nextIndexToSelect = _nextIndexToSelect - FileStatusListView.SelectedIndices.Count + 1;
        }

        public void SelectStoredNextIndex(int defaultIndex = -1)
        {
            _nextIndexToSelect = Math.Min(_nextIndexToSelect, FileStatusListView.Items.Count - 1);
            if (_nextIndexToSelect < 0 && defaultIndex > -1)
                _nextIndexToSelect = Math.Min(defaultIndex, FileStatusListView.Items.Count - 1);
            if (_nextIndexToSelect > -1)
                SelectedIndex = _nextIndexToSelect;
            _nextIndexToSelect = -1;
        }

        public event EventHandler SelectedIndexChanged;
        public event EventHandler DataSourceChanged;

        public new event EventHandler DoubleClick;
        public new event KeyEventHandler KeyDown;

        void FileStatusListView_DoubleClick(object sender, EventArgs e)
        {
            if (DoubleClick == null)
            {
                if (AppSettings.OpenSubmoduleDiffInSeparateWindow && SelectedItem.IsSubmodule)
                {
                    OpenSubmodule();
                }
                else
                {
                    UICommands.StartFileHistoryDialog(this, SelectedItem.Name, Revision);
                }
            }
            else
                DoubleClick(sender, e);
        }

        private void OpenSubmodule()
        {
            var submoduleName = SelectedItem.Name;
            SelectedItem.SubmoduleStatus.ContinueWith(
                (t) =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName = Application.ExecutablePath;
                    process.StartInfo.Arguments = "browse -commit=" + t.Result.Commit;
                    process.StartInfo.WorkingDirectory = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator());
                    process.Start();
                });
        }

        void FileStatusListView_ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var cm = sender as ContextMenuStrip;
            if (!cm.Items.Find(_openSubmoduleMenuItem.Name, true).Any())
            {
                cm.Items.Insert(1, _openSubmoduleMenuItem);
            }

            bool isSubmoduleSelected = SelectedItem != null && SelectedItem.IsSubmodule;

            _openSubmoduleMenuItem.Visible = isSubmoduleSelected;

            if (isSubmoduleSelected)
            {
                _openSubmoduleMenuItem.Font = AppSettings.OpenSubmoduleDiffInSeparateWindow ?
                    new Font(_openSubmoduleMenuItem.Font,  FontStyle.Bold) :
                    new Font(_openSubmoduleMenuItem.Font, FontStyle.Regular);
            }
        }

        void FileStatusListView_SelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private static int GetItemImageIndex(GitItemStatus gitItemStatus)
        {
            if (gitItemStatus.IsDeleted)
                return 0;
            if (gitItemStatus.IsNew || !gitItemStatus.IsTracked)
                return 1;
            if (gitItemStatus.IsChanged || gitItemStatus.IsConflict)
            {
                if (!gitItemStatus.IsSubmodule || gitItemStatus.SubmoduleStatus == null ||
                    !gitItemStatus.SubmoduleStatus.IsCompleted)
                    return 2;

                var status = gitItemStatus.SubmoduleStatus.Result;
                if (status == null)
                    return 2;
                if (status.Status == SubmoduleStatus.FastForward)
                    return 6 + (status.IsDirty ? 1 : 0);
                if (status.Status == SubmoduleStatus.Rewind)
                    return 8 + (status.IsDirty ? 1 : 0);
                if (status.Status == SubmoduleStatus.NewerTime)
                    return 10 + (status.IsDirty ? 1 : 0);
                if (status.Status == SubmoduleStatus.OlderTime)
                    return 12 + (status.IsDirty ? 1 : 0);
                return !status.IsDirty ? 2 : 5;
            }
            if (gitItemStatus.IsRenamed)
                return 3;
            if (gitItemStatus.IsCopied)
                return 4;
            return 14; // icon unknown
        }

        [Browsable(false)]
        [DefaultValue(true)]
        public bool IsEmpty => GitItemStatuses == null || !GitItemStatuses.Any();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IList<GitItemStatus> GitItemStatuses
        {
            get
            {
                var result = new List<GitItemStatus>();
                var data = GitItemStatusesWithParents;
                if (data != null)
                    foreach (var plist in data.Values)
                        result.AddAll(plist);

                return result;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IList<GitItemStatus> GitItemFilteredStatuses
        {
            get
            {
                var result = new List<GitItemStatus>();
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
                foreach (ListViewItem Item in FileStatusListView.SelectedItems)
                {
                    previouslySelectedItems.Add(Item.Tag as GitItemStatus);
                }

                DataSourceChanged?.Invoke(this, new EventArgs());
            }

            FileStatusListView.BeginUpdate();
            FileStatusListView.ShowGroups = GitItemStatusesWithParents.Count > 1;
            FileStatusListView.Groups.Clear();
            FileStatusListView.Items.Clear();

            var clientSizeWidth = AppSettings.TruncatePathMethod == "compact" || AppSettings.TruncatePathMethod == "trimstart";
            var fileNameOnlyMode = AppSettings.TruncatePathMethod == "fileNameOnly";

            var list = new List<ListViewItem>();
            foreach (var pair in GitItemStatusesWithParents)
            {
                ListViewGroup group = null;
                if (pair.Key != null)
                {
                    var groupName = "";
                    if (pair.Key.Guid == CombinedDiff.Text)
                    {
                        groupName = CombinedDiff.Text;
                    }
                    else
                    {
                        groupName = _DiffWithParent.Text + " " + GetDescriptionForRevision(pair.Key.Guid);
                    }

                    group = new ListViewGroup(groupName);
                    group.Tag = pair.Key;
                    FileStatusListView.Groups.Add(group);
                }
                foreach (var item in pair.Value)
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

                        var listItem = new ListViewItem(text, group);
                        listItem.ImageIndex = GetItemImageIndex(item);
                        if (item.SubmoduleStatus != null && !item.SubmoduleStatus.IsCompleted)
                        {
                            var capturedItem = item;
                            item.SubmoduleStatus.ContinueWith((task) => listItem.ImageIndex = GetItemImageIndex(capturedItem),
                                                              CancellationToken.None,
                                                              TaskContinuationOptions.OnlyOnRanToCompletion,
                                                              TaskScheduler.FromCurrentSynchronizationContext());
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
                DataSourceChanged?.Invoke(this, new EventArgs());
                if (SelectFirstItemOnSetItems)
                    SelectFirstVisibleItem();
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
                return;
            var group = FileStatusListView.Groups.Cast<ListViewGroup>().
                FirstOrDefault(gr => gr.Items.Count > 0);
            if (group != null)
            {
                ListViewItem sortedFirstGroupItem = FileStatusListView.Items.Cast<ListViewItem>().
                    FirstOrDefault(item => item.Group == group);
                if (sortedFirstGroupItem != null)
                    sortedFirstGroupItem.Selected = true;
            }
            else
                FileStatusListView.Items[0].Selected = true;
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
                            break;
                        FileStatusListView.BeginUpdate();
                        try
                        {
                            for (var i = 0; i < FileStatusListView.Items.Count; i++)
                                FileStatusListView.Items[i].Selected = true;
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

                var items = AllItems;
                int i = 0;
                foreach (var item in items)
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

        public void SetDiffs(GitRevision selectedRev = null, GitRevision parentRev = null, IList<GitItemStatus> items = null)
        {
            Revision = selectedRev;
            if (parentRev == null) { parentRev = new GitRevision(""); }
            IGitItemsWithParents dictionary = items == null ? null :
                dictionary = new GitItemsWithParents { { parentRev, items } };

            GitItemStatusesWithParents = dictionary;
        }

        public void SetDiffs(IList<GitRevision> revisions)
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
            NoFiles.Text = _noDiffFilesChangesDefaultText; // Temporary
            if (revisions.Count > 2)
            {
                // Not a limitations, to keep compatibility with existing RevisionDiff
                NoFiles.Text = _UnsupportedMultiselectAction.Text;
            }
            else if (Revision != null)
            {
                GitRevision[] parentRevs;
                if (revisions.Count == 1)
                {
                    var list = new List<GitRevision>();
                    foreach (var item in Revision.ParentGuids)
                    {
                        // Note: RevisionGrid could in some forms be used to get the parent guids
                        list.Add(new GitRevision(item));
                    };
                    parentRevs = list.ToArray();
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
                        parentRevs = new GitRevision[] { parentRevs[0] };

                    foreach (var rev in parentRevs)
                    {
                        dictionary.Add(rev, Module.GetDiffFilesWithSubmodulesStatus(rev.Guid, Revision.Guid));
                    }

                    // Show combined (merge conflicts) only when A is only parent
                    var isMergeCommit = AppSettings.ShowDiffForAllParents &&
                        Revision.ParentGuids != null && Revision.ParentGuids.Count() > 1
                        && revisions.Count == 1;
                    if (isMergeCommit)
                    {
                        var conflicts = Module.GetCombinedDiffFileList(Revision.Guid);
                        if (conflicts.Any())
                        {
                            // Create a mock commit
                            var rev = new GitRevision(CombinedDiff.Text);
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
                FilterWatermarkLabel.Visible = filesPresent;
            }
        }

        #region Filtering


        private long _lastUserInputTime;
        private string _ToolTipText = "";

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
                var timer = new System.Windows.Forms.Timer { Interval = 250 };
                timer.Tick += (s, a) =>
                {
                    if (NoUserInput(timerLastChanged))
                    {
                        _ToolTipText = "";
                        var fileCount = 0;
                        try
                        {
                            fileCount = FilterFiles(FilterComboBox.Text);
                        }
                        catch (ArgumentException ae)
                        {
                            _ToolTipText = ae.Message;
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
            FilterToolTip.SetToolTip(FilterComboBox, _ToolTipText);
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
