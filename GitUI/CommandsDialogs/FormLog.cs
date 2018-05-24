using System;

namespace GitUI.CommandsDialogs
{
    public partial class FormLog : GitModuleForm
    {
        private FormLog()
            : this(null)
        {
        }

        public FormLog(GitUICommands commands)
            : base(enablePositionRestore: true, commands)
        {
            InitializeComponent();
            Translate();

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
            if (DiffFiles.SelectedItem == null)
            {
                diffViewer.ViewPatch(null);
                return;
            }

            using (WaitCursorScope.Enter())
            {
                diffViewer.ViewChangesAsync(RevisionGrid.GetSelectedRevisions(), DiffFiles.SelectedItem, string.Empty);
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