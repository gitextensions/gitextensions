using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Repository;
using GitUI.Properties;

namespace GitUI
{
    public partial class DashboardItem : GitExtensionsControl
    {
        public DashboardItem(Repository repository)
        {
            InitializeComponent(); Translate();

            if (repository == null)
                return;

            Bitmap icon = null;
            if (repository.RepositoryType == RepositoryType.RssFeed)
                icon = Resources.rss.ToBitmap();
            if (repository.RepositoryType == RepositoryType.Repository)
                icon = Resources._14;
            if (repository.RepositoryType == RepositoryType.History)
                icon = Resources.history.ToBitmap();

            Initialize(icon, repository.Path, repository.Title, repository.Description);
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        public DashboardItem(Bitmap icon, string title)
        {
            InitializeComponent(); Translate();

            Initialize(icon, title, title, null);
        }

        public DashboardItem(Bitmap icon, string title, string text)
        {
            InitializeComponent(); Translate();

            Initialize(icon, title, title, text);
        }

        private void Initialize(Bitmap icon, string path, string title, string text)
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


            Height = _NO_TRANSLATE_Title.Height + 6;
            if (_NO_TRANSLATE_Description.Visible)
            {
                _NO_TRANSLATE_Description.Top = _NO_TRANSLATE_Title.Height + 4;
                Height += _NO_TRANSLATE_Description.Height + 2;
            }



            if (icon != null)
                Icon.Image = icon;


            var toolTip = new ToolTip
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

        public string GetTitle()
        {
            return _NO_TRANSLATE_Title.Text;
        }

        public string Path { get; set; }

        private void DashboardItem_Resize(object sender, EventArgs e)
        {

        }

        private void DashboardItem_SizeChanged(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Title.Width = Width - _NO_TRANSLATE_Title.Location.X;
            _NO_TRANSLATE_Description.Width = Width - _NO_TRANSLATE_Title.Location.X;
        }

        private void DashboardItem_Enter(object sender, EventArgs e)
        {

        }

        private void DashboardItem_Leave(object sender, EventArgs e)
        {

        }

        private void DashboardItem_MouseEnter(object sender, EventArgs e)
        {
            BackColor = SystemColors.ControlLight;
        }

        private void DashboardItem_MouseLeave(object sender, EventArgs e)
        {
            BackColor = SystemColors.Control;
        }
    }
}
