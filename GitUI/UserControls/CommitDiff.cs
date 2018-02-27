using System;
using System.Threading.Tasks;
using GitCommands;

namespace GitUI.UserControls
{
    public partial class CommitDiff : GitModuleControl
    {
        public CommitDiff()
        {
            InitializeComponent();
            Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();
            DiffFiles.SetDiffs();
        }

        public void SetRevision(string revisionGuid)
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

        private async Task ViewSelectedDiff()
        {
            GitRevision revision = DiffFiles.Revision;
            if (DiffFiles.SelectedItem != null && revision != null)
            {
                await DiffText.ViewChanges(DiffFiles.SelectedItemParent?.Guid, revision.Guid, DiffFiles.SelectedItem, string.Empty);
            }
        }
    }
}
