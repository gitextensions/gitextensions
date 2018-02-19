using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormDiff : GitModuleForm
    {
        private readonly RevisionGrid RevisionGrid;
        private string _baseCommitDisplayStr;
        private string _headCommitDisplayStr;
        private GitRevision _baseRevision;
        private GitRevision _headRevision;
        private readonly GitRevision _mergeBase;

        ToolTip _toolTipControl = new ToolTip();

        private readonly TranslationString anotherBranchTooltip =
            new TranslationString("Select another branch");
        private readonly TranslationString anotherCommitTooltip =
            new TranslationString("Select another commit");
        private readonly TranslationString btnSwapTooltip =
            new TranslationString("Swap BASE and Compare commits");

        public FormDiff(GitUICommands aCommands, RevisionGrid revisionGrid, string baseCommitSha,
            string headCommitSha, string baseCommitDisplayStr, string headCommitDisplayStr) : base(aCommands)
        {
            RevisionGrid = revisionGrid;
            _baseCommitDisplayStr = baseCommitDisplayStr;
            _headCommitDisplayStr = headCommitDisplayStr;

            InitializeComponent();
            Translate();

            _toolTipControl.SetToolTip(btnAnotherBaseBranch, anotherBranchTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherHeadBranch, anotherBranchTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherBaseCommit, anotherCommitTooltip.Text);
            _toolTipControl.SetToolTip(btnAnotherHeadCommit, anotherCommitTooltip.Text);
            _toolTipControl.SetToolTip(btnSwap, btnSwapTooltip.Text);

            if (!IsUICommandsInitialized)
            {// UICommands is not initialized in translation unit test.
                return;
            }

            _baseRevision = new GitRevision(baseCommitSha);
            _headRevision = new GitRevision(headCommitSha);
            _mergeBase = new GitRevision(Module.GetMergeBase(_baseRevision.Guid, _headRevision.Guid));

            lblBaseCommit.BackColor = AppSettings.DiffRemovedColor;
            lblHeadCommit.BackColor = AppSettings.DiffAddedColor;

            DiffFiles.SelectedIndexChanged += DiffFiles_SelectedIndexChanged;

            DiffFiles.ContextMenuStrip = DiffContextMenu;

            this.Load += (sender, args) => PopulateDiffFiles();
            this.DiffText.ExtraDiffArgumentsChanged += DiffTextOnExtraDiffArgumentsChanged;
        }

        private void DiffTextOnExtraDiffArgumentsChanged(object sender, EventArgs eventArgs)
        {
            ShowSelectedFileDiff();
        }

        private void PopulateDiffFiles()
        {
            lblBaseCommit.Text = _baseCommitDisplayStr;
            lblHeadCommit.Text = _headCommitDisplayStr;

            if (ckCompareToMergeBase.Checked)
            {
                DiffFiles.SetDiffs(new List<GitRevision> {_headRevision, _mergeBase});
            }
            else
            {
                DiffFiles.SetDiffs(new List<GitRevision> {_headRevision, _baseRevision});
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
            var baseCommit = ckCompareToMergeBase.Checked ? _mergeBase : _baseRevision;

            IList<GitRevision> items = new List<GitRevision> { _headRevision, baseCommit };
            if (items.Count() == 1)
                items.Add(new GitRevision(DiffFiles.SelectedItemParent));
            DiffText.ViewChanges(items, DiffFiles.SelectedItem, String.Empty);
        }

        private void btnSwap_Click(object sender, EventArgs e)
        {
            var orgBaseRev = _baseRevision;
            _baseRevision = _headRevision;
            _headRevision = orgBaseRev;

            var orgBaseStr = _baseCommitDisplayStr;
            _baseCommitDisplayStr = _headCommitDisplayStr;
            _headCommitDisplayStr = orgBaseStr;
            PopulateDiffFiles();
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            GitUI.RevisionDiffKind diffKind;

            if (sender == aLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffALocal;
            else if (sender == bLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffBLocal;
            else if (sender == parentOfALocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffAParentLocal;
            else if (sender == parentOfBLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffBParentLocal;
            else
            {
                Debug.Assert(sender == aBToolStripMenuItem, "Not implemented DiffWithRevisionKind: " + sender);
                diffKind = GitUI.RevisionDiffKind.DiffAB;
            }

            foreach (var selectedItem in DiffFiles.SelectedItems)
            {
                RevisionGrid.OpenWithDifftool(selectedItem.Name, selectedItem.OldName, diffKind, selectedItem.IsTracked);
            }
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
                UICommands.StartFileHistoryDialog(this, item.Name, _baseRevision, false);
            }
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                UICommands.StartFileHistoryDialog(this, item.Name, _baseRevision, true, true);
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

        private void btnPickAnotherBranch_Click(object sender, EventArgs e)
        {
            PickAnotherBranch(_baseRevision, ref _baseCommitDisplayStr, ref _baseRevision);
        }
        private void btnAnotherCommit_Click(object sender, EventArgs e)
        {
            PickAnotherCommit(_baseRevision, ref _baseCommitDisplayStr, ref _baseRevision);
        }

        private void btnAnotherHeadBranch_Click(object sender, EventArgs e)
        {
            PickAnotherBranch(_headRevision, ref _headCommitDisplayStr, ref _headRevision);
        }

        private void btnAnotherHeadCommit_Click(object sender, EventArgs e)
        {
            PickAnotherCommit(_headRevision, ref _headCommitDisplayStr, ref _headRevision);
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            aLocalToolStripMenuItem.Enabled = _baseRevision != null && _baseRevision.Guid != GitRevision.UnstagedGuid && !Module.IsBareRepository();
            bLocalToolStripMenuItem.Enabled = _headRevision != null && _headRevision.Guid != GitRevision.UnstagedGuid && !Module.IsBareRepository();
            parentOfALocalToolStripMenuItem.Enabled = parentOfBLocalToolStripMenuItem.Enabled = !Module.IsBareRepository();

            bool isExactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;
            blameToolStripMenuItem.Visible = isExactlyOneItemSelected && !(DiffFiles.SelectedItem.IsSubmodule || _baseRevision.IsArtificial());
        }

        private void PickAnotherBranch(GitRevision preSelectCommit, ref string displayStr, ref GitRevision revision)
        {
            using (var form = new FormCompareToBranch(UICommands, preSelectCommit.Guid))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    displayStr = form.BranchName;
                    revision = new GitRevision(Module.RevParse(form.BranchName));
                    PopulateDiffFiles();
                }
            }
        }

        private void PickAnotherCommit(GitRevision preSelect, ref string displayStr, ref GitRevision revision)
        {
            using (var form = new FormChooseCommit(UICommands, preselectCommit: preSelect.Guid, showArtificial: true))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    revision = form.SelectedRevision;
                    displayStr = form.SelectedRevision.Subject;
                    PopulateDiffFiles();
                }
            }
        }
    }
}
