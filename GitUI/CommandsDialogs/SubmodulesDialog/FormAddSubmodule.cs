using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.SubmodulesDialog
{
    public partial class FormAddSubmodule : GitModuleForm
    {
        private readonly TranslationString _remoteAndLocalPathRequired
            = new TranslationString("A remote path and local path are required");

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormAddSubmodule()
        {
            InitializeComponent();
        }

        public FormAddSubmodule(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = await RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                Directory.DataSource = repositoryHistory;
                Directory.DisplayMember = nameof(Repository.Path);
                Directory.Text = "";
                LocalPath.Text = "";
            });
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this, Directory.Text);

            if (userSelectedPath != null)
            {
                Directory.Text = userSelectedPath;
            }
        }

        private void AddClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Directory.Text) || string.IsNullOrEmpty(LocalPath.Text))
            {
                MessageBox.Show(this, _remoteAndLocalPathRequired.Text, Text);
                return;
            }

            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.AddSubmoduleCmd(Directory.Text, LocalPath.Text, Branch.Text, chkForce.Checked));

                Close();
            }
        }

        private void DirectorySelectedIndexChanged(object sender, EventArgs e)
        {
            DirectoryTextUpdate(null, null);
        }

        private void BranchDropDown(object sender, EventArgs e)
        {
            var module = new GitModule(Directory.Text);

            var heads = new List<IGitRef>
            {
                GitRef.NoHead(module)
            };

            if (module.IsValidGitWorkingDir())
            {
                heads.AddRange(module.GetRefs(false));
            }

            Branch.DisplayMember = nameof(IGitRef.Name);
            Branch.DataSource = heads;
        }

        private void DirectoryTextUpdate(object sender, EventArgs e)
        {
            string path = PathUtil.GetRepositoryName(Directory.Text);

            if (path != "")
            {
                LocalPath.Text = path;
            }
        }
    }
}