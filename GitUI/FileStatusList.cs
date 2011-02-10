using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;

namespace GitUI
{
    public partial class FileStatusList : GitExtensionsControl
    {
        public FileStatusList()
        {
            InitializeComponent(); Translate();

            FileStatusListBox.DrawMode = DrawMode.OwnerDrawVariable;
            FileStatusListBox.MeasureItem += new MeasureItemEventHandler(FileStatusListBox_MeasureItem);
            FileStatusListBox.DrawItem += new DrawItemEventHandler(FileStatusListBox_DrawItem);
            FileStatusListBox.SelectedIndexChanged += new EventHandler(FileStatusListBox_SelectedIndexChanged);
            FileStatusListBox.DoubleClick += new EventHandler(FileStatusListBox_DoubleClick);
            FileStatusListBox.MouseMove += new MouseEventHandler(FileStatusListBox_MouseMove);
            FileStatusListBox.Sorted = true;
            FileStatusListBox.SelectionMode = SelectionMode.MultiExtended;
            FileStatusListBox.MouseDown += new MouseEventHandler(FileStatusListBox_MouseDown);

            NoFiles.Visible = false;
            NoFiles.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);
        }

        void FileStatusListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = (int)e.Graphics.MeasureString(FileStatusListBox.Items[e.Index].ToString(), FileStatusListBox.Font).Height;
        }

        public void SetNoFilesText(string text)
        {
            NoFiles.Text = text;
        }

        void FileStatusListBox_MouseDown(object sender, MouseEventArgs e)
        {
            //SELECT
            if (e.Button == MouseButtons.Right)
            {
                Point point = new Point(e.X, e.Y);
                int hoverIndex = FileStatusListBox.IndexFromPoint(point);

                if (hoverIndex >= 0)
                {
                    if (!FileStatusListBox.GetSelected(hoverIndex))
                    {
                        while (FileStatusListBox.SelectedIndices.Count > 0)
                        {
                            FileStatusListBox.SetSelected(FileStatusListBox.SelectedIndices[0], false);
                        }

                        FileStatusListBox.SetSelected(hoverIndex, true);
                    }
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
                    // Reset the rectangle if the mouse is not over an item in the ListBox.
                    dragBoxFromMouseDown = Rectangle.Empty;
            }
        }


        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return FileStatusListBox.ContextMenuStrip;
            }
            set
            {
                FileStatusListBox.ContextMenuStrip = value;
            }
        }

        public override ContextMenu ContextMenu
        {
            get
            {
                return FileStatusListBox.ContextMenu;
            }
            set
            {
                FileStatusListBox.ContextMenu = value;
            }
        }

        private Rectangle dragBoxFromMouseDown;

        void FileStatusListBox_MouseMove(object sender, MouseEventArgs e)
        {
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
                        string fileName = GitCommands.Settings.WorkingDir + item.Name;

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
            ListBox listBox = sender as ListBox;
            if (listBox != null)
            {
                Point point = new Point(e.X, e.Y);
                int hoverIndex = listBox.IndexFromPoint(point);
                if (hoverIndex >= 0 && hoverIndex <= listBox.Items.Count)
                {
                    string text = listBox.Items[hoverIndex].ToString();

                    float fTextWidth = listBox.CreateGraphics().MeasureString(text, listBox.Font).Width;

                    //Use width-itemheight because the icon drawn in front of the text is the itemheight
                    if (fTextWidth > (FileStatusListBox.Width - FileStatusListBox.ItemHeight))
                    {
                        if (!DiffFilesTooltip.GetToolTip(listBox).Equals(text))
                            DiffFilesTooltip.SetToolTip(listBox, text);
                    }
                    else
                        DiffFilesTooltip.RemoveAll();
                }
                else
                {
                    DiffFilesTooltip.RemoveAll();
                }
            }
        }

        public IList<GitItemStatus> SelectedItems
        {
            get
            {
                IList<GitItemStatus> selectedItems = new List<GitItemStatus>();
                foreach (object selectedItem in FileStatusListBox.SelectedItems)
                {
                    selectedItems.Add((GitItemStatus)selectedItem);
                }

                return selectedItems;
            }
        }

        public GitItemStatus SelectedItem
        {
            get
            {
                return (GitItemStatus)FileStatusListBox.SelectedItem;
            }
            set
            {
                FileStatusListBox.SelectedItem = value;
            }
        }

        public event EventHandler SelectedIndexChanged;

        public new event EventHandler DoubleClick;
        public new event KeyEventHandler KeyDown;

        void FileStatusListBox_DoubleClick(object sender, EventArgs e)
        {
            if (this.DoubleClick == null)
                GitUICommands.Instance.StartFileHistoryDialog(SelectedItem.Name, Revision);
            else
                this.DoubleClick(sender, e);
        }

        void FileStatusListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(sender, e);
        }

        void FileStatusListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Bounds.Height > 0 && e.Bounds.Width > 0 && e.Index >= 0)
            {
                e.DrawBackground();
                e.DrawFocusRectangle();

                GitItemStatus gitItemStatus = (GitItemStatus)FileStatusListBox.Items[e.Index];

                if (gitItemStatus.IsDeleted)
                    e.Graphics.DrawImage(Resources.Removed, e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                else
                    if (gitItemStatus.IsNew || !gitItemStatus.IsTracked)
                        e.Graphics.DrawImage(Resources.Added, e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                    else
                        if (gitItemStatus.IsChanged)
                            e.Graphics.DrawImage(Resources.Modified, e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                        else
                            if (gitItemStatus.IsRenamed)
                                e.Graphics.DrawImage(Resources.Renamed, e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);

                string text;
                if (gitItemStatus.IsRenamed)
                    text = string.Concat(gitItemStatus.Name, " (", gitItemStatus.OldName, ")");
                else
                    text = gitItemStatus.Name;

                e.Graphics.DrawString(text, FileStatusListBox.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + e.Bounds.Height, e.Bounds.Top);
            }
        }

        public IList<GitItemStatus> GitItemStatuses
        {
            get
            {
                return FileStatusListBox.DataSource as IList<GitItemStatus>;
            }

            set
            {
                if (value == null || value.Count == 0)
                    NoFiles.Visible = true;
                else
                    NoFiles.Visible = false;

                FileStatusListBox.DataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the revision.
        /// </summary>
        /// <value>The revision.</value>
        public GitRevision Revision { get; set; }

        private void NoFiles_SizeChanged(object sender, EventArgs e)
        {
            NoFiles.Location = new Point(5, 5);
            NoFiles.Size = new Size(Size.Width - 10, Size.Height - 10);
        }

        private void FileStatusListBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    {
                        if (e.Control)
                        {
                            try
                            {
                                FileStatusListBox.SuspendLayout();
                                FileStatusListBox.ClearSelected();
                                for (int n = FileStatusListBox.Items.Count - 1; n >= 0; n--)
                                {
                                    FileStatusListBox.SetSelected(n, true);
                                }
                                e.Handled = true;
                            }
                            finally
                            {
                                FileStatusListBox.ResumeLayout();
                            }
                        }
                        break;
                    }
                default:
                    if (this.KeyDown != null)
                        this.KeyDown(sender, e);
                    break;
            }
                
        }


    }
}
