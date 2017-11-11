using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Repository;
using GitUI.Properties;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public sealed partial class DashboardItem : GitExtensionsControl
    {
        private ToolTip toolTip;
        private readonly AsyncLoader _branchNameLoader;

        private DashboardItem()
        {
            InitializeComponent();
            Translate();
            this.flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Icon.Width = this.Icon.Height;
        }

        public DashboardItem(Repository repository)
            : this()
        {
            if (repository == null)
                return;

            Bitmap icon = GetRepositoryIcon(repository);


            if (AppSettings.DashboardShowCurrentBranch)
            {
                _branchNameLoader = new AsyncLoader();
                _branchNameLoader.Load(() =>
                {
                    if (!GitCommands.GitModule.IsBareRepository(repository.Path))
                    {
                        return GitModule.GetSelectedBranchFast(repository.Path);
                    }
                    return string.Empty;
                },
                UpdateBranchName);
            }

            Initialize(icon, repository.Path, repository.Title, repository.Description);
        }

        public DashboardItem(Bitmap icon, string title)
            : this()
        {
            Initialize(icon, title, title, null);
        }

        public void Close()
        {
            if (toolTip != null)
                toolTip.RemoveAll();
        }

        private void Initialize(Bitmap icon, string path, string title, string text)
        {
            _NO_TRANSLATE_Title.Text = title;
            _NO_TRANSLATE_Title.AutoEllipsis = true;

            Path = path;

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Title.Text))
                _NO_TRANSLATE_Title.Text = Path;

            bool hasDescription = !string.IsNullOrEmpty(text);
            if (!hasDescription)
            {
                _NO_TRANSLATE_Description.AutoSize = false;
                _NO_TRANSLATE_Description.Size = Size.Empty;
            }
            _NO_TRANSLATE_Description.Text = text;

            if (icon != null)
                Icon.Image = icon;

            toolTip = new ToolTip
                              {
                                  InitialDelay = 1,
                                  AutomaticDelay = 100,
                                  AutoPopDelay = 5000,
                                  UseFading = false,
                                  UseAnimation = false,
                                  ReshowDelay = 1
                              };
            toolTip.SetToolTip(_NO_TRANSLATE_Title, Path);

            _NO_TRANSLATE_Title.MouseDown += Title_MouseDown;
            _NO_TRANSLATE_Title.Click += Title_Click;
            _NO_TRANSLATE_Description.Click += Title_Click;
            Icon.Click += Title_Click;
        }

        private void UpdateBranchName(string branchName)
        {
            _NO_TRANSLATE_BranchName.Visible = !string.IsNullOrEmpty(branchName);
            _NO_TRANSLATE_BranchName.Text = branchName;
        }

        private void CancelBranchNameLoad()
        {
            if (_branchNameLoader != null)
            {
                _branchNameLoader.Cancel();
            }
        }

        void Title_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        void Title_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (ContextMenuStrip != null)
                    ContextMenuStrip.Show((Control)sender, e.Location);
            }
        }

        public string Path { get; private set; }

        private void DashboardItem_SizeChanged(object sender, EventArgs e)
        {
            ////_NO_TRANSLATE_Title.Width = Width - _NO_TRANSLATE_Title.Location.X;
            ////_NO_TRANSLATE_Description.Width = Width - _NO_TRANSLATE_Title.Location.X;
        }

        private void DashboardItem_MouseEnter(object sender, EventArgs e)
        {
            this.BackColor = SystemColors.ControlLight;
        }

        private void DashboardItem_MouseLeave(object sender, EventArgs e)
        {
            if ((sender == Icon || sender == _NO_TRANSLATE_Title) &&
                this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                return;

            this.BackColor = SystemColors.Control;
        }

        void DashboardItem_VisibleChanged(object sender, System.EventArgs e)
        {
            if (!Visible)
            {
                CancelBranchNameLoad();
            }
        }

        private static Bitmap GetRepositoryIcon(Repository repository)
        {
            switch (repository.RepositoryType)
            {
                case RepositoryType.Repository:
                    return Resources.Star;
                case RepositoryType.RssFeed:
                    return Resources.rss;
                case RepositoryType.History:
                    return Resources.history;
                default:
                    throw new ArgumentException("Repository type is not supported.", "repository");
            }
        }

        private void OnKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                OnClick(e);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelBranchNameLoad();
                if (_branchNameLoader != null)
                    _branchNameLoader.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
