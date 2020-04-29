using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

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
            diffViewer.ExtraDiffArgumentsChanged += DiffViewerExtraDiffArgumentsChanged;
            diffViewer.TopScrollReached += FileViewer_TopScrollReached;
            diffViewer.BottomScrollReached += FileViewer_BottomScrollReached;
            InitializeComplete();
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
                diffViewer.ViewChangesAsync(DiffFiles.SelectedItem);
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

        private void FileViewer_TopScrollReached(object sender, EventArgs e)
        {
            DiffFiles.SelectPreviousVisibleItem();
            diffViewer.ScrollToBottom();
        }

        private void FileViewer_BottomScrollReached(object sender, EventArgs e)
        {
            DiffFiles.SelectNextVisibleItem();
            diffViewer.ScrollToTop();
        }
    }
}
