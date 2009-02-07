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
    public partial class FormFormatPath : Form
    {
        public FormFormatPath()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                OutputPath.Text = dialog.SelectedPath;
        }

        private void FormFormatPath_Load(object sender, EventArgs e)
        {
            string selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            SelectedBranch.Text = "Current branch: " + selectedHead;
        }

        private void FormatPatch_Click(object sender, EventArgs e)
        {
            string rev1 = "";
            string rev2 = "";
            string result = "";

            if (RevisionGrid.GetRevisions().Count > 0)
            {
                if (RevisionGrid.GetRevisions().Count == 1)
                {
                    rev1 = RevisionGrid.GetRevisions()[0].ParentGuids[0];
                    rev2 = RevisionGrid.GetRevisions()[0].Guid;
                    result = new GitCommands.GitCommands().FormatPatch(rev1, rev2, OutputPath.Text);
                }

                if (RevisionGrid.GetRevisions().Count == 2)
                {
                    rev1 = RevisionGrid.GetRevisions()[0].ParentGuids[0];
                    rev2 = RevisionGrid.GetRevisions()[1].Guid;
                    result = new GitCommands.GitCommands().FormatPatch(rev1, rev2, OutputPath.Text);
                }

                if (RevisionGrid.GetRevisions().Count > 2)
                {
                    foreach (GitRevision revision in RevisionGrid.GetRevisions())
                    {
                        rev1 = revision.ParentGuids[0];
                        rev2 = revision.Guid;
                        result += new GitCommands.GitCommands().FormatPatch(rev1, rev2, OutputPath.Text);
                    }
                }
            } else
            if (string.IsNullOrEmpty(rev1) || string.IsNullOrEmpty(rev2))
            {
                MessageBox.Show("You need to select 2 revisions", "Patch error");
                return;
            }

            MessageBox.Show(result, "Patch result");
            Close();
        }
    }
}
