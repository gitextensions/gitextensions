using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private FormCommitDiff(GitUICommands commands)
            : base(commands)
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

        public FormCommitDiff(GitUICommands commands, string revisionGuid)
            : this(commands)
        {
            // We cannot use the GitRevision from revision grid. When a filtered commit list
            // is shown (file history/normal filter) the parent guids are not the 'real' parents,
            // but the parents in the filtered list.
            GitRevision revision = Module.GetRevision(revisionGuid);

            if (revision != null)
            {
                commitInfo.Revision = revision;

                DiffFiles.SetDiffs(new[] { revision });

                Text = "Diff - " + GitRevision.ToShortSha(revision.Guid) + " - " + revision.AuthorDate + " - " + revision.Author + " - " + Module.WorkingDir;
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
            if (DiffFiles.SelectedItem != null && DiffFiles.Revision != null)
            {
                await DiffText.ViewChanges(DiffFiles.SelectedItemParent?.Guid, DiffFiles.Revision?.Guid, DiffFiles.SelectedItem, string.Empty);
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
