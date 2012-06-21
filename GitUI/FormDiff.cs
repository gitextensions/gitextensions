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
            : this(Settings.Module)
        {
        }

        public FormDiff(GitModule module)
        {
            InitializeComponent();
            Translate();

            module_ = module;
            diffViewer.ExtraDiffArgumentsChanged += DiffViewerExtraDiffArgumentsChanged;
        }

        public FormDiff(GitRevision revision)
            : this(Settings.Module, revision)
        {
        }

        public FormDiff(GitModule module, GitRevision revision)
        {
            InitializeComponent();
            Translate();

            module_ = module;
            RevisionGrid.SetSelectedRevision(revision);
        }

        private GitModule module_;

        private void FormDiffFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("diff");
        }

        private void FormDiffLoad(object sender, EventArgs e)
        {
            RevisionGrid.Load();

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

            var revisions = RevisionGrid.GetSelectedRevisions();
            if (revisions.Count == 2)
            {
                selectedPatch = 
                    module_.GetSingleDiff(
                            revisions[0].Guid,
                            revisions[1].Guid,
                            DiffFiles.SelectedItem.Name,
                            DiffFiles.SelectedItem.OldName,
                            diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);
            }
            else
            {
                var revision = revisions[0];
                selectedPatch =
                    module_.GetSingleDiff(
                            revision.Guid,
                            revision.ParentGuids[0],
                            DiffFiles.SelectedItem.Name,
                            DiffFiles.SelectedItem.OldName,
                            diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);
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
                var revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 1)
                {
                    DiffFiles.GitItemStatuses =
                        module_.GetDiffFiles(
                            revisions[0].Guid,
                            revisions[0].ParentGuids[0]);
                }
                else if (revisions.Count == 2)
                {
                    DiffFiles.GitItemStatuses =
                        module_.GetDiffFiles(
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