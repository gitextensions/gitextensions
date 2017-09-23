using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Repository;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class DashboardEditor : GitExtensionsControl
    {
        public DashboardEditor()
        {
            InitializeComponent();
            Translate();
            Initialize();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory)_NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private bool bChangingDataSource;

        public void Initialize()
        {
            bChangingDataSource = true;
            _NO_TRANSLATE_Categories.DataSource = null;
            _NO_TRANSLATE_Categories.DataSource = Repositories.RepositoryCategories;
            bChangingDataSource = false;
            _NO_TRANSLATE_Categories.DisplayMember = "Description";
        }

        private void Categories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
            {
                if (!bChangingDataSource)
                    splitContainer1.Panel2.Enabled = false;
                return;
            }

            if (!bChangingDataSource)
                splitContainer1.Panel2.Enabled = true;
            var repositoryCategory = (RepositoryCategory)_NO_TRANSLATE_Categories.SelectedItem;
            RepositoriesGrid.DataSource = repositoryCategory.Repositories;

            _NO_TRANSLATE_Caption.Text = repositoryCategory.Description;

            RssFeedType_CheckedChanged(this, null);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            var category = new RepositoryCategory("new");
            Repositories.RepositoryCategories.Add(category);
            Categories_SelectedIndexChanged(null, null);
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            Repositories.RepositoryCategories.Remove((RepositoryCategory)_NO_TRANSLATE_Categories.SelectedItem);
            Categories_SelectedIndexChanged(null, null);
        }

        private void Caption_TextChanged(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory)_NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.Description = _NO_TRANSLATE_Caption.Text;
            Initialize();
        }

        private void RssFeedType_CheckedChanged(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory)_NO_TRANSLATE_Categories.SelectedItem;

            RepositoriesGrid.ReadOnly = false;
            RepositoriesGrid.Enabled = true;
            RepositoriesGrid.BackgroundColor = Color.White;

            repositoryCategory.SetIcon();
        }

        private void Caption_Validating(object sender, CancelEventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory)_NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RepositoriesGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory)_NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RepositoriesGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            //We handle this ourself, let .net think the action is cancelled.
            e.Cancel = true;
            if (e.Row.DataBoundItem == null)
                return;

            var datasource = ((BindingList<Repository>)((DataGridView)sender).DataSource);
            var repositoryToRemove = (Repository)e.Row.DataBoundItem;

            RepositoriesGrid.DataSource = null;
            datasource.Remove(repositoryToRemove);
            RepositoriesGrid.DataSource = datasource;
        }
    }
}