using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using System.Collections.Generic;

namespace GitUI
{
    public partial class FormCherryPickCommitSmall : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _noneParentSelectedText =
            new TranslationString("None parent is selected!");
        private readonly TranslationString _noneParentSelectedTextCaption =
            new TranslationString("Error");
        #endregion

        private bool IsMerge;
        public FormCherryPickCommitSmall(GitRevision revision)
        {
            Revision = revision;
            InitializeComponent();

            Translate();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            IsMerge = Settings.Module.IsMerge(Revision.Guid);
            if (IsMerge)
            {
                GitRevision[] Parents = Settings.Module.GetParents(Revision.Guid);
                for (int i = 0; i < Parents.Length; i++)
                {
                    ParentsList.Items.Add(i + 1 + "");
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(Parents[i].Message);
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(Parents[i].Author);
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(Parents[i].CommitDate.ToShortDateString());
                }
                ParentsList.TopItem.Selected = true;
            }
            else
            {
                ParentsList.Visible = false;
                ParentsLabel.Visible = false;
                Height = Height - (ParentsList.Height + ParentsLabel.Height);
                Pick.Location = new System.Drawing.Point(Pick.Location.X,
                    Pick.Location.Y - (ParentsList.Height + ParentsLabel.Height));
                AutoCommit.Location = new System.Drawing.Point(AutoCommit.Location.X,
                    AutoCommit.Location.Y - (ParentsList.Height + ParentsLabel.Height));
                checkAddReference.Location = new System.Drawing.Point(checkAddReference.Location.X,
                    checkAddReference.Location.Y - (ParentsList.Height + ParentsLabel.Height));
            }

        }

        public GitRevision Revision { get; set; }

        private void FormCherryPickCommitSmall_Load(object sender, EventArgs e)
        {
            Commit.Text = string.Format(Strings.GetCommitHashText() + ": {0}", Revision.Guid);
            Author.Text = string.Format(Strings.GetAuthorText() + ": {0}", Revision.Author);
            Date.Text = string.Format(Strings.GetCommitDateText() + ": {0}", Revision.CommitDate);
            Message.Text = string.Format(Strings.GetMessageText() + ": {0}", Revision.Message);
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            List<string> argumentsList = new List<string>();
            bool CanExecute = true;
            if (IsMerge)
            {
                if (ParentsList.SelectedItems.Count == 0)
                {
                    MessageBox.Show(this, _noneParentSelectedText.Text, _noneParentSelectedTextCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CanExecute = false;
                }
                else
                {
                    argumentsList.Add("-m " + (ParentsList.SelectedItems[0].Index + 1));
                }
            }
            if (checkAddReference.Checked)
            {
                argumentsList.Add("-x");
            }
            if (CanExecute)
            {
                using (var frm = new FormProcess(GitCommandHelpers.CherryPickCmd(Revision.Guid, AutoCommit.Checked, string.Join(" ", argumentsList.ToArray())))) frm.ShowDialog(this);
                MergeConflictHandler.HandleMergeConflicts(this);
                Close();
            }
        }
    }
}
