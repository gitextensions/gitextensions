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
    public partial class FormDiffSmall : GitExtensionsForm
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

            if (revision.ParentGuids.Count > 0)
                DiffFiles.DataSource = GitCommands.GitCommands.GetDiffFiles(revision.Guid, revision.ParentGuids[0]);
            else
                DiffFiles.DataSource = null;
            RevisionInfo.Text = GitCommands.GitCommands.GetCommitInfo(revision.Guid);
        }

        public void SetRevision(string revision)
        {
            Revision = new GitRevision();
            Revision.Guid = revision;
            Revision.ParentGuids.Add(revision + "^");
            SetRevision(Revision);
        }


        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (DiffFiles.SelectedItem is Patch)
            {
                {
                    Patch patch = (Patch)DiffFiles.SelectedItem;
                    DiffText.ViewPatch(patch.Text);
                }

            }
            else
                if (DiffFiles.SelectedItem is string)
                {
                    Patch selectedPatch = GitCommands.GitCommands.GetSingleDiff(Revision.Guid, Revision.ParentGuids[0], (string)DiffFiles.SelectedItem);
                    if (selectedPatch != null)
                    {
                        DiffText.ViewPatch(selectedPatch.Text);
                    }
                    else
                    {
                        DiffText.ViewPatch("");
                    }
                }
        }

        private void DiffFiles_DoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is string)
            {
                {
                    GitUICommands.Instance.StartFileHistoryDialog((string)DiffFiles.SelectedItem);
                }
            }
        }
    }
}
