using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Repository;
using GitUI.Properties;

namespace GitUI
{
    public sealed partial class DashboardItem : GitExtensionsControl
    {
        private ToolTip toolTip;

        private DashboardItem()
        {
            InitializeComponent();
            Translate();
        }

        public DashboardItem(Repository repository)
            : this()
        {
            if (repository == null)
                return;

            Bitmap icon = GetRepositoryIcon(repository);


            string branchName = string.Empty;

            if (GitCommands.Settings.DashboardShowCurrentBranch)
            {
                if (!GitCommands.GitModule.IsBareRepository(repository.Path))
                    branchName = GitCommands.GitModule.GetSelectedBranchFast(repository.Path);
            }

            Initialize(icon, repository.Path, repository.Title, repository.Description, branchName);
        }

        public DashboardItem(Bitmap icon, string title)
            : this()
        {
            Initialize(icon, title, title, null, null);
        }

        public void Close()
        {
            if (toolTip != null)
                toolTip.RemoveAll();
        }

        private void Initialize(Bitmap icon, string path, string title, string text, string branchName)
        {
            _NO_TRANSLATE_Title.Text = title;
            _NO_TRANSLATE_Title.AutoEllipsis = true;

            Path = path;

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Title.Text))
                _NO_TRANSLATE_Title.Text = Path;

            _NO_TRANSLATE_Description.Visible = !string.IsNullOrEmpty(text);
            _NO_TRANSLATE_Description.Text = text;

            //if (Description.Visible)
            //{
            //    SizeF size = Description.CreateGraphics().MeasureString(Description.Text, Description.Font);
            //    int lines = ((int)size.Width / (int)Description.Width) + 1;
            //    Description.Height = ((int)size.Height) * lines;
            //}

            _NO_TRANSLATE_BranchName.Visible = !string.IsNullOrEmpty(branchName);
            _NO_TRANSLATE_BranchName.Text = branchName;

            Height = _NO_TRANSLATE_Title.Height + 6;
            if (_NO_TRANSLATE_Description.Visible)
            {
                _NO_TRANSLATE_Description.Top = _NO_TRANSLATE_Title.Height + 4;
                Height += _NO_TRANSLATE_Description.Height + 2;
            }


            if (icon != null)
                Icon.Image = icon;


            toolTip = new ToolTip
                              {
                                  InitialDelay = 1,
                                  AutomaticDelay = 1,
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
            _NO_TRANSLATE_Title.Width = Width - _NO_TRANSLATE_Title.Location.X;
            _NO_TRANSLATE_Description.Width = Width - _NO_TRANSLATE_Title.Location.X;
        }

        private void DashboardItem_MouseEnter(object sender, EventArgs e)
        {
            BackColor = SystemColors.ControlLight;
        }

        private void DashboardItem_MouseLeave(object sender, EventArgs e)
        {
            BackColor = SystemColors.Control;
        }

        private static Bitmap GetRepositoryIcon(Repository repository)
        {
            switch (repository.RepositoryType)
            {
                case RepositoryType.Repository:
                    return Resources.Star;
                case RepositoryType.RssFeed:
                    return Resources.rss.ToBitmap();
                case RepositoryType.History:
                    return Resources.history.ToBitmap();
                default:
                    throw new ArgumentException("Repository type is not supported.", "repository");
            }
        }
    }
}
