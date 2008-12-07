using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class MergePatch : Form
    {
        public MergePatch()
        {
            InitializeComponent();
            EnableButtons();
        }

        private void EnableButtons()
        {
            if (Directory.Exists(GitCommands.Settings.WorkingDir + ".git\\rebase-apply\\"))
            {
                BrowsePatch.Enabled = false;
                Apply.Enabled = false;
                PatchFile.ReadOnly = true;
                Resolved.Enabled = true;
                Mergetool.Enabled = true;
                Skip.Enabled = true;
                Abort.Enabled = true;
            }
            else
            {
                BrowsePatch.Enabled = true;
                Apply.Enabled = true;
                PatchFile.ReadOnly = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }
        }

        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private string SelectPatchFile(string initialDirectory)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "Patch file (*.Patch)|*.Patch";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select patch file";
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : PatchFile.Text;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            PatchFile.Text = SelectPatchFile(@".");
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            string result = GitCommands.GitCommands.Patch(PatchFile.Text);
            Output.Text = result;
            if (result.Contains("Patch failed"))
            {
                EnableButtons();
            }
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.exe", "mergetool --tool=kdiff3");

            if (MessageBox.Show("Resolved all conflicts? Run resolved?", "Conflicts solved", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Output.Text += "\n";
                Output.Text += GitCommands.GitCommands.Resolved();
                EnableButtons();
            }
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            Output.Text += "\n";
            Output.Text += GitCommands.GitCommands.Skip();
            EnableButtons();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Output.Text += "\n";
            Output.Text += GitCommands.GitCommands.Resolved();
            EnableButtons();
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            Output.Text += "\n";
            Output.Text += GitCommands.GitCommands.Abort();
            EnableButtons();
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            FormAddFiles form = new FormAddFiles();
            form.Show();
        }
    }
}
