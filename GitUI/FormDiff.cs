using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormDiff : GitModuleForm
    {
        private FormDiff()
            : this(null)
        {
        }

        public FormDiff(GitUICommands aCommands)
            : base(true, aCommands)
        {
            InitializeComponent();
            Translate();

            diffViewer.ExtraDiffArgumentsChanged += DiffViewerExtraDiffArgumentsChanged;
        }

        public FormDiff(GitUICommands aCommands, GitRevision revision)
            : this(aCommands)
        {
            RevisionGrid.SetSelectedRevision(revision);
        }

        private void FormDiffLoad(object sender, EventArgs e)
        {
            RevisionGrid.Load();
        }

        private void DiffFilesSelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedFileDiff();
        }

        private void ViewSelectedFileDiff()
        {
            if (DiffFiles.SelectedItem == null)
            {
                diffViewer.ViewPatch("");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            diffViewer.ViewPatch(RevisionGrid, DiffFiles.SelectedItem, String.Empty);
            Cursor.Current = Cursors.Default;
        }

        private void RevisionGridSelectionChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            DiffFiles.GitItemStatuses = null;
            DiffFiles.SetDiffs(RevisionGrid.GetSelectedRevisions());
            Cursor.Current = Cursors.Default;
        }

        private void DiffViewerExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedFileDiff();
        }
    }
}