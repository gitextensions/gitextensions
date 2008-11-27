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
    public partial class FormCheckout : Form
    {
        public FormCheckout()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                OutPut.Text = "";

                CheckoutDto dto = new CheckoutDto(Branch.Text);
                GitCommands.Checkout commit = new GitCommands.Checkout(dto);
                commit.Execute();

                OutPut.Text = "Command executed \n" + dto.Result;
            }
            catch
            {
            }
        }

        private void FormCheckout_Load(object sender, EventArgs e)
        {
            Branch.DisplayMember = "Name";
            Branch.DataSource = GitCommands.GitCommands.GetHeads();
        }

        private void Branch_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
