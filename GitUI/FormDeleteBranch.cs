using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormDeleteBranch : GitExtensionsForm
    {
        private readonly string defaultBranch;

        public FormDeleteBranch(string defaultBranch)
        {
            InitializeComponent();
            this.defaultBranch = defaultBranch;
        }

        private void FormDeleteBranch_Load(object sender, EventArgs e)
        {
            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(false, true);

            if (defaultBranch != null)
            {
                Branches.Text = defaultBranch;
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to delete this branch?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!", "Delete branch", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Command executed " + Environment.NewLine + "" + GitCommands.GitCommands.DeleteBranch(Branches.Text, ForceDelete.Checked), "Delete branch");
                }
            }
            catch
            {
            }
            Close();
        }
    }
}
