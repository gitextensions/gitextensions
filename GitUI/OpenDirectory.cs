using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.IO;

namespace GitUI
{
    public partial class Open : GitExtensionsForm
    {
        public Open()
        {
            InitializeComponent();

            Directory.DataSource = RepositoryHistory.MostRecentRepositories;

            Load.Select();

            Directory.Focus();
            Directory.Select();

        }

        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.SelectedPath = Directory.Text;

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                Directory.Text = browseDialog.SelectedPath;
            }
        }

        private void Load_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(Directory.Text))
            {
                Settings.WorkingDir = Directory.Text;

                RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);

                Close();
            }
            else
            {
                MessageBox.Show("Directory does not exist.", "Error");
            }
        }

        private void Directory_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Load_Click(null, null);
            }
        }

        private void Directory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Open_Load(object sender, EventArgs e)
        {

        }

    }
}
