using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormRevertCommitSmall : GitModuleForm
    {
        private readonly TranslationString _commitInfo = new TranslationString("Commit: {0}");
        private readonly TranslationString _authorInfo = new TranslationString("Author: {0}");
        private readonly TranslationString _dateInfo = new TranslationString("Commit date: {0}");
        private readonly TranslationString _commitMessage = new TranslationString("Message: {0}");
        private readonly TranslationString _noneParentSelectedText =  new TranslationString("None parent is selected!");
        private readonly TranslationString _noneParentSelectedTextCaption = new TranslationString("Error");

        private bool _isMerge;

        public FormRevertCommitSmall(GitUICommands aCommands, GitRevision Revision)
            : base(aCommands)
        {
            this.Revision = Revision;

            InitializeComponent(); Translate();
        }

        public GitRevision Revision { get; set; }

        private void FormRevertCommitSmall_Load(object sender, EventArgs e)
        {
            Commit.Text = string.Format(_commitInfo.Text, Revision.Guid);
            Author.Text = string.Format(_authorInfo.Text, Revision.Author);
            Date.Text = string.Format(_dateInfo.Text, Revision.CommitDate);
            Message.Text = string.Format(_commitMessage.Text, Revision.Message);

            _isMerge = Module.IsMerge(Revision.Guid);
            if (_isMerge)
            {
                var parents = Module.GetParents(Revision.Guid);
                for (int i = 0; i < parents.Length; i++)
                {
                    ParentsList.Items.Add(i + 1 + "");
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(parents[i].Message);
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(parents[i].Author);
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(parents[i].CommitDate.ToShortDateString());
                }
                ParentsList.TopItem.Selected = true;
            }
            else
            {
                ParentsList.Visible = false;
                ParentsLabel.Visible = false;
                Height = Height - (ParentsList.Height + ParentsLabel.Height);
            }
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            var parentIndex = 0;
            if (_isMerge)
            {
                if (ParentsList.SelectedItems.Count != 1)
                {
                    MessageBox.Show(this, _noneParentSelectedText.Text, _noneParentSelectedTextCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    parentIndex = ParentsList.SelectedItems[0].Index + 1;
                }
            }

            FormProcess.ShowDialog(this, GitCommandHelpers.RevertCmd(Revision.Guid, AutoCommit.Checked, parentIndex));
            MergeConflictHandler.HandleMergeConflicts(UICommands, this, AutoCommit.Checked);
            Close();
        }
    }
}
