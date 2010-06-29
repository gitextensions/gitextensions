using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormResetCurrentBranch : GitExtensionsForm
    {
        TranslationString branchInfo = new TranslationString("Reset {0} to:");
        TranslationString commitInfo = new TranslationString("Commit: {0}");
        TranslationString authorInfo = new TranslationString("Author: {0}");
        TranslationString dateInfo = new TranslationString("Commit date: {0}");
        TranslationString commitMessage = new TranslationString("Message: {0}");
        TranslationString resetHardWarning = new TranslationString("You are about to discard ALL local changes, are you sure?");
        TranslationString resetCaption = new TranslationString("Reset branch");
      
        public FormResetCurrentBranch(GitRevision Revision)
        {
            this.Revision = Revision;

            InitializeComponent(); Translate();
        }

        public GitRevision Revision { get; set; }

        private void FormResetCurrentBranch_Load(object sender, EventArgs e)
        {
            if (Revision == null)
                throw new Exception("No revision");

            _BranchInfo.Text = string.Format(branchInfo.Text, GitCommands.GitCommands.GetSelectedBranch());
            _Commit.Text = string.Format(commitInfo.Text, Revision.Guid);
            _Author.Text = string.Format(authorInfo.Text, Revision.Author);
            _Date.Text = string.Format(dateInfo.Text, Revision.CommitDate);
            _Message.Text = string.Format(commitMessage.Text, Revision.Message);

        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (Soft.Checked)
            {
                new FormProcess(GitCommands.GitCommands.ResetSoftCmd(Revision.Guid)).ShowDialog();
            }
            else
                if (Mixed.Checked)
                {
                    new FormProcess(GitCommands.GitCommands.ResetMixedCmd(Revision.Guid)).ShowDialog();
                }
                else
                    if (Hard.Checked)
                    {
                        if (MessageBox.Show(resetHardWarning.Text, resetCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            new FormProcess(GitCommands.GitCommands.ResetHardCmd(Revision.Guid)).ShowDialog();
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
