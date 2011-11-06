using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormCherryPick : GitExtensionsForm
    {
        private readonly TranslationString _noRevisionSelectedMsgBox =
            new TranslationString("Select 1 revision to pick.");
        private readonly TranslationString _noRevisionSelectedMsgBoxCaption =
            new TranslationString("Cherry pick");
        private readonly TranslationString _cmdExecutedMsgBox =
            new TranslationString("Command executed");
        private readonly TranslationString _cmdExecutedMsgBoxCaption =
            new TranslationString("Cherry pick");

        public FormCherryPick()
        {
            InitializeComponent(); Translate();
        }

        private void FormCherryPick_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("cherry-pick");
        }

        private void FormCherryPick_Load(object sender, EventArgs e)
        {
            RevisionGrid.Load();

            RestorePosition("cherry-pick");
        }

        private void CherryPick_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (RevisionGrid.GetRevisions().Count != 1)
            {
                MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, _noRevisionSelectedMsgBoxCaption.Text);
                return;
            }
            bool formClosed = false;
            string arguments = "";
            bool IsMerge = Settings.Module.IsMerge(RevisionGrid.GetRevisions()[0].Guid);
            if (IsMerge && !autoParent.Checked)
            {
                GitRevision[] ParentsRevisions = Settings.Module.GetParents(RevisionGrid.GetRevisions()[0].Guid);
                var choose = new FormCherryPickMerge(ParentsRevisions);
                choose.ShowDialog(this);
                if (choose.OkClicked)
                    arguments = "-m " + (choose.ParentsList.SelectedItems[0].Index + 1);
                else
                    formClosed = true;
            }
            else if (IsMerge)
                arguments = "-m 1";

            if (!formClosed)
            {
                MessageBox.Show(this, _cmdExecutedMsgBox.Text + " " + Environment.NewLine + Settings.Module.CherryPick(RevisionGrid.GetRevisions()[0].Guid, AutoCommit.Checked, arguments), _cmdExecutedMsgBoxCaption.Text);

                MergeConflictHandler.HandleMergeConflicts();

                RevisionGrid.RefreshRevisions();

                Cursor.Current = Cursors.Default;
            }
            
        }
    }
}
