using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI
{
    public partial class FormDiff : GitExtensionsForm
    {
        public FormDiff()
        {
            InitializeComponent();
            Translate();

            diffViewer.ExtraDiffArgumentsChanged += DiffViewerExtraDiffArgumentsChanged;
        }

        public FormDiff(GitRevision revision)
        {
            InitializeComponent();
            Translate();


            RevisionGrid.SetSelectedRevision(revision);
        }


        private void FormDiffFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("diff");
        }

        private void FormDiffLoad(object sender, EventArgs e)
        {
            RestorePosition("diff");
        }

        private void DiffFilesSelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedFileDiff();
        }

        private void ViewSelectedFileDiff()
        {
            Cursor.Current = Cursors.WaitCursor;

            if (DiffFiles.SelectedItem == null)
                return;

            Patch selectedPatch;

            var revisions = RevisionGrid.GetRevisions();
            if (revisions.Count == 2)
            {
                selectedPatch =
                    GitCommands.GitCommandHelpers
                        .GetSingleDiff(
                            revisions[0].Guid,
                            revisions[1].Guid,
                            DiffFiles.SelectedItem.Name,
                            diffViewer.GetExtraDiffArguments());
            }
            else
            {
                var revision = revisions[0];
                selectedPatch =
                    GitCommands.GitCommandHelpers
                        .GetSingleDiff(
                            revision.Guid,
                            revision.ParentGuids[0],
                            DiffFiles.SelectedItem.Name,
                            diffViewer.GetExtraDiffArguments());
            }

            diffViewer.ViewPatch(selectedPatch != null ? selectedPatch.Text : "");
            Cursor.Current = Cursors.Default;
        }

        private void RevisionGridSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DiffFiles.GitItemStatuses = null;
                var revisions = RevisionGrid.GetRevisions();

                if (revisions.Count == 1)
                    DiffFiles.GitItemStatuses =
                        GitCommandHelpers.GetDiffFiles(
                            revisions[0].Guid,
                            revisions[0].ParentGuids[0]);

                if (revisions.Count == 2)
                {
                    DiffFiles.GitItemStatuses =
                        GitCommandHelpers.GetDiffFiles(
                            revisions[0].Guid,
                            revisions[1].Guid);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            Cursor.Current = Cursors.Default;
        }

        private void DiffViewerExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedFileDiff();
        }
    }
}