using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Properties;
using System.Text.RegularExpressions;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class Dashboard : GitExtensionsControl
    {
        TranslationString openRepository = new TranslationString("Open repository");
        TranslationString cloneRepository = new TranslationString("Clone repository");
        TranslationString createRepository = new TranslationString("Create new repository");
        TranslationString cloneFork = new TranslationString("Clone {0} repository");
        TranslationString develop = new TranslationString("Develop");
        TranslationString donate = new TranslationString("Donate");
        TranslationString translate = new TranslationString("Translate");
        TranslationString issues = new TranslationString("Issues");

        public Dashboard()
        {
            InitializeComponent(); Translate();

            RecentRepositories.DashboardItemClick += dashboardItem_Click;
            RecentRepositories.DisableContextMenu();
            RecentRepositories.DashboardCategoryChanged += dashboardCategory_DashboardCategoryChanged;
            //Repositories.RepositoryCategories.ListChanged += new ListChangedEventHandler(RepositoryCategories_ListChanged);

            var image = Lemmings.GetPictureBoxImage(DateTime.Now);
            if (image != null)
                pictureBox1.Image = image;

            this.Load += Dashboard_Load;
        }

        void Dashboard_Load(object sender, EventArgs e)
        {
            DonateCategory.Dock = DockStyle.Top;
            //Show buttons
            CommonActions.DisableContextMenu();
            DashboardItem openItem = new DashboardItem(Resources._40, openRepository.Text);
            openItem.Click += openItem_Click;
            CommonActions.AddItem(openItem);

            DashboardItem cloneItem = new DashboardItem(Resources._46, cloneRepository.Text);
            cloneItem.Click += cloneItem_Click;
            CommonActions.AddItem(cloneItem);

            foreach (var el in GitUI.RepoHosting.RepoHosts.GitHosters)
            {
                var gitHoster = el;
                DashboardItem di = new DashboardItem(Resources._46, string.Format(cloneFork.Text, el.Description));
                di.Click += (repoSender, eventArgs) => { GitUICommands.Instance.StartCloneForkFromHoster(gitHoster); };
                CommonActions.AddItem(di);
            }
            
            DashboardItem createItem = new DashboardItem(Resources._14, createRepository.Text);
            createItem.Click += createItem_Click;
            CommonActions.AddItem(createItem);

            DonateCategory.DisableContextMenu();
            DashboardItem GitHubItem = new DashboardItem(Resources.develop.ToBitmap(), develop.Text);
            GitHubItem.Click += GitHubItem_Click;
            DonateCategory.AddItem(GitHubItem);
            DashboardItem DonateItem = new DashboardItem(Resources.dollar.ToBitmap(), donate.Text);
            DonateItem.Click += DonateItem_Click;
            DonateCategory.AddItem(DonateItem);
            DashboardItem TranslateItem = new DashboardItem(Resources._24, translate.Text);
            TranslateItem.Click += TranslateItem_Click;
            DonateCategory.AddItem(TranslateItem);
            DashboardItem IssuesItem = new DashboardItem(Resources.bug, issues.Text);
            IssuesItem.Click += IssuesItem_Click;
            DonateCategory.AddItem(IssuesItem);
        }
         
        public event EventHandler WorkingDirChanged;

        public virtual void OnWorkingDirChanged()
        {
            if (WorkingDirChanged != null)
                WorkingDirChanged(this, null);
        }
        
        private int AddDashboardEntry(int y, RepositoryCategory entry)
        {
            DashboardCategory dashboardCategory = new DashboardCategory(entry.Description, entry)
                                                      {
                                                          Location = new Point(0, y),
                                                          Width = splitContainer5.Panel2.Width
                                                      };
            splitContainer5.Panel2.Controls.Add(dashboardCategory);
            dashboardCategory.BringToFront();
            y += dashboardCategory.Height;

            dashboardCategory.DashboardItemClick += dashboardItem_Click;
            dashboardCategory.DashboardCategoryChanged += dashboardCategory_DashboardCategoryChanged;

            return y;
        }

        void dashboardCategory_DashboardCategoryChanged(object sender, EventArgs e)
        {
            Recalculate();
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

        private bool Recalculating;

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
                        currentDashboardCategory.DashboardCategoryChanged -= dashboardCategory_DashboardCategoryChanged;
                        currentDashboardCategory.DashboardItemClick -= dashboardItem_Click;
                        currentDashboardCategory.Clear();
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

                        if (currentDashboardCategory == null || currentDashboardCategory.RepositoryCategory != entry)
                            continue;

                        dashboardCategory = currentDashboardCategory;                            
                        dashboardCategory.Recalculate();
                        dashboardCategory.Location = new Point(0, y);
                        y += dashboardCategory.Height;
                        break;
                    }

                    if (dashboardCategory == null)
                    {
                        y = AddDashboardEntry(y, entry);
                    }
                }

                RecentRepositories.Recalculate();
            }
            finally
            {
                Recalculating = false;
            }
        }

        public override void Refresh()
        {
            initialized = false;
            ShowRecentRepositories();
        }

        private bool initialized;

        public void ShowRecentRepositories()
        {
            if (!Visible)
            {
                return;
            }
            //Make sure the dashboard is only initialized once
            if (!initialized)
            {
                //Remove favourites
                for (int i = splitContainer5.Panel2.Controls.Count; i > 0; i--)
                {
                    DashboardCategory dashboarCategory = splitContainer5.Panel2.Controls[i - 1] as DashboardCategory;
                    if (dashboarCategory != null)
                    {
                        dashboarCategory.DashboardCategoryChanged -= dashboardCategory_DashboardCategoryChanged;
                        dashboarCategory.DashboardItemClick -= dashboardItem_Click;
                        dashboarCategory.Clear();
                        splitContainer5.Panel2.Controls.RemoveAt(i - 1);
                    }
                }

                //Show favourites
                int y = 0;
                foreach (RepositoryCategory entry in Repositories.RepositoryCategories)
                {
                    y = AddDashboardEntry(y, entry);
                }

                splitContainer7.SplitterDistance = splitContainer7.Height - (DonateCategory.Height + 25);
                
                initialized = true;
            }
            
            //Show recent repositories
            RecentRepositories.Clear();
            RecentRepositories.RepositoryCategory = Repositories.RepositoryHistory;

        }

        void TranslateItem_Click(object sender, EventArgs e)
        {
            new FormTranslate().ShowDialog();
        }

        void GitHubItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://github.com/spdr870/gitextensions");
        }

        void IssuesItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://github.com/spdr870/gitextensions/issues");
        }

        void dashboardItem_Click(object sender, EventArgs e)
        {
            DashboardItem label = sender as DashboardItem;
            if (label == null || string.IsNullOrEmpty(label.Path))
                return;

            //Open urls in browser, but open directories in GitExtensions
            if (Regex.IsMatch(label.Path,
                              @"^(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~\/|\/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:\/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|\/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?$"))
            {
                System.Diagnostics.Process.Start(label.Path);
            }
            else
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
            Recalculate();
        }
    }
}
