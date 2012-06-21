using System;
using System.IO;
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
using System.Configuration;

namespace GitUI
{
    public partial class Dashboard : GitExtensionsControl
    {
        private readonly TranslationString cloneFork = new TranslationString("Clone {0} repository");
        private readonly TranslationString cloneRepository = new TranslationString("Clone repository");
        private readonly TranslationString cloneSvnRepository = new TranslationString("Clone SVN repository");
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
            pictureBox1.Location = new Point(this.Width - pictureBox1.Image.Width - 10, this.Height - pictureBox1.Image.Height - 10);

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
            var openItem = new DashboardItem(Resources.Folder, openRepository.Text);
            openItem.Click += openItem_Click;
            CommonActions.AddItem(openItem);

            var cloneItem = new DashboardItem(Resources.SaveAs, cloneRepository.Text);
            cloneItem.Click += cloneItem_Click;
            CommonActions.AddItem(cloneItem);

            var cloneSvnItem = new DashboardItem(Resources.SaveAs, cloneSvnRepository.Text);
            cloneSvnItem.Click += cloneSvnItem_Click;
            CommonActions.AddItem(cloneSvnItem);

            foreach (IRepositoryHostPlugin el in RepoHosts.GitHosters)
            {
                IRepositoryHostPlugin gitHoster = el;
                var di = new DashboardItem(Resources.SaveAs, string.Format(cloneFork.Text, el.Description));
                di.Click += (repoSender, eventArgs) => GitUICommands.Instance.StartCloneForkFromHoster(this, gitHoster);
                CommonActions.AddItem(di);
            }

            var createItem = new DashboardItem(Resources.Star, createRepository.Text);
            createItem.Click += createItem_Click;
            CommonActions.AddItem(createItem);

            DonateCategory.DisableContextMenu();
            var GitHubItem = new DashboardItem(Resources.develop.ToBitmap(), develop.Text);
            GitHubItem.Click += GitHubItem_Click;
            DonateCategory.AddItem(GitHubItem);
            var DonateItem = new DashboardItem(Resources.dollar.ToBitmap(), donate.Text);
            DonateItem.Click += DonateItem_Click;
            DonateCategory.AddItem(DonateItem);
            var TranslateItem = new DashboardItem(Resources.EditItem, translate.Text);
            TranslateItem.Click += TranslateItem_Click;
            DonateCategory.AddItem(TranslateItem);
            var IssuesItem = new DashboardItem(Resources.bug, issues.Text);
            IssuesItem.Click += IssuesItem_Click;
            DonateCategory.AddItem(IssuesItem);
        }

        public void SaveSplitterPositions()
        {
            try
            {
                Properties.Settings.Default.Dashboard_MainSplitContainer_SplitterDistance = splitContainer5.SplitterDistance;
                Properties.Settings.Default.Dashboard_CommonSplitContainer_SplitterDistance = splitContainer6.SplitterDistance;
                Properties.Settings.Default.Save();
            }
            catch (ConfigurationException)
            {
                //TODO: howto restore a corrupted config? Properties.Settings.Default.Reset() doesn't work.
            }
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

            splitContainer6.Panel1MinSize = 1;
            splitContainer6.Panel2MinSize = 1;
            splitContainer7.Panel1MinSize = 1;
            splitContainer7.Panel2MinSize = 1;

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

            SetSplitterPositions();
        }

        private void SetSplitterPositions()
        {
            try
            {
                SetSplitterDistance(
                    splitContainer6,
                    Properties.Settings.Default.Dashboard_CommonSplitContainer_SplitterDistance,
                    (int)Math.Max(2, (CommonActions.Height * 1.2)));

                SetSplitterDistance(
                    splitContainer7,
                    0, // No settings property for this splitter. Will use default always.
                    Math.Max(2, splitContainer7.Height - (DonateCategory.Height + 25)));

                SetSplitterDistance(
                    splitContainer5,
                    Properties.Settings.Default.Dashboard_MainSplitContainer_SplitterDistance,
                    315);
            }
            catch (ConfigurationException)
            {
                //TODO: howto restore a corrupted config? Properties.Settings.Default.Reset() doesn't work.
            }
        }

        private void SetSplitterDistance(SplitContainer splitContainer, int value, int @default)
        {
            if (value != 0)
            {
                try
                {
                    splitContainer.SplitterDistance = value;
                }
                catch
                {
                    splitContainer.SplitterDistance = @default;
                }
            }
            else
                splitContainer.SplitterDistance = @default;
        }

        private void TranslateItem_Click(object sender, EventArgs e)
        {
            new FormTranslate().ShowDialog(this);
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
                OpenPath(label.Path);
            }
        }

        private void OpenPath(string path)
        {
            Settings.WorkingDir = path;

            if (!Settings.Module.ValidWorkingDir())
            {
                DialogResult dialogResult = MessageBox.Show(this, directoryIsNotAValidRepository.Text, directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if (dialogResult == DialogResult.Cancel)
                {
                    Settings.WorkingDir = string.Empty;
                    return;
                }
                if (dialogResult == DialogResult.Yes)
                {
                    Settings.WorkingDir = string.Empty;
                    Repositories.RepositoryHistory.RemoveRecentRepository(path);
                    Refresh();
                    return;
                }
            }

            Repositories.AddMostRecentRepository(Settings.WorkingDir);
            OnWorkingDirChanged();
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            var open = new Open();
            open.ShowDialog(this);
            OnWorkingDirChanged();
        }

        private void cloneItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCloneDialog(this))
                OnWorkingDirChanged();
        }

        private void cloneSvnItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSvnCloneDialog(this))
                OnWorkingDirChanged();
        }

        private void createItem_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartInitializeDialog(this, Settings.WorkingDir);

            OnWorkingDirChanged();
        }

        private static void DonateItem_Click(object sender, EventArgs e)
        {
            Process.Start(
                @"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WAL2SSDV8ND54&lc=US&item_name=GitExtensions&no_note=1&no_shipping=1&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            new FormDashboardEditor().ShowDialog(this);
            Refresh();
        }

        private void groupLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {
            var fileNameArray = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNameArray != null)
            {
                if (fileNameArray.Length != 1)
                    return;

                string dir = fileNameArray[0];
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    OpenPath(dir);
                }
                return;
            }
            var text = e.Data.GetData(DataFormats.UnicodeText) as string;
            if (!string.IsNullOrEmpty(text))
            {
                var lines = text.Split('\n');
                if (lines.Length != 1)
                    return;
                string url = lines[0];
                if (!string.IsNullOrEmpty(url))
                {
                    if (GitUICommands.Instance.StartCloneDialog(this, url))
                        OnWorkingDirChanged();
                }
            }
        }

        private void groupLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {
            var fileNameArray = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNameArray != null)
            {
                if (fileNameArray.Length != 1)
                    return;
                string dir = fileNameArray[0];
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    //Allow drop (copy, not move) folders
                    e.Effect = DragDropEffects.Copy;
                }
                return;
            }
            var text = e.Data.GetData(DataFormats.UnicodeText) as string;
            if (!string.IsNullOrEmpty(text))
            {
                var lines = text.Split('\n');
                if (lines.Length != 1)
                    return;
                string url = lines[0];
                if (!string.IsNullOrEmpty(url))
                {
                    //Allow drop (copy, not move) folders
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }
    }
}
