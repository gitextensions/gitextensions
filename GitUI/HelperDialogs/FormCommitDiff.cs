using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private readonly GitRevision revision;

        private FormCommitDiff()
            : this(null, null)
        { 
        
        }

        public FormCommitDiff(GitUICommands aCommands, GitRevision revision)
            : base(aCommands)
        {
            InitializeComponent(); 
            Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();

            this.revision = revision;

            DiffFiles.GitItemStatuses = null;
            if (this.revision != null)
            {
                DiffFiles.GitItemStatuses = Module.GetDiffFiles(revision.Guid, revision.Guid + "^");

                commitInfo.Revision = revision;
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

        private void ViewSelectedDiff()
        {
            Cursor.Current = Cursors.WaitCursor;

            if (DiffFiles.SelectedItem != null && revision != null)
            {
                Patch selectedPatch = Module.GetSingleDiff(revision.Guid, revision.Guid + "^", DiffFiles.SelectedItem.Name, DiffFiles.SelectedItem.OldName, DiffText.GetExtraDiffArguments(), DiffText.Encoding);
                DiffText.ViewPatch(selectedPatch);
            }
            Cursor.Current = Cursors.Default;
        }

        void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

    }
}
