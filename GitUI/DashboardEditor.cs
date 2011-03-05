using System;
using System.ComponentModel;
using System.Windows.Forms;
using GitCommands.Repository;
using System.Drawing;

namespace GitUI
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

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        public void Initialize()
        {
            _NO_TRANSLATE_Categories.DataSource = null;
            _NO_TRANSLATE_Categories.DataSource = Repositories.RepositoryCategories;
            _NO_TRANSLATE_Categories.DisplayMember = "Description";
        }

        private void Categories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            RepositoriesGrid.DataSource = repositoryCategory.Repositories;

            _NO_TRANSLATE_Caption.Text = repositoryCategory.Description;
            RssFeedType.Checked = repositoryCategory.CategoryType == RepositoryCategoryType.RssFeed;
            RepositoriesType.Checked = !RssFeedType.Checked;
            _NO_TRANSLATE_RssFeed.Text = repositoryCategory.RssFeedUrl;

            RssFeedType_CheckedChanged(this, null);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            Repositories.RepositoryCategories.Add(new RepositoryCategory("new"));
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            Repositories.RepositoryCategories.Remove((RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem);
        }

        private void Caption_TextChanged(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.Description = _NO_TRANSLATE_Caption.Text;
            Initialize();
        }

        private void RssFeedType_CheckedChanged(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;


            if (RssFeedType.Checked)
            {
                RepositoriesGrid.ReadOnly = true;
                RepositoriesGrid.Enabled = false;
                RepositoriesGrid.BackgroundColor = Color.Gray;
                _NO_TRANSLATE_RssFeed.Enabled = true;
                repositoryCategory.CategoryType = RepositoryCategoryType.RssFeed;
            }

            if (RepositoriesType.Checked)
            {
                RepositoriesGrid.ReadOnly = false;
                RepositoriesGrid.Enabled = true;
                RepositoriesGrid.BackgroundColor = Color.White;
                _NO_TRANSLATE_RssFeed.Enabled = false;
                repositoryCategory.CategoryType = RepositoryCategoryType.Repositories;
            }

            repositoryCategory.SetIcon();
        }

        private void RssFeed_TextChanged(object sender, EventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.RssFeedUrl = _NO_TRANSLATE_RssFeed.Text;
        }

        private void RssFeed_Validating(object sender, CancelEventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            if (repositoryCategory.CategoryType == RepositoryCategoryType.RssFeed)
                repositoryCategory.DownloadRssFeed();
        }

        private void Caption_Validating(object sender, CancelEventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RssFeedType_Validating(object sender, CancelEventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RepositoriesType_Validating(object sender, CancelEventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RepositoriesGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_NO_TRANSLATE_Categories.SelectedItem == null)
                return;

            var repositoryCategory = (RepositoryCategory) _NO_TRANSLATE_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RepositoriesGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            //We handle this ourself, let .net think the action is cancelled.
            e.Cancel = true;
            if (e.Row.DataBoundItem == null)
                return;

            var datasource = ((BindingList<Repository>) ((DataGridView) sender).DataSource);
            var repositoryToRemove = (Repository) e.Row.DataBoundItem;

            RepositoriesGrid.DataSource = null;
            datasource.Remove(repositoryToRemove);
            RepositoriesGrid.DataSource = datasource;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void RepositoriesType_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}