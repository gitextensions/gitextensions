using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using GitUI.UserControls;
using ResourceManager.Translation;

namespace GitUI
{
    using GitItemsWithParents = IDictionary<string, IList<GitItemStatus>>;

    public sealed partial class FileStatusList : GitModuleControl
    {
        private readonly TranslationString _UnsupportedMultiselectAction =
            new TranslationString("Operation not supported");
        private readonly TranslationString _DiffWithParent =
            new TranslationString("Diff with parent");

        private const int ImageSize = 16;

        public FileStatusList()
        {
            InitializeComponent(); Translate();
            _noDiffFilesChangesDefaultText = NoFiles.Text;
#if !__MonoCS__ // TODO Drag'n'Drop doesn't work on Mono/Linux
            FileStatusListView.MouseMove += FileStatusListView_MouseMove;
            FileStatusListView.MouseDown += FileStatusListView_MouseDown;
#endif
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
            }
            FileStatusListView.SmallImageList = _images;
            FileStatusListView.LargeImageList = _images;

            NoFiles.Visible = false;
            NoFiles.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);
        }

        private static ImageList _images;

        private readonly string _noDiffFilesChangesDefaultText;

        public void SetNoFilesText(string text)
        {
            NoFiles.Text = text;
        }

        public string GetNoFilesText()
        {
            return NoFiles.Text;
        }

        public override bool Focused
        {
            get
            {
                return FileStatusListView.Focused;
            }
        }

        public new void Focus()
        {
            if (FileStatusListView.Items.Count > 0)
            {
                if (SelectedItem == null)
                    SelectedIndex = 0;
                FileStatusListView.Focus();
            }
        }

        private string GetItemText(Graphics graphics, GitItemStatus gitItemStatus)
        {
            var pathFormatter = new PathFormatter(graphics, FileStatusListView.Font);

            return pathFormatter.FormatTextForDrawing(FileStatusListView.ClientSize.Width - ImageSize,
                                                      gitItemStatus.Name, gitItemStatus.OldName);
        }

        private void FileStatusListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (e.Bounds.Height <= 0 || e.Bounds.Width <= 0 || e.ItemIndex < 0)
                return;

            e.DrawBackground();
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                e.Item.ForeColor = SystemColors.HighlightText;
            }
            else
                e.Item.ForeColor = SystemColors.WindowText;
            e.DrawFocusRectangle();

            e.Graphics.FillRectangle(Brushes.White, e.Bounds.Left, e.Bounds.Top, ImageSize, e.Bounds.Height);

            int centeredImageTop = e.Bounds.Top;
            if ((e.Bounds.Height - ImageSize) > 1)
                centeredImageTop = e.Bounds.Top + ((e.Bounds.Height - ImageSize) / 2);

            if (e.Item.ImageIndex != -1)
            {
                var image = e.Item.ImageList.Images[e.Item.ImageIndex];
                e.Graphics.DrawImage(image, e.Bounds.Left, centeredImageTop, ImageSize, ImageSize);
            }

            GitItemStatus gitItemStatus = (GitItemStatus)e.Item.Tag;

            string text = GetItemText(e.Graphics, gitItemStatus);

            using (var solidBrush = new SolidBrush(e.Item.ForeColor))
            {
                e.Graphics.DrawString(text, e.Item.ListView.Font,
                                      solidBrush, e.Bounds.Left + ImageSize, e.Bounds.Top);
            }
        }

