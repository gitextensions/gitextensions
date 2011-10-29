using System;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormRevertCommitSmall : GitExtensionsForm
    {
        readonly TranslationString commitInfo = new TranslationString("Commit: {0}");
        readonly TranslationString authorInfo = new TranslationString("Author: {0}");
        readonly TranslationString dateInfo = new TranslationString("Commit date: {0}");
        readonly TranslationString commitMessage = new TranslationString("Message: {0}");


        public FormRevertCommitSmall(GitRevision Revision)
        {
            this.Revision = Revision;

            InitializeComponent(); Translate();
        }

        public GitRevision Revision { get; set; }

        private void FormRevertCommitSmall_Load(object sender, EventArgs e)
        {
            Commit.Text = string.Format(commitInfo.Text, Revision.Guid);
            Author.Text = string.Format(authorInfo.Text, Revision.Author);
            Date.Text = string.Format(dateInfo.Text, Revision.CommitDate);
            Message.Text = string.Format(commitMessage.Text, Revision.Message);
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.RevertCmd(Revision.Guid, AutoCommit.Checked), PerFormSettingsName()).ShowDialog();

            MergeConflictHandler.HandleMergeConflicts();

            Close();
        }
    }
}
