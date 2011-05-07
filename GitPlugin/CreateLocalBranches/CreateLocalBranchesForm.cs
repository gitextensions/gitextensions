using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace CreateLocalBranches
{
    public partial class CreateLocalBranchesForm : Form
    {
        private GitUIBaseEventArgs m_gitUiCommands;

        public CreateLocalBranchesForm(GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();

            m_gitUiCommands = gitUiCommands;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (string reference in m_gitUiCommands.GitCommands.RunGit("branch -a").Split('\n'))
            {
                try
                {
                    if (string.IsNullOrEmpty(reference)) continue;

                    string branchName = reference.Trim('*', ' ', '\n', '\r');

                    if (branchName.StartsWith("remotes/" + Remote.Text + "/"))
                        m_gitUiCommands.GitCommands.RunGit(string.Concat("branch --track ", branchName.Replace("remotes/" + Remote.Text + "/", ""), " ", branchName));
                }
                catch
                {
                }
            }
        }
    }
}
