﻿using System;
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
        private readonly TranslationString directoryIsNotAValidRepositoryCaption = new TranslationString("Open");
        private readonly TranslationString directoryIsNotAValidRepository = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to abort and remove it from the recent repositories list?");
        private bool initialized;

        public Dashboard()
        {
            InitializeComponent();
            Translate();

            RecentRepositories.DashboardItemClick += dashboardItem_Click;
            RecentRepositories.RepositoryRemoved += RecentRepositories_RepositoryRemoved;
            RecentRepositories.DisableContextMenu();
            RecentRepositories.DashboardCategoryChanged += dashboardCategory_DashboardCategoryChanged;
            //Repositories.RepositoryCategories.ListChanged += new ListChangedEventHandler(RepositoryCategories_ListChanged);

            Bitmap image = Lemmings.GetPictureBoxImage(DateTime.Now);
            if (image != null)
            {
                pictureBox1.Image = image;
            }

            // Do this at runtime, because it is difficult to keep consistent at design time.
            pictureBox1.BringToFront();
            pictureBox1.Location = new Point(this.Width - 145, this.Height - 145);

            Load += Dashboard_Load;
        }

        void RecentRepositories_RepositoryRemoved(Repository repository)
        {
            if (repository != null)
                Repositories.RepositoryHistory.RemoveRepository(repository);
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

        private void AddDashboardEntry(RepositoryCategory entry)
        {
            var dashboardCategory = new DashboardCategory(entry.Description, entry);
            this.groupLayoutPanel.Controls.Add(dashboardCategory);

            dashboardCategory.DashboardItemClick += dashboardItem_Click;
            dashboardCategory.DashboardCategoryChanged += dashboardCategory_DashboardCategoryChanged;
        }

        private void dashboardCategory_DashboardCategoryChanged(object sender, EventArgs e)
        {
            Refresh();
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

            // Make sure the dashboard is only initialized once
            if (!initialized)
            {
                // Remove favorites
                var categories = (from DashboardCategory i in this.groupLayoutPanel.Controls
                                  select i).ToList();
                this.groupLayoutPanel.Controls.Clear();
                foreach (var category in categories)
                {
                    category.DashboardCategoryChanged -= dashboardCategory_DashboardCategoryChanged;
                    category.DashboardItemClick -= dashboardItem_Click;
                    category.Clear();
                }

                // Show favorites
                foreach (var category in Repositories.RepositoryCategories)
                {
                    AddDashboardEntry(category);
                }

                initialized = true;
            }

            splitContainer7.Panel1MinSize = 1;
            splitContainer7.Panel2MinSize = 1;
            splitContainer7.SplitterDistance = Math.Max(2, splitContainer7.Height - (DonateCategory.Height + 25));

            RecentRepositories.Clear();

            RepositoryCategory filteredRecentRepositoryHistory = new RepositoryCategory();
            filteredRecentRepositoryHistory.Description = Repositories.RepositoryHistory.Description;
            filteredRecentRepositoryHistory.CategoryType = Repositories.RepositoryHistory.CategoryType;

            foreach (Repository repository in Repositories.RepositoryHistory.Repositories)
            {
                if (!Repositories.RepositoryCategories.Any(c => c.Repositories.Any(r => r.Path != null && r.Path.Equals(repository.Path, StringComparison.CurrentCultureIgnoreCase))))
                {
                    repository.RepositoryType = RepositoryType.History;
                    filteredRecentRepositoryHistory.Repositories.Add(repository);
                }
            }

            RecentRepositories.RepositoryCategory = filteredRecentRepositoryHistory;

            pictureBox1.BringToFront();

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

                if (!Settings.ValidWorkingDir())
                {
                    DialogResult dialogResult = MessageBox.Show(directoryIsNotAValidRepository.Text, directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    if (dialogResult == DialogResult.Cancel)
                    {
                        Settings.WorkingDir = string.Empty;
                        return;
                    }
                    if (dialogResult == DialogResult.Yes)
                    {
                        Settings.WorkingDir = string.Empty;
                        Repositories.RepositoryHistory.RemoveRecentRepository(label.Path);
                        Refresh();
                        return;
                    }
                }

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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ShowRecentRepositories();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            new FormDashboardEditor().ShowDialog();
            Refresh();
        }
    }
}
