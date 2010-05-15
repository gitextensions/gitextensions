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
        System.ComponentModel.ComponentResourceManager resources;

        public FormResetCurrentBranch(GitRevision Revision)
        {
            resources = new System.ComponentModel.ComponentResourceManager(typeof(FormResetCurrentBranch));
            this.Revision = Revision;

            InitializeComponent();
        }

        public GitRevision Revision { get; set; }

        private void FormResetCurrentBranch_Load(object sender, EventArgs e)
        {
            BranchInfo.Text = string.Format(resources.GetString("BranchInfo.Text"), GitCommands.GitCommands.GetSelectedBranch());
            Commit.Text = string.Format(resources.GetString("Commit.Text"), Revision.Guid);
            Author.Text = string.Format(resources.GetString("Author.Text"), Revision.Author);
            Date.Text = string.Format(resources.GetString("Date.Text"), Revision.CommitDate);
            Message.Text = string.Format(resources.GetString("Message.Text"), Revision.Message);

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
                        if (MessageBox.Show(resources.GetString("msg:reset branch"), "Reset branch", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
