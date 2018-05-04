using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormOpenDirectory : GitExtensionsForm
    {
        private readonly TranslationString _warningOpenFailed =
            new TranslationString("Directory does not exist.");

        private readonly TranslationString _warningOpenFailedCaption =
            new TranslationString("Error");

        private GitModule _choosenModule;

        public FormOpenDirectory(GitModule currentModule)
        {
            InitializeComponent();
            Translate();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                _NO_TRANSLATE_Directory.DataSource = GetDirectories(currentModule, repositoryHistory);
                Load.Select();
                _NO_TRANSLATE_Directory.Focus();
                _NO_TRANSLATE_Directory.Select();
            });
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            // scale up for hi DPI
            MaximumSize = DpiUtil.Scale(new Size(800, 116));
            MinimumSize = DpiUtil.Scale(new Size(450, 116));
        }

        private static IReadOnlyList<string> GetDirectories(GitModule currentModule, IEnumerable<Repository> repositoryHistory)
        {
            List<string> directories = new List<string>();

            if (AppSettings.DefaultCloneDestinationPath.IsNotNullOrWhitespace())
            {
                directories.Add(PathUtil.EnsureTrailingPathSeparator(AppSettings.DefaultCloneDestinationPath));
            }

            if (!string.IsNullOrWhiteSpace(currentModule?.WorkingDir))
            {
                DirectoryInfo di = new DirectoryInfo(currentModule.WorkingDir);
                if (di.Parent != null)
                {
                    directories.Add(PathUtil.EnsureTrailingPathSeparator(di.Parent.FullName));
                }
            }

            directories.AddRange(repositoryHistory.Select(r => r.Path));

            if (directories.Count == 0)
            {
                if (AppSettings.RecentWorkingDir.IsNotNullOrWhitespace())
                {
                    directories.Add(PathUtil.EnsureTrailingPathSeparator(AppSettings.RecentWorkingDir));
                }

                string homeDir = EnvironmentConfiguration.GetHomeDir();
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
                return open._choosenModule;
            }
        }

        private void LoadClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Directory.Text = _NO_TRANSLATE_Directory.Text.Trim();
            if (Directory.Exists(_NO_TRANSLATE_Directory.Text))
            {
                _choosenModule = new GitModule(_NO_TRANSLATE_Directory.Text);
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(_choosenModule.WorkingDir));
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
                {
                    return;
                }

                string parentPath = currentDirectory.Parent.FullName.TrimEnd('\\');
                _NO_TRANSLATE_Directory.Text = parentPath;
                _NO_TRANSLATE_Directory.Focus();
                _NO_TRANSLATE_Directory.Select(_NO_TRANSLATE_Directory.Text.Length, 0);
                SendKeys.Send(@"\");
            }
            catch (Exception)
            {
                // no-op
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
