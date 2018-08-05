using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormOpenDirectory : GitExtensionsForm
    {
        private readonly TranslationString _warningOpenFailed = new TranslationString("Directory does not exist.");
        private readonly TranslationString _warningOpenFailedCaption = new TranslationString("Error");

        [CanBeNull] private GitModule _chosenModule;

        public FormOpenDirectory([CanBeNull] GitModule currentModule)
        {
            InitializeComponent();
            InitializeComplete();

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

        private static IReadOnlyList<string> GetDirectories([CanBeNull] GitModule currentModule, IEnumerable<Repository> repositoryHistory)
        {
            var directories = new List<string>();

            if (AppSettings.DefaultCloneDestinationPath.IsNotNullOrWhitespace())
            {
                directories.Add(AppSettings.DefaultCloneDestinationPath.EnsureTrailingPathSeparator());
            }

            if (!string.IsNullOrWhiteSpace(currentModule?.WorkingDir))
            {
                var di = new DirectoryInfo(currentModule.WorkingDir);
                if (di.Parent != null)
                {
                    directories.Add(di.Parent.FullName.EnsureTrailingPathSeparator());
                }
            }

            directories.AddRange(repositoryHistory.Select(r => r.Path));

            if (directories.Count == 0)
            {
                if (AppSettings.RecentWorkingDir.IsNotNullOrWhitespace())
                {
                    directories.Add(AppSettings.RecentWorkingDir.EnsureTrailingPathSeparator());
                }

                string homeDir = EnvironmentConfiguration.GetHomeDir();
                if (homeDir.IsNotNullOrWhitespace())
                {
                    directories.Add(homeDir.EnsureTrailingPathSeparator());
                }
            }

            return directories.Distinct().ToList();
        }

        [CanBeNull]
        public static GitModule OpenModule(IWin32Window owner, [CanBeNull] GitModule currentModule)
        {
            using (var open = new FormOpenDirectory(currentModule))
            {
                open.ShowDialog(owner);
                return open._chosenModule;
            }
        }

        private void LoadClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Directory.Text = _NO_TRANSLATE_Directory.Text.Trim();
            if (Directory.Exists(_NO_TRANSLATE_Directory.Text))
            {
                _chosenModule = new GitModule(_NO_TRANSLATE_Directory.Text);
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(_chosenModule.WorkingDir));
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
                var currentDirectory = new DirectoryInfo(_NO_TRANSLATE_Directory.Text);
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
            catch
            {
                // no-op
            }
        }

        private void _NO_TRANSLATE_Directory_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var currentDirectory = new DirectoryInfo(_NO_TRANSLATE_Directory.Text);
                folderGoUpButton.Enabled = currentDirectory.Exists && currentDirectory.Parent != null;
            }
            catch
            {
                folderGoUpButton.Enabled = false;
            }
        }
    }
}
