﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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

        private DashboardTheme _selectedTheme;

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;

        public Dashboard()
        {
            InitializeComponent();
            InitializeComplete();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
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

        public void RefreshContent()
        {
            InitDashboardLayout();
            ApplyTheme();
            userRepositoriesList.ShowRecentRepositories();

            void ApplyTheme()
            {
                _selectedTheme = ColorHelper.IsLightTheme() ? DashboardTheme.Light : DashboardTheme.Dark;
                BackgroundImage = _selectedTheme.BackgroundImage;

                BackColor = _selectedTheme.Primary;
                pnlLogo.BackColor = _selectedTheme.PrimaryVeryDark;
                flpnlStart.BackColor = _selectedTheme.PrimaryLight;
                flpnlContribute.BackColor = _selectedTheme.PrimaryVeryLight;
                lblContribute.ForeColor = _selectedTheme.SecondaryHeadingText;
                userRepositoriesList.BranchNameColor = _selectedTheme.SecondaryText;
                userRepositoriesList.FavouriteColor = _selectedTheme.AccentedText;
                userRepositoriesList.ForeColor = _selectedTheme.PrimaryText;
                userRepositoriesList.HeaderColor = _selectedTheme.SecondaryHeadingText;
                userRepositoriesList.HeaderBackColor = _selectedTheme.PrimaryDark;
                userRepositoriesList.HoverColor = _selectedTheme.PrimaryLight;
                userRepositoriesList.MainBackColor = _selectedTheme.Primary;

                foreach (var item in flpnlContribute.Controls.OfType<LinkLabel>().Union(flpnlStart.Controls.OfType<LinkLabel>()))
                {
                    item.LinkColor = _selectedTheme.PrimaryText;
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
                                lastControl = CreateLink(panel, string.Format(_cloneFork.Text, gitHoster.Description), Images.CloneRepoGitHub,
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

                void AddLinks(Panel panel, Func<Panel, Control> addLinks, Action<Panel, Control> onLayout)
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
                    var linkLabel = new LinkLabel
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
                    linkLabel.MouseHover += (s, e) => linkLabel.LinkColor = _selectedTheme.AccentedText;
                    linkLabel.MouseLeave += (s, e) => linkLabel.LinkColor = _selectedTheme.PrimaryText;

                    if (handler is not null)
                    {
                        linkLabel.Click += handler;
                    }

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
            GitModule module = FormOpenDirectory.OpenModule(this, currentModule: null);
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
