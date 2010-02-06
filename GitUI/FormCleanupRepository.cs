using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormCleanupRepository : Form
    {
        public FormCleanupRepository()
        {
            InitializeComponent();
            PreviewOutput.ReadOnly = true;
        }

        private void Preview_Click(object sender, EventArgs e)
        {
            FormProcess form = new FormProcess(GitCommands.GitCommands.CleanUpCmd(true, RemoveDirectories.Checked, RemoveNonIgnored.Checked, RemoveIngnored.Checked));
            PreviewOutput.Text = form.outputString.ToString();
        }

        private void Cleanup_Click(object sender, EventArgs e)
        {
            FormProcess form = new FormProcess(GitCommands.GitCommands.CleanUpCmd(false, RemoveDirectories.Checked, RemoveNonIgnored.Checked, RemoveIngnored.Checked));
            PreviewOutput.Text = form.outputString.ToString();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
