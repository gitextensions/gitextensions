using System;

namespace GitUI.CommandsDialogs
{
    public partial class FormLog : GitModuleForm
    {
        private readonly CancellationTokenSequence _viewChangesSequence = new();

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

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewChangesSequence.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
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
                _ = diffViewer.ViewChangesAsync(DiffFiles.SelectedItem,
                    cancellationToken: _viewChangesSequence.Next());
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
