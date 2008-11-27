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
                OutPut.Text = "";
                BranchDto dto = new BranchDto(BName.Text);
                GitCommands.Branch commit = new GitCommands.Branch(dto);
                commit.Execute();

                OutPut.Text = "Command executed \n" + dto.Result;
                
            }
            catch
            {
            }
        }
    }
}
