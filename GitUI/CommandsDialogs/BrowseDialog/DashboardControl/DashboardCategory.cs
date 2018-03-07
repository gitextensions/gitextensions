using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Repository;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    [DefaultEvent("DashboardItemClick")]
    public partial class DashboardCategory : GitExtensionsControl
    {
        #region Translation
        private readonly TranslationString _moveToCategory =
            new TranslationString("Move to category");
        private readonly TranslationString _moveCategoryUp =
            new TranslationString("Move up");
        private readonly TranslationString _moveCategoryDown =
            new TranslationString("Move down");
        private readonly TranslationString _removeCategory =
            new TranslationString("Remove");
        private readonly TranslationString _editCategory =
            new TranslationString("Edit");
        private readonly TranslationString _newCategory =
            new TranslationString("New category");
        #endregion

        private RepositoryCategory _repositoryCategory;

        public DashboardCategory()
        {
            InitializeComponent();

            // Do this here, so that at design time, the form will keep its size.
            flowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

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
            get { return _repositoryCategory; }
            set
            {
                _repositoryCategory = value;
                InitRepositoryCategory();
            }
        }

        [Category("Appearance")]
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
            if (_repositoryCategory == null)
            {
                return;
            }

            var contextMenu = new ContextMenuStrip();

            contextMenu.Opening += contextMenu_Opening;
            var moveToMenuItem = new ToolStripMenuItem(_moveToCategory.Text, null,
                                                       new ToolStripMenuItem("moveto"));
            moveToMenuItem.DropDownOpening += moveToMenuItem_DropDownOpening;
            contextMenu.Items.Add(moveToMenuItem);
            var moveUpMenuItem = new ToolStripMenuItem(_moveCategoryUp.Text);
            moveUpMenuItem.Click += moveUpMenuItem_Click;
            contextMenu.Items.Add(moveUpMenuItem);
            var moveDownMenuItem = new ToolStripMenuItem(_moveCategoryDown.Text);
            moveDownMenuItem.Click += moveDownMenuItem_Click;
            contextMenu.Items.Add(moveDownMenuItem);
            var removeMenuItem = new ToolStripMenuItem(_removeCategory.Text);
            removeMenuItem.Click += removeMenuItem_Click;
            contextMenu.Items.Add(removeMenuItem);
            var editMenuItem = new ToolStripMenuItem(_editCategory.Text);
            editMenuItem.Click += editMenuItem_Click;
            contextMenu.Items.Add(editMenuItem);

            SuspendLayout();
            flowLayoutPanel.SuspendLayout();

            foreach (Repository repository in _repositoryCategory.Repositories)
            {
                var dashboardItem = new DashboardItem(repository);
                dashboardItem.Click += dashboardItem_Click;
                dashboardItem.Tag = repository;

                dashboardItem.ContextMenuStrip = contextMenu;
                AddItem(dashboardItem);
            }

            flowLayoutPanel.ResumeLayout();
            ResumeLayout();
        }

        private Repository _repository;

        private void contextMenu_Opening(object sender, EventArgs e)
        {
            _repository = (Repository)((ContextMenuStrip)sender).SourceControl.Tag;
        }

        private void MoveItem(bool moveUp)
        {
            if (_repository == null)
            {
                return;
            }

            int index = RepositoryCategory.Repositories.IndexOf(_repository);
            RepositoryCategory.Repositories.Remove(_repository);
            int newIndex = moveUp ? Math.Max(index - 1, 0) :
                Math.Min(index + 1, RepositoryCategory.Repositories.Count);
            RepositoryCategory.Repositories.Insert(newIndex, _repository);
            Recalculate();
        }

        private void moveUpMenuItem_Click(object sender, EventArgs e)
        {
            MoveItem(true);
        }

        private void moveDownMenuItem_Click(object sender, EventArgs e)
        {
            MoveItem(false);
        }

        private void editMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new FormDashboardEditor())
            {
                frm.ShowDialog(this);
            }

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
            if (_repository == null)
            {
                return;
            }

            RepositoryCategory.RemoveRepository(_repository);
            repositoryRemoved(_repository);
            dashboardCategoryChanged(this, null);
        }

        private void moveToMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var moveToMenuItem = (ToolStripMenuItem)sender;

            moveToMenuItem.DropDownItems.Clear();

            foreach (RepositoryCategory repositoryCategory in Repositories.RepositoryCategories)
            {
                ToolStripItem addToItem = moveToMenuItem.DropDownItems.Add(repositoryCategory.Description);
                addToItem.Click += addToItem_Click;
            }

            if (moveToMenuItem.DropDownItems.Count > 0)
            {
                moveToMenuItem.DropDownItems.Add(new ToolStripSeparator());
            }

            var newCategoryMenuItem = new ToolStripMenuItem(_newCategory.Text);
            newCategoryMenuItem.Click += newCategoryMenuItem_Click;
            moveToMenuItem.DropDownItems.Add(newCategoryMenuItem);
        }

        private void newCategoryMenuItem_Click(object sender, EventArgs e)
        {
            if (_repository == null)
            {
                return;
            }

            RepositoryCategory newRepositoryCategory;

            using (var formDashboardCategoryTitle = new FormDashboardCategoryTitle())
            {
                formDashboardCategoryTitle.ShowDialog(this);

                if (string.IsNullOrEmpty(formDashboardCategoryTitle.GetTitle()))
                {
                    return;
                }

                newRepositoryCategory = new RepositoryCategory(formDashboardCategoryTitle.GetTitle());
            }

            RepositoryCategory.RemoveRepository(_repository);
            _repository.RepositoryType = RepositoryType.Repository;
            newRepositoryCategory.AddRepository(_repository);

            Repositories.RepositoryCategories.Add(newRepositoryCategory);

            dashboardCategoryChanged(this, null);
        }

        [Category("Action")]
        public event EventHandler DashboardItemClick;

        private void dashboardItem_Click(object sender, EventArgs e)
        {
            DashboardItemClick?.Invoke(sender, e);
        }

        public event EventHandler DashboardCategoryChanged;

        private void dashboardCategoryChanged(object sender, EventArgs e)
        {
            DashboardCategoryChanged?.Invoke(sender, e);
        }

        public class RepositoryEventArgs : EventArgs
        {
            public RepositoryEventArgs(Repository repository)
            {
                Repository = repository;
            }

            public Repository Repository { get; }
        }

        public event EventHandler<RepositoryEventArgs> RepositoryRemoved;

        private void repositoryRemoved(Repository repository)
        {
            RepositoryRemoved?.Invoke(this, new RepositoryEventArgs(repository));
        }

        public void AddItem(DashboardItem dashboardItem)
        {
            dashboardItem.Margin = new Padding(10, 0, 0, 0);
            flowLayoutPanel.Controls.Add(dashboardItem);
        }

        public void Clear()
        {
            var items = (from DashboardItem i in flowLayoutPanel.Controls
                         select i).ToList();

            flowLayoutPanel.Controls.Clear();

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
            {
                return;
            }

            if (_repository == null)
            {
                return;
            }

            foreach (RepositoryCategory newRepositoryCategory in Repositories.RepositoryCategories)
            {
                if (newRepositoryCategory.Description.Equals(toolStripItem.Text))
                {
                    RepositoryCategory.RemoveRepository(_repository);
                    _repository.RepositoryType = RepositoryType.Repository;
                    newRepositoryCategory.AddRepository(_repository);
                }
            }

            dashboardCategoryChanged(this, null);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new FormDashboardEditor())
            {
                frm.ShowDialog(this);
            }

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
