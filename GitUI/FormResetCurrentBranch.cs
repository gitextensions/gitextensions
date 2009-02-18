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
    public partial class FormResetCurrentBranch : GitExtensionsForm
    {
        public FormResetCurrentBranch(GitRevision Revision)
        {
            this.Revision = Revision;

            InitializeComponent();
        }

        public GitRevision Revision { get; set; }

        private void FormResetCurrentBranch_Load(object sender, EventArgs e)
        {
            BranchInfo.Text = string.Format("Reset {0} to:", GitCommands.GitCommands.GetSelectedBranch());
            Commit.Text = string.Format("Commit: {0}", Revision.Guid);
            Author.Text = string.Format("Author: {0}", Revision.Author);
            Date.Text = string.Format("Commit date: {0}", Revision.Date);
            Message.Text = string.Format("Message: {0}", Revision.Message);

        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (Soft.Checked)
            {
                new FormProcess(GitCommands.GitCommands.ResetSoftCmd(Revision.Guid));
            }
            else
                if (Mixed.Checked)
                {
                    new FormProcess(GitCommands.GitCommands.ResetMixedCmd(Revision.Guid));
                }
                else
                    if (Hard.Checked)
                    {
                        if (MessageBox.Show("You are about to discard ALL local changes, are you sure?", "Reset branch", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            new FormProcess(GitCommands.GitCommands.ResetHardCmd(Revision.Guid));
                        }
                        else
                        {
                            return;
                        }
                    }

            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
