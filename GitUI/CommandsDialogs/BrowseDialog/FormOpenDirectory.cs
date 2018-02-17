﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormOpenDirectory : GitExtensionsForm
    {
        private readonly TranslationString _warningOpenFailed =
            new TranslationString("Directory does not exist.");

        private readonly TranslationString _warningOpenFailedCaption =
            new TranslationString("Error");

        private GitModule chosenModule;

        public FormOpenDirectory(GitModule currentModule)
        {
            InitializeComponent();
            Translate();

            _NO_TRANSLATE_Directory.DataSource = GetDirectories(currentModule);

            Load.Select();

            _NO_TRANSLATE_Directory.Focus();
            _NO_TRANSLATE_Directory.Select();
        }

        private static IList<string> GetDirectories(GitModule currentModule)
        {
            List<string> directories = new List<string>();

            if (AppSettings.DefaultCloneDestinationPath.IsNotNullOrWhitespace())
            {
                directories.Add(AppSettings.DefaultCloneDestinationPath.EnsureTrailingPathSeparator());
            }

            if (!string.IsNullOrWhiteSpace(currentModule?.WorkingDir))
            {
                DirectoryInfo di = new DirectoryInfo(currentModule.WorkingDir);
                if (di.Parent != null)
                {
                    directories.Add(di.Parent.FullName.EnsureTrailingPathSeparator());
                }
            }

            directories.AddRange(Repositories.RepositoryHistory.Repositories.Select(r => r.Path));

            if (directories.Count == 0)
            {
                if (AppSettings.RecentWorkingDir.IsNotNullOrWhitespace())
                {
                    directories.Add(AppSettings.RecentWorkingDir.EnsureTrailingPathSeparator());
                }

                string homeDir = GitCommandHelpers.GetHomeDir();
                if (homeDir.IsNotNullOrWhitespace())
                {
                    directories.Add(homeDir.EnsureTrailingPathSeparator());
                }
            }

            return directories.Distinct().ToList();
        }

        public static GitModule OpenModule(IWin32Window owner, GitModule currentModule)
        {
            using (var open = new FormOpenDirectory(currentModule))
            {
                open.ShowDialog(owner);
                return open.chosenModule;
            }
        }

        private void LoadClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Directory.Text = _NO_TRANSLATE_Directory.Text.Trim();
            if (Directory.Exists(_NO_TRANSLATE_Directory.Text))
            {
                chosenModule = new GitModule(_NO_TRANSLATE_Directory.Text);
                Repositories.AddMostRecentRepository(chosenModule.WorkingDir);
                Close();
            }
            else
            {
                MessageBox.Show(this, _warningOpenFailed.Text, _warningOpenFailedCaption.Text);
            }
        }

        private void DirectoryKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                LoadClick(null, null);
            }
        }

        private void folderGoUpButton_Click(object sender, EventArgs e)
        {
            try
            {
                DirectoryInfo currentDirectory = new DirectoryInfo(_NO_TRANSLATE_Directory.Text);
                if (currentDirectory.Parent == null)
                    return;
                string parentPath = currentDirectory.Parent.FullName.TrimEnd('\\');
                _NO_TRANSLATE_Directory.Text = parentPath;
                _NO_TRANSLATE_Directory.Focus();
                _NO_TRANSLATE_Directory.Select(_NO_TRANSLATE_Directory.Text.Length, 0);
                SendKeys.Send(@"\");
            }
            catch (Exception)
            {
            }
        }

        private void _NO_TRANSLATE_Directory_TextChanged(object sender, EventArgs e)
        {
            try
            {
                DirectoryInfo currentDirectory = new DirectoryInfo(_NO_TRANSLATE_Directory.Text);
                folderGoUpButton.Enabled = currentDirectory.Exists && currentDirectory.Parent != null;
            }
            catch (Exception)
            {
                folderGoUpButton.Enabled = false;
            }
        }
    }
}
