using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AutoCheckForUpdates
{
    public partial class Updates : Form
    {
        public Updates(string url)
        {
            InitializeComponent();
            link.Text = url;
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(link.Text);
        }
    }
}
