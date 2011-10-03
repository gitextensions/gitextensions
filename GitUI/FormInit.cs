﻿using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormInit : GitExtensionsForm
    {
        private readonly TranslationString _chooseDirectory =
            new TranslationString("Please choose a directory.");

        private readonly TranslationString _chooseDirectoryCaption =
            new TranslationString("Choose directory");

        private readonly TranslationString _chooseDirectoryNotFile =
            new TranslationString("Cannot initialize a new repository on a file.\nPlease choose a directory.");

        private readonly TranslationString _chooseDirectoryNotFileCaption =
            new TranslationString("Error");

        private readonly TranslationString _initMsgBoxCaption =
            new TranslationString("Initialize new repository");

        

        public FormInit(string dir)
        {
            InitializeComponent();
            Translate();
            Directory.Text = dir;
        }

        public FormInit()
        {
            InitializeComponent();
            Translate();

            if (!Settings.ValidWorkingDir())
                Directory.Text = Settings.WorkingDir;
        }

        private void DirectoryDropDown(object sender, EventArgs e)
        {
            Directory.DataSource = Repositories.RepositoryHistory.Repositories;
            Directory.DisplayMember = "Path";
        }

        private void InitClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Directory.Text))
            {
                MessageBox.Show(_chooseDirectory.Text,_chooseDirectoryCaption.Text);
                return;
            }

            if (File.Exists(Directory.Text))
            {
                MessageBox.Show(_chooseDirectoryNotFile.Text,_chooseDirectoryNotFileCaption.Text);
                return;
            }

            Settings.WorkingDir = Directory.Text;

            if (!System.IO.Directory.Exists(Settings.WorkingDir))
                System.IO.Directory.CreateDirectory(Settings.WorkingDir);

            MessageBox.Show(GitCommandHelpers.Init(Central.Checked, Central.Checked), _initMsgBoxCaption.Text);

            Repositories.RepositoryHistory.AddMostRecentRepository(Directory.Text);

            Close();
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == DialogResult.OK)
                Directory.Text = browseDialog.SelectedPath;
        }
    }
}