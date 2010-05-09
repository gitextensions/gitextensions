using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;

namespace GitUI
{
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();

            ShowRecentRepositories();

            Repositories.RepositoryCategories.ListChanged += new ListChangedEventHandler(RepositoryCategories_ListChanged);
        }


        public event EventHandler WorkingDirChanged;

        public virtual void OnWorkingDirChanged()
        {
            if (WorkingDirChanged != null)
                WorkingDirChanged(this, null);
        }
        
        private void ShowFavourites()
        {
            if (Visible)
            {
                for (int i = splitContainer5.Panel2.Controls.Count; i > 0; i--)
                {
                    if (splitContainer5.Panel2.Controls[i - 1] is DashboardCategory)
                        splitContainer5.Panel2.Controls.RemoveAt(i - 1);
                }


                int y = 0;
                foreach (RepositoryCategory entry in Repositories.RepositoryCategories)
                {
                    y = AddDashboardEntry(y, entry);
                }
            }
        }

        private int AddDashboardEntry(int y, RepositoryCategory entry)
        {
            DashboardCategory dashboardCategory = new DashboardCategory(entry.Description, entry);
            dashboardCategory.Location = new Point(0, y);
            dashboardCategory.Width = splitContainer5.Panel2.Width;
            if (entry.CategoryType == RepositoryCategoryType.Repositories)
                dashboardCategory.DashboardItemClick += new EventHandler(dashboardItem_Click);
            if (entry.CategoryType == RepositoryCategoryType.RssFeed)
                dashboardCategory.DashboardItemClick += new EventHandler(dashboardItemRss_Click);
            splitContainer5.Panel2.Controls.Add(dashboardCategory);
            dashboardCategory.BringToFront();
            y += dashboardCategory.Height;

            //Recalculate hieght when list is changed
            entry.ListChanged += new ListChangedEventHandler(entry_ListChanged);
            return y;
        }

        void RepositoryCategories_ListChanged(object sender, ListChangedEventArgs e)
        {
            Recalculate();

        }
        //Recalculate hieght when list is changed
        private void entry_ListChanged(object sender, ListChangedEventArgs e)
        {
            Recalculate();
        }

        private bool Recalculating = false;
        private void Recalculate()
        {
            if (Recalculating)
                return;
            int y = 0;

            try
            {
                Recalculating = true;
                //Remove deleted entries
                for (int i = splitContainer5.Panel2.Controls.Count - 1; i >= 0; i--)
                {
                    DashboardCategory currentDashboardCategory = splitContainer5.Panel2.Controls[i] as DashboardCategory;

                    if (currentDashboardCategory != null && !Repositories.RepositoryCategories.Contains(currentDashboardCategory.RepositoryCategory))
                    {
                        splitContainer5.Panel2.Controls.RemoveAt(i);
                    }
                }
                
                foreach (RepositoryCategory entry in Repositories.RepositoryCategories)
                {
                    DashboardCategory dashboardCategory = null;
                    //Try to find existing entry first
                    for (int i = splitContainer5.Panel2.Controls.Count - 1; i >= 0; i--)
                    {
                        DashboardCategory currentDashboardCategory = splitContainer5.Panel2.Controls[i] as DashboardCategory;

                        if (currentDashboardCategory != null && currentDashboardCategory.RepositoryCategory == entry)
                        {
                            dashboardCategory = currentDashboardCategory;

                            dashboardCategory.Location = new Point(0, y);
                            y += dashboardCategory.Height;
                            break;

                        }
                    }

                    if (dashboardCategory == null)
                    {
                        y = AddDashboardEntry(y, entry);
                    }
                }
            }
            finally
            {
                Recalculating = false;
            }
        }

        public void ShowRecentRepositories()
        {
            ShowFavourites();
            if (Visible)
            {
                RecentRepositories.Clear();
                RecentRepositories.RepositoryCategory = Repositories.RepositoryHistory;
                RecentRepositories.DashboardItemClick += new EventHandler(dashboardItem_Click);
                RecentRepositories.DisableContextMenu();


                CommonActions.Clear();
                CommonActions.DisableContextMenu();
                DashboardItem openItem = new DashboardItem(Resources._40, "Open repository");
                openItem.Click += new EventHandler(openItem_Click);
                CommonActions.AddItem(openItem);
                DashboardItem cloneItem = new DashboardItem(Resources._46, "Clone repository");
                cloneItem.Click += new EventHandler(cloneItem_Click);
                CommonActions.AddItem(cloneItem);
                DashboardItem createItem = new DashboardItem(Resources._14, "Create new repository");
                createItem.Click += new EventHandler(createItem_Click);
                CommonActions.AddItem(createItem);
                DonateCategory.Clear();
                DonateCategory.DisableContextMenu();
                DashboardItem GitHubItem = new DashboardItem(Resources.develop.ToBitmap(), "Develop");
                GitHubItem.Click += new EventHandler(GitHubItem_Click);
                DonateCategory.AddItem(GitHubItem);
                DashboardItem DonateItem = new DashboardItem(Resources.dollar.ToBitmap(), "Donate");
                DonateItem.Click += new EventHandler(DonateItem_Click);
                DonateCategory.AddItem(DonateItem);
            }
        }

        void GitHubItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://github.com/spdr870/gitextensions");
        }

        void dashboardItemRss_Click(object sender, EventArgs e)
        {
            DashboardItem label = sender as DashboardItem;
            if (label != null && !string.IsNullOrEmpty(label.GetTitle()))
            {
                System.Diagnostics.Process.Start(label.Path);
               
            }
        }


        void dashboardItem_Click(object sender, EventArgs e)
        {
            DashboardItem label = sender as DashboardItem;
            if (label != null && !string.IsNullOrEmpty(label.Path))
            {
                GitCommands.Settings.WorkingDir = label.Path;
                Repositories.RepositoryHistory.AddMostRecentRepository(GitCommands.Settings.WorkingDir);

                OnWorkingDirChanged();
            }
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            Open open = new Open();
            open.ShowDialog();
            OnWorkingDirChanged();

        }

        private void cloneItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCloneDialog()) 
                OnWorkingDirChanged();
        }

        private void createItem_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartInitializeDialog(GitCommands.Settings.WorkingDir);

            OnWorkingDirChanged();
        }

        private void DonateItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WAL2SSDV8ND54&lc=US&item_name=GitExtensions&no_note=1&no_shipping=1&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void splitContainer5_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ShowRecentRepositories();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            new FormDashboardEditor().ShowDialog();
        }
    }
}
