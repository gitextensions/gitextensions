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
            Commit.Text = string.Format(Commit.Text, Revision.Guid);
            Author.Text = string.Format(Author.Text, Revision.Author);
            Date.Text = string.Format(Date.Text, Revision.CommitDate);
            Message.Text = string.Format(Message.Text, Revision.Message);
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.RevertCmd(Revision.Guid, AutoCommit.Checked), PerFormSettingsName()).ShowDialog();

            MergeConflictHandler.HandleMergeConflicts();

            Close();
        }
    }
}
