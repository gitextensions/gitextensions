using System;
using System.Linq;

namespace GitUI.CommandsDialogs
{
    public partial class FormLog : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormLog()
        {
            InitializeComponent();
        }

        public FormLog(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            diffViewer.ExtraDiffArgumentsChanged += DiffViewerExtraDiffArgumentsChanged;
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
            using (WaitCursorScope.Enter())
            {
                var revisions = RevisionGrid.GetSelectedRevisions();
                var selectedRev = revisions.FirstOrDefault();
                var firstId = revisions.Skip(1).LastOrDefault()?.ObjectId ?? selectedRev?.FirstParentGuid;
                diffViewer.ViewChangesAsync(firstId, selectedRev, DiffFiles.SelectedItem);
            }
        }

        private void RevisionGridSelectionChanged(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                DiffFiles.SetDiffs(RevisionGrid.GetSelectedRevisions());
            }
        }

        private void DiffViewerExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedFileDiff();
        }
    }
}
