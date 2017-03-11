using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.SubmodulesDialog
{
    public partial class FormAddSubmodule : GitModuleForm
    {
        private readonly TranslationString _remoteAndLocalPathRequired
            = new TranslationString("A remote path and local path are required");

        public FormAddSubmodule(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
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

            Cursor.Current = Cursors.WaitCursor;
            var addSubmoduleCmd = GitCommandHelpers.AddSubmoduleCmd(Directory.Text, LocalPath.Text, Branch.Text, chkForce.Checked);
            FormProcess.ShowDialog(this, addSubmoduleCmd);

            Close();
            Cursor.Current = Cursors.Default;
        }

        private void DirectorySelectedIndexChanged(object sender, EventArgs e)
        {
            DirectoryTextUpdate(null, null);
        }

        private void FormAddSubmoduleShown(object sender, EventArgs e)
        {
            Directory.DataSource = Repositories.RemoteRepositoryHistory.Repositories;
            Directory.DisplayMember = "Path";
            Directory.Text = "";
            LocalPath.Text = "";
        }

        private void BranchDropDown(object sender, EventArgs e)
        {
            GitModule module = new GitModule(Directory.Text);
            Branch.DisplayMember = "Name";
            IList<IGitRef> heads;
            if (module.IsValidGitWorkingDir())
                heads = module.GetRefs(false);
            else
                heads = new List<IGitRef>();
            heads.Insert(0, GitRef.NoHead(module));
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