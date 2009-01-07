using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormInit : Form
    {
        public FormInit(string dir)
        {
            InitializeComponent();
            Directory.Text = dir;
        }

        public FormInit()
        {
            InitializeComponent();
        }

        private void Directory_DropDown(object sender, EventArgs e)
        {
            Directory.DataSource = RepositoryHistory.MostRecentRepositories;
        }

        private void Init_Click(object sender, EventArgs e)
        {
            GitCommands.Settings.WorkingDir = Directory.Text;

            if (!System.IO.Directory.Exists(GitCommands.Settings.WorkingDir))
                System.IO.Directory.CreateDirectory(GitCommands.Settings.WorkingDir);
            
            MessageBox.Show(GitCommands.GitCommands.Init(Central.Checked, Central.Checked), "Initialize new repository");

            RepositoryHistory.AddMostRecentRepository(Directory.Text);
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            
            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                Directory.Text = browseDialog.SelectedPath;
            }
        }
    }
}
