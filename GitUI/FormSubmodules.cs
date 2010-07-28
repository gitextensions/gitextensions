using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormSubmodules : GitExtensionsForm
    {
        public FormSubmodules()
        {
            InitializeComponent();
            Translate();
        }

        private void FormSubmodulesFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("submodules");
        }

        private void FormSubmodulesLoad(object sender, EventArgs e)
        {
            RestorePosition("submodules");
        }

        private void AddSubmoduleClick(object sender, EventArgs e)
        {
            var formAddSubmodule = new FormAddSubmodule();
            formAddSubmodule.ShowDialog();
            Initialize();
        }

        private void FormSubmodulesShown(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            Cursor.Current = Cursors.WaitCursor;
            Submodules.DataSource = (new GitCommands.GitCommands()).GetSubmodules();
        }

        private void SubmodulesSelectionChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Submodules.SelectedRows.Count != 1) 
                return;

            var submodule = Submodules.SelectedRows[0].DataBoundItem as GitSubmodule;
            if (submodule == null) 
                return;

            SubModuleName.Text = submodule.Name;
            SubModuleRemotePath.Text = submodule.RemotePath;
            SubModuleLocalPath.Text = submodule.LocalPath;
            SubModuleCommit.Text = submodule.CurrentCommitGuid;
            SubModuleBranch.Text = submodule.Branch;
            SubModuleStatus.Text = submodule.Status;
        }

        private void SynchronizeSubmoduleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommands.GitCommands.SubmoduleSyncCmd(SubModuleName.Text));
            process.ShowDialog();
            Initialize();
        }

        private void InitSubmoduleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(SubModuleName.Text));
            process.ShowDialog();
            Initialize();
        }

        private void UpdateSubmoduleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GitUICommands.Instance.StartUpdateSubmodulesDialog();
            Initialize();
        }
    }
}