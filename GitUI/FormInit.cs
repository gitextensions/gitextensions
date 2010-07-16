using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormInit : GitExtensionsForm
    {
        TranslationString chooseDirectory = new TranslationString("Please choose a directory.");
        TranslationString chooseDirectoryNotFile = new TranslationString("Cannot initialize a new repository on a file.\nPlease choose a directory.");

        public FormInit(string dir)
        {
            InitializeComponent(); Translate();
            Directory.Text = dir;
        }

        public FormInit()
        {
            InitializeComponent(); Translate();

            if (!GitCommands.Settings.ValidWorkingDir())
                Directory.Text = GitCommands.Settings.WorkingDir;
        }

        private void Directory_DropDown(object sender, EventArgs e)
        {
            Directory.DataSource = GitCommands.Repositories.RepositoryHistory.Repositories;
            Directory.DisplayMember = "Path";
        }

        private void Init_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Directory.Text))
            {
                MessageBox.Show(chooseDirectory.Text);
                return;
            }

            if (File.Exists(Directory.Text))
            {
                MessageBox.Show(chooseDirectoryNotFile.Text);
                return;
            }

            GitCommands.Settings.WorkingDir = Directory.Text;

            if (!System.IO.Directory.Exists(GitCommands.Settings.WorkingDir))
                System.IO.Directory.CreateDirectory(GitCommands.Settings.WorkingDir);
            
            MessageBox.Show(GitCommands.GitCommands.Init(Central.Checked, Central.Checked), "Initialize new repository");

            GitCommands.Repositories.RepositoryHistory.AddMostRecentRepository(Directory.Text);
            
            Close();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            
            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                Directory.Text = browseDialog.SelectedPath;
            }
        }

        private void FormInit_Load(object sender, EventArgs e)
        {

        }
    }
}
