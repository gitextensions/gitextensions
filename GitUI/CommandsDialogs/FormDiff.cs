using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public partial class FormDiff : GitModuleForm
    {
        private readonly RevisionGrid RevisionGrid;
        private string _leftDisplayStr;
        private string _rightDisplayStr;
        private GitRevision _leftRevision;
        private GitRevision _rightRevision;
        private readonly GitRevision _mergeBase;

        public FormDiff(GitUICommands aCommands, RevisionGrid revisionGrid, string leftCommitSha,
            string rightCommitSha, string leftDisplayStr, string rightDisplayStr) : base(aCommands)
        {
            RevisionGrid = revisionGrid;
            _leftDisplayStr = leftDisplayStr;
            _rightDisplayStr = rightDisplayStr;

            InitializeComponent();
            Translate();

            _leftRevision = new GitRevision(Module, leftCommitSha);
            _rightRevision = new GitRevision(Module, rightCommitSha);
            _mergeBase = new GitRevision(Module, Module.GetMergeBase(_leftRevision.Guid, _rightRevision.Guid));

            lblLeftCommit.BackColor = AppSettings.DiffRemovedColor;
            lblRightCommit.BackColor = AppSettings.DiffAddedColor;

            DiffFiles.SelectedIndexChanged += DiffFiles_SelectedIndexChanged;

            DiffFiles.ContextMenuStrip = DiffContextMenu;

            this.Load += (sender, args) => PopulateDiffFiles();
        }

        private void PopulateDiffFiles()
        {
            lblLeftCommit.Text = _leftDisplayStr;
            lblRightCommit.Text = _rightDisplayStr;

            if (ckCompareToMergeBase.Checked)
            {
                DiffFiles.SetDiffs(new List<GitRevision> {_rightRevision, _mergeBase});
            }
            else
            {
                DiffFiles.SetDiffs(new List<GitRevision> {_rightRevision, _leftRevision});
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectedFileDiff();
        }
        private void ShowSelectedFileDiff()
        {
            if (DiffFiles.SelectedItem == null)
            {
                DiffText.ViewPatch("");
                return;
            }

            IList<GitRevision> items = new List<GitRevision> { _rightRevision, _leftRevision };
            if (items.Count() == 1)
                items.Add(new GitRevision(Module, DiffFiles.SelectedItemParent));
            DiffText.ViewChanges(items, DiffFiles.SelectedItem, String.Empty);
        }

        private void btnSwap_Click(object sender, EventArgs e)
        {
            var orgLeftRev = _leftRevision;
            _leftRevision = _rightRevision;
            _rightRevision = orgLeftRev;

            var orgLeftStr = _leftDisplayStr;
            _leftDisplayStr = _rightDisplayStr;
            _rightDisplayStr = orgLeftStr;
            PopulateDiffFiles();
        }

        private void btnPickAnotherBranch_Click(object sender, EventArgs e)
        {
            using (var form = new FormCompareToBranch(UICommands, _leftRevision.Guid))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _leftDisplayStr = form.BranchName;
                    _leftRevision = new GitRevision(Module, Module.RevParse(form.BranchName));
                    PopulateDiffFiles();
                }
            }
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            var selectedItem = DiffFiles.SelectedItem;
            GitUIExtensions.DiffWithRevisionKind diffKind;

            if (sender == aLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffALocal;
            else if (sender == bLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffBLocal;
            else if (sender == parentOfALocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffAParentLocal;
            else if (sender == parentOfBLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffBParentLocal;
            else
            {
                Debug.Assert(sender == aBToolStripMenuItem, "Not implemented DiffWithRevisionKind: " + sender);
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffAB;
            }

            string parentGuid = RevisionGrid.GetSelectedRevisions().Count() == 1 ? DiffFiles.SelectedItemParent : null;

            RevisionGrid.OpenWithDifftool(selectedItem.Name, selectedItem.OldName, diffKind, parentGuid);
        }

        private void copyFilenameToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormBrowse.CopyFullPathToClipboard(DiffFiles, Module);
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid))
                    UICommands.StartFileHistoryDialog(this, item.Name);
                else
                    UICommands.StartFileHistoryDialog(this, item.Name, revisions[0], false);
            }
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid))
                    UICommands.StartFileHistoryDialog(this, item.Name, null, false, true);
                else
                    UICommands.StartFileHistoryDialog(this, item.Name, revisions[0], true, true);
            }
        }

        private void findInDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var candidates = DiffFiles.GitItemStatuses;

            Func<string, IList<GitItemStatus>> FindDiffFilesMatches = (string name) =>
            {

                string nameAsLower = name.ToLower();

                return candidates.Where(item =>
                {
                    return item.Name != null && item.Name.ToLower().Contains(nameAsLower)
                        || item.OldName != null && item.OldName.ToLower().Contains(nameAsLower);
                }
                    ).ToList();
            };

            GitItemStatus selectedItem;
            using (var searchWindow = new SearchWindow<GitItemStatus>(FindDiffFilesMatches)
            {
                Owner = this
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }
            if (selectedItem != null)
            {
                DiffFiles.SelectedItem = selectedItem;
            }
        }

        private void ckCompareToMergeBase_CheckedChanged(object sender, EventArgs e)
        {
            PopulateDiffFiles();
        }
    }
}
