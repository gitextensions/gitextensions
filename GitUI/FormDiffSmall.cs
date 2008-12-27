using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PatchApply;
using GitCommands;

namespace GitUI
{
    public partial class FormDiffSmall : Form
    {
        public FormDiffSmall()
        {
            InitializeComponent();
        }

        private void FormDiffSmall_Load(object sender, EventArgs e)
        {

        }

        private GitRevision Revision = null;

        public void SetRevision(GitRevision revision)
        {
            Revision = revision;
            DiffFiles.DataSource = null;
            DiffFiles.DisplayMember = "FileNameB";

            DiffFiles.DataSource = GitCommands.GitCommands.GetDiffFiles(revision.Guid, revision.ParentGuids[0]);
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is Patch)
            {
                {
                    Patch patch = (Patch)DiffFiles.SelectedItem;
                    DiffText.Text = patch.Text;
                    DiffText.Refresh();
                    EditorOptions.SetSyntax(DiffText, patch.FileNameB);
                }

            }
            else
                if (DiffFiles.SelectedItem is string)
                {
                    Patch selectedPatch = GitCommands.GitCommands.GetSingleDiff(Revision.Guid, Revision.ParentGuids[0], (string)DiffFiles.SelectedItem);
                    if (selectedPatch != null)
                    {
                        EditorOptions.SetSyntax(DiffText, selectedPatch.FileNameB);
                        DiffText.Text = selectedPatch.Text;
                    }
                    else
                    {
                        DiffText.Text = "";
                    }
                    DiffText.Refresh();
                }
        }

        private void DiffFiles_DoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is string)
            {
                {
                    new FormFileHistory((string)DiffFiles.SelectedItem).ShowDialog();
                }
            }
        }
    }
}
