using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

            List<GitRevision> revisions = GitCommands.GitCommands.GitRevisions(selectedHead);
            Revisions.DataSource = revisions;
        }

        private void FormatPatch_Click(object sender, EventArgs e)
        {
            string rev1 = "";
            string rev2 = "";

            if (Revisions.SelectedRows.Count > 0)
            {
                if (Revisions.SelectedRows[0].DataBoundItem is GitRevision)
                {
                    IGitItem revision = (IGitItem)Revisions.SelectedRows[0].DataBoundItem;

                    rev1 = ((GitRevision)Revisions.SelectedRows[0].DataBoundItem).ParentGuids[0];
                    rev2 = ((GitRevision)Revisions.SelectedRows[0].DataBoundItem).Guid;
                }

                if (Revisions.SelectedRows.Count == 2)
                {
                    if (Revisions.SelectedRows[0].DataBoundItem is GitRevision &&
                        Revisions.SelectedRows[1].DataBoundItem is GitRevision)
                    {
                        rev1 = ((GitRevision)Revisions.SelectedRows[0].DataBoundItem).ParentGuids[0];
                        rev2 = ((GitRevision)Revisions.SelectedRows[1].DataBoundItem).Guid;
                    }
                }
            }
            if (string.IsNullOrEmpty(rev1) || string.IsNullOrEmpty(rev2))
            {
                MessageBox.Show("You need to select 2 revisions", "Patch error");
                return;
            }

            string result = new GitCommands.GitCommands().FormatPatch(rev1, rev2, OutputPath.Text);

            MessageBox.Show(result, "Patch result");
        }
    }
}
