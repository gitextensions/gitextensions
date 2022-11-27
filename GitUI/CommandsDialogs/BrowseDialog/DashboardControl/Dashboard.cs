using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    [ThemeAware]
    public partial class Dashboard : GitModuleControl
    {
        private readonly TranslationString _cloneFork = new("Clone {0} repository");
        private readonly TranslationString _cloneRepository = new("Clone repository");
        private readonly TranslationString _createRepository = new("Create new repository");
        private readonly TranslationString _develop = new("Develop");
        private readonly TranslationString _donate = new("Donate");
        private readonly TranslationString _issues = new("Issues");
        private readonly TranslationString _openRepository = new("Open repository");
        private readonly TranslationString _translate = new("Translate");

        public event EventHandler<GitModuleEventArgs>? GitModuleChanged;

        public Dashboard()
        {
            InitializeComponent();
            InitializeComplete();

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            Visible = false;

            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.Dock = DockStyle.Fill;
            pnlLeft.Dock = DockStyle.Fill;
            flpnlStart.Dock = DockStyle.Fill;
            flpnlContribute.Dock = DockStyle.Bottom;
            flpnlContribute.SendToBack();

            userRepositoriesList.GitModuleChanged += OnModuleChanged;

            // apply scaling
            pnlLogo.Padding = DpiUtil.Scale(pnlLogo.Padding);
            userRepositoriesList.HeaderHeight = pnlLogo.Height;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // Focus the control in order for the search bar to have focus once the dashboard is shown
            userRepositoriesList.Focus();
        }

        public void RefreshContent()
        {
            DashboardTheme selectedTheme = ColorHelper.IsLightTheme() ? DashboardTheme.Light : DashboardTheme.Dark;

            InitDashboardLayout();
            ApplyTheme();
            userRepositoriesList.ShowRecentRepositories();

            void ApplyTheme()
            {
                BackgroundImage = selectedTheme.BackgroundImage;

                BackColor = selectedTheme.Primary;
                pnlLogo.BackColor = selectedTheme.PrimaryVeryDark;
                flpnlStart.BackColor = selectedTheme.PrimaryLight;
                flpnlContribute.BackColor = selectedTheme.PrimaryVeryLight;
                lblContribute.ForeColor = selectedTheme.SecondaryHeadingText;
                userRepositoriesList.BranchNameColor = selectedTheme.SecondaryText;
                userRepositoriesList.FavouriteColor = selectedTheme.AccentedText;
                userRepositoriesList.ForeColor = selectedTheme.PrimaryText;
                userRepositoriesList.HeaderColor = selectedTheme.SecondaryHeadingText;
                userRepositoriesList.HeaderBackColor = selectedTheme.PrimaryDark;
                userRepositoriesList.HoverColor = selectedTheme.PrimaryLight;
                userRepositoriesList.MainBackColor = selectedTheme.Primary;

                foreach (var item in flpnlContribute.Controls.OfType<LinkLabel>().Union(flpnlStart.Controls.OfType<LinkLabel>()))
                {
                    item.LinkColor = selectedTheme.PrimaryText;
                }

                Invalidate(true);
            }

            void InitDashboardLayout()
            {
                try
                {
                    pnlLeft.SuspendLayout();

                    AddLinks(flpnlContribute,
                        panel =>
                        {
                            panel.Controls.Add(lblContribute);
                            lblContribute.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints + 5.5f);

                            CreateLink(panel, _develop.Text, Images.Develop.AdaptLightness(), GitHubItem_Click);
                            CreateLink(panel, _donate.Text, Images.DollarSign, DonateItem_Click);
                            CreateLink(panel, _translate.Text, Images.Translate.AdaptLightness(), TranslateItem_Click);
                            var lastControl = CreateLink(panel, _issues.Text, Images.Bug, IssuesItem_Click);
                            return lastControl;
                        },
                        (panel, lastControl) =>
                        {
                            var height = lastControl.Location.Y + lastControl.Size.Height + panel.Padding.Bottom;
                            panel.Height = height;
                            panel.MinimumSize = new Size(0, height);
                        });

                    AddLinks(flpnlStart,
                        panel =>
                        {
                            CreateLink(panel, _createRepository.Text, Images.RepoCreate, createItem_Click);
                            CreateLink(panel, _openRepository.Text, Images.RepoOpen, openItem_Click);
                            var lastControl = CreateLink(panel, _cloneRepository.Text, Images.CloneRepoGit, cloneItem_Click);

                            foreach (var gitHoster in PluginRegistry.GitHosters)
                            {
                                lastControl = CreateLink(panel, string.Format(_cloneFork.Text, gitHoster.Name), Images.CloneRepoGitHub,
                                    (repoSender, eventArgs) => UICommands.StartCloneForkFromHoster(this, gitHoster, GitModuleChanged));
                            }

                            return lastControl;
                        },
                        (panel, lastControl) =>
                        {
                            var height = lastControl.Location.Y + lastControl.Size.Height + panel.Padding.Bottom;
                            panel.MinimumSize = new Size(0, height);
                        });
                }
                finally
                {
                    pnlLeft.ResumeLayout(false);
                    pnlLeft.PerformLayout();
                    AutoScrollMinSize = new Size(0, pnlLogo.Height + flpnlStart.MinimumSize.Height + flpnlContribute.MinimumSize.Height);
                }

                static void AddLinks(Panel panel, Func<Panel, Control> addLinks, Action<Panel, Control> onLayout)
                {
                    panel.SuspendLayout();
                    panel.Controls.Clear();

                    var lastControl = addLinks(panel);

                    panel.ResumeLayout(false);
                    panel.PerformLayout();

                    onLayout(panel, lastControl);
                }

                Control CreateLink(Control container, string text, Image icon, EventHandler handler)
                {
                    var padding24 = DpiUtil.Scale(24);
                    var padding3 = DpiUtil.Scale(3);
                    LinkLabel linkLabel = new()
                    {
                        AutoSize = true,
                        AutoEllipsis = true,
                        Font = AppSettings.Font,
                        Image = DpiUtil.Scale(icon),
                        ImageAlign = ContentAlignment.MiddleLeft,
                        LinkBehavior = LinkBehavior.NeverUnderline,
                        Margin = new Padding(padding3, 0, padding3, DpiUtil.Scale(8)),
                        Padding = new Padding(padding24, padding3, padding3, padding3),
                        TabStop = true,
                        Text = text,
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    linkLabel.MouseHover += (s, e) => linkLabel.LinkColor = selectedTheme.AccentedText;
                    linkLabel.MouseLeave += (s, e) => linkLabel.LinkColor = selectedTheme.PrimaryText;
                    linkLabel.Click += handler;

                    container.Controls.Add(linkLabel);

                    return linkLabel;
                }
            }
        }

        protected virtual void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            var handler = GitModuleChanged;
            handler?.Invoke(this, e);
        }

        private void dashboard_ParentChanged(object sender, EventArgs e)
        {
            if (Parent is null)
            {
                Visible = false;
                return;
            }

            Visible = true;
        }

        private static void TranslateItem_Click(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/wiki/Translations");
        }

        private static void GitHubItem_Click(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions");
        }

        private static void IssuesItem_Click(object sender, EventArgs e)
        {
            UserEnvironmentInformation.CopyInformation();
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/issues");
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            GitModule? module = FormOpenDirectory.OpenModule(this, currentModule: null);
            if (module is not null)
            {
                OnModuleChanged(this, new GitModuleEventArgs(module));
            }
        }

        private void cloneItem_Click(object sender, EventArgs e)
        {
            UICommands.StartCloneDialog(this, null, false, OnModuleChanged);
        }

        private void createItem_Click(object sender, EventArgs e)
        {
            UICommands.StartInitializeDialog(this, Module.WorkingDir, OnModuleChanged);
        }

        private static void DonateItem_Click(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(FormDonate.DonationUrl);
        }
    }
}
