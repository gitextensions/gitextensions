using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormDeleteBranch : Form
    {
        public FormDeleteBranch()
        {
            InitializeComponent();
        }

        private void FormDeleteBranch_Load(object sender, EventArgs e)
        {
            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(true);
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to delete this branch?\nDeleting a branch can cause commits to be deleted too!", "Delete branch", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Command executed \n" + GitCommands.GitCommands.DeleteBranch(Branches.Text), "Delete branch");
                }
            }
            catch
            {
            }
        }
    }
}
