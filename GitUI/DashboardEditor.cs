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
        public DashboardEditor()
        {
            InitializeComponent();
            Initialize();
        }


        public void Initialize()
        {
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
        }

    }
}