#if !__MonoCS__ // TODO Drag'n'Drop doesnt work on Mono/Linux
        void FileStatusListView_MouseDown(object sender, MouseEventArgs e)
        {
            //SELECT
            if (e.Button == MouseButtons.Right)
            {
                var hover = FileStatusListView.HitTest(e.Location);

                if (hover.Item != null && !hover.Item.Selected)
                {
                    ClearSelected();

                    hover.Item.Selected = true;
                }
            }

            //DRAG
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
#endif

        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return FileStatusListView.ContextMenuStrip;
            }
            set
            {
                FileStatusListView.ContextMenuStrip = value;
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

#if !__MonoCS__ // TODO Drag'n'Drop doesnt work on Mono/Linux
        private Rectangle dragBoxFromMouseDown;

        void FileStatusListView_MouseMove(object sender, MouseEventArgs e)
        {
            ListView listView = sender as ListView;

            //DRAG
            // If the mouse moves outside the rectangle, start the drag.
            if (dragBoxFromMouseDown != Rectangle.Empty &&
                !dragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                if (SelectedItems.Any())
                {
                    StringCollection fileList = new StringCollection();

                    foreach (GitItemStatus item in SelectedItems)
                    {
                        string fileName = Path.Combine(Module.WorkingDir, item.Name);

                        fileList.Add(fileName.Replace('/', '\\'));
                    }

                    DataObject obj = new DataObject();
                    obj.SetFileDropList(fileList);

                    // Proceed with the drag and drop, passing in the list item.                   
                    DoDragDrop(obj, DragDropEffects.Copy);
                    dragBoxFromMouseDown = Rectangle.Empty;
                }
            }

            //TOOLTIP
            if (listView != null)
            {
                var point = new Point(e.X, e.Y);
                var hover = listView.HitTest(point);
                if (hover.Item != null)
                {
                    var gitItemStatus = (GitItemStatus)hover.Item.Tag;

                    string text;
                    if (gitItemStatus.IsRenamed || gitItemStatus.IsCopied)
                        text = string.Concat(gitItemStatus.Name, " (", gitItemStatus.OldName, ")");
                    else
                        text = gitItemStatus.Name;

                    float fTextWidth = listView.CreateGraphics().MeasureString(text, listView.Font).Width + 17;

                    //Use width-itemheight because the icon drawn in front of the text is the itemheight
                    if (fTextWidth > (FileStatusListView.Width - FileStatusListView.GetItemRect(hover.Item.Index).Height))
                    {
                        if (!hover.Item.ToolTipText.Equals(gitItemStatus.ToString()))
                            hover.Item.ToolTipText = gitItemStatus.ToString();
                    }
                    else
                        hover.Item.ToolTipText = "";
                }
            }
        }
#endif

        [Browsable(false)]
        public IEnumerable<GitItemStatus> AllItems
        {
            get
            {
                return (FileStatusListView.Items.Cast<ListViewItem>().
                    Select(selectedItem => (GitItemStatus) selectedItem.Tag));
            }
        }

        [Browsable(false)]
        public IEnumerable<GitItemStatus> SelectedItems
        {
            get
            {
                return FileStatusListView.SelectedItems.Cast<ListViewItem>().
                    Select(i => (GitItemStatus)i.Tag);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitItemStatus SelectedItem
        {
            get
            {
                foreach (ListViewItem item in FileStatusListView.SelectedItems)
                    return (GitItemStatus)item.Tag;
                return null;
            }
            set
            {
                ClearSelected();
                if (value == null)
                    return;
                foreach (ListViewItem item in FileStatusListView.SelectedItems)
                    if (item.Tag == value)
                        item.Selected = true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string SelectedItemParent
        {
            get
            {
                foreach (ListViewItem item in FileStatusListView.SelectedItems)
                    return item.Group != null ? (string)item.Group.Tag : null;
                return null;
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
                    FileStatusListView.Items[value].Selected = true;
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
                UICommands.StartFileHistoryDialog(this, SelectedItem.Name, Revision);
            else
                DoubleClick(sender, e);
        }

        void FileStatusListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, e);
        }

        private static int GetItemImageIndex(GitItemStatus gitItemStatus)
        {
            if (gitItemStatus.IsDeleted)
                return 0;
            if (gitItemStatus.IsNew || !gitItemStatus.IsTracked)
                return 1;
            if (gitItemStatus.IsChanged)
            {
                if (!gitItemStatus.IsSubmodule || gitItemStatus.SubmoduleStatus == null ||
                    !gitItemStatus.SubmoduleStatus.IsCompleted)
                    return 2;

                var status = gitItemStatus.SubmoduleStatus.Result;
                if (status.Status == SubmoduleStatus.FastForward || status.Status == SubmoduleStatus.NewerTime)
                    return 6 + (status.IsDirty ? 1 : 0);
                if (status.Status == SubmoduleStatus.Rewind || status.Status == SubmoduleStatus.OlderTime)
                    return 8 + (status.IsDirty ? 1 : 0);
                return !status.IsDirty ? 2 : 5;
            }
            else if (gitItemStatus.IsRenamed)
                return 3;
            else if (gitItemStatus.IsCopied)
                return 4;
            return -1;
        }

        [Browsable(false)]
        [DefaultValue(true)]
        public bool IsEmpty
        {
            get { return GitItemStatuses == null || !GitItemStatuses.Any(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IList<GitItemStatus> GitItemStatuses
        {
            get
            {
                var data = GitItemStatusesWithParents;
                if (data != null && data.Count > 0)
                    return data.ElementAt(0).Value;
                return new List<GitItemStatus>();
            }
            set
            {
                if (value == null)
                    GitItemStatusesWithParents = null;
                else
                    SetGitItemStatuses(null, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string GitFirstParent
        {
            get
            {
                var data = GitItemStatusesWithParents;
                if (data != null && data.Count > 0)
                    return data.ElementAt(0).Key;
                return null;
            }
        }

        public void SetGitItemStatuses(string parentRev, IList<GitItemStatus> items)
        {
            var dictionary = new Dictionary<string, IList<GitItemStatus>> {{parentRev ?? "", items}};
            GitItemStatusesWithParents = dictionary;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitItemsWithParents GitItemStatusesWithParents
        {
            get
            {
                var dictionary = new Dictionary<string, IList<GitItemStatus>>();
                foreach (ListViewItem item in FileStatusListView.Items)
                {
                    string parentRev = item.Group != null ? item.Group.Tag as string : "";
                    if (!dictionary.ContainsKey(parentRev))
                        dictionary.Add(parentRev, new List<GitItemStatus>());
                    dictionary[parentRev].Add((GitItemStatus)item.Tag);
                }
                return dictionary;
            }
            set
            {
                if (value == null || !value.Any())
                    NoFiles.Visible = true;
                else
                    NoFiles.Visible = false;

                bool empty = FileStatusListView.Items.Count == 0;
                FileStatusListView.ShowGroups = value != null && value.Count > 1;
                FileStatusListView.Groups.Clear();
                FileStatusListView.Items.Clear();
                if (value == null)
                    return;
                FileStatusListView.BeginUpdate();
                var list = new List<ListViewItem>();
                foreach (var pair in value)
                {
                    ListViewGroup group = null;
                    if (!String.IsNullOrEmpty(pair.Key))
                    {
                        string shortHash = pair.Key.Length > 8 ? pair.Key.Substring(0, 8) : pair.Key;
                        group = new ListViewGroup(_DiffWithParent.Text + " " + shortHash);
                        group.Tag = pair.Key;
                        FileStatusListView.Groups.Add(group);
                    }
                    foreach (var item in pair.Value)
                    {
                        var listItem = new ListViewItem(item.Name, group);
                        listItem.ImageIndex = GetItemImageIndex(item);
                        if (item.SubmoduleStatus != null && !item.SubmoduleStatus.IsCompleted)
                        {
                            var capturedItem = item;
                            item.SubmoduleStatus.ContinueWith((task) => listItem.ImageIndex = GetItemImageIndex(capturedItem),
                                                              CancellationToken.None,
                                                              TaskContinuationOptions.OnlyOnRanToCompletion,
                                                              TaskScheduler.FromCurrentSynchronizationContext());
                        }
                        listItem.Tag = item;
                        list.Add(listItem);
                    };
                }
                FileStatusListView.Items.AddRange(list.ToArray());
                FileStatusListView.EndUpdate();
                FileStatusListView.SetGroupState(ListViewGroupState.Collapsible);
                if (DataSourceChanged != null)
                    DataSourceChanged(this, new EventArgs());
                if (FileStatusListView.Items.Count > 0)
                {
                    SelectFirstVisibleItem();
                }
                else if (FileStatusListView.Items.Count == 0 && !empty)
                {
                    //bug in the ListView control where supplying an empty list will not trigger a SelectedIndexChanged event, so we force it to trigger
                    FileStatusListView_SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        private void SelectFirstVisibleItem()
        {
            var group = FileStatusListView.Groups.Cast<ListViewGroup>().
                FirstOrDefault(gr => gr.Items.Count > 0);
            if (group != null)
            {
                ListViewItem sortedFirstGroupItem = FileStatusListView.Items.Cast<ListViewItem>().
                    FirstOrDefault(item => item.Group == group);
                if (sortedFirstGroupItem != null)
                    sortedFirstGroupItem.Selected = true;
            }
            else if (FileStatusListView.Items.Count > 0)
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
            NoFiles.Location = new Point(5, 5);
            NoFiles.Size = new Size(Size.Width - 10, Size.Height - 10);
            Refresh();
            FileStatusListView.BeginUpdate();
            FileStatusListView.Columns[0].Width = FileStatusListView.ClientSize.Width;
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
                    if (KeyDown != null)
                        KeyDown(sender, e);
                    break;
            }
        }

        public int SetSelectionFilter(string filter)
        {
            return FilterFiles(RegexFor(filter));
        }

        private static Regex RegexFor(string value)
        {
            return string.IsNullOrEmpty(value)
                ? new Regex("^$", RegexOptions.Compiled)
                : new Regex(value, RegexOptions.Compiled);
        }

        private int FilterFiles(Regex filter)
        {
            try
            {
                SuspendLayout();

                var items = AllItems;
                int i = 0;
                foreach (var item in items)
                {
                    FileStatusListView.Items[i].Selected = filter.IsMatch(item.Name);
                    i++;
                }

                return FileStatusListView.SelectedIndices.Count;
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        public void SetDiffs(List<GitRevision> revisions)
        {
            NoFiles.Text = _noDiffFilesChangesDefaultText;
            switch (revisions.Count)
            {
                case 0:
                    GitItemStatuses = null;
                    break;

                case 1: // diff "parent" --> "selected revision"
                    var revision = revisions[0];

                    Revision = revision;

                    if (revision == null)
                        GitItemStatuses = null;
                    else if (revision.ParentGuids == null || revision.ParentGuids.Length == 0)
                        GitItemStatuses = Module.GetTreeFiles(revision.TreeGuid, true);
                    else
                    {
                        if (revision.Guid == GitRevision.UnstagedGuid) //working dir changes
                            GitItemStatuses = Module.GetUnstagedFilesWithSubmodulesStatus();
                        else if (revision.Guid == GitRevision.IndexGuid) //index
                            GitItemStatuses = Module.GetStagedFilesWithSubmodulesStatus();
                        else
                        {
                            GitItemsWithParents dictionary = new Dictionary<string, IList<GitItemStatus>>();
                            foreach (var parentRev in revision.ParentGuids)
                            {
                                dictionary.Add(parentRev, Module.GetDiffFilesWithSubmodulesStatus(revision.Guid, parentRev));
                            }
                            GitItemStatusesWithParents = dictionary;
                        }
                    }
                    break;

                case 2: // diff "first clicked revision" --> "second clicked revision"
                    bool artificialRevSelected = revisions[0].IsArtificial() || revisions[1].IsArtificial();
                    if (artificialRevSelected)
                    {
                        NoFiles.Text = _UnsupportedMultiselectAction.Text;
                        GitItemStatuses = null;
                    }
                    else
                        SetGitItemStatuses(revisions[1].Guid, Module.GetDiffFilesWithSubmodulesStatus(revisions[0].Guid, revisions[1].Guid));
                    break;

                default: // more than 2 revisions selected => no diff
                    NoFiles.Text = _UnsupportedMultiselectAction.Text;
                    GitItemStatuses = null;
                    break;
            }
        }
    }

}
