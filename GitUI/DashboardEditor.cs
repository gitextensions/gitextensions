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
    public partial class DashboardEditor : GitExtensionsControl
    {
        public DashboardEditor()
        {
            InitializeComponent(); Translate();
            Initialize();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        public void Initialize()
        {
            _Categories.DataSource = null;
            _Categories.DataSource = Repositories.RepositoryCategories;
            _Categories.DisplayMember = "Description";
        }

        private void Categories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            RepositoriesGrid.DataSource = repositoryCategory.Repositories;

            _Caption.Text = repositoryCategory.Description;
            RssFeedType.Checked = repositoryCategory.CategoryType == RepositoryCategoryType.RssFeed;
            RepositoriesType.Checked = !RssFeedType.Checked;
            _RssFeed.Text = repositoryCategory.RssFeedUrl;

            RssFeedType_CheckedChanged(this, null);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            Repositories.RepositoryCategories.Add(new RepositoryCategory("new"));
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            Repositories.RepositoryCategories.Remove((RepositoryCategory)_Categories.SelectedItem);
        }

        private void Caption_TextChanged(object sender, EventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            repositoryCategory.Description = _Caption.Text;
            Initialize();
        }

        private void RssFeedType_CheckedChanged(object sender, EventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return; 
            
            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;


            if (RssFeedType.Checked)
            {
                RepositoriesGrid.ReadOnly = true;
                RepositoriesGrid.Enabled = false;
                _RssFeed.Enabled = true;
                repositoryCategory.CategoryType = RepositoryCategoryType.RssFeed;
            }

            if (RepositoriesType.Checked)
            {
                RepositoriesGrid.ReadOnly = false;
                RepositoriesGrid.Enabled = true;
                _RssFeed.Enabled = false;
                repositoryCategory.CategoryType = RepositoryCategoryType.Repositories;
            }

            repositoryCategory.SetIcon();            
        }

        private void RssFeed_TextChanged(object sender, EventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return; 
            
            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            repositoryCategory.RssFeedUrl = _RssFeed.Text;
        }

        private void RssFeed_Validating(object sender, CancelEventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            if (repositoryCategory.CategoryType == RepositoryCategoryType.RssFeed)
                repositoryCategory.DownloadRssFeed();
        }

        private void Caption_Validating(object sender, CancelEventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RssFeedType_Validating(object sender, CancelEventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RepositoriesType_Validating(object sender, CancelEventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

        private void RepositoriesGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)_Categories.SelectedItem;
            repositoryCategory.SetIcon();
        }

    }
}
