using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;

namespace GitUI
{
    public partial class DashboardItem : UserControl
    {
        public DashboardItem(Repository repository)
        {
            InitializeComponent();

            Bitmap icon = null;
            if (repository.RepositoryType == RepositoryType.RssFeed)
                icon = Resources.rss.ToBitmap();
            if (repository.RepositoryType == RepositoryType.Repository)
                icon = Resources._14;
            if (repository.RepositoryType == RepositoryType.History)
                icon = Resources.history.ToBitmap();

            Initialize(icon, repository.Path, repository.Title, repository.Description);
        }

        public DashboardItem(Bitmap icon, string title)
        {
            InitializeComponent();

            Initialize(icon, title, title, null);
        }

        public DashboardItem(Bitmap icon, string title, string text)
        {
            InitializeComponent();

            Initialize(icon, title, title, text);
        }

        private void Initialize(Bitmap icon, string path, string title, string text)
        {
            Title.Text = title;
            Title.AutoEllipsis = true;

            Path = path;

            if (string.IsNullOrEmpty(Title.Text))
                Title.Text = Path;

            Description.Visible = !string.IsNullOrEmpty(text);
            Description.Text = text;

            //if (Description.Visible)
            //{
            //    SizeF size = Description.CreateGraphics().MeasureString(Description.Text, Description.Font);
            //    int lines = ((int)size.Width / (int)Description.Width) + 1;
            //    Description.Height = ((int)size.Height) * lines;
            //}


            this.Height = 20;
            if (Description.Visible)
                this.Height += Description.Height;
            
                

            if (icon != null)
                Icon.Image = icon;


            ToolTip toolTip = new ToolTip();
            toolTip.InitialDelay = 1;
            toolTip.AutomaticDelay = 1;
            toolTip.AutoPopDelay = 5000;
            toolTip.UseFading = false;
            toolTip.UseAnimation = false;
            toolTip.ReshowDelay = 1;
            toolTip.SetToolTip(Title, Title.Text);

            Title.MouseDown += new MouseEventHandler(Title_MouseDown);
            Title.Click += new EventHandler(Title_Click);
            Description.Click += new EventHandler(Title_Click);
            Icon.Click += new EventHandler(Title_Click);
        }

        void Title_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        void Title_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.ContextMenuStrip != null)
                    this.ContextMenuStrip.Show(e.Location);
            }
        }

        public string GetTitle()
        {
            return Title.Text;
        }

        public string Path { get; set; }

        private void DashboardItem_Resize(object sender, EventArgs e)
        {

        }

        private void DashboardItem_SizeChanged(object sender, EventArgs e)
        {
            Title.Width = Width - Title.Location.X;
            Description.Width = Width - Title.Location.X;
        }

        private void DashboardItem_Enter(object sender, EventArgs e)
        {

        }

        private void DashboardItem_Leave(object sender, EventArgs e)
        {

        }

        private void DashboardItem_MouseEnter(object sender, EventArgs e)
        {
            this.BackColor = SystemColors.ControlLight;
        }

        private void DashboardItem_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = SystemColors.Control;
        }
    }
}
