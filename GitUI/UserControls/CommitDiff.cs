using System;
using System.Linq;
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

        public void SetRevision(string revisionGuid, string fileToSelect)
        {
            // We cannot use the GitRevision from revision grid. When a filtered commit list
            // is shown (file history/normal filter) the parent guids are not the 'real' parents,
            // but the parents in the filtered list.
            GitRevision revision = Module.GetRevision(revisionGuid);

            if (revision != null)
            {
                DiffFiles.SetDiffs(new[] { revision });
                if (fileToSelect != null)
                {
                    var itemToSelect = DiffFiles.AllItems.FirstOrDefault(i => i.Name == fileToSelect);
                    if (itemToSelect != null)
                    {
                        DiffFiles.SelectedItem = itemToSelect;
                    }
                }

                commitInfo.Revision = revision;

                Text = "Diff - " + GitRevision.ToShortSha(revision.Guid) + " - " + revision.AuthorDate + " - " + revision.Author + " - " + Module.WorkingDir;
            }
        }

        private async void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await ViewSelectedDiffAsync();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            try
            {
                await ViewSelectedDiffAsync();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ViewSelectedDiffAsync()
        {
            GitRevision revision = DiffFiles.Revision;
            if (DiffFiles.SelectedItem != null && revision != null)
            {
                await DiffText.ViewChangesAsync(DiffFiles.SelectedItemParent?.Guid, revision.Guid, DiffFiles.SelectedItem, string.Empty,
                    openWithDifftool: null /* use default */);
            }
        }
    }
}
