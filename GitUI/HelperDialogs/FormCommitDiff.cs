using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private readonly string _revisionGuid;
        private readonly GitRevision _revision;

        private FormCommitDiff()
            : this(null)
        { 
        
        }

        private FormCommitDiff(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();
            DiffFiles.GitItemStatuses = null;
        }

        public FormCommitDiff(GitUICommands aCommands, GitRevision revision)
            : this(aCommands)
        {
            _revision = revision;
            _revisionGuid = revision.Guid;

            if (_revision != null)
            {
                DiffFiles.SetDiff(revision);

                commitInfo.Revision = _revision;
            }
        }

        public FormCommitDiff(GitUICommands aCommands, string revision)
            : this(aCommands)
        {
            _revisionGuid = revision;

            if (_revisionGuid != null)
            {
                DiffFiles.SetGitItemStatuses(_revision + "^", Module.GetDiffFiles(_revisionGuid, _revisionGuid + "^"));

                commitInfo.Revision = Module.GetRevision(_revisionGuid);
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

        private void ViewSelectedDiff()
        {
            Cursor.Current = Cursors.WaitCursor;

            if (DiffFiles.SelectedItem != null)
            {
                if (_revision != null)
                {
                    DiffText.ViewChanges(_revision.Guid, DiffFiles.SelectedItemParent, DiffFiles.SelectedItem, String.Empty);
                }
                else if (_revisionGuid != null)
                {
                    Patch selectedPatch = Module.GetSingleDiff(_revisionGuid, _revisionGuid + "^",
                        DiffFiles.SelectedItem.Name, DiffFiles.SelectedItem.OldName,
                        DiffText.GetExtraDiffArguments(), DiffText.Encoding, true);
                    DiffText.ViewPatch(selectedPatch);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

    }
}
