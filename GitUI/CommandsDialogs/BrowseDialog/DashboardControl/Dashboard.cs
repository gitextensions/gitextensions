using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Properties;
using GitUIPluginInterfaces.RepositoryHosts;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class Dashboard : GitModuleControl
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
        private readonly TranslationString directoryIsNotAValidRepositoryOpenIt = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to open it?");
        private readonly TranslationString _showCurrentBranch = new TranslationString("Show current branch");
        private bool initialized;
        private SplitterManager _splitterManager = new SplitterManager(new AppSettingsPath("Dashboard"));

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

        void RecentRepositories_RepositoryRemoved(object sender, DashboardCategory.RepositoryEventArgs e)
        {
            var repository = e.Repository;
            if (repository != null)
                Repositories.RepositoryHistory.RemoveRepository(repository);
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            DonateCategory.Dock = DockStyle.Top;
            //Show buttons
            CommonActions.DisableContextMenu();
            var openItem = new DashboardItem(Resources.IconRepoOpen, openRepository.Text);
            openItem.Click += openItem_Click;
            CommonActions.AddItem(openItem);

            var cloneItem = new DashboardItem(Resources.IconCloneRepoGit, cloneRepository.Text);
            cloneItem.Click += cloneItem_Click;
            CommonActions.AddItem(cloneItem);

            var cloneSvnItem = new DashboardItem(Resources.IconCloneRepoSvn, cloneSvnRepository.Text);
            cloneSvnItem.Click += cloneSvnItem_Click;
            CommonActions.AddItem(cloneSvnItem);

            foreach (IRepositoryHostPlugin el in RepoHosts.GitHosters)
            {
                IRepositoryHostPlugin gitHoster = el;
                var di = new DashboardItem(Resources.IconCloneRepoGithub, string.Format(cloneFork.Text, el.Description));
                di.Click += (repoSender, eventArgs) => UICommands.StartCloneForkFromHoster(this, gitHoster, GitModuleChanged);
                CommonActions.AddItem(di);
            }

            var createItem = new DashboardItem(Resources.IconRepoCreate, createRepository.Text);
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

            //
            // create Show current branch menu item and add to Dashboard menu
            //
            var showCurrentBranchMenuItem = new ToolStripMenuItem(_showCurrentBranch.Text);
            showCurrentBranchMenuItem.Click += showCurrentBranchMenuItem_Click;
            showCurrentBranchMenuItem.Checked = GitCommands.AppSettings.DashboardShowCurrentBranch;

            var menuStrip = FindControl<MenuStrip>(Parent.Parent.Parent, p => true); // TODO: improve: Parent.Parent.Parent == FormBrowse
            var dashboardMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "dashboardToolStripMenuItem");
            dashboardMenu.DropDownItems.Add(showCurrentBranchMenuItem);
        }

        /// <summary>
        /// code duplicated from GerritPlugin.cs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="form"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private T FindControl<T>(Control form, Func<T, bool> predicate)
            where T : Control
        {
            return FindControl(form.Controls, predicate);
        }

        /// <summary>
        /// code duplicated from GerritPlugin.cs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controls"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private T FindControl<T>(IEnumerable controls, Func<T, bool> predicate)
            where T : Control
        {
            foreach (Control control in controls)
            {
                var result = control as T;

                if (result != null && predicate(result))
                    return result;

                result = FindControl(control.Controls, predicate);

                if (result != null)
                    return result;
            }

            return null;
        }

        public void SaveSplitterPositions()
        {
            _splitterManager.SaveSplitters();
        }

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;

        public virtual void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            var handler = GitModuleChanged;
            if (handler != null)
                handler(this, e);
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

            commonSplitContainer.Panel1MinSize = 1;
            commonSplitContainer.Panel2MinSize = 1;
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
        }

        void showCurrentBranchMenuItem_Click(object sender, EventArgs e)
        {
            bool newValue = !GitCommands.AppSettings.DashboardShowCurrentBranch;
            GitCommands.AppSettings.DashboardShowCurrentBranch = newValue;
            ((ToolStripMenuItem)sender).Checked = newValue;
            Refresh();
        }

        public void SetSplitterPositions()
        {
            _splitterManager.AddSplitter(commonSplitContainer, "commonSplitContainer", Math.Max(2, (int)(CommonActions.Height * 1.2)));
            _splitterManager.AddSplitter(splitContainer7, "splitContainer7", Math.Max(2, splitContainer7.Height - (DonateCategory.Height + 25)));
            _splitterManager.AddSplitter(mainSplitContainer, "mainSplitContainer", 315);
            _splitterManager.RestoreSplitters();
        }

        private void TranslateItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.transifex.com/git-extensions/git-extensions/translate/");
        }

        private static void GitHubItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://github.com/gitextensions/gitextensions");
        }

        private static void IssuesItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://github.com/gitextensions/gitextensions/issues");
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
            GitModule module = new GitModule(path);

            if (!module.IsValidGitWorkingDir())
            {
                DialogResult dialogResult = MessageBox.Show(this, directoryIsNotAValidRepository.Text,
                    directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
                if (dialogResult == DialogResult.Yes)
                {
                    Repositories.RepositoryHistory.RemoveRecentRepository(path);
                    Refresh();
                    return;
                }
            }

            Repositories.AddMostRecentRepository(module.WorkingDir);
            OnModuleChanged(this, new GitModuleEventArgs(module));
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            GitModule module = FormOpenDirectory.OpenModule(this, currentModule: null);
            if (module != null)
                OnModuleChanged(this, new GitModuleEventArgs(module));
        }

        private void cloneItem_Click(object sender, EventArgs e)
        {
            UICommands.StartCloneDialog(this, null, false, OnModuleChanged);
        }

        private void cloneSvnItem_Click(object sender, EventArgs e)
        {
            UICommands.StartSvnCloneDialog(this, OnModuleChanged);
        }

        private void createItem_Click(object sender, EventArgs e)
        {
            UICommands.StartInitializeDialog(this, Module.WorkingDir, OnModuleChanged);
        }

        private static void DonateItem_Click(object sender, EventArgs e)
        {
            Process.Start(FormDonate.DonationUrl);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            using (var frm = new FormDashboardEditor()) frm.ShowDialog(this);
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
                    GitModule module = new GitModule(dir);

                    if (!module.IsValidGitWorkingDir())
                    {
                        DialogResult dialogResult = MessageBox.Show(this, directoryIsNotAValidRepositoryOpenIt.Text,
                            directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                        if (dialogResult == DialogResult.No)
                            return;
                    }

                    Repositories.AddMostRecentRepository(module.WorkingDir);
                    OnModuleChanged(this, new GitModuleEventArgs(module));
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
                    UICommands.StartCloneDialog(this, url, false, OnModuleChanged);
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
