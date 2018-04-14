using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public sealed partial class DashboardItem : GitExtensionsControl
    {
        private ToolTip _toolTip;
        private readonly AsyncLoader _branchNameLoader;

        private DashboardItem()
        {
            InitializeComponent();
            Translate();
            flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Icon.Width = Icon.Height;
        }

        public DashboardItem(Repository repository)
            : this()
        {
            if (repository == null)
            {
                return;
            }

            if (AppSettings.DashboardShowCurrentBranch)
            {
                _branchNameLoader = new AsyncLoader();
                _branchNameLoader.LoadAsync(() =>
                {
                    if (!GitModule.IsBareRepository(repository.Path))
                    {
                        return GitModule.GetSelectedBranchFast(repository.Path);
                    }

                    return string.Empty;
                },
                UpdateBranchName);
            }

            Initialize(null, repository.Path, null, null);
        }

        public DashboardItem(Bitmap icon, string title)
            : this()
        {
            Initialize(icon, title, title, null);
        }

        public void Close()
        {
            _toolTip?.RemoveAll();
        }

        private void Initialize(Bitmap icon, string path, string title, string text)
        {
            _NO_TRANSLATE_Title.Text = title;
            _NO_TRANSLATE_Title.AutoEllipsis = true;

            Path = path;

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Title.Text))
            {
                _NO_TRANSLATE_Title.Text = Path;
            }

            bool hasDescription = !string.IsNullOrEmpty(text);
            if (!hasDescription)
            {
                _NO_TRANSLATE_Description.AutoSize = false;
                _NO_TRANSLATE_Description.Size = Size.Empty;
            }

            _NO_TRANSLATE_Description.Text = text;

            if (icon != null)
            {
                Icon.Image = icon;
            }

            _toolTip = new ToolTip
            {
                InitialDelay = 1,
                AutomaticDelay = 100,
                AutoPopDelay = 5000,
                UseFading = false,
                UseAnimation = false,
                ReshowDelay = 1
            };
            _toolTip.SetToolTip(_NO_TRANSLATE_Title, Path);

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
            _branchNameLoader?.Cancel();
        }

        private void Title_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void Title_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip?.Show((Control)sender, e.Location);
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
            BackColor = SystemColors.ControlLight;
        }

        private void DashboardItem_MouseLeave(object sender, EventArgs e)
        {
            if ((sender == Icon || sender == _NO_TRANSLATE_Title) &&
                ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                return;
            }

            BackColor = SystemColors.Control;
        }

        private void DashboardItem_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
                CancelBranchNameLoad();
            }
        }

        private void OnKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                OnClick(e);
            }
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
                _branchNameLoader?.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
