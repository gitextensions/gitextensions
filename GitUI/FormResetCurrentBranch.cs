﻿using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormResetCurrentBranch : GitExtensionsForm
    {
        readonly TranslationString branchInfo = new TranslationString("Reset {0} to:");
        readonly TranslationString commitInfo = new TranslationString("Commit: {0}");
        readonly TranslationString authorInfo = new TranslationString("Author: {0}");
        readonly TranslationString dateInfo = new TranslationString("Commit date: {0}");
        readonly TranslationString commitMessage = new TranslationString("Message: {0}");
        readonly TranslationString resetHardWarning = new TranslationString("You are about to discard ALL local changes, are you sure?");
        readonly TranslationString resetCaption = new TranslationString("Reset branch");

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

            _NO_TRANSLATE_BranchInfo.Text = string.Format(branchInfo.Text, GitCommandHelpers.GetSelectedBranch());
            _NO_TRANSLATE_Commit.Text = string.Format(commitInfo.Text, Revision.Guid);
            _NO_TRANSLATE_Author.Text = string.Format(authorInfo.Text, Revision.Author);
            _NO_TRANSLATE_Date.Text = string.Format(dateInfo.Text, Revision.CommitDate);
            _NO_TRANSLATE_Message.Text = string.Format(commitMessage.Text, Revision.Message);

        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (Soft.Checked)
            {
                new FormProcess(GitCommandHelpers.ResetSoftCmd(Revision.Guid), PerFormSettingsName()).ShowDialog();
            }
            else
                if (Mixed.Checked)
                {
                    new FormProcess(GitCommandHelpers.ResetMixedCmd(Revision.Guid), PerFormSettingsName()).ShowDialog();
                }
                else
                    if (Hard.Checked)
                    {
                        if (MessageBox.Show(resetHardWarning.Text, resetCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        {
                            new FormProcess(GitCommandHelpers.ResetHardCmd(Revision.Guid), PerFormSettingsName()).ShowDialog();
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
