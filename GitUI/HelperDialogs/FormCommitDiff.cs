using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private readonly GitRevision _revision;

        private FormCommitDiff(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();
            DiffFiles.GitItemStatuses = null;
        }

        private FormCommitDiff()
            : this(null)
        { 
        
        }

        public FormCommitDiff(GitUICommands aCommands, string revision)
            : this(aCommands)
        {
            //We cannot use the GitRevision from revision grid. When a filtered commit list
            //is shown (file history/normal filter) the parent guids are not the 'real' parents,
            //but the parents in the filtered list.
            _revision = Module.GetRevision(revision);

            if (_revision != null)
            {
                DiffFiles.SetDiff(_revision);

                commitInfo.Revision = _revision;

                Text = "Diff - " + GitRevision.ToShortSha(_revision.Guid) + " - " + _revision.AuthorDate + " - " + _revision.Author + " - " + Module.WorkingDir; ;
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

        private void ViewSelectedDiff()
        {
            if (DiffFiles.SelectedItem != null && _revision != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                DiffText.ViewChanges(DiffFiles.SelectedItemParent, _revision.Guid, DiffFiles.SelectedItem, String.Empty);
                Cursor.Current = Cursors.Default;
            }
        }

        void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }
    }
}
