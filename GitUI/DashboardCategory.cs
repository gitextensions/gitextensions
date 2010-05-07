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

                if (repositoryCategory != null)
                    foreach (Repository repository in repositoryCategory.Repositories)
                    {
                        DashboardItem dashboardItem = new DashboardItem(repository);
                        dashboardItem.Click += new EventHandler(dashboardItem_Click);
                        AddItem(dashboardItem);
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
    }
}
