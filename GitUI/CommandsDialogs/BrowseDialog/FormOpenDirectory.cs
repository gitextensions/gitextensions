using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager;
using System.Collections.Generic;
using System.Linq;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormOpenDirectory : GitExtensionsForm
    {
        private readonly TranslationString _warningOpenFailed =
            new TranslationString("Directory does not exist.");

        private readonly TranslationString _warningOpenFailedCaption =
            new TranslationString("Error");

        private GitModule choosenModule = null;

        public FormOpenDirectory(GitModule currentModule)
        {
            InitializeComponent();
            Translate();

            _NO_TRANSLATE_Directory.DataSource = GetDirectories(currentModule);

            Load.Select();

            _NO_TRANSLATE_Directory.Focus();
            _NO_TRANSLATE_Directory.Select();
        }

        private IList<string> GetDirectories(GitModule currentModule)
        {
            List<string> directories = new List<string>();

            if (AppSettings.DefaultCloneDestinationPath.IsNotNullOrWhitespace())
            {
                directories.Add(PathUtil.EnsureTrailingPathSeparator(AppSettings.DefaultCloneDestinationPath));
            }

            if (currentModule != null && !string.IsNullOrWhiteSpace(currentModule.WorkingDir))
            {
                DirectoryInfo di = new DirectoryInfo(currentModule.WorkingDir);
                if (di.Parent != null)
                {
                    directories.Add(PathUtil.EnsureTrailingPathSeparator(di.Parent.FullName));
                }
            }

            directories.AddRange(Repositories.RepositoryHistory.Repositories.Select(r => r.Path));

            if (directories.Count == 0)
            {
                if (AppSettings.RecentWorkingDir.IsNotNullOrWhitespace())
                {
                    directories.Add(PathUtil.EnsureTrailingPathSeparator(AppSettings.RecentWorkingDir));
                }

                string homeDir = GitCommandHelpers.GetHomeDir();
                if (homeDir.IsNotNullOrWhitespace())
                {
                    directories.Add(PathUtil.EnsureTrailingPathSeparator(homeDir));
                }
            }
            
            return directories.Distinct().ToList();
        }

        public static GitModule OpenModule(IWin32Window owner, GitModule currentModule)
        {
            using (var open = new FormOpenDirectory(currentModule))
            {
                open.ShowDialog(owner);
                return open.choosenModule;
            }
        }

        private void LoadClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Directory.Text = _NO_TRANSLATE_Directory.Text.Trim();
            if (Directory.Exists(_NO_TRANSLATE_Directory.Text))
            {
                choosenModule = new GitModule(_NO_TRANSLATE_Directory.Text);
                Repositories.AddMostRecentRepository(choosenModule.WorkingDir);
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

        private void folderGoUpbutton_Click(object sender, EventArgs e)
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
                folderGoUpbutton.Enabled = currentDirectory.Exists && currentDirectory.Parent != null;
            }
            catch (Exception)
            {
                folderGoUpbutton.Enabled = false;
            }
        }
    }
}
