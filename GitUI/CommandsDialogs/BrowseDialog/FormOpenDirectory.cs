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
    }
}