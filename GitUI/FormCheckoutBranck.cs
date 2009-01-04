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
    public partial class FormCheckoutBranck : Form
    {
        public FormCheckoutBranck()
        {
            InitializeComponent();

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(false);
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                new FormProcess("checkout \"" + Branches.Text + "\"");
                //CheckoutDto dto = new CheckoutDto(Branches.Text);

                //GitCommands.Checkout commit = new GitCommands.Checkout(dto);
                //commit.Execute();
                //MessageBox.Show("Command executed \n" + dto.Result, "Checkout");
            }
            catch
            {
            }
        }

        private void FormCheckoutBranck_Load(object sender, EventArgs e)
        {

        }
    }
}
