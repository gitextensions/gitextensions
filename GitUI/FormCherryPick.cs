using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

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



            MessageBox.Show("Command executed " + Environment.NewLine + GitCommands.GitCommands.CherryPick(RevisionGrid.GetRevisions()[0].Guid, AutoCommit.Checked), "Cherry pick");

            MergeConflictHandler.HandleMergeConflicts();

            RevisionGrid.RefreshRevisions();
            Cursor.Current = Cursors.Default;
        }
    }
}
