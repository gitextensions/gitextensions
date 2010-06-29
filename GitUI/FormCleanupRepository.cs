using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormCleanupRepository : GitExtensionsForm
    {
        public FormCleanupRepository()
        {
            InitializeComponent(); Translate();
            PreviewOutput.ReadOnly = true;
        }

        private void Preview_Click(object sender, EventArgs e)
        {
            FormProcess form = new FormProcess(GitCommands.GitCommands.CleanUpCmd(true, RemoveDirectories.Checked, RemoveNonIgnored.Checked, RemoveIngnored.Checked));
            form.ShowDialog();
            PreviewOutput.Text = form.OutputString.ToString();
        }

        private void Cleanup_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cleanup the repository?", "Cleanup", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                FormProcess form = new FormProcess(GitCommands.GitCommands.CleanUpCmd(false, RemoveDirectories.Checked, RemoveNonIgnored.Checked, RemoveIngnored.Checked));
                form.ShowDialog();
                PreviewOutput.Text = form.OutputString.ToString();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
