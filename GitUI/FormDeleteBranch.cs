using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
                MessageBox.Show("Command executed \n" + GitCommands.GitCommands.DeleteBranch(Branches.Text), "Delete branch");
            }
            catch
            {
            }
        }
    }
}
