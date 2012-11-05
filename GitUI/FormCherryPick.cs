using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormCherryPick : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _noneParentSelectedText =
            new TranslationString("None parent is selected!");
        private readonly TranslationString _noneParentSelectedTextCaption =
            new TranslationString("Error");
        #endregion

        private bool IsMerge;

        private FormCherryPick()
            : this(null, null)
        { }

        public FormCherryPick(GitUICommands aCommands, GitRevision revision)
            : base(aCommands)
        {
            Revision = revision;
            InitializeComponent();

            Translate();
        }

        public GitRevision Revision { get; set; }

        private void FormCherryPickCommitSmall_Load(object sender, EventArgs e)
        {
            OnRevisionChanged();
        }

        private void OnRevisionChanged()
        {
            commitSummaryUserControl1.Revision = Revision;

            IsMerge = Module.IsMerge(Revision.Guid);

            if (IsMerge)
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

            panelParentsList.Visible = IsMerge;
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
                FormProcess.ShowDialog(this, GitCommandHelpers.CherryPickCmd(Revision.Guid, AutoCommit.Checked, string.Join(" ", argumentsList.ToArray())));
                MergeConflictHandler.HandleMergeConflicts(UICommands, this, AutoCommit.Checked);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        public void CopyOptions(FormCherryPick source)
        {
            AutoCommit.Checked = source.AutoCommit.Checked;
            checkAddReference.Checked = source.checkAddReference.Checked;
        }

        private void btnChooseRevision_Click(object sender, EventArgs e)
        {
            using (var chooseForm = new FormChooseCommit(UICommands, Revision.Guid))
            {
                if (chooseForm.ShowDialog() == DialogResult.OK && chooseForm.SelectedRevision != null)
                {
                    Revision = chooseForm.SelectedRevision;
                }
            }

            OnRevisionChanged();
        }
    }
}
