using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private readonly string _revision;

        private FormCommitDiff()
            : this(null, null)
        { 
        
        }

        public FormCommitDiff(GitUICommands aCommands, string revision)
            : base(aCommands)
        {
            InitializeComponent(); 
            Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();

            _revision = revision;

            DiffFiles.GitItemStatuses = null;
            if (_revision != null)
            {
                DiffFiles.SetGitItemStatuses(_revision + "^", Module.GetDiffFiles(_revision, _revision + "^"));

                commitInfo.RevisionGuid = _revision;
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

        private void ViewSelectedDiff()
        {
            Cursor.Current = Cursors.WaitCursor;

            if (DiffFiles.SelectedItem != null && _revision != null)
            {
                Patch selectedPatch = Module.GetSingleDiff(_revision, _revision + "^", DiffFiles.SelectedItem.Name, DiffFiles.SelectedItem.OldName, DiffText.GetExtraDiffArguments(), DiffText.Encoding);
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
