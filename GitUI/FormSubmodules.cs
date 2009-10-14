using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormSubmodules : GitExtensionsForm
    {
        public FormSubmodules()
        {
            InitializeComponent();
        }

        private void FormSubmodules_Load(object sender, EventArgs e)
        {
            
        }

        private void AddSubmodule_Click(object sender, EventArgs e)
        {
            FormAddSubmodule formAddSubmodule = new FormAddSubmodule();
            formAddSubmodule.ShowDialog();
            Initialize();
        }

        private void FormSubmodules_Shown(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            Cursor.Current = Cursors.WaitCursor;
            Submodules.DataSource = (new GitCommands.GitCommands()).GetSubmodules();
        }

        private void Submodules_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Submodules_SelectionChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Submodules.SelectedRows.Count == 1)
            {
                GitSubmodule submodule = Submodules.SelectedRows[0].DataBoundItem as GitSubmodule;
                if (submodule != null)
                {
                    SubModuleName.Text = submodule.Name;
                    SubModuleRemotePath.Text = submodule.RemotePath;
                    SubModuleLocalPath.Text = submodule.LocalPath;
                    SubModuleCommit.Text = submodule.CurrentCommitGuid;
                    SubModuleBranch.Text = submodule.Branch;
                    SubModuleStatus.Text = submodule.Status;
                }
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void SynchronizeSubmodule_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleSyncCmd(SubModuleName.Text));
            Initialize();
        }

        private void InitSubmodule_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(SubModuleName.Text));
            Initialize();
        }

        private void UpdateSubmodule_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GitUICommands.Instance.StartUpdateSubmodulesDialog();
            Initialize();
        }

    }
}
