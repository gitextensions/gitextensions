using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Editor;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class Dashboard : GitModuleControl
    {
        // ReSharper disable InconsistentNaming - can't rename them as they are referenced by translations
        private readonly TranslationString cloneFork = new TranslationString("Clone {0} repository");
        private readonly TranslationString cloneRepository = new TranslationString("Clone repository");
        private readonly TranslationString cloneSvnRepository = new TranslationString("Clone SVN repository");
        private readonly TranslationString createRepository = new TranslationString("Create new repository");
        private readonly TranslationString develop = new TranslationString("Develop");
        private readonly TranslationString donate = new TranslationString("Donate");
        private readonly TranslationString issues = new TranslationString("Issues");
        private readonly TranslationString openRepository = new TranslationString("Open repository");
        private readonly TranslationString translate = new TranslationString("Translate");
        private readonly TranslationString showCurrentBranch = new TranslationString("Show current branch");
        // ReSharper restore InconsistentNaming
        private DashboardTheme _selectedTheme;

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;


        public Dashboard()
        {
            InitializeComponent();
            Visible = false;
            Translate();

            recentRepositoriesList1.GitModuleChanged += OnModuleChanged;

            ApplyScaling();
        }


        // need this to stop flickering of the background images, nothing else works
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public override void Refresh()
        {
            InitDashboardLayout();
            ApplyTheme();
            using (var recentRepositoriesLoader = new AsyncLoader())
            {
                recentRepositoriesLoader.Load(() =>
                {
                    var mostRecentRepos = new List<RecentRepoInfo>();
                    var lessRecentRepos = new List<RecentRepoInfo>();

                    var splitter = new RecentRepoSplitter();
                    splitter.SplitRecentRepos(Repositories.RepositoryHistory.Repositories, mostRecentRepos, lessRecentRepos);

                    return mostRecentRepos.Union(lessRecentRepos).Select(ri => ri.Repo);
                },
                recentRepositoriesList1.ShowRecentRepositories);
            }
        }


        protected virtual void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            var handler = GitModuleChanged;
            handler?.Invoke(this, e);
        }


        private void ApplyScaling()
        {
            var scaleFactor = GetScaleFactor();

            ApplyPaddingScaling(pnlLogo, scaleFactor);
            ApplyPaddingScaling(flpnlContribute, scaleFactor);
            ApplyPaddingScaling(flpnlContribute, scaleFactor);

            tableLayoutPanel1.ColumnStyles[1].Width *= scaleFactor;
            pnlLogo.Height = (int)(pnlLogo.Height * scaleFactor);
            pnlLogo.Padding = new Padding((pnlLogo.Width - lblLogo.Width) / 3, 0, 0, pnlLogo.Padding.Bottom);
            recentRepositoriesList1.HeaderHeight = pnlLogo.Height;
        }

        private void ApplyTheme(DashboardTheme theme = null)
        {
            if (theme == null)
            {
                if (AppSettings.DashboardThemeIndex < 0)
                {
                    AppSettings.DashboardThemeIndex = SetDashboardThemeFirstTime();
                }

                switch (AppSettings.DashboardThemeIndex)
                {
                    case 1: theme = DashboardTheme.Light; break;
                    case 2: theme = DashboardTheme.Dark; break;
                    default:
                        {
                            // select the theme based on the current Windows color scheme
                            // if the default text color is light, then it is likely that
                            // the user runs a dark theme, else set the light theme
                            theme = SystemColors.ControlText.IsLightColor() ? DashboardTheme.Dark : DashboardTheme.Light;
                            break;
                        }
                }
                _selectedTheme = theme;
            }

            BackColor = theme.Primary;
            pnlLogo.BackColor = theme.PrimaryVeryDark;
            flpnlStart.BackColor = theme.PrimaryLight;
            flpnlContribute.BackColor = theme.PrimaryVeryLight;
            lblLogo.ForeColor = theme.PrimaryHeadingText;
            lblContribute.ForeColor = theme.SecondaryHeadingText;
            recentRepositoriesList1.BranchNameColor = theme.SecondaryText;
            recentRepositoriesList1.FavouriteColor = theme.AccentedText;
            recentRepositoriesList1.ForeColor = theme.PrimaryText;
            recentRepositoriesList1.HeaderColor = theme.SecondaryHeadingText;
            recentRepositoriesList1.HeaderBackColor = theme.PrimaryDark;
            recentRepositoriesList1.HoverColor = theme.PrimaryLight;
            recentRepositoriesList1.MainBackColor = theme.Primary;
            BackgroundImage = theme.BackgroundImage;

            foreach (var item in flpnlContribute.Controls.OfType<LinkLabel>().Union(flpnlStart.Controls.OfType<LinkLabel>()))
            {
                item.LinkColor = theme.PrimaryText;
            }
        }

        private Control CreateLink(Control container, float scaleFactor, string text, Image icon, EventHandler handler)
        {
            var linkLabel = new LinkLabel
            {
                AutoSize = true,
                AutoEllipsis = true,
                Font = AppSettings.Font,// new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204),
                Image = icon,
                ImageAlign = ContentAlignment.MiddleLeft,
                LinkBehavior = LinkBehavior.NeverUnderline,
                Location = new Point(33, 151),
                Margin = new Padding((int)(3 * scaleFactor), 0, (int)(3 * scaleFactor), (int)(8 * scaleFactor)),
                Padding = new Padding((int)(24 * scaleFactor), 0, 0, 0),
                TabStop = true,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            linkLabel.MouseHover += (s, e) => linkLabel.LinkColor = _selectedTheme.AccentedText;
            linkLabel.MouseLeave += (s, e) => linkLabel.LinkColor = _selectedTheme.PrimaryText;

            if (handler != null)
            {
                linkLabel.Click += handler;
            }

            container.Controls.Add(linkLabel);

            return linkLabel;
        }

        private static T FindControl<T>(IEnumerable controls, Func<T, bool> predicate) where T : Control
        {
            foreach (Control control in controls)
            {
                var result = control as T;
                if (result != null && predicate(result))
                {
                    return result;
                }
                result = FindControl(control.Controls, predicate);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private void InitDashboardLayout()
        {
            var scaleFactor = GetScaleFactor();

            flpnlStart.SuspendLayout();
            flpnlStart.Controls.Clear();

            CreateLink(flpnlStart, scaleFactor, createRepository.Text, Resources.IconRepoCreate, createItem_Click);
            CreateLink(flpnlStart, scaleFactor, openRepository.Text, Resources.IconRepoOpen, openItem_Click);
            CreateLink(flpnlStart, scaleFactor, cloneRepository.Text, Resources.IconCloneRepoGit, cloneItem_Click);
            var lastControl = CreateLink(flpnlStart, scaleFactor, cloneSvnRepository.Text, Resources.IconCloneRepoSvn, cloneSvnItem_Click);

            foreach (var gitHoster in RepoHosts.GitHosters)
            {
                lastControl = CreateLink(flpnlStart, scaleFactor, string.Format(cloneFork.Text, gitHoster.Description), Resources.IconCloneRepoGithub,
                                         (repoSender, eventArgs) => UICommands.StartCloneForkFromHoster(this, gitHoster, GitModuleChanged));
            }

            var height = (int)((lastControl.Location.Y + lastControl.Size.Height) * scaleFactor) + flpnlStart.Padding.Bottom;
            flpnlStart.MinimumSize = new Size(0, height);
            flpnlStart.ResumeLayout(true);

            flpnlContribute.SuspendLayout();
            flpnlContribute.Controls.Clear();
            flpnlContribute.Controls.Add(lblContribute);
            lblContribute.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints + 5.5f); 

            CreateLink(flpnlContribute, scaleFactor, develop.Text, Resources.develop.ToBitmap(), GitHubItem_Click);
            CreateLink(flpnlContribute, scaleFactor, donate.Text, Resources.dollar.ToBitmap(), DonateItem_Click);
            CreateLink(flpnlContribute, scaleFactor, translate.Text, Resources.EditItem, TranslateItem_Click);
            lastControl = CreateLink(flpnlContribute, scaleFactor, issues.Text, Resources.bug, IssuesItem_Click);

            height = (int)((lastControl.Location.Y + lastControl.Size.Height) * scaleFactor) + flpnlContribute.Padding.Bottom;
            flpnlContribute.Height = height;
            flpnlContribute.MinimumSize = new Size(0, height);
            flpnlContribute.ResumeLayout(true);

            AutoScrollMinSize = new Size(0, pnlLogo.Height + flpnlContribute.MinimumSize.Height + flpnlContribute.MinimumSize.Height);
        }

        private int SetDashboardThemeFirstTime()
        {
            using (var dlg = new FormFirstTimeDashboardTheme())
            {
                dlg.ShowDialog(this);
                return dlg.SelectedThemeIndex;
            }
        }


        private void dashboard_ParentChanged(object sender, EventArgs e)
        {
            if (Parent == null)
            {
                Visible = false;
                return;
            }

            ApplyTheme();

            //
            // create Show current branch menu item and add to Dashboard menu
            //
            var showCurrentBranchMenuItem = new ToolStripMenuItem(showCurrentBranch.Text);
            showCurrentBranchMenuItem.Click += showCurrentBranchMenuItem_Click;
            showCurrentBranchMenuItem.Checked = AppSettings.DashboardShowCurrentBranch;

            var menuStrip = FindControl<MenuStrip>(Parent.Parent.Parent.Controls, p => true); // TODO: improve: Parent.Parent.Parent == FormBrowse
            var dashboardMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "dashboardToolStripMenuItem");
            dashboardMenu?.DropDownItems.Add(showCurrentBranchMenuItem);

            Visible = true;
        }

        private void showCurrentBranchMenuItem_Click(object sender, EventArgs e)
        {
            bool newValue = !AppSettings.DashboardShowCurrentBranch;
            AppSettings.DashboardShowCurrentBranch = newValue;
            ((ToolStripMenuItem)sender).Checked = newValue;
            Refresh();
        }

        private static void TranslateItem_Click(object sender, EventArgs e)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Process.Start(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "TranslationApp.exe"));
        }

        private static void GitHubItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://github.com/gitextensions/gitextensions");
        }

        private static void IssuesItem_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://github.com/gitextensions/gitextensions/issues");
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            GitModule module = FormOpenDirectory.OpenModule(this);
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
            Process.Start(
                @"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WAL2SSDV8ND54&lc=US&item_name=GitExtensions&no_note=1&no_shipping=1&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void lblLogo_MouseClick(object sender, MouseEventArgs e)
        {
#if DEBUG
            _selectedTheme = _selectedTheme == DashboardTheme.Dark ? DashboardTheme.Light : DashboardTheme.Dark;
            ApplyTheme(_selectedTheme);
#endif
        }
    }
}
