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
    public partial class DashboardEditor : UserControl
    {
        System.ComponentModel.ComponentResourceManager resources;

        public DashboardEditor()
        {
            resources = new ComponentResourceManager(typeof(FormBrowse));

            InitializeComponent();
            Initialize();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            repositoryCategory.OnListChanged(null, null);
        }

        public void Initialize()
        {
            Categories.DataSource = null;
            Categories.DataSource = Repositories.RepositoryCategories;
            Categories.DisplayMember = "Description";
        }

        private void Categories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            RepositoriesGrid.DataSource = repositoryCategory.Repositories;

            Caption.Text = repositoryCategory.Description;
            RssFeedType.Checked = repositoryCategory.CategoryType == RepositoryCategoryType.RssFeed;
            RepositoriesType.Checked = !RssFeedType.Checked;
            RssFeed.Text = repositoryCategory.RssFeedUrl;

            RssFeedType_CheckedChanged(this, null);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            Repositories.RepositoryCategories.Add(new RepositoryCategory("new"));
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;

            Repositories.RepositoryCategories.Remove((RepositoryCategory)Categories.SelectedItem);
        }

        private void Caption_TextChanged(object sender, EventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            repositoryCategory.Description = Caption.Text;
            Initialize();
        }

        private void RssFeedType_CheckedChanged(object sender, EventArgs e)
        {
            if (Categories.SelectedItem == null)
                return; 
            
            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;


            if (RssFeedType.Checked)
            {
                RepositoriesGrid.ReadOnly = true;
                RepositoriesGrid.Enabled = false;
                RssFeed.Enabled = true;
                repositoryCategory.CategoryType = RepositoryCategoryType.RssFeed;
            }

            if (RepositoriesType.Checked)
            {
                RepositoriesGrid.ReadOnly = false;
                RepositoriesGrid.Enabled = true;
                RssFeed.Enabled = false;
                repositoryCategory.CategoryType = RepositoryCategoryType.Repositories;
            }
            
        }

        private void RssFeed_TextChanged(object sender, EventArgs e)
        {
            if (Categories.SelectedItem == null)
                return; 
            
            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            repositoryCategory.RssFeedUrl = RssFeed.Text;
        }

        private void RssFeed_Validating(object sender, CancelEventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            if (repositoryCategory.CategoryType == RepositoryCategoryType.RssFeed)
                repositoryCategory.DownloadRssFeed();
        }

        private void Caption_Validating(object sender, CancelEventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            repositoryCategory.OnListChanged(null, null);
        }

        private void RssFeedType_Validating(object sender, CancelEventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            repositoryCategory.OnListChanged(null, null);
        }

        private void RepositoriesType_Validating(object sender, CancelEventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;

            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            repositoryCategory.OnListChanged(null, null);
        }

        private void RepositoriesGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (Categories.SelectedItem == null)
                return;
            RepositoryCategory repositoryCategory = (RepositoryCategory)Categories.SelectedItem;
            repositoryCategory.OnListChanged(null, null);
        }

    }
}
