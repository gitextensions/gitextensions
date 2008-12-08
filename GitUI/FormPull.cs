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
    public partial class FormPull : Form
    {
        public FormPull()
        {
            InitializeComponent();
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                PullSource.Text = dialog.SelectedPath;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.exe", "mergetool --tool=kdiff3");

            if (MessageBox.Show("Resolved all conflicts? Commit?", "Conflicts solved", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //Output.Text += "\n";
                FormCommit form = new FormCommit();
                form.ShowDialog();
            }
        }

        private void PullSource_TextChanged(object sender, EventArgs e)
        {
            Branches.DataSource = null;
        }

        private void Branches_DropDown(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PullSource.Text))
            {
                Branches.DataSource = null;
                return;
            }

            string realWorkingDir = GitCommands.Settings.WorkingDir;

            try
            {
                GitCommands.Settings.WorkingDir = PullSource.Text;
                Branches.DisplayMember = "Name";
                Branches.DataSource = GitCommands.GitCommands.GetHeads(false);
            }
            finally
            {
                GitCommands.Settings.WorkingDir = realWorkingDir;
            }
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PullSource.Text))
            {
                MessageBox.Show("Please select a source directory");
                return;
            }

            Output.Text = GitCommands.GitCommands.Pull(PullSource.Text, Branches.SelectedText);
        }

    }
}
