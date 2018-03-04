using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private FormCommitDiff(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();
            DiffFiles.SetDiffs();
        }

        private FormCommitDiff()
            : this(null)
        {
        }

        public FormCommitDiff(GitUICommands aCommands, string revisionGuid)
            : this(aCommands)
        {
            // We cannot use the GitRevision from revision grid. When a filtered commit list
            // is shown (file history/normal filter) the parent guids are not the 'real' parents,
            // but the parents in the filtered list.
            GitRevision revision = Module.GetRevision(revisionGuid);

            if (revision != null)
            {
                DiffFiles.SetDiffs(revision);

                Text = "Diff - " + GitRevision.ToShortSha(revision.Guid) + " - " + revision.AuthorDate + " - " + revision.Author + " - " + Module.WorkingDir; ;
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

        private void ViewSelectedDiff()
        {
            if (DiffFiles.SelectedItem != null && DiffFiles.Revision != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                DiffText.ViewChanges(DiffFiles.SelectedItemParent?.Guid, DiffFiles.Revision?.Guid, DiffFiles.SelectedItem, String.Empty);
                Cursor.Current = Cursors.Default;
            }
        }

        void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }
    }
}
