using System;
using System.Threading.Tasks;
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

                Text = "Diff - " + _revision.Guid.Substring(0, 10) + " - " + _revision.AuthorDate + " - " + _revision.Author + " - " + Module.WorkingDir; ;
            }
        }

        private async void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await ViewSelectedDiff();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ViewSelectedDiff()
        {
            if (DiffFiles.SelectedItem != null && _revision != null)
            {
                await DiffText.ViewChanges(DiffFiles.SelectedItemParent, DiffFiles.Revision?.Guid, DiffFiles.SelectedItem, string.Empty);
            }
        }

        private async void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            try
            {
                await ViewSelectedDiff();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
