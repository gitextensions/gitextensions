using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormBranchSmall : GitExtensionsForm
    {
        public FormBranchSmall()
        {
            InitializeComponent();
        }

        public GitRevision Revision { get; set; }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {

                if (Revision  == null)
                {
                    MessageBox.Show("Select 1 revision to create the branch on.", "Branch");
                    return;
                }
                else
                {
                    new FormProcess(GitCommands.GitCommands.BranchCmd(BName.Text, Revision.Guid, ChechoutAfterCreate.Checked));

                    Close();
                }
            }
            catch
            {
            }
        }

        private void BName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Ok_Click(null, null);
        }
    }
}
