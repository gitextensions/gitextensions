using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;

namespace GitUI
{
    public partial class FileStatusList : UserControl
    {
        public FileStatusList()
        {
            InitializeComponent();

            FileStatusListBox.DrawMode = DrawMode.OwnerDrawFixed;
            FileStatusListBox.DrawItem += new DrawItemEventHandler(FileStatusListBox_DrawItem);
            FileStatusListBox.SelectedIndexChanged += new EventHandler(FileStatusListBox_SelectedIndexChanged);
            FileStatusListBox.DoubleClick += new EventHandler(FileStatusListBox_DoubleClick);
            FileStatusListBox.MouseMove += new MouseEventHandler(FileStatusListBox_MouseMove);
            FileStatusListBox.Sorted = true;
            FileStatusListBox.SelectionMode = SelectionMode.MultiExtended;
            FileStatusListBox.MouseDown += new MouseEventHandler(FileStatusListBox_MouseDown);
        }

        void FileStatusListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point point = new Point(e.X, e.Y);
                int hoverIndex = FileStatusListBox.IndexFromPoint(point);

                if (hoverIndex >= 0)
                {

                    foreach (int selectionIndex in FileStatusListBox.SelectedIndices)
                    {
                        FileStatusListBox.SetSelected(selectionIndex, false);
                    }

                    FileStatusListBox.SetSelected(hoverIndex, true);
                }
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

        void FileStatusListBox_MouseMove(object sender, MouseEventArgs e)
        {
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

        void FileStatusListBox_DoubleClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartFileHistoryDialog(SelectedItem.Name);
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

                if (gitItemStatus.IsChanged)
                    e.Graphics.DrawImage(Resources.Modified, e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                else
                    if (gitItemStatus.IsDeleted)
                        e.Graphics.DrawImage(Resources.Removed, e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                    else
                        if (gitItemStatus.IsNew)
                            e.Graphics.DrawImage(Resources.Added, e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                
                e.Graphics.DrawString(gitItemStatus.Name, FileStatusListBox.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + e.Bounds.Height, e.Bounds.Top);
            }
        }

        public IList<GitItemStatus> GitItemStatusses 
        {
            get
            {
                return FileStatusListBox.DataSource as IList<GitItemStatus>;
            }

            set
            {
                FileStatusListBox.DataSource = value;
            }
        }


    }
}
