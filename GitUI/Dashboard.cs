using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands.Repository;
using GitUI.Properties;
using GitUI.RepoHosting;
using GitUIPluginInterfaces.RepositoryHosts;
using ResourceManager.Translation;
using Settings = GitCommands.Settings;

namespace GitUI
{
    public partial class Dashboard : GitExtensionsControl
    {
        private readonly TranslationString cloneFork = new TranslationString("Clone {0} repository");
        private readonly TranslationString cloneRepository = new TranslationString("Clone repository");
        private readonly TranslationString createRepository = new TranslationString("Create new repository");
        private readonly TranslationString develop = new TranslationString("Develop");
        private readonly TranslationString donate = new TranslationString("Donate");
        private readonly TranslationString issues = new TranslationString("Issues");
        private readonly TranslationString openRepository = new TranslationString("Open repository");
        private readonly TranslationString translate = new TranslationString("Translate");
        private bool Recalculating;
        private bool initialized;

        public Dashboard()
        {
            InitializeComponent();
            Translate();

            RecentRepositories.DashboardItemClick += dashboardItem_Click;
            RecentRepositories.DisableContextMenu();
            RecentRepositories.DashboardCategoryChanged += dashboardCategory_DashboardCategoryChanged;
            //Repositories.RepositoryCategories.ListChanged += new ListChangedEventHandler(RepositoryCategories_ListChanged);

            Bitmap image = Lemmings.GetPictureBoxImage(DateTime.Now);
            if (image != null)
                pictureBox1.Image = image;

            Load += Dashboard_Load;
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            DonateCategory.Dock = DockStyle.Top;
            //Show buttons
            CommonActions.DisableContextMenu();
            var openItem = new DashboardItem(Resources._40, openRepository.Text);
            openItem.Click += openItem_Click;
            CommonActions.AddItem(openItem);

            var cloneItem = new DashboardItem(Resources._46, cloneRepository.Text);
            cloneItem.Click += cloneItem_Click;
            CommonActions.AddItem(cloneItem);

            foreach (IRepositoryHostPlugin el in RepoHosts.GitHosters)
            {
                IRepositoryHostPlugin gitHoster = el;
                var di = new DashboardItem(Resources._46, string.Format(cloneFork.Text, el.Description));
                di.Click += (repoSender, eventArgs) => GitUICommands.Instance.StartCloneForkFromHoster(gitHoster);
                CommonActions.AddItem(di);
            }

            var createItem = new DashboardItem(Resources._14, createRepository.Text);
            createItem.Click += createItem_Click;
            CommonActions.AddItem(createItem);

            DonateCategory.DisableContextMenu();
            var GitHubItem = new DashboardItem(Resources.develop.ToBitmap(), develop.Text);
            GitHubItem.Click += GitHubItem_Click;
            DonateCategory.AddItem(GitHubItem);
            var DonateItem = new DashboardItem(Resources.dollar.ToBitmap(), donate.Text);
            DonateItem.Click += DonateItem_Click;
            DonateCategory.AddItem(DonateItem);
            var TranslateItem = new DashboardItem(Resources._24, translate.Text);
            TranslateItem.Click += TranslateItem_Click;
            DonateCategory.AddItem(TranslateItem);
            var IssuesItem = new DashboardItem(Resources.bug, issues.Text);
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
            var dashboardCategory = new DashboardCategory(entry.Description, entry)
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

        private void dashboardCategory_DashboardCategoryChanged(object sender, EventArgs e)
        {
            Recalculate();
        }

        //Recalculate hieght when list is changed

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
                    var currentDashboardCategory = splitContainer5.Panel2.Controls[i] as DashboardCategory;

                    if (currentDashboardCategory != null &&
                        !Repositories.RepositoryCategories.Contains(currentDashboardCategory.RepositoryCategory))
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
                        var currentDashboardCategory = splitContainer5.Panel2.Controls[i] as DashboardCategory;

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
                Repositories.RepositoryCategories.Aggregate(0, AddDashboardEntry);

                splitContainer7.SplitterDistance = splitContainer7.Height - (DonateCategory.Height + 25);

                initialized = true;
                splitContainer7.SplitterDistance = splitContainer7.Height - (DonateCategory.Height + 25);

                initialized = true;
            }

            RecentRepositories.Clear();
            RecentRepositories.RepositoryCategory = Repositories.RepositoryHistory;

        }

        private static void TranslateItem_Click(object sender, EventArgs e)
        {
            new FormTranslate().ShowDialog();
        }

        private static void GitHubItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://github.com/spdr870/gitextensions");
        }

        private static void IssuesItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://github.com/spdr870/gitextensions/issues");
        }

        private void dashboardItem_Click(object sender, EventArgs e)
        {
            var label = sender as DashboardItem;
            if (label == null || string.IsNullOrEmpty(label.Path))
                return;

            //Open urls in browser, but open directories in GitExtensions
            if (Regex.IsMatch(label.Path,
                              @"^(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~\/|\/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:\/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|\/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=?(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?$"))
            {
                Process.Start(label.Path);
            }
            else
            {
                Settings.WorkingDir = label.Path;
                Repositories.RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);

                OnWorkingDirChanged();
            }
        }


        private void openItem_Click(object sender, EventArgs e)
        {
            var open = new Open();
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
            GitUICommands.Instance.StartInitializeDialog(Settings.WorkingDir);

            OnWorkingDirChanged();
        }

        private static void DonateItem_Click(object sender, EventArgs e)
        {
            Process.Start(
                @"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WAL2SSDV8ND54&lc=US&item_name=GitExtensions&no_note=1&no_shipping=1&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
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
