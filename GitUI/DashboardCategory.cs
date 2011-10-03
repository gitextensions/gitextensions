﻿using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Repository;

namespace GitUI
{
    public partial class DashboardCategory : GitExtensionsControl
    {
        private RepositoryCategory m_repositoryCategory;

        public DashboardCategory()
        {
            InitializeComponent();

            // Do this here, so that at design time, the form will keep its size.
            this.flowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            SetUpFonts();
            Translate();
        }

        public DashboardCategory(string title, RepositoryCategory repositoryCategory)
            : this()
        {
            Title = title;
            RepositoryCategory = repositoryCategory;
        }

        public RepositoryCategory RepositoryCategory
        {
            get { return m_repositoryCategory; }
            set
            {
                m_repositoryCategory = value;

                if (m_repositoryCategory != null && m_repositoryCategory.CategoryType == RepositoryCategoryType.RssFeed)
                    m_repositoryCategory.DownloadRssFeed();

                InitRepositoryCategory();
            }
        }

        public string Title
        {
            get { return _NO_TRANSLATE_Caption.Text; }
            set { _NO_TRANSLATE_Caption.Text = value; }
        }

        public void DisableContextMenu()
        {
            _NO_TRANSLATE_Caption.ContextMenuStrip = null;
        }

        private void SetUpFonts()
        {
            _NO_TRANSLATE_Caption.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 10, FontStyle.Bold,
                                                  GraphicsUnit.Point);
        }

        private void InitRepositoryCategory()
        {
            if (m_repositoryCategory != null)
            {
                foreach (Repository repository in m_repositoryCategory.Repositories)
                {
                    var dashboardItem = new DashboardItem(repository);
                    dashboardItem.Click += dashboardItem_Click;
                    AddItem(dashboardItem);
                    
                    if (m_repositoryCategory.CategoryType == RepositoryCategoryType.Repositories)
                    {
                        var contextMenu = new ContextMenuStrip();
                        var moveToMenuItem = new ToolStripMenuItem("Move To Category", null,
                                                                   new ToolStripMenuItem("moveto")) {Tag = repository};
                        moveToMenuItem.DropDownOpening += moveToMenuItem_DropDownOpening;
                        contextMenu.Items.Add(moveToMenuItem);
                        var moveUpMenuItem = new ToolStripMenuItem("Move Up") {Tag = repository};
                        moveUpMenuItem.Click += moveUpMenuItem_Click;
                        contextMenu.Items.Add(moveUpMenuItem);
                        var moveDownMenuItem = new ToolStripMenuItem("Move Down") {Tag = repository};
                        moveDownMenuItem.Click += moveDownMenuItem_Click;
                        contextMenu.Items.Add(moveDownMenuItem);
                        var removeMenuItem = new ToolStripMenuItem("Remove") {Tag = repository};
                        removeMenuItem.Click += removeMenuItem_Click;
                        contextMenu.Items.Add(removeMenuItem);
                        var editMenuItem = new ToolStripMenuItem("Edit");
                        editMenuItem.Click += editMenuItem_Click;
                        contextMenu.Items.Add(editMenuItem);


                        var showCurrentBranchMenuItem = new ToolStripMenuItem("Show curren branch");
                        showCurrentBranchMenuItem.Click += new EventHandler(showCurrentBranchMenuItem_Click);
                        showCurrentBranchMenuItem.Checked = GitCommands.Settings.DashboardShowCurrentBranch;
                        contextMenu.Items.Add(showCurrentBranchMenuItem);

                        dashboardItem.ContextMenuStrip = contextMenu;
                    }
                }
            }
        }

