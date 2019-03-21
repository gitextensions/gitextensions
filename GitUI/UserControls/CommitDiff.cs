﻿using System;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

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
            DiffFiles.Focus();
            DiffFiles.SetDiffs();

            splitContainer1.SplitterDistance = DpiUtil.Scale(200);
            splitContainer2.SplitterDistance = DpiUtil.Scale(260);
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
                    var itemToSelect = DiffFiles.AllItems.FirstOrDefault(i => i.Name == fileToSelect);
                    if (itemToSelect != null)
                    {
                        DiffFiles.SelectedItem = itemToSelect;
                    }
                }

                commitInfo.Revision = revision;

                Text = "Diff - " + revision.ObjectId.ToShortString() + " - " + revision.AuthorDate + " - " + revision.Author + " - " + Module.WorkingDir;
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

        // Partly the same as RevisionDiffControl.cs ShowSelectedFileDiffAsync()
        private async Task ViewSelectedDiffAsync()
        {
            if (DiffFiles.SelectedItem == null || DiffFiles.Revision == null)
            {
                return;
            }

            if (DiffFiles.SelectedItemParent?.Guid == GitRevision.CombinedDiffGuid)
            {
                var diffOfConflict = Module.GetCombinedDiffContent(DiffFiles.Revision, DiffFiles.SelectedItem.Name,
                    DiffText.GetExtraDiffArguments(), DiffText.Encoding);

                if (string.IsNullOrWhiteSpace(diffOfConflict))
                {
                    diffOfConflict = Strings.UninterestingDiffOmitted;
                }

                DiffText.ViewPatch(text: diffOfConflict, openWithDifftool: null /* not implemented */);
                return;
            }

            await DiffText.ViewChangesAsync(DiffFiles.SelectedItemParent?.ObjectId, DiffFiles.Revision?.ObjectId, DiffFiles.SelectedItem, string.Empty,
                openWithDifftool: null /* use default */);
        }
    }
}
