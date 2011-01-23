using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Repository;

namespace GitUI
{
    public partial class DashboardCategory : GitExtensionsControl
    {
        private RepositoryCategory m_repositoryCategory;
        private int top = 26;

        public DashboardCategory()
        {
            InitializeComponent();
            SetUpFonts();
            Translate();
        }

        public DashboardCategory(string title, RepositoryCategory repositoryCategory)
        {
            InitializeComponent();
            SetUpFonts();
            Translate();

            Title = title;
            RepositoryCategory = repositoryCategory;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
                Height = top = 26;
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

                        dashboardItem.ContextMenuStrip = contextMenu;
                    }
                }
            }
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


        public void AddItem(DashboardItem dashboardItem)
        {
            dashboardItem.Location = new Point(10, top);
            dashboardItem.Width = Width - 13;
            Controls.Add(dashboardItem);
            top += dashboardItem.Height;
            Height = top;
        }

        public void Clear()
        {
            for (int i = Controls.Count; i > 0; i--)
            {
                var dashboardItem = Controls[i - 1] as DashboardItem;
                if (dashboardItem != null)
                {
                    dashboardItem.Click -= dashboardItem_Click;
                    Controls.RemoveAt(i - 1);
                    dashboardItem.Dispose();
                }
            }
            top = 26;
            Height = top;
        }

        private void DashboardCategory_SizeChanged(object sender, EventArgs e)
        {
            for (int i = Controls.Count; i > 0; i--)
            {
                if (Controls[i - 1] is DashboardItem)
                    Controls[i - 1].Width = Width - 13;
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