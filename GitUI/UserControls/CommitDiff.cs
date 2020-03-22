using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls
{
    public partial class CommitDiff : GitModuleControl
    {
        /// <summary>
        /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
        /// </summary>
        public event Action EscapePressed;

        public CommitDiff()
        {
            InitializeComponent();
            InitializeComplete();

            DiffText.EscapePressed += () => EscapePressed?.Invoke();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffText.TopScrollReached += FileViewer_TopScrollReached;
            DiffText.BottomScrollReached += FileViewer_BottomScrollReached;
            DiffFiles.Focus();
            DiffFiles.ClearDiffs();

            splitContainer1.SplitterDistance = DpiUtil.Scale(200);
            splitContainer2.SplitterDistance = DpiUtil.Scale(260);
        }

        private void FileViewer_TopScrollReached(object sender, EventArgs e)
        {
            DiffFiles.SelectPreviousVisibleItem();
            DiffText.ScrollToBottom();
        }

        private void FileViewer_BottomScrollReached(object sender, EventArgs e)
        {
            DiffFiles.SelectNextVisibleItem();
            DiffText.ScrollToTop();
        }

        public void SetRevision([CanBeNull] ObjectId objectId, [CanBeNull] string fileToSelect)
        {
            // We cannot use the GitRevision from revision grid. When a filtered commit list
            // is shown (file history/normal filter) the parent guids are not the 'real' parents,
            // but the parents in the filtered list.
            GitRevision revision = Module.GetRevision(objectId);

            if (revision != null)
            {
                DiffFiles.SetDiffs(new[] { revision });
                if (fileToSelect != null)
                {
                    var itemToSelect = DiffFiles.AllItems.FirstOrDefault(i => i.Item.Name == fileToSelect);
                    if (itemToSelect != null)
                    {
                        DiffFiles.SelectedGitItem = itemToSelect.Item;
                    }
                }

                commitInfo.Revision = revision;

                Text = "Diff - " + revision.ObjectId.ToShortString() + " - " + revision.AuthorDate + " - " + revision.Author + " - " + Module.WorkingDir;
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ViewSelectedDiffAsync();
            }).FileAndForget();
        }

        private void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ViewSelectedDiffAsync();
            }).FileAndForget();
        }

        private async Task ViewSelectedDiffAsync()
        {
            await DiffText.ViewChangesAsync(DiffFiles.SelectedItem);
        }
    }
}
