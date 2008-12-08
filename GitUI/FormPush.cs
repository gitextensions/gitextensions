using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormPush : Form
    {
        public FormPush()
        {
            InitializeComponent();
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                PushDestination.Text = dialog.SelectedPath;
            
        }

        private void Push_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PushDestination.Text))
            {
                MessageBox.Show("Please select a destination directory");
                return;
            }

            Output.Text = GitCommands.GitCommands.Push(PushDestination.Text);
        }
    }
}
