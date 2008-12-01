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
    public partial class FormSettigns : Form
    {
        public FormSettigns()
        {
            InitializeComponent();

            UserName.Text = GitCommands.GitCommands.GetSetting("user.name");
            UserEmail.Text = GitCommands.GitCommands.GetSetting("user.email");
        }

        private void UserName_TextChanged(object sender, EventArgs e)
        {
        }

        private void UserEmail_TextChanged(object sender, EventArgs e)
        {
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.SetSetting("user.name", UserName.Text);
            GitCommands.GitCommands.SetSetting("user.email", UserEmail.Text);
            Close();
        }
    }
}
