using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormBranch : Form
    {
        public FormBranch()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {

                if (RevisionGrid.GetRevisions().Count != 1)
                {
                    MessageBox.Show("Select 1 revision to create the branch on.", "Branch");
                    return;
                }

                MessageBox.Show("Command executed \n" + GitCommands.GitCommands.Branch(BName.Text, RevisionGrid.GetRevisions()[0].Guid), "Branch");

                RevisionGrid.RefreshRevisions();
            }
            catch
            {
            }
        }

        private void Checkout_Click(object sender, EventArgs e)
        {
            new FormCheckoutBranck().ShowDialog();
            RevisionGrid.RefreshRevisions();
        }
    }
}