        void showCurrentBranchMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.Settings.DashboardShowCurrentBranch = !GitCommands.Settings.DashboardShowCurrentBranch;
            dashboardCategoryChanged(null, null);
        }

        private void moveUpMenuItem_Click(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            var repository = toolStripItem.Tag as Repository;

            if (repository == null)
                return;

            int index = RepositoryCategory.Repositories.IndexOf(repository);
            RepositoryCategory.Repositories.Remove(repository);
            RepositoryCategory.Repositories.Insert(Math.Max(index - 1, 0), repository);
            Recalculate();
        }

        private void moveDownMenuItem_Click(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            var repository = toolStripItem.Tag as Repository;

            if (repository == null)
                return;

            int index = RepositoryCategory.Repositories.IndexOf(repository);
            RepositoryCategory.Repositories.Remove(repository);
            RepositoryCategory.Repositories.Insert(Math.Min(index + 1, RepositoryCategory.Repositories.Count),
                                                   repository);
            Recalculate();
        }

        private void editMenuItem_Click(object sender, EventArgs e)
        {
            new FormDashboardEditor().ShowDialog();
            dashboardCategoryChanged(this, null);
        }

        public void Recalculate()
        {
            Title = RepositoryCategory.Description;
            Clear();
            InitRepositoryCategory();
        }

        private void removeMenuItem_Click(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            var repository = toolStripItem.Tag as Repository;

            if (repository == null)
                return;

            RepositoryCategory.RemoveRepository(repository);
            repositoryRemoved(repository);
            dashboardCategoryChanged(this, null);
        }

        private void moveToMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var moveToMenuItem = (ToolStripMenuItem) sender;

            moveToMenuItem.DropDownItems.Clear();

            foreach (RepositoryCategory repositoryCategory in Repositories.RepositoryCategories)
            {
                if (repositoryCategory.CategoryType == RepositoryCategoryType.Repositories)
                {
                    ToolStripItem addToItem = moveToMenuItem.DropDownItems.Add(repositoryCategory.Description);
                    addToItem.Tag = moveToMenuItem.Tag;
                    addToItem.Click += addToItem_Click;
                }
            }

            if (moveToMenuItem.DropDownItems.Count > 0)
                moveToMenuItem.DropDownItems.Add(new ToolStripSeparator());

            var newCategoryMenuItem = new ToolStripMenuItem("New category") {Tag = moveToMenuItem.Tag};
            newCategoryMenuItem.Click += newCategoryMenuItem_Click;
            moveToMenuItem.DropDownItems.Add(newCategoryMenuItem);
        }

        private void newCategoryMenuItem_Click(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            var repository = toolStripItem.Tag as Repository;

            if (repository == null)
                return;

            var formDashboardCategoryTitle = new FormDashboardCategoryTitle();
            formDashboardCategoryTitle.ShowDialog();

            if (string.IsNullOrEmpty(formDashboardCategoryTitle.GetTitle()))
                return;

            var newRepositoryCategory = new RepositoryCategory(formDashboardCategoryTitle.GetTitle());

            RepositoryCategory.RemoveRepository(repository);
            repository.RepositoryType = RepositoryType.Repository;
            newRepositoryCategory.AddRepository(repository);

            Repositories.RepositoryCategories.Add(newRepositoryCategory);

            dashboardCategoryChanged(this, null);
        }

        public event EventHandler DashboardItemClick;

        private void dashboardItem_Click(object sender, EventArgs e)
        {
            if (DashboardItemClick != null)
                DashboardItemClick(sender, e);
        }

        public event EventHandler DashboardCategoryChanged;

        private void dashboardCategoryChanged(object sender, EventArgs e)
        {
            if (DashboardCategoryChanged != null)
                DashboardCategoryChanged(sender, e);
        }

        public delegate void RepositoryRemovedHandler(Repository repository);

        public event RepositoryRemovedHandler RepositoryRemoved;

        private void repositoryRemoved(Repository repository)
        {
            if (RepositoryRemoved != null)
                RepositoryRemoved(repository);
        }


        public void AddItem(DashboardItem dashboardItem)
        {
            dashboardItem.Margin = new Padding(10, 0, 0, 0);
            this.flowLayoutPanel.Controls.Add(dashboardItem);
        }

        public void Clear()
        {
            var items = (from DashboardItem i in this.flowLayoutPanel.Controls
                         select i).ToList();

            this.flowLayoutPanel.Controls.Clear();

            foreach (var item in items)
            {
                item.Click -= dashboardItem_Click;
                item.Close();
            }
        }

        private void addToItem_Click(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            var repository = toolStripItem.Tag as Repository;

            if (repository == null)
                return;

            foreach (RepositoryCategory newRepositoryCategory in Repositories.RepositoryCategories)
            {
                if (newRepositoryCategory.Description.Equals(toolStripItem.Text))
                {
                    RepositoryCategory.RemoveRepository(repository);
                    repository.RepositoryType = RepositoryType.Repository;
                    newRepositoryCategory.AddRepository(repository);
                }
            }

            dashboardCategoryChanged(this, null);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormDashboardEditor().ShowDialog();
            dashboardCategoryChanged(this, null);
        }

        private void removeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Repositories.RepositoryCategories.Remove(RepositoryCategory);
            dashboardCategoryChanged(this, null);
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = Repositories.RepositoryCategories.IndexOf(RepositoryCategory);
            Repositories.RepositoryCategories.Remove(RepositoryCategory);
            Repositories.RepositoryCategories.Insert(Math.Max(index - 1, 0), RepositoryCategory);
            dashboardCategoryChanged(this, null);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = Repositories.RepositoryCategories.IndexOf(RepositoryCategory);
            Repositories.RepositoryCategories.Remove(RepositoryCategory);
            Repositories.RepositoryCategories.Insert(Math.Min(index + 1, Repositories.RepositoryCategories.Count),
                                                     RepositoryCategory);
            dashboardCategoryChanged(this, null);
        }
    }
}
