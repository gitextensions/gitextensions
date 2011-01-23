using System;
using GitCommands;

namespace GitUI
{
    public partial class FormRevertCommitSmall : GitExtensionsForm
    {
        public FormRevertCommitSmall(GitRevision Revision)
        {
            this.Revision = Revision;

            InitializeComponent(); Translate();
        }

        public GitRevision Revision { get; set; }

        private void FormRevertCommitSmall_Load(object sender, EventArgs e)
        {
            Commit.Text = string.Format("Commit: {0}", Revision.Guid);
            Author.Text = string.Format("Author: {0}", Revision.Author);
            Date.Text = string.Format("Commit date: {0}", Revision.CommitDate);
            Message.Text = string.Format("Message: {0}", Revision.Message);
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.RevertCmd(Revision.Guid, AutoCommit.Checked)).ShowDialog();

            MergeConflictHandler.HandleMergeConflicts();

            Close();
        }
    }
}
