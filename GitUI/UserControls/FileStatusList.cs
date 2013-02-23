﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        private const int ImageSize = 16;

        public FileStatusList()
        {
            InitializeComponent(); Translate();
            _NoDiffFilesChangesText = NoFiles.Text;
#if !__MonoCS__ // TODO Drag'n'Drop doesn't work on Mono/Linux
            FileStatusListView.MouseMove += FileStatusListView_MouseMove;
            FileStatusListView.MouseDown += FileStatusListView_MouseDown;
#endif
            if (_images == null)
            {
                _images = new ImageList();
                _images.Images.Add(Resources.Removed);
                _images.Images.Add(Resources.Added);
                _images.Images.Add(Resources.Modified);
                _images.Images.Add(Resources.Renamed);
                _images.Images.Add(Resources.Copied);
                _images.Images.Add(Resources.IconSubmoduleDirty);
                _images.Images.Add(Resources.IconSubmoduleRevisionUp);
                _images.Images.Add(Resources.IconSubmoduleRevisionUpDirty);
                _images.Images.Add(Resources.IconSubmoduleRevisionDown);
                _images.Images.Add(Resources.IconSubmoduleRevisionDownDirty);
            }
            FileStatusListView.SmallImageList = _images;
            FileStatusListView.LargeImageList = _images;

            NoFiles.Visible = false;
            NoFiles.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);
        }

        private static ImageList _images;

        private string _NoDiffFilesChangesText;

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

        void FileStatusListView_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            var gitItemStatus = (GitItemStatus)FileStatusListView.Items[e.Index].Tag;

            e.ItemHeight = Math.Max((int)e.Graphics.MeasureString(gitItemStatus.Name, FileStatusListView.Font).Height, ImageSize);
            //Do NOT set e.ItemWidth because it will crash in MONO
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

            var image = e.Item.ImageList.Images[e.Item.ImageIndex];

            if (image != null)
                e.Graphics.DrawImage(image, e.Bounds.Left, centeredImageTop, ImageSize, ImageSize);

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

                if (hover.Item != null)
                {
                    ClearSelected();

                    hover.Item.Selected = true;
                }
            }

            //DRAG
            if (e.Button == MouseButtons.Left)
            {
                if (SelectedItems.Count > 0)
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
            ListView listBox = sender as ListView;

            //DRAG
            // If the mouse moves outside the rectangle, start the drag.
            if (dragBoxFromMouseDown != Rectangle.Empty &&
                !dragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                if (SelectedItems.Count > 0)
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
            if (listBox != null)
            {
                var point = new Point(e.X, e.Y);
                var hover = listBox.HitTest(point);
                if (hover.Item != null)
                {
                    GitItemStatus gitItemStatus = (GitItemStatus)hover.Item.Tag;

                    string text;
                    if (gitItemStatus.IsRenamed || gitItemStatus.IsCopied)
                        text = string.Concat(gitItemStatus.Name, " (", gitItemStatus.OldName, ")");
                    else
                        text = gitItemStatus.Name;

                    float fTextWidth = listBox.CreateGraphics().MeasureString(text, listBox.Font).Width + 17;

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
        public IList<GitItemStatus> AllItems
        {
            get
            {
                return (FileStatusListView.Items.Cast<ListViewItem>().
                    Select(selectedItem => (GitItemStatus) selectedItem.Tag)).ToList();
            }
        }

        [Browsable(false)]
        public IList<GitItemStatus> SelectedItems
        {
            get
            {
                return FileStatusListView.SelectedItems.Cast<ListViewItem>().
                    Select(i => (GitItemStatus)i.Tag).ToList();
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

        public void SelectStoredNextIndex()
        {
            _nextIndexToSelect = Math.Min(_nextIndexToSelect, FileStatusListView.Items.Count - 1);
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
                if (!gitItemStatus.IsSubmodule || gitItemStatus.SubmoduleStatus == null)
                    return 2;

                var status = gitItemStatus.SubmoduleStatus;
                if (status.IsDirty && status.Commit == status.OldCommit)
                    return 5;
                if (status.Status == SubmoduleStatus.FastForward || status.Status == SubmoduleStatus.NewerTime)
                    return 6 + (status.IsDirty ? 1 : 0);
                if (status.Status == SubmoduleStatus.Rewind || status.Status == SubmoduleStatus.OlderTime)
                    return 8 + (status.IsDirty ? 1 : 0);
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

                //FileStatusListView.HorizontalExtent = 0;
                int prevSelectedIndex = SelectedIndex;
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
                        group = new ListViewGroup("Diff with parent " + shortHash);
                        group.Tag = pair.Key;
                        FileStatusListView.Groups.Add(group);
                    }
                    foreach (var item in pair.Value)
                    {
                        var listItem = new ListViewItem(item.Name, group);
                        listItem.ImageIndex = GetItemImageIndex(item);
                        listItem.Tag = item;
                        list.Add(listItem);
                    }
                }
                FileStatusListView.Items.AddRange(list.ToArray());
                FileStatusListView.EndUpdate();
                FileStatusListView.SetGroupState(ListViewGroupState.Collapsible);
                if (DataSourceChanged != null)
                    DataSourceChanged(this, new EventArgs());
                if (!value.Any() && prevSelectedIndex >= 0)
                {
                    //bug in the ListView control where supplying an empty list will not trigger a SelectedIndexChanged event, so we force it to trigger
                    FileStatusListView_SelectedIndexChanged(this, EventArgs.Empty);
                }
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

                var items = FileStatusListView.Items.Cast<GitItemStatus>().ToList();
                for (var i = 0; i < items.Count; i++)
                {
                    FileStatusListView.Items[i].Selected = filter.IsMatch(items[i].Name);
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
            NoFiles.Text = _NoDiffFilesChangesText;
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
