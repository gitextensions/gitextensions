using System;

using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormCherryPick : GitExtensionsForm
    {
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
                MessageBox.Show("Select 1 revision to pick.", "Cherry pick");
                return;
            }
            bool formClosed = false;
            string arguments = "";
            bool IsMerge = GitCommandHelpers.IsMerge(RevisionGrid.GetRevisions()[0].Guid);
            if (IsMerge && !autoParent.Checked)
            {
                GitRevision[] ParentsRevisions = GitCommandHelpers.GetParents(RevisionGrid.GetRevisions()[0].Guid);
                var choose = new FormCherryPickMerge(ParentsRevisions);
                choose.ShowDialog();
                if (choose.OkClicked)
                    arguments = "-m " + (choose.ParentsList.SelectedItems[0].Index + 1);
                else
                    formClosed = true;
            }
            else if (IsMerge)
                arguments = "-m 1";

            if (!formClosed)
            {
                MessageBox.Show("Command executed " + Environment.NewLine + GitCommandHelpers.CherryPick(RevisionGrid.GetRevisions()[0].Guid, AutoCommit.Checked, arguments), "Cherry pick");

                MergeConflictHandler.HandleMergeConflicts();

                RevisionGrid.RefreshRevisions();

                Cursor.Current = Cursors.Default;
            }
            
        }
    }
}
