using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtUtils;
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
            Branch.DataSource = LoadRemoteRepoBranches(Module.GitExecutable, url: Directory.Text);
        }

        private void DirectoryTextUpdate(object sender, EventArgs e)
        {
            string path = PathUtil.GetRepositoryName(Directory.Text);

            if (path != "")
            {
                LocalPath.Text = path;
            }
        }

        /// <summary>
        /// Returns the branches of a remote repository as strings; ignores git errors and warnings.
        /// </summary>
        /// 'git ls-remotes --heads "URL"' is completely independent from a local repo clone.
        /// Hence there is no need for a GitModule.
        /// <param name="gitExecutable">the git executable</param>
        /// <param name="url">the repo URL; can also be a local path</param>
        private static IEnumerable<string> LoadRemoteRepoBranches(IExecutable gitExecutable, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Array.Empty<string>();
            }

            var gitArguments = new GitArgumentBuilder("ls-remote") { "--heads", url.ToPosixPath().Quote() };
            var heads = gitExecutable.GetOutput(gitArguments);
            return heads.SplitLines()
                        .Select(head =>
                        {
                            int branchIndex = head.IndexOf(GitRefName.RefsHeadsPrefix);
                            return branchIndex == -1 ? null : head.Substring(branchIndex + GitRefName.RefsHeadsPrefix.Length);
                        })
                        .Where(branch => branch != null)
                        .ToImmutableList();
        }

        internal readonly struct TestAccessor
        {
            public static IEnumerable<string> LoadRemoteRepoBranches(IExecutable gitExecutable, string url)
                => FormAddSubmodule.LoadRemoteRepoBranches(gitExecutable, url);
        }
    }
}