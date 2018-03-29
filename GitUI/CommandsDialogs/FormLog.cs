using System;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public partial class FormLog : GitModuleForm
    {
        private FormLog()
            : this(null)
        {
        }

        public FormLog(GitUICommands commands)
            : base(true, commands)
        {
            InitializeComponent();
            Translate();

            diffViewer.ExtraDiffArgumentsChanged += DiffViewerExtraDiffArgumentsChanged;
        }

        public FormLog(GitUICommands commands, GitRevision revision)
            : this(commands)
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