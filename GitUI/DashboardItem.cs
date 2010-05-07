using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class DashboardItem : UserControl
    {
        public DashboardItem(Repository repository)
        {
            InitializeComponent();

            Bitmap icon = null;
            if (repository.Icon != null)
                icon = repository.Icon.ToBitmap();
            Initialize(icon, repository.Path, repository.Description);
        }

        public DashboardItem(Bitmap icon, string title)
        {
            InitializeComponent();

            Initialize(icon, title, null);
        }

        public DashboardItem(Bitmap icon, string title, string text)
        {
            InitializeComponent();

            Initialize(icon, title, text);
        }

        private void Initialize(Bitmap icon, string title, string text)
        {
            Title.Text = title;
            Title.AutoEllipsis = true;

            Description.Visible = !string.IsNullOrEmpty(text);
            Description.Text = text;

            if (Description.Visible)
                this.Height = 35;
            else
                this.Height = 20;

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

        //public override ContextMenuStrip ContextMenuStrip
        //{
        //    get
        //    {
        //        return Title.ContextMenuStrip;
        //    }
        //    set
        //    {
        //        Title.ContextMenuStrip = value;
        //    }
        //}

        //public override ContextMenu ContextMenu
        //{
        //    get
        //    {
        //        return Title.ContextMenu;
        //    }
        //    set
        //    {
        //        Title.ContextMenu = value;
        //    }
        //}

    }
}
