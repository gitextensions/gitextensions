using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class DashboardCategory : UserControl
    {


        public DashboardCategory()
        {
            InitializeComponent();
        }

        public DashboardCategory(string title, RepositoryCategory repositoryCategory)
        {
            InitializeComponent();

            this.Title = title;
            this.RepositoryCategory = repositoryCategory;
        }

        private RepositoryCategory repositoryCategory;
        public RepositoryCategory RepositoryCategory
        {
            get
            {
                return repositoryCategory;
            }
            set
            {
                repositoryCategory = value;

                InitRepositoryCategory();
            }
        }

        private void InitRepositoryCategory()
        {
            if (repositoryCategory != null)
            {
                if (repositoryCategory.CategoryType == RepositoryCategoryType.RssFeed)
                    repositoryCategory.DownloadRssFeed();

                foreach (Repository repository in repositoryCategory.Repositories)
                {
                    DashboardItem dashboardItem = new DashboardItem(repository);
                    dashboardItem.Click += new EventHandler(dashboardItem_Click);
                    AddItem(dashboardItem);

                    if (repositoryCategory.CategoryType == RepositoryCategoryType.Repositories)
                    {
                        dashboardItem.ContextMenuStrip = RecentRepositoriesContextMenu;
                    }
                }
            }
        }

        public event EventHandler DashboardItemClick;


        void dashboardItem_Click(object sender, EventArgs e)
        {
            if (DashboardItemClick != null)
                DashboardItemClick(sender, e);
        }

        public string Title
        {
            get
            {
                return Caption.Text;
            }
            set
            {
                Caption.Text = value;
            }
        }

        int top = 26;

        public void AddItem(DashboardItem dashboardItem)
        {
            dashboardItem.Location = new Point(10, top);
            dashboardItem.Width = this.Width - 13;
            this.Controls.Add(dashboardItem);
            top += dashboardItem.Height;
            this.Height = top;
        }

        public void Clear()
        {
            for (int i = Controls.Count; i > 0; i--)
            {
                if (Controls[i - 1] is DashboardItem)
                    Controls.RemoveAt(i - 1);
            }
            top = 26;
        }

        private void DashboardCategory_SizeChanged(object sender, EventArgs e)
        {
            for (int i = Controls.Count; i > 0; i--)
            {
                if (Controls[i - 1] is DashboardItem)
                    Controls[i - 1].Width = Width - 13;
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            RepositoryCategory.RemoveRepository(toolStripItem.Text);

            InitRepositoryCategory();
        }

        private void addToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            addToToolStripMenuItem.DropDownItems.Clear();

            foreach(RepositoryCategory repositoryCategory in Repositories.RepositoryCategories)
            {
                if (repositoryCategory.CategoryType == RepositoryCategoryType.Repositories)
                {
                    ToolStripItem addToItem = addToToolStripMenuItem.DropDownItems.Add(repositoryCategory.Description);
                    addToItem.Click += new EventHandler(addToItem_Click);
                }
            }
        }

        void addToItem_Click(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            foreach(RepositoryCategory newRepositoryCategory in Repositories.RepositoryCategories)
            {
                if (newRepositoryCategory.Description.Equals(toolStripItem.Text))
                {
                    Repository repositoryToMove = RepositoryCategory.FindRepository(toolStripItem.Text);
                    if (repositoryToMove != null)
                    {
                        RepositoryCategory.RemoveRepository(toolStripItem.Text);
                        newRepositoryCategory.AddRepository(repositoryToMove);
                    }
                }
            }
        }
    }
}
